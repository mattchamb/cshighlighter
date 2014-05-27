namespace Highlighter.Web

module Solutions =
    
    open System
    open System.IO
    open HighlighterLib.SolutionProcessing
    open HighlighterLib.Formatting
    open HighlighterLib.Templating
    open Microsoft.WindowsAzure.Storage.Blob

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

    let buildSolutionStructure (rootDir: DirectoryInfo) (sol: ProcessedSolution) : SolutionFolder =
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
                        yield new SolutionFile(relativeToPath rootDir f, f.Name, procFile.Content) 
                    | None -> 
                        ignore() ]
            let noFilesFound = List.isEmpty files
            let noFilesInSubdirs = 
                subDirs
                |> List.forall (fun (dir: SolutionFolder) -> Seq.isEmpty dir.SolutionFiles && Seq.isEmpty dir.SubFolders)
            if noFilesFound && noFilesInSubdirs then
                None
            else 
                let folder = new SolutionFolder(currentDir.Name, isProjectFolder currentDir sol.Projects, subDirs, files)
                Some folder
        match traverseDirectories rootDir with
        | Some folder -> folder
        | None -> failwith <| sprintf "Tried to build the solution structure for solution %s in directory %s; but no processed files were found." sol.Path rootDir.FullName

    type RenderedContent =
        {
            RelativePath: string;
            Content: string
        }

    let getAllFiles (root: SolutionFolder) =
        let rec inner (currentFolder: SolutionFolder) =
            seq {
                yield! currentFolder.SubFolders |> Seq.collect inner
                yield! currentFolder.SolutionFiles
            }
        inner root

    let renderSolutionToBlobStorage (container: CloudBlobContainer) (solutionId: string) (solution: ProcessedSolution) =
        let solutionStructure = buildSolutionStructure (Directory.GetParent(solution.Path)) solution
        
        let allFiles = getAllFiles solutionStructure
        let allContent = 
            let renderedFiles = 
                allFiles
                |> Seq.map (fun file ->
                        let html = file.Contents.Html
                        let contentUris = WebContent.getOrUploadContent container solutionId file.Contents.Javascript file.Contents.Stylesheet
                        let htmlPath = sprintf "%s.html" file.RelativePath
                        let htmlUri = new Uri(sprintf "%s/%s/%s" (container.Uri.ToString()) solutionId htmlPath)
                        let ghf = htmlUri.MakeRelativeUri contentUris.Style
                        let content = Render.SinglePage(file.Contents.Html, [htmlUri.MakeRelativeUri contentUris.Style], [htmlUri.MakeRelativeUri contentUris.Script])
                        {RelativePath = htmlPath; Content = content})
                        
            let directoryPage = Render.Directory solutionStructure
            seq {
                yield! renderedFiles
                yield {RelativePath = "Directory.html"; Content = directoryPage}
            }
        allContent
        |> Seq.iter (fun item -> 
            let path = solutionId + "/" + item.RelativePath;
            Storage.storeBlob container path (Storage.Html item.Content) |> ignore)