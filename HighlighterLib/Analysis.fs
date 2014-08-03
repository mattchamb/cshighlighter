namespace HighlighterLib

module Analysis =
    open System
    open System.Collections.Generic
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text
    open Types
    open Visitors

    type FileAnalysisResult =
        {
            FilePath: string
            ClassifiedTokens: TokenClassification seq
            DeclaredTypes: DeclaredType seq
        }

    let getDeclaredTypes syntaxTreeRoot model =
        let v = new TypeDeclarationVisitor(model)
        v.DefaultVisit syntaxTreeRoot
        let declaredTypes = v.getOutput()
        declaredTypes

    let createHighlightingModel syntaxTreeRoot model =
        let v = new TokenVisitor(model)
        v.DefaultVisit syntaxTreeRoot
        let elements = v.getOutput()
        elements
      
    let compilationForSource trees =
        CSharpCompilation.Create("highlightingCompilation", trees, [|new MetadataFileReference(typeof<Object>.Assembly.Location); new MetadataFileReference(typeof<Uri>.Assembly.Location)|])

    let parseCode (code: string) path =
        CSharpSyntaxTree.ParseText (code, path)

    let analyseFile (code: string) path = 
        let pathStr =
            match path with
            | None -> "standalone_file.cs"
            | Some p -> p
        let syntaxTree = parseCode code pathStr
        let compilation = compilationForSource [|syntaxTree|]
        let root = syntaxTree.GetRoot()
        let model = compilation.GetSemanticModel(syntaxTree)
        
        let classifiedTokens = createHighlightingModel root model
        let declaredTypes = getDeclaredTypes root model

        { FilePath = pathStr; ClassifiedTokens = classifiedTokens; DeclaredTypes = declaredTypes }

