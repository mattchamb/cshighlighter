namespace HighlighterLib

module JsonTransform =

    open System
    open System.Collections.Generic
    open Analysis
    open Types
    open Microsoft.CodeAnalysis
    open Newtonsoft.Json
    open Newtonsoft.Json.Converters
    open SolutionProcessing

    [<AutoOpen>]
    module Types =
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

        type CodeSpan = 
            {kind: TokenKind;
            text: string;
            symbolId: int option}

        type CodeLocation = 
            {sourceId: int option;
            line: int option;
            col: int option;
            assembly: string option}

        type JsonSymbolInfo = 
            {symbolId: int;
            displayText: string;
            locations: CodeLocation seq}

        type JsonTokenisedFile = 
            {sourceId: int;
            path: string;
            codeTokens: CodeSpan seq}

        type JsonProj = 
            {path: string;
            sourceIds: int seq}

        type JsonSol = 
            {path: string;
            projects: JsonProj seq}

        let Optional<'a> = { new JsonConverter() with
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
    
    let tokenToSerializableFormat (symbolId: int option) (token: TokenClassification) : CodeSpan =

        let codeSpan kind token =
            { kind = kind; text = token.ToString(); symbolId = symbolId; }

        match token with
        | Unformatted tok -> codeSpan TokenKind.Unformatted tok
        | Keyword tok -> codeSpan TokenKind.Keyword tok
        | Identifier tok -> codeSpan TokenKind.Identifier tok
        | NamespaceReference (tok, _) -> codeSpan TokenKind.Identifier tok
        | NamedTypeDeclaration (tok, _) -> codeSpan TokenKind.TypeDecl tok
        | NamedTypeReference (tok, sym) -> 
            match tok.ToString() with
            | "var" -> codeSpan TokenKind.Keyword tok
            | _ -> codeSpan TokenKind.TypeRef tok
        | StringLiteral tok -> codeSpan TokenKind.StringLiteral tok
        | NumericLiteral tok -> codeSpan TokenKind.NumericLiteral tok
        | CharacterLiteral tok -> codeSpan TokenKind.CharLiteral tok
        | LocalVariableDeclaration (tok, _) -> codeSpan TokenKind.LocalDecl tok
        | LocalVariableReference (tok, sym) -> codeSpan TokenKind.LocalRef tok
        | FieldDeclaration (tok, _) -> codeSpan TokenKind.FieldDecl tok
        | FieldReference (tok, sym) -> codeSpan TokenKind.FieldRef tok
        | ParameterDeclaration (tok, _) -> codeSpan TokenKind.ParamDecl tok
        | ParameterReference (tok, sym) -> codeSpan TokenKind.ParamRef tok
        | PropertyDeclaration (tok, _) -> codeSpan TokenKind.PropDecl tok
        | PropertyReference (tok, sym) -> codeSpan TokenKind.PropRef tok
        | MethodDeclaration (tok, _) -> codeSpan TokenKind.MethodDecl tok
        | MethodReference (tok, sym) -> codeSpan TokenKind.MethodRef tok
        | EnumMemberDeclaration (tok, _) -> codeSpan TokenKind.EnumMemberDecl tok
        | EnumMemberReference (tok, sym) -> codeSpan TokenKind.EnumMemberRef tok
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

    let toJson : (obj -> string) =
        let settings = new JsonSerializerSettings()
        settings.Converters <- [| new StringEnumConverter(); Optional<int>; Optional<String>; Optional<CodeLocation seq> |]
        settings.Formatting <- Formatting.Indented
        settings.NullValueHandling <- NullValueHandling.Ignore
        let serialize obj = JsonConvert.SerializeObject (obj, settings)
        serialize

    let transformFile (getSourceId: string -> int) (symbolIndexes: IDictionary<ISymbol, int>) (input: FileAnalysisResult) =
        let codeTokens = 
            input.ClassifiedTokens 
            |> Seq.map 
                (fun tok ->
                    let symbolInfoId = 
                        match getSymbol tok with
                        | None -> None
                        | Some sym -> 
                            Some symbolIndexes.[sym]
                    tokenToSerializableFormat symbolInfoId tok)
            |> Seq.toList

        /// Concatenates the text elements of a CodeSpan list in reverse order.
        let combineSpans: (CodeSpan list -> String) = 
            Seq.fold (fun s t -> sprintf "%s%s" t.text s) String.Empty

        let collapseSpans spansToCollapse = 
            match spansToCollapse with
            | head :: _ ->
                let collapsedText = combineSpans spansToCollapse 
                Some { head with text = collapsedText }
            | [] -> None

        let reducedCodeSpans = 
            // Combines sequential TokenKind.Unformatted elements into a single element.
            Seq.unfold 
                (fun (spansToCollapse, rest) -> 
                    // spansToCollapse = the current repeated sequence of elements
                    // rest = the remaining un-collapsed token sequence
                    match rest with
                    | head :: tail ->
                        match head.kind with
                        | TokenKind.Unformatted -> 
                            Some (List.empty, (head :: spansToCollapse, tail)) // yield List.Empty because the trivia element is delayed to possibly collapse with the next element
                        | _ -> 
                            Some ([collapseSpans spansToCollapse; Some head], (List.empty, tail))
                    | [] -> 
                        match spansToCollapse with
                        | [] -> None
                        | _ -> 
                            Some ([collapseSpans spansToCollapse], (List.empty, List.empty))
                ) ([], codeTokens)
            |> Seq.collect (List.choose id)

        {   
            sourceId = getSourceId (input.FilePath);
            path = input.FilePath;
            codeTokens = reducedCodeSpans
        } 

    /// Gets a dictionary that lets us lookup and index/id from the element.
    /// This is useful when we want to get a key to lookup an element.
    let toIndexesDict (elements: _ seq) = 
        let dict = new Dictionary<_,_>()
        elements
        |> Seq.iteri (fun i e -> if not (dict.ContainsKey e) then dict.Add(e, i))
        dict

    /// Convert ISymbols into a JSON-able format
    let indexesToSymInfo (getSourceId: string -> int option) (idxs: Dictionary<ISymbol, int>) =
        let mapLocation (loc: Location) =
            match loc.Kind with
            | LocationKind.SourceFile ->
                let span = loc.GetLineSpan()
                let sourceId = getSourceId loc.FilePath
                match sourceId with
                | Some _ -> 
                    Some {
                        sourceId = sourceId
                        line = Some span.StartLinePosition.Line
                        col = Some span.StartLinePosition.Character
                        assembly = None
                    }
                | None -> None
            | LocationKind.MetadataFile ->
                Some {
                    sourceId = None
                    line = None
                    col = None
                    assembly = Some (loc.MetadataModule.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
                }
            | _ -> None

        let toSymbolInfo id (sym: ISymbol) =
            let text = sprintf "(%A) %s" sym.Kind (sym.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            let locations = 
                sym.Locations
                |> Seq.choose mapLocation
            {symbolId = id; displayText = text; locations = locations}
        idxs
        |> Seq.map 
            (fun kvp ->
                let id = kvp.Value
                let sym = kvp.Key
                toSymbolInfo id sym)

    let jsonFormatFile (input: FileAnalysisResult) =
        let symbolIndexes = 
            input.ClassifiedTokens
            |> Seq.choose getSymbol
            |> toIndexesDict
        let a = transformFile (fun f -> 0) symbolIndexes input
        toJson (a, indexesToSymInfo (fun f -> Some 0) symbolIndexes)

    let getAllInSolution f (solution: ProcessedSolution) = 
        seq {
            for proj in solution.Projects do
                for file in proj.Files do
                    yield f file
        }

    let collectReferencedSymbols solution =
        getAllInSolution (fun f -> f.ClassifiedTokens) solution
        |> Seq.collect id
        |> Seq.choose getSymbol
        |> Seq.distinct

    let collectAllFilePaths solution =
        getAllInSolution (fun f -> f.FilePath) solution
        |> Seq.distinct

    let collectAllFiles solution =
        getAllInSolution id solution

    let solutionToJson (solution: ProcessedSolution) : string * string * string =
        
        let referencedSymbols = collectReferencedSymbols solution
        let symIndexes = toIndexesDict referencedSymbols

        let allFiles = collectAllFilePaths solution
        let fileIndexes = toIndexesDict allFiles

        /// Try to get the Id for the given source path.
        /// There is a case where we will lookup paths that arent in the solution, and it is safer not to give away this info.
        let getSourceId path = 
            if fileIndexes.ContainsKey path then Some fileIndexes.[path]
            else None

        let symInfo = indexesToSymInfo getSourceId symIndexes
        
        let mapProj (proj: ProcessedProject) : JsonProj = {
                path = proj.Path
                sourceIds = proj.Files |> Seq.map (fun f -> fileIndexes.[f.FilePath])
            }

        let soln = {
                path = solution.Path
                projects = solution.Projects |> Seq.map mapProj
            }

        let sourceFiles = 
            collectAllFiles solution
            |> Seq.map (transformFile (fun f -> fileIndexes.[f]) symIndexes)

        (toJson soln), (toJson symInfo), (toJson sourceFiles)