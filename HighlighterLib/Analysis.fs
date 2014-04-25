module Analysis

open System
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.Text

type OutputElement =
    | Unformatted of text: string
    | NewLine
    | Comment of text: string
    | Keyword of text: string
    | Identifier of text: string
    | TypeDeclaration of text: string * location: TextSpan
    | StringLiteral of text: string
    | NumericLiteral of text: string
    | BeginRegion of text: string
    | EndRegion of text: string
    | LocalVariableDeclaration of text: string * location: TextSpan
    | LocalVariableReference of text: string * declarationLocation: TextSpan
    | FieldDeclaration of text: string * location: TextSpan
    | FieldReference of text: string * declarationLocation: TextSpan
    | ParameterReference of text: string * declarationLocation: TextSpan
    | ParameterDeclaration of text: string * location: TextSpan
    | PropertyDeclaration of text: string * location: TextSpan
    | PropertyReference of text: string * declarationLocation: TextSpan

let semanticModel tree =
    let compilation = CSharpCompilation.Create("asdf", [|tree|], [|new MetadataFileReference(typeof<Object>.Assembly.Location)|])
    compilation.GetSemanticModel tree

type Visitor(model : SemanticModel) =
    inherit CSharpSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia)

    let mutable outputElements: OutputElement list = []

    let addElement ele =
        outputElements <- ele :: outputElements

    let isDecendedFromKind (levels:int) (kind:SyntaxKind)  (token:SyntaxToken) =
        let rec isChildInternal (node:SyntaxNode) (currentLevel:int) =
            if node = null then
                false
            else if currentLevel = levels && node.CSharpKind() = kind then
                true
            else
                isChildInternal node.Parent (currentLevel + 1)
        isChildInternal token.Parent 1

    let isGrandparentOfKind = isDecendedFromKind 2
    let isParentOfKind = isDecendedFromKind 1

    let isVarDecl (token:SyntaxToken) = 
        token.Parent.Parent.CSharpKind() = SyntaxKind.VariableDeclaration && token.ToString() = "var"

    let identifierElement (token:SyntaxToken) = 
        let text = token.ToString()
        let tokenKind = token.Parent.CSharpKind()
        if isVarDecl token then 
            Keyword text
        else
            let location = token.GetLocation().SourceSpan
            match tokenKind with
            | SyntaxKind.ClassDeclaration
            | SyntaxKind.StructDeclaration -> TypeDeclaration (text, location)
            | SyntaxKind.PropertyDeclaration -> PropertyDeclaration (text, location)
            | SyntaxKind.VariableDeclarator -> 
                match token.Parent.Parent.Parent.CSharpKind() with
                | SyntaxKind.FieldDeclaration -> FieldDeclaration (text, location)
                | SyntaxKind.LocalDeclarationStatement -> LocalVariableDeclaration (text, location)
                | _ -> failwith "ASDASD"
            | SyntaxKind.Parameter -> ParameterDeclaration (text, location)
            | SyntaxKind.IdentifierToken -> Identifier text
            | _ ->
                let symbol = model.GetSymbolInfo(token.Parent).Symbol
                if symbol <> null then
                    let declLoc = symbol.Locations.[0].SourceSpan
                    match symbol.Kind with
                    | SymbolKind.Field -> FieldReference (text, declLoc)
                    | SymbolKind.Local -> LocalVariableReference (text, declLoc)
                    | SymbolKind.Parameter -> ParameterReference (text, declLoc)
                    | SymbolKind.Property -> PropertyReference (text, declLoc)
                    | _ -> Identifier text
                else 
                    Identifier text

    member x.getOutput() = outputElements |> List.rev // Reverse the elements because we are appending to the front when building the list.

    override x.VisitTrivia trivia =
        let output = match trivia.CSharpKind() with
                        | SyntaxKind.MultiLineCommentTrivia
                        | SyntaxKind.SingleLineCommentTrivia -> Comment (trivia.ToString())
                        | SyntaxKind.EndOfLineTrivia -> NewLine
                        | SyntaxKind.WhitespaceTrivia -> Unformatted (trivia.ToString())
                        | SyntaxKind.RegionDirectiveTrivia -> BeginRegion (trivia.ToString())
                        | SyntaxKind.EndRegionDirectiveTrivia -> EndRegion (trivia.ToString())
                        | _ -> Unformatted (trivia.ToString())
        addElement output

    override x.VisitToken token =
        x.VisitLeadingTrivia token
        
        if token.IsKeyword() then
            Keyword (token.Text)
        else
            let kind = token.CSharpKind()
            match kind with
            | SyntaxKind.IdentifierToken -> identifierElement token
            | SyntaxKind.StringLiteralToken ->  StringLiteral (token.ToString())
            | SyntaxKind.NumericLiteralToken ->  NumericLiteral (token.ToString())
            | _ -> Unformatted (token.ToString())
        |> addElement
        x.VisitTrailingTrivia token

let analyseCode (text: string) =
    let tree = CSharpSyntaxTree.ParseText text
    let root = tree.GetRoot() :?> CompilationUnitSyntax
    let model = semanticModel tree
    let v = new Visitor(model)
    v.DefaultVisit root
    let elements = v.getOutput()
    elements |> List.toArray