namespace CSHighlighter

module Analysis =

    open System
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text

    type SourceInput =
        {
            Path: string;
            Contents: string
        }

    type OutputElement =
        | Unformatted of SyntaxToken
        | Trivia of TriviaElement
        | Keyword of SyntaxToken
        | Identifier of SyntaxToken
        | StringLiteral of SyntaxToken
        | NumericLiteral of SyntaxToken
        | LocalVariableDeclaration of SyntaxToken * ISymbol
        | LocalVariableReference of SyntaxToken * ISymbol
        | FieldDeclaration of SyntaxToken
        | FieldReference of SyntaxToken * ISymbol
        | ParameterReference of SyntaxToken * ISymbol
        | ParameterDeclaration of SyntaxToken
        | PropertyDeclaration of SyntaxToken
        | PropertyReference of SyntaxToken * ISymbol
        | NamedTypeDeclaration of SyntaxToken
        | NamedTypeReference of SyntaxToken * ISymbol
        | MethodDeclaration of SyntaxToken
        | MethodReference of SyntaxToken * ISymbol
    and TriviaElement =
        | Comment of SyntaxTrivia
        | BeginRegion of SyntaxTrivia
        | EndRegion of SyntaxTrivia
        | NewLine
        | Whitespace of SyntaxTrivia
        | UnformattedTrivia of SyntaxTrivia

    type Visitor(model : SemanticModel) =
        inherit CSharpSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia)

        let mutable outputElements: OutputElement list = []

        let addElement ele =
            outputElements <- ele :: outputElements

        let isVarDecl (token:SyntaxToken) = 
            token.Parent.Parent.CSharpKind() = SyntaxKind.VariableDeclaration && token.ToString().Equals("var", StringComparison.OrdinalIgnoreCase)

        let identifierElement (token:SyntaxToken) = 
            let tokenKind = token.Parent.CSharpKind()
            if isVarDecl token then 
                Keyword token
            else
                let symbol = model.GetSymbolInfo(token.Parent).Symbol
                match tokenKind with
                | SyntaxKind.ClassDeclaration
                | SyntaxKind.EnumDeclaration
                | SyntaxKind.StructDeclaration -> NamedTypeDeclaration token
                | SyntaxKind.PropertyDeclaration -> PropertyDeclaration token
                | SyntaxKind.MethodDeclaration -> MethodDeclaration token
                | SyntaxKind.VariableDeclarator -> 
                    match token.Parent.Parent.Parent.CSharpKind() with
                    | SyntaxKind.FieldDeclaration -> FieldDeclaration token
                    | SyntaxKind.LocalDeclarationStatement -> LocalVariableDeclaration (token, symbol)
                    | _ -> Identifier token
                | SyntaxKind.Parameter -> ParameterDeclaration token
                | SyntaxKind.IdentifierToken -> Identifier token
                | _ ->
                    if symbol <> null then
                        let declLoc = symbol.Locations.[0].SourceSpan
                        match symbol.Kind with
                        | SymbolKind.Field -> FieldReference (token, symbol)
                        | SymbolKind.Local -> LocalVariableReference (token, symbol)
                        | SymbolKind.Parameter -> ParameterReference (token, symbol)
                        | SymbolKind.Property -> PropertyReference (token, symbol)
                        | SymbolKind.NamedType -> NamedTypeReference (token, symbol)
                        | SymbolKind.Method -> MethodReference (token, symbol)
                        | _ -> Identifier token
                    else 
                        Identifier token
        
        override x.VisitToken token =
            x.VisitLeadingTrivia token
            if token.IsKeyword() then
                Keyword token
            else
                let kind = token.CSharpKind()
                match kind with
                | SyntaxKind.IdentifierToken -> identifierElement token
                | SyntaxKind.StringLiteralToken ->  StringLiteral token
                | SyntaxKind.NumericLiteralToken ->  NumericLiteral token
                | _ -> Unformatted token
            |> addElement
            x.VisitTrailingTrivia token

        override x.VisitTrivia trivia =
            let output = match trivia.CSharpKind() with
                            | SyntaxKind.MultiLineCommentTrivia
                            | SyntaxKind.SingleLineCommentTrivia -> Comment trivia
                            | SyntaxKind.EndOfLineTrivia -> NewLine
                            | SyntaxKind.WhitespaceTrivia -> Whitespace trivia
                            | SyntaxKind.RegionDirectiveTrivia -> BeginRegion trivia
                            | SyntaxKind.EndRegionDirectiveTrivia -> EndRegion trivia
                            | _ -> UnformattedTrivia trivia
                        |> Trivia
            addElement output

        member x.getOutput() = outputElements |> List.rev // Reverse the elements because we are appending to the front when building the list.


    let compilationForSource trees =
        CSharpCompilation.Create("highlightingCompilation", trees, [|new MetadataFileReference(typeof<Object>.Assembly.Location)|])

    let parseCode (f:SourceInput) =
        CSharpSyntaxTree.ParseText (f.Contents, f.Path)

    let createHighlightingModel syntaxTreeRoot model =
        let v = new Visitor(model)
        v.DefaultVisit syntaxTreeRoot
        let elements = v.getOutput()
        List.toArray elements
      
    let analyseFile (file: SourceInput) = 
        let syntaxTree = parseCode file
        let compilation = compilationForSource [|syntaxTree|]
        let root = syntaxTree.GetRoot()
        let model = compilation.GetSemanticModel(syntaxTree)
        createHighlightingModel root model

    let analyseFiles (files: SourceInput seq) =
        let syntaxTrees =
            files
            |> Seq.map parseCode
            |> Seq.toArray
        let compilation = compilationForSource syntaxTrees
        let fileOutputs =
            syntaxTrees
            |> Array.map 
                (fun t -> 
                    let root = t.GetRoot()
                    let model = compilation.GetSemanticModel(t)
                    createHighlightingModel root model)
        fileOutputs


