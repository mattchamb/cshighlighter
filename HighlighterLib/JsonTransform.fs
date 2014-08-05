namespace HighlighterLib

module JsonTransform =

    open System
    open System.Collections.Generic
    open System.Text
    open System.IO
    open Analysis
    open Types
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text
    open Newtonsoft.Json
    open Newtonsoft.Json.Converters
    open SolutionProcessing
    


    type TokenKind =
        | Unformatted = 1
        | Keyword = 2
        | Identifier = 3
        | TypeDecl = 4
        | TypeRef = 5
        | StringLiteral = 6
        | NumericLiteral = 7
        | CharLiteral = 8
        | LocalDecl = 9
        | LocalRef = 10
        | FieldDecl = 11
        | FieldRef = 12
        | ParamDecl = 13
        | ParamRef = 14
        | PropDecl = 15
        | PropRef = 16
        | MethodDecl = 17
        | MethodRef = 18
        | EnumMemberDecl = 19
        | EnumMemberRef = 20
        | SemanticError = 21
        | Trivia = 22
        | Comment = 23
        | Region = 24
        | DisabledText = 25

    type CodeLocation =
        {
            sourceFile: string
            line: int
        }

    type CodeSpan = 
        {
            kind: TokenKind
            text: string
            tipId: int option
            symbolDefs: CodeLocation seq option
        }
        
    let getSymbol t : ISymbol option =
        match t with
        | LocalVariableDeclaration(_, sym) -> Some (sym :> ISymbol)
        | LocalVariableReference(_, sym) -> Some (sym :> ISymbol)
        | FieldDeclaration(_, sym) -> Some (sym :> ISymbol)
        | FieldReference(_, sym) -> Some (sym :> ISymbol)
        | ParameterReference(_, sym) -> Some (sym :> ISymbol)
        | ParameterDeclaration(_, sym) -> Some (sym :> ISymbol)
        | PropertyReference(_, sym) -> Some (sym :> ISymbol)
        | PropertyDeclaration(_, sym) -> Some (sym :> ISymbol)
        | NamedTypeDeclaration(_, sym) -> Some (sym :> ISymbol)
        | NamedTypeReference(_, sym) -> Some (sym :> ISymbol)
        | MethodReference(_, sym) -> Some (sym :> ISymbol)
        | MethodDeclaration(_, sym) -> Some (sym :> ISymbol)
        | EnumMemberDeclaration(_, sym) -> Some (sym :> ISymbol)
        | EnumMemberReference(_, sym) -> Some (sym :> ISymbol)
        | NamespaceReference(_, sym) -> Some (sym :> ISymbol)
        | _ -> None
    
    let tokenToSerializableFormat (tipId: int option) (token: TokenClassification) : CodeSpan =

        let codeSpan kind token =
            { kind = kind; text = token.ToString(); tipId = tipId; symbolDefs = None }

        let codeSpanWithRefs kind token locations =
            let mapLocation (loc: Location) =
                match loc.Kind with
                | LocationKind.SourceFile ->
                    let span = loc.GetLineSpan()
                    Some {
                        sourceFile = loc.FilePath
                        line = span.StartLinePosition.Line
                    }
                | _ -> None
            let defLocations = Some (locations |> Seq.choose mapLocation)
            { kind = kind; text = token.ToString(); tipId = tipId; symbolDefs = defLocations }

        match token with
        | Unformatted tok -> codeSpan TokenKind.Unformatted tok
        | Keyword tok -> codeSpan TokenKind.Keyword tok
        | Identifier tok -> codeSpan TokenKind.Identifier tok
        | NamespaceReference (tok, _) -> codeSpan TokenKind.Identifier tok
        | NamedTypeDeclaration (tok, _) -> codeSpan TokenKind.TypeDecl tok
        | NamedTypeReference (tok, sym) -> 
            match tok.ToString() with
            | "var" -> codeSpanWithRefs TokenKind.Keyword tok sym.Locations
            | _ -> codeSpanWithRefs TokenKind.TypeRef tok sym.Locations
        | StringLiteral tok -> codeSpan TokenKind.StringLiteral tok
        | NumericLiteral tok -> codeSpan TokenKind.NumericLiteral tok
        | CharacterLiteral tok -> codeSpan TokenKind.CharLiteral tok
        | LocalVariableDeclaration (tok, _) -> codeSpan TokenKind.LocalDecl tok
        | LocalVariableReference (tok, sym) -> codeSpanWithRefs TokenKind.LocalRef tok sym.Locations
        | FieldDeclaration (tok, _) -> codeSpan TokenKind.FieldDecl tok
        | FieldReference (tok, sym) -> codeSpanWithRefs TokenKind.FieldRef tok sym.Locations
        | ParameterDeclaration (tok, _) -> codeSpan TokenKind.ParamDecl tok
        | ParameterReference (tok, sym) -> codeSpanWithRefs TokenKind.ParamRef tok sym.Locations
        | PropertyDeclaration (tok, _) -> codeSpan TokenKind.PropDecl tok
        | PropertyReference (tok, sym) -> codeSpanWithRefs TokenKind.PropRef tok sym.Locations
        | MethodDeclaration (tok, _) -> codeSpan TokenKind.MethodDecl tok
        | MethodReference (tok, sym) -> codeSpanWithRefs TokenKind.MethodRef tok sym.Locations
        | EnumMemberDeclaration (tok, _) -> codeSpan TokenKind.EnumMemberDecl tok
        | EnumMemberReference (tok, sym) -> codeSpanWithRefs TokenKind.EnumMemberRef tok sym.Locations
        | SemanticError (tok, errors) -> codeSpan TokenKind.SemanticError tok
        | Trivia tr -> 
            match tr with
            | TriviaElement.BeginRegion s -> codeSpan TokenKind.Region s
            | TriviaElement.EndRegion s -> codeSpan TokenKind.Region s
            | TriviaElement.Comment s -> codeSpan TokenKind.Comment s
            | TriviaElement.NewLine -> codeSpan TokenKind.Unformatted Environment.NewLine
            | TriviaElement.UnformattedTrivia s -> codeSpan TokenKind.Unformatted s
            | TriviaElement.Whitespace s -> codeSpan TokenKind.Unformatted s
            | TriviaElement.DisabledText s -> codeSpan TokenKind.DisabledText s
    
    let createToolTip (sym: ISymbol) =
        let locationText =
            let loc = Seq.head sym.Locations
            if loc.IsInMetadata then
                loc.MetadataModule.ToDisplayString()
            else
                Path.GetFileName loc.FilePath
        sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), locationText 

    let createTipDict (tokens: TokenClassification seq) =
        let d = new Dictionary<_,_>()
        tokens 
        |> Seq.choose getSymbol
        |> Seq.iter 
            (fun sym -> 
                if not <| d.ContainsKey sym then
                    d.Add (sym, createToolTip sym))
        d

    type ToolTip =
        {
            tipId: int
            text: string
            location: string
        }

    let T<'a> = { new JsonConverter() with
            override x.CanConvert t =
                t.Equals typeof<Option<'a>>
            override x.WriteJson (writer, obj, ser) =
                let opt = obj :?> Option<'a>
                match opt with
                | Some a -> ser.Serialize(writer, a)
                | None -> ser.Serialize(writer, null)
            override x.ReadJson (_, _, _, _) =
                failwith "Reading JSON not supported by this JsonConverter"
            override x.CanRead = false
            override x.CanWrite = true
        }

    let toJson : (obj -> string) =
        let settings = new JsonSerializerSettings()
        settings.Converters <- [| new StringEnumConverter(); T<int>; T<String>; T<CodeLocation seq> |]
        settings.Formatting <- Formatting.Indented
        settings.DefaultValueHandling <- DefaultValueHandling.Ignore
        settings.NullValueHandling <- NullValueHandling.Ignore
        let serialize obj = JsonConvert.SerializeObject (obj, settings)
        serialize

    type JsonFile =  {
        path: string
        toolTips: ToolTip seq
        typeDecls: DeclaredType seq
        codeTokens: CodeSpan seq
    }

    type JsonProj = {
        path: string
        files: JsonFile seq
    }

    type JsonSol = {
        path: string
        projects: JsonProj seq
    }

    let transformFile (input: FileAnalysisResult) =

        let tokens = input.ClassifiedTokens

        let tipDict = createTipDict tokens
        let symbolIndexes = 
            let dict = new Dictionary<_,_>()
            tipDict 
            |> Seq.iteri (fun i sym -> dict.Add(sym.Key, i))
            dict

        let serToolTips =
            tipDict
            |> Seq.map 
                (fun kvp ->
                    let id = symbolIndexes.[kvp.Key]
                    let text, location = kvp.Value
                    {tipId = id; text = text; location = location})
        
        let codeTokens = 
            tokens 
            |> Seq.map 
                (fun tok ->
                    let tipId = 
                        match getSymbol tok with
                        | None -> None
                        | Some sym -> Some symbolIndexes.[sym]
                    tokenToSerializableFormat tipId tok)
            |> Seq.toList

        let combineSpans: (CodeSpan list -> String) = 
            Seq.fold (fun s t -> sprintf "%s%s" s t.text) String.Empty

        let collapseSpans spansToCollapse = 
            match spansToCollapse with
            | head :: _ ->
                let collapsedText =
                    spansToCollapse 
                    |> combineSpans
                Some { head with text = collapsedText }
            | [] -> None

        let reducedCodeSpans = 
            Seq.unfold 
                (fun (spansToCollapse, rest) -> 
                    match rest with
                    | head :: tail ->
                        match head.kind with
                        | TokenKind.Unformatted -> 
                            Some (List.empty, (head :: spansToCollapse, tail)) // yield List.Empty because the trivia element is delayed to possibly collapse with the next element
                        | _ -> 
                            let spans = List.rev spansToCollapse
                            Some ([collapseSpans spans; Some head], (List.empty, tail))
                    | [] -> 
                        match spansToCollapse with
                        | [] -> None
                        | _ -> 
                            let spans = List.rev spansToCollapse
                            Some ([collapseSpans spans], (List.empty, List.empty))
                ) ([], codeTokens)
            |> Seq.collect (List.choose id)

        {
            path = input.FilePath
            toolTips = serToolTips
            typeDecls = input.DeclaredTypes
            codeTokens = reducedCodeSpans
        } 

    let jsonFormat (input: FileAnalysisResult) =
        toJson <| transformFile input

    let solutionToJson (solution: ProcessedSolution) : string =
        
        let mapProj (proj: ProcessedProject) : JsonProj =
            {
                path = proj.Path
                files = proj.Files |> Seq.map transformFile
            }
        let soln =
            {
                path = solution.Path
                projects = solution.Projects |> Seq.map mapProj
            } |> toJson
        soln