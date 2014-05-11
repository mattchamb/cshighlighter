namespace CSHighlighter

module Formatting =

    open System
    open System.Text
    open System.IO
    open Analysis
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text
    open System.Net
    open HighlighterLib.Templating

    type FormattingEnvironment =
        | Standalone
        | Project of Solution * DocumentId

    type HtmlAttribute =
        | Id of string
        | Class of string array
        | HoverId of string // Used to add the .highlighted class to all elements which have a class that matches this value.
        | Title of string
        | Href of string

    type HtmlElement =
        | Span of contents: string * attributes: HtmlAttribute list
        | Anchor of contents: string * attributes: HtmlAttribute list
        | Literal of text: string
    
    module HtmlPrinting =
        let combineClasses (c: string array) = String.Join(" ", c)

        let combineAttributes (attributes: HtmlAttribute list) = 
            let sb = new StringBuilder()
            attributes
            |> List.iter (
                fun attr ->
                    match attr with
                    | Id t -> sb.AppendFormat("id=\"{0}\" ", t) |> ignore
                    | Class c -> sb.AppendFormat("class=\"{0}\" ", combineClasses c) |> ignore
                    | HoverId t -> sb.AppendFormat("data-hover=\"{0}\" ", t) |> ignore
                    | Title t -> sb.AppendFormat("title=\"{0}\" ", t) |> ignore
                    | Href t -> sb.AppendFormat("href=\"{0}\" ", t) |> ignore
                )
            sb.ToString()

        let formatElement (htmlEle:HtmlElement) =
            match htmlEle with
            | Span (contents, attributes) -> 
                let encoded = WebUtility.HtmlEncode contents
                let attributeText = combineAttributes attributes
                sprintf @"<span %s>%s</span>" attributeText encoded
                
            | Anchor (contents, attributes) -> 
                let encoded = WebUtility.HtmlEncode contents
                let attributeText = combineAttributes attributes
                sprintf @"<a %s>%s</a>" attributeText encoded
            | Literal text -> WebUtility.HtmlEncode text


    let htmlFormat (env: FormattingEnvironment) (tokens: TokenClassification seq) =

        /// Gets information that allows the destLoc to be referenced from the current formatting environment.
        /// The destLoc can be either in the file that is currently being formatted; some other source file; or a metadata reference.
        /// For metadata references, this function returns None. Otherwise it returns Some (destLocId, destIdRef, className)
        let getReferenceInfo (destLoc: Location) =
            /// The href location for an element id
            let internalFileIdRef (span: TextSpan) = 
                sprintf "#loc%d_%d" span.Start span.End

            let htmlifyPath path =
                sprintf "%s.html" path

            let classNameForLocation (span: TextSpan) =
                sprintf "loc%d_%d" span.Start span.End

            /// Get the id attribute for the thing that is defined in this span.
            let idForLocation (span: TextSpan) =
                // Currently the id and class names match. This may change in the future.
                classNameForLocation span

            let destInfo =
                match destLoc.Kind with
                | LocationKind.SourceFile -> 
                    let span = destLoc.SourceSpan
                    let destIdRef = internalFileIdRef span
                    let destFilePath = new Uri(htmlifyPath destLoc.SourceTree.FilePath)
                    let className = classNameForLocation span
                    let destLocId = idForLocation span
                    match env with
                    | Standalone -> 
                        Some (destLocId, destIdRef, className)
                    | Project (sol, docId) ->
                        let doc = sol.GetDocument docId
                        let currentDoc = new Uri (htmlifyPath doc.FilePath)
                        let relativeUri = currentDoc.MakeRelativeUri(destFilePath)
                        let destPath = relativeUri.ToString()
                        Some (destLocId, sprintf "%s%s" destPath destIdRef, className)

                | _ -> 
                    None
            destInfo

        let symbolReferenceInfo (symbol: ISymbol) =
            // Currently only supporting one location for partial classes, etc.
            let loc = Seq.head symbol.Locations
            getReferenceInfo loc

        let tokenReferenceInfo (tok: SyntaxToken) =
            let loc = tok.GetLocation()
            getReferenceInfo loc

        let getSymbolTitle (sym: ISymbol) =
            let displayStr = sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            let loc = sym.Locations.[0]
            match loc.Kind with
            | LocationKind.SourceFile -> sprintf "%s\n%s" displayStr (Path.GetFileName loc.SourceTree.FilePath)
            | LocationKind.MetadataFile -> sprintf "%s\n%s" displayStr (loc.MetadataModule.ContainingAssembly.ToDisplayString())
            | _ -> displayStr

        let intoSpan attributes text = Span (text, attributes)
        let intoLiteralSpan spanClass text = Span (text, [ Class [|spanClass|] ])
        let intoHref attributes text = Anchor (text, attributes)
        let intoTitledSpan spanClass title text = Span (text, [ Class [|spanClass|]; Title title ])

        let comment = intoLiteralSpan "comment"
        let keyword = intoLiteralSpan "keyword"
        let ident = intoLiteralSpan "identifier"
        let stringLiteral = intoLiteralSpan "stringLiteral"
        let numericLiteral = intoLiteralSpan "numericLiteral"
        let region text = intoLiteralSpan "region" (text + Environment.NewLine)
        let semanticError = intoTitledSpan "semanticError" 

        /// There is reference another defined symbol. The symbol may or may not be in the file that we are formatting.
        let sourceReference referenceClass tok sym =
            let refInfo = symbolReferenceInfo sym
            let symbolDisplayText = getSymbolTitle sym
            match refInfo with
            | Some (destLocId, destHref, className) -> 
                
                let attribs = [
                        Class [|referenceClass; className|];
                        Href destHref;
                        HoverId className;
                        Title symbolDisplayText
                    ]
                intoHref attribs
            | None -> 
                let attribs = [
                        Class [|referenceClass|];
                        Title symbolDisplayText
                    ]
                intoSpan attribs
                
        let localRef = sourceReference "localRef"
        let fieldRef = sourceReference "fieldRef"
        let paramRef = sourceReference "paramRef"
        let propRef = sourceReference "propRef"
        let methodRef = sourceReference "methodRef"
        let namedTypeRef = sourceReference "namedTypeRef"

        /// There is something being declared in the file that we are formatting.
        let sourceDeclaration declClass tok =
            let refInfo = tokenReferenceInfo tok
            match refInfo with
            | Some (destLocId, _, className) -> 
                let attribs = [
                        Class [|declClass; className|];
                        HoverId className;
                        Id destLocId
                    ]
                intoSpan attribs
            | None -> 
                let attribs = [
                        Class [|declClass|];
                    ]
                intoSpan attribs

        let propDecl = sourceDeclaration "propDecl"
        let paramDecl = sourceDeclaration "paramDecl"
        let fieldDecl = sourceDeclaration "fieldDecl"
        let localDecl = sourceDeclaration "localDecl"
        let methodDecl = sourceDeclaration "methodDecl"
        let namedTypeDecl = sourceDeclaration "namedTypeDecl"

        let toStr tok =
            tok.ToString()

        let classificationTransform ele = 
            match ele with
            | Unformatted tok -> Literal <| toStr tok
            | Keyword tok -> keyword <| toStr tok
            | Identifier tok -> Literal <| toStr tok
            | NamedTypeDeclaration tok -> namedTypeDecl tok <| toStr tok
            | NamedTypeReference (tok, sym) -> namedTypeRef tok sym <| toStr tok
            | StringLiteral tok -> stringLiteral <| toStr tok
            | NumericLiteral tok -> numericLiteral <| toStr tok
            | LocalVariableDeclaration (tok, sym) -> localDecl tok <| toStr tok
            | LocalVariableReference (tok, sym) -> localRef tok sym <| toStr tok
            | FieldDeclaration tok -> fieldDecl tok <| toStr tok
            | FieldReference (tok, sym) -> fieldRef tok sym <| toStr tok
            | ParameterDeclaration tok -> paramDecl tok <| toStr tok
            | ParameterReference (tok, sym) -> paramRef tok sym <| toStr tok
            | PropertyDeclaration tok -> propDecl tok <| toStr tok
            | PropertyReference (tok, sym) -> propRef tok sym <| toStr tok
            | MethodDeclaration tok -> methodDecl tok <| toStr tok
            | MethodReference (tok, sym) -> methodRef tok sym <| toStr tok
            | SemanticError (tok, errors) -> 
                let errorMessage = String.Join(Environment.NewLine, errors)
                semanticError errorMessage <| toStr tok
            | Trivia tr -> 
                match tr with
                | TriviaElement.BeginRegion s -> region <| toStr s 
                | TriviaElement.Comment s -> comment <| toStr s 
                | TriviaElement.EndRegion s -> region <| toStr s 
                | TriviaElement.NewLine -> Literal Environment.NewLine
                | TriviaElement.UnformattedTrivia s -> Literal <| toStr s 
                | TriviaElement.Whitespace s -> Literal <| toStr s 
            //| _ -> failwith "DONT KNOW HOW TO FORMAT"

        let htmlOutput = 
            let out = new StringBuilder()
            tokens
            |> Seq.map classificationTransform
            |> Seq.iter 
                (fun h -> 
                    HtmlPrinting.formatElement h
                    |> out.Append
                    |> ignore)

            out.ToString();
        htmlOutput
