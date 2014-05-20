namespace CSHighlighter

module Highlighting =
    
    open System
    open System.Collections.Generic
    open System.IO
    open HighlighterLib.Shared
    open HighlighterLib.Templating

    let renderStandalone code = 
        let analysis = Analysis.analyseFile code
        let preformattedOutput = Formatting.htmlFormat Formatting.Standalone analysis.ClassifiedTokens
        Render.SinglePage(preformattedOutput)

    let renderStandaloneSharedContent code cssLoc jsLoc =
        let analysis = Analysis.analyseFile code
        let preformattedOutput = Formatting.htmlFormat Formatting.Standalone analysis.ClassifiedTokens
        Render.SinglePage(preformattedOutput, cssLoc, jsLoc)

    type RenderedContent =
        {
            RelativePath: string;
            Content: string
        }

    let getAllFiles (root: SolutionFolder) =
        let rec inner (currentFolder: SolutionFolder) =
            seq {
                yield! currentFolder.subFolders |> Seq.collect inner
                yield! currentFolder.solutionFiles
            }
        inner root

    let renderSolution (solution: SolutionFolder) (css: Uri) (js: Uri) =
        let allFiles = getAllFiles solution
        let renderedContent = 
            allFiles
            |> Seq.map (fun file ->
                    let content = Render.SinglePage(file.contents, [css], [js])
                    let htmlPath = sprintf "%s.html" file.relativePath
                    {RelativePath = htmlPath; Content = content})
                        
        let dir = Render.Directory solution
        seq {
            yield! renderedContent
            yield {RelativePath = "Directory.html"; Content = dir}
        }

    