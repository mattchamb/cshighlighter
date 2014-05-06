namespace CSHighlighter

module Formatting =

    open System
    open System.Text
    open Analysis
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text
    open System.Net
    open HighlighterLib.Templating

    type HtmlAttribute =
        | Id of string
        | Class of string array
        | HoverId of string
        | Title of string
        | Href of string

    type HtmlElement =
        | Span of contents: string * attributes: HtmlAttribute list
        | Anchor of contents: string * attributes: HtmlAttribute list
        | Literal of text: string
    
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

    let formatHtmlElement (htmlEle:HtmlElement) =
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

    let locationToString (location:Location) = 
        match location.Kind with
            | LocationKind.SourceFile -> Some <| sprintf "loc%d_%d" location.SourceSpan.Start location.SourceSpan.End
            | _ -> None

    let symbolLocationString (symbol:ISymbol) =
        let loc = Seq.head symbol.Locations
        locationToString loc

    let tokenLocationString (tok:SyntaxToken) =
        let loc = tok.GetLocation()
        locationToString loc

    let htmlFormat (eles: OutputElement array) =

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

        let sourceReference referenceClass tok sym =
            let symbolLocation = symbolLocationString sym
            let symbolDisplayText = sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            match symbolLocation with
            | Some x -> 
                let attribs = [
                        Class [|referenceClass; x|];
                        Href <| sprintf "#%s" x;
                        HoverId x;
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

        let sourceDeclaration declClass tok =
            let tokenLocation = tokenLocationString tok
            match tokenLocation with
            | Some x -> 
                let attribs = [
                        Class [|declClass; x|];
                        HoverId x;
                        Id x
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

        let toStr tok =
            tok.ToString()

        let htmlElementTransform ele = 
            match ele with
            | Unformatted tok -> Literal <| toStr tok
            | Keyword tok -> keyword <| toStr tok
            | Identifier tok -> Literal <| toStr tok
            | NamedTypeDeclaration tok -> ident <| toStr tok
            | NamedTypeReference (tok, sym) -> ident <| toStr tok
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
            eles
            |> Array.map htmlElementTransform
            |> Array.iter 
                (fun h -> 
                    formatHtmlElement h
                    |> out.Append
                    |> ignore)

            out.ToString();
        htmlOutput
