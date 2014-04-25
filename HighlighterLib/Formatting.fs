module Formatting

open System
open Analysis
open Microsoft.CodeAnalysis.Text

let plainFormat (eles: OutputElement array) =
    let formatter ele = 
        match ele with
        | Unformatted s -> s
        | Comment s -> "comment(" + s + ")"
        | Keyword s -> "keyword(" + s + ")"
        | NewLine -> Environment.NewLine
        | Identifier s -> sprintf "identifier(%s)" s
        | StringLiteral s -> sprintf "string(%s)" s
        | NumericLiteral s -> sprintf "numeric(%s)" s
        | BeginRegion s -> sprintf "beginregion(%s)" s
        | EndRegion s -> sprintf "endregion(%s)" s
        | TypeDeclaration (s, _) -> sprintf "typeidentifier(%s)" s
        | _ -> failwith "DONT KNOW HOW TO FORMAT"
    let combine (state: string) (element: OutputElement) =
        let formatted = formatter element
        state + formatted
    let resultText = eles |> Array.fold combine ""
    resultText
    

let htmlFormat (eles: OutputElement array) =

    let intoSpan spanClass text = 
        sprintf @"<span class=""%s"">%s</span>" spanClass text

    let locationToClassName (loc:TextSpan) =    
        sprintf "loc%dx%d" loc.Start loc.End

    let comment = intoSpan "comment"
    let keyword = intoSpan "keyword"
    let ident = intoSpan "identifier"
    let stringLiteral = intoSpan "stringLiteral"
    let numericLiteral = intoSpan "numericLiteral"
    let region = intoSpan "region"
    let localDecl loc = intoSpan <| sprintf "localDecl %s" (locationToClassName loc)
    let localRef loc = intoSpan <| sprintf "localRef %s" (locationToClassName loc)
    let fieldDecl loc = intoSpan <| sprintf "fieldDecl %s" (locationToClassName loc)
    let fieldRef loc = intoSpan <| sprintf "fieldRef %s" (locationToClassName loc)
    let paramDecl loc = intoSpan <| sprintf "paramDecl %s" (locationToClassName loc)
    let paramRef loc = intoSpan <| sprintf "paramRef %s" (locationToClassName loc)

    

    let formatter ele = 
        match ele with
        | Unformatted s -> s
        | Comment s -> comment s
        | Keyword s -> keyword s
        | NewLine -> "<br/>"
        | Identifier s -> s
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
        //| _ -> failwith "DONT KNOW HOW TO FORMAT"

    let locationClassName ele = 
        match ele with
        | TypeDeclaration (_, loc) -> Some <| locationToClassName loc
        | LocalVariableDeclaration (_, loc) -> Some <| locationToClassName loc
        | FieldDeclaration (_, loc) -> Some <| locationToClassName loc
        | ParameterDeclaration (_, loc) -> Some <| locationToClassName loc
        | _ -> None

    let hoverClasses =
        seq {
            yield "<style>"
            for className in Seq.choose locationClassName eles do
                yield sprintf "#formattedCode .%s:hover { background-color: aqua; }%s" className Environment.NewLine
            yield "</style>"
        }
    let o =
        seq {
            yield! hoverClasses
            yield @"<div id=""formattedCode"">"
            yield "<pre>"
            for e in eles do
                yield formatter e
            yield "</pre>"
            yield @"</div>"
        }
    String.Join(String.Empty, o)
    
