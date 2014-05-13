namespace CSHighlighter

module Analysis =

    open System
    open System.Collections.Generic
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text

    type TokenClassification =
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
        | EnumMemberDeclaration of SyntaxToken
        | EnumMemberReference of SyntaxToken * ISymbol
        | SemanticError of SyntaxToken * Diagnostic array
    and TriviaElement =
        | Comment of SyntaxTrivia
        | BeginRegion of SyntaxTrivia
        | EndRegion of SyntaxTrivia
        | NewLine
        | Whitespace of SyntaxTrivia
        | UnformattedTrivia of SyntaxTrivia

    type AnalysisResult =
        {
            ClassifiedTokens: List<TokenClassification>
        }

    type Visitor(model : SemanticModel) =
        inherit CSharpSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia)
        let modelDiagnostics = model.GetDiagnostics()

        let outputElements = new List<TokenClassification>()

        let addElement ele =
            outputElements.Add ele

        let isVarDecl (token:SyntaxToken) = 
            token.Parent.Parent.CSharpKind() = SyntaxKind.VariableDeclaration && token.ToString().Equals("var", StringComparison.OrdinalIgnoreCase)

        let examineDefinedSymbols (token: SyntaxToken) = 
            let definedSymbols = model.LookupSymbols(token.SpanStart, null, token.ToString(), true)
            match definedSymbols.Length with
            | 1 -> 
                let symbol = definedSymbols.[0]
                match symbol.Kind with
                | SymbolKind.Method -> MethodReference (token, symbol)
                | SymbolKind.NamedType -> NamedTypeReference (token, symbol)
                | _ -> Identifier token
            | _ -> 
                Identifier token

        let semanticErrors (token:SyntaxToken) =
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

        let identifierElement (token:SyntaxToken) = 
            let tokenKind = token.Parent.CSharpKind()
            if isVarDecl token then 
                Keyword token
            else
                match semanticErrors token with
                | Some errors -> SemanticError (token, errors)
                | None ->
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
                    | SyntaxKind.EnumMemberDeclaration -> EnumMemberDeclaration token
                    | SyntaxKind.IdentifierToken -> Identifier token
                    | _ ->
                        if symbol <> null then
                            let declLoc = symbol.Locations.[0].SourceSpan
                            match symbol.Kind with
                            | SymbolKind.Field -> 
                                match symbol with
                                | ClassOrStructField -> FieldReference (token, symbol)
                                | EnumField -> EnumMemberReference (token, symbol)
                            | SymbolKind.Local -> LocalVariableReference (token, symbol)
                            | SymbolKind.Parameter -> ParameterReference (token, symbol)
                            | SymbolKind.Property -> PropertyReference (token, symbol)
                            | SymbolKind.NamedType -> NamedTypeReference (token, symbol)
                            | SymbolKind.Method -> MethodReference (token, symbol)
                            | _ -> Identifier token
                        else 
                            examineDefinedSymbols token
        
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

        override x.VisitClassDeclaration decl =
            let t = model.GetDeclaredSymbol(decl)
            let members = t.GetMembers()
            base.VisitClassDeclaration decl

        override x.VisitEnumDeclaration decl =
            base.VisitEnumDeclaration decl

        override x.VisitStructDeclaration decl =
            base.VisitStructDeclaration decl

        member x.getOutput() = outputElements


    let compilationForSource trees =
        CSharpCompilation.Create("highlightingCompilation", trees, [|new MetadataFileReference(typeof<Object>.Assembly.Location)|])

    let parseCode (code: string) =
        CSharpSyntaxTree.ParseText (code, String.Empty)

    let createHighlightingModel syntaxTreeRoot model =
        let v = new Visitor(model)
        v.DefaultVisit syntaxTreeRoot
        let elements = v.getOutput()
        elements
      
    let analyseFile (code: string) = 
        let syntaxTree = parseCode code
        let compilation = compilationForSource [|syntaxTree|]
        let root = syntaxTree.GetRoot()
        let model = compilation.GetSemanticModel(syntaxTree)
        let classifiedTokens = createHighlightingModel root model
        { ClassifiedTokens = classifiedTokens }

//    let analyseFiles (files: SourceInput seq) =
//        let syntaxTrees =
//            files
//            |> Seq.map parseCode
//            |> Seq.toArray
//        let compilation = compilationForSource syntaxTrees
//        let fileOutputs =
//            syntaxTrees
//            |> Array.map 
//                (fun t -> 
//                    let root = t.GetRoot()
//                    let model = compilation.GetSemanticModel(t)
//                    createHighlightingModel root model)
//        fileOutputs


