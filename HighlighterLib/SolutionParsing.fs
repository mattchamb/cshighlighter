namespace CSHighlighter

module SolutionParsing =

    open System
    open System.Collections.Generic
    open System.IO
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text

    type ProcessedFile = {
        Path: string;
        Content: string
    }

    type ProcessedProject = {
        Path: string;
        Files: ProcessedFile array
    }

    type ProcessedSolution = {
        Path: string;
        Projects: ProcessedProject array
    }

    let openSolution solutionPath = 
        let workspace = MSBuild.MSBuildWorkspace.Create()
        workspace.OpenSolutionAsync(solutionPath) 
        |> Async.AwaitTask

    let processDocument (doc: Document) =
        async {
            let! model = doc.GetSemanticModelAsync() |> Async.AwaitTask
            let! root = doc.GetSyntaxRootAsync() |> Async.AwaitTask
            let highlightingModel = Analysis.createHighlightingModel root model
            let html = Formatting.htmlFormat highlightingModel
            return {
                Path = doc.FilePath;
                Content = html
            }
        }

    let processProject (proj: Project) =
        async {
            let! formattedFiles = 
                proj.Documents
                |> Seq.map processDocument
                |> Async.Parallel
            return {
                Path = proj.FilePath;
                Files = formattedFiles
            }
        }

    let processSolution (sol: Solution) =
        async {
            let! dependencyGraph = sol.GetProjectDependencyGraphAsync() |> Async.AwaitTask
            let! projects = 
                dependencyGraph.GetTopologicallySortedProjects() 
                |> Seq.map (fun projId -> sol.GetProject projId)
                |> Seq.map processProject
                |> Async.Parallel
            return {
                Path = sol.FilePath;
                Projects = projects
            }
        }
    
    let analyseSolution solutionPath =
        async {
            let! rawSoln = openSolution solutionPath
            return! processSolution rawSoln
        } |> Async.RunSynchronously