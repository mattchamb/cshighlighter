namespace CSHighlighter

module Hightlighting =
    
    open System
    open System.Collections.Generic
    open System.IO
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

    let renderSolution (solution: SolutionParsing.ProcessedSolution) (css: Uri) (js: Uri) =
        let baseUri = new Uri(solution.Path)
        let baseDir = (Directory.GetParent solution.Path).FullName
        let renderedFiles = new List<string>()
        let renderedContent = 
            seq {
                for proj in solution.Projects do
                    for file in proj.Files do
                        if file.Path.StartsWith(baseDir) then
                            let fileUri = new Uri(file.Path + ".html")
                            let relativeUri = baseUri.MakeRelativeUri fileUri
                            let relativePath = relativeUri.ToString()
                            renderedFiles.Add(relativePath)
                            let content = Render.SinglePage(file.Content, [css], [js])
                            yield {RelativePath = relativePath; Content = content}
                let dir = Render.Directory renderedFiles
                yield {RelativePath = "Directory.html"; Content = dir}
            }
        renderedContent

    