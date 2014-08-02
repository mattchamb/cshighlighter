module Visitors
open System
open System.Collections.Generic
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Types

type TypeMemberVisitor() =
    inherit SymbolVisitor<TypeMember option>()
    let m (sym: ISymbol) kind = Some { MemberName = sym.Name; Kind = kind; Signature = sym.ToDisplayString() }
    override x.VisitField sym = 
        m sym Field
    override x.VisitProperty sym = 
        m sym Property
    override x.VisitMethod sym = 
        m sym Method
    override x.VisitEvent sym = 
        m sym Field
    override x.DefaultVisit sym =
        None

type NamedTypeVisitor() =
    inherit SymbolVisitor<TypeMember list>()

    let getMemberDetails =
        let memberVisitor = TypeMemberVisitor()
        let visit (symbol: ISymbol) =
            symbol.Accept memberVisitor
        visit

    override x.DefaultVisit sym =
        List.empty

    override x.VisitNamedType sym =
        sym.GetMembers()
        |> Seq.choose getMemberDetails
        |> Seq.toList

type TypeDeclarationVisitor(model: SemanticModel) =
    inherit CSharpSyntaxWalker(SyntaxWalkerDepth.Node)
    
    let outputElements = ref List.empty<DeclaredType>

    let getMembers (a:INamedTypeSymbol) =
        let declVisitor = NamedTypeVisitor()
        a.Accept declVisitor
    
    let declType typeKind sym =
        let members = getMembers sym
        { TypeName = sym.Name; Kind = typeKind; Members = members }

    override x.VisitClassDeclaration decl =
        let t = declType Class <| model.GetDeclaredSymbol(decl) 
        outputElements := t :: !outputElements
            

    override x.VisitEnumDeclaration decl =
        let t = declType Enum <| model.GetDeclaredSymbol(decl) 
        outputElements := t :: !outputElements

    override x.VisitStructDeclaration decl =
        let t = declType Struct <| model.GetDeclaredSymbol(decl) 
        outputElements := t :: !outputElements

    member x.getOutput() = !outputElements

type TokenType =
        | Declaration of ISymbol
        | Reference of ISymbol
        | Unknown

type TokenVisitor(model: SemanticModel) =
    inherit CSharpSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia)
    let modelDiagnostics = model.GetDiagnostics()

    let outputElements = new List<TokenClassification>()

    let addElement ele =
        outputElements.Add ele

    let semanticErrors (token: SyntaxToken) =
        let tokenDiagnostics = 
            modelDiagnostics
            |> Seq.filter (fun d -> d.Location.SourceSpan.Contains(token.Span))
            |> Seq.toArray
        let errors =
            tokenDiagnostics
            |> Seq.filter (fun diag -> diag.Severity = DiagnosticSeverity.Error)
            |> Seq.toArray
        match errors.Length with
        | 0 -> None
        | _ -> Some errors
        
    let (| ClassOrStructField | EnumField |) (symbol: ISymbol) =
        match symbol.ContainingType with
        | null -> ClassOrStructField
        | _ ->
            match symbol.ContainingType.EnumUnderlyingType with
            | null -> ClassOrStructField
            | _ -> EnumField

    let tokenType (token: SyntaxToken) =
        let declarationSymbol = model.GetDeclaredSymbol(token.Parent)
        match declarationSymbol with
        | null -> 
            // The token isnt part of a declaration node, so try to get symbol info.
            let referenceSymbol = model.GetSymbolInfo(token.Parent).Symbol
            match referenceSymbol with
            | null -> 
                // we couldnt find symbol information for the node, so we will look at all symbols in scope by name.
                let namedSymbols = model.LookupSymbols(token.SpanStart, null, token.ToString(), true)
                match namedSymbols.Length with
                | 1 -> 
                    Reference (namedSymbols.[0])
                | _ ->
                    Unknown
            | _ -> 
                Reference referenceSymbol
        | _ -> 
            // The token is part of a declaration syntax node.
            Declaration declarationSymbol

    let transformDeclaration (token: SyntaxToken) (symbol: ISymbol) =
        match symbol with
        | :? ILocalSymbol as sym -> LocalVariableDeclaration (token, sym)
        | :? INamedTypeSymbol as sym -> NamedTypeDeclaration (token, sym)
        | :? IPropertySymbol as sym -> PropertyDeclaration token
        | :? IMethodSymbol as sym -> MethodDeclaration token
        | :? IParameterSymbol as sym -> ParameterDeclaration token
        | :? IFieldSymbol as sym ->
            match sym with
            | ClassOrStructField -> FieldDeclaration (token, sym)
            | EnumField -> EnumMemberDeclaration (token, sym)
        | _ -> Identifier token

    let transformReference (token: SyntaxToken) (symbol: ISymbol) =
        match symbol with
            | :? IFieldSymbol as sym -> 
                match sym with
                | ClassOrStructField -> FieldReference (token, sym)
                | EnumField -> EnumMemberReference (token, sym)
            | :? ILocalSymbol as sym -> LocalVariableReference (token, sym)
            | :? IParameterSymbol as sym -> ParameterReference (token, sym)
            | :? IPropertySymbol as sym -> PropertyReference (token, sym)
            | :? INamedTypeSymbol as sym -> NamedTypeReference (token, sym)
            | :? IMethodSymbol as sym -> MethodReference (token, sym)
            | :? INamespaceSymbol as sym -> Identifier token
            | _ -> Identifier token

    let classifyIdentifierToken (token: SyntaxToken) = 
        match tokenType token with
        | Declaration declarationSymbol ->
            transformDeclaration token declarationSymbol
        | Reference referenceSymbol ->
            transformReference token referenceSymbol
        | _ ->
            Identifier token
        
    override x.VisitToken token =
        x.VisitLeadingTrivia token
        match semanticErrors token with
        | Some errors -> SemanticError (token, errors)
        | None ->
            if token.IsKeyword() || token.IsContextualKeyword() then
                Keyword token
            else
                let kind = token.CSharpKind()
                match kind with
                | SyntaxKind.IdentifierToken -> classifyIdentifierToken token
                | SyntaxKind.StringLiteralToken -> StringLiteral token
                | SyntaxKind.NumericLiteralToken -> NumericLiteral token
                | SyntaxKind.CharacterLiteralToken -> CharacterLiteral token
                | _ -> Unformatted token
        |> addElement
        x.VisitTrailingTrivia token

    override x.VisitTrivia trivia =
        if trivia.HasStructure then
            x.Visit (trivia.GetStructure())
        else
            let output = 
                match trivia.CSharpKind() with
                | SyntaxKind.MultiLineCommentTrivia
                | SyntaxKind.SingleLineCommentTrivia -> Comment trivia
                | SyntaxKind.EndOfLineTrivia -> NewLine
                | SyntaxKind.WhitespaceTrivia -> Whitespace trivia
                | SyntaxKind.RegionDirectiveTrivia -> BeginRegion trivia
                | SyntaxKind.EndRegionDirectiveTrivia -> EndRegion trivia
                | SyntaxKind.DisabledTextTrivia -> DisabledText trivia
                | _ -> UnformattedTrivia trivia
                |> Trivia
            addElement output

    member x.getOutput() = outputElements