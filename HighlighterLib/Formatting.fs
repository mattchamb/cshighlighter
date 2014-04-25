namespace Formatting
module Formatting =

    open System
    open Analysis
    open Microsoft.CodeAnalysis.Text
    open System.Net

    let locationToClassName (loc:TextSpan) =
            sprintf "loc%dx%d" loc.Start loc.End

    let locationClassName ele = 
        match ele with
        | TypeDeclaration (_, loc) -> Some <| locationToClassName loc
        | LocalVariableDeclaration (_, loc) -> Some <| locationToClassName loc
        | FieldDeclaration (_, loc) -> Some <| locationToClassName loc
        | ParameterDeclaration (_, loc) -> Some <| locationToClassName loc
        | PropertyDeclaration (_, loc) -> Some <| locationToClassName loc
        | _ -> None

    let generateCss (eles: OutputElement array) =
        let hoverClasses = 
            seq {
                for hClass in Array.choose locationClassName eles do
                    yield sprintf ".%s:hover { background-color: aqua; }%s" hClass Environment.NewLine
            }
        String.Join(Environment.NewLine, hoverClasses)

    let htmlFormat (eles: OutputElement array) =

        let intoSpan spanClass id text = sprintf @"<span id=""%s"" class=""%s"">%s</span>" id spanClass (WebUtility.HtmlEncode text)
        let intoLiteralSpan spanClass text = sprintf @"<span class=""%s"">%s</span>" spanClass (WebUtility.HtmlEncode text)
        let intoHref spanClass id text = sprintf @"<a href=""#%s"" class=""%s"">%s</a>" id spanClass (WebUtility.HtmlEncode text)

        let comment = intoLiteralSpan "comment"
        let keyword = intoLiteralSpan "keyword"
        let ident = intoLiteralSpan "identifier"
        let stringLiteral = intoLiteralSpan "stringLiteral"
        let numericLiteral = intoLiteralSpan "numericLiteral"
        let region = intoLiteralSpan "region"

        let localRef loc = 
            let c = locationToClassName loc
            intoHref (sprintf "localRef %s" c) c
        let fieldRef loc = 
            let c = locationToClassName loc
            intoHref (sprintf "fieldRef %s" c) c
        let paramRef loc = 
            let c = locationToClassName loc
            intoHref (sprintf "paramRef %s" c) c
        let propRef loc = 
            let c = locationToClassName loc
            intoHref (sprintf "propRef %s" c) c

        let propDecl loc = 
            let c = locationToClassName loc
            intoSpan (sprintf "propDecl %s" c) c
        let paramDecl loc = 
            let c = locationToClassName loc
            intoSpan (sprintf "paramDecl %s" c) c
        let fieldDecl loc = 
            let c = locationToClassName loc
            intoSpan (sprintf "fieldDecl %s" c) c
        let localDecl loc =     
            let c = locationToClassName loc
            intoSpan (sprintf "localDecl %s" c) c


        let formatter ele = 
            match ele with
            | Unformatted s -> WebUtility.HtmlEncode s
            | Comment s -> comment s
            | Keyword s -> keyword s
            | NewLine -> "<br/>"
            | Identifier s -> WebUtility.HtmlEncode s
            | TypeDeclaration (s, loc) -> ident s
            | StringLiteral s -> stringLiteral s
            | NumericLiteral s -> numericLiteral s
            | BeginRegion s -> region (s + Environment.NewLine)
            | EndRegion s -> region (s + Environment.NewLine)
            | LocalVariableDeclaration (s, loc) -> localDecl loc s
            | LocalVariableReference (s, loc) -> localRef loc s
            | FieldDeclaration (s, loc) -> fieldDecl loc s
            | FieldReference (s, loc) -> fieldRef loc s
            | ParameterDeclaration (s, loc) -> paramDecl loc s
            | ParameterReference (s, loc) -> paramRef loc s
            | PropertyDeclaration (s, loc) -> propDecl loc s
            | PropertyReference (s, loc) -> propRef loc s
            //| _ -> failwith "DONT KNOW HOW TO FORMAT"
        String.Join(String.Empty, Array.map formatter eles)
    
