namespace CSHighlighter

module SolutionParsing =

    open System
    open System.Collections.Generic
    open System.IO
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Text
    open Formatting
    open HighlighterLib.Shared
    
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
        async {
            let workspace = MSBuild.MSBuildWorkspace.Create()
            let! solution = workspace.OpenSolutionAsync(solutionPath) |> Async.AwaitTask
            return workspace
        }

    let processDocument (doc: Document) =
        async {
            let! model = doc.GetSemanticModelAsync() |> Async.AwaitTask
            let! root = doc.GetSyntaxRootAsync() |> Async.AwaitTask
            let highlightingModel = Analysis.createHighlightingModel root model
            let env = FormattingEnvironment.Project(doc.Project.Solution, doc.Id)
            let html = Formatting.htmlFormat env highlightingModel
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

    let isProjectFolder (dir: DirectoryInfo) (projs: ProcessedProject array) = 
        dir.EnumerateFiles("*.csproj")
        |> Seq.exists(fun f ->
            projs 
            |> Seq.exists(fun p -> p.Path = f.FullName))

    let relativeToPath (dir: DirectoryInfo) (file: FileInfo) =
        let dirUri = new Uri (dir.FullName)
        let fileUri = new Uri (file.FullName)
        let relativeUri = dirUri.MakeRelativeUri fileUri
        relativeUri.ToString()

    let buildSolutionStructure (rootDir: DirectoryInfo) (sol: ProcessedSolution) =
        let allFiles = 
            sol.Projects
            |> Seq.collect (fun proj -> proj.Files)
        let rec traverseDirectories (currentDir: DirectoryInfo) = 
            let subDirs = 
                currentDir.EnumerateDirectories()
                |> Seq.choose traverseDirectories
                |> Seq.toList

            let files: SolutionFile list =
                [ for f in currentDir.EnumerateFiles() do
                    let foundFile = 
                        allFiles
                        |> Seq.tryFind (fun file -> file.Path = f.FullName)
                    match foundFile with
                    | Some procFile -> 
                        yield { 
                            relativePath = relativeToPath rootDir f;
                            fileName = f.Name;
                            contents = procFile.Content 
                        }
                    | None -> 
                        ignore() ]
            let noFilesFound = List.isEmpty files
            let noFilesInSubdirs = 
                subDirs
                |> List.forall (fun dir -> Seq.isEmpty dir.solutionFiles && Seq.isEmpty dir.subFolders)
            if noFilesFound && noFilesInSubdirs then
                None
            else 
                let folder = {
                    folerName = currentDir.Name;
                    isProject = isProjectFolder currentDir sol.Projects;
                    subFolders = subDirs;
                    solutionFiles = files
                }
                Some folder
        match traverseDirectories rootDir with
        | Some folder -> folder
        | None -> failwith <| sprintf "Tried to build the solution structure for solution %s in directory %s; but no processed files were found." sol.Path rootDir.FullName

    
    let analyseSolution solutionPath =
        async {
            use! workspace = openSolution solutionPath
            let solutionDir = Directory.GetParent solutionPath
            let! processed = processSolution workspace.CurrentSolution
            let structure = buildSolutionStructure solutionDir processed
            return structure
        } |> Async.RunSynchronously