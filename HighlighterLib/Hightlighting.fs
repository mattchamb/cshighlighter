namespace CSHighlighter

module Hightlighting =
    
    open HighlighterLib.Templating

    let renderStandalone code = 
        let analysis = Analysis.analyseFile {Path = ""; Contents = code }
        let preformattedOutput = Formatting.htmlFormat analysis
        Render.SinglePage(preformattedOutput)

    let renderStandaloneSharedContent code cssLoc jsLoc =
        let analysis = Analysis.analyseFile {Path = ""; Contents = code }
        let preformattedOutput = Formatting.htmlFormat analysis
        Render.SinglePage(preformattedOutput, cssLoc, jsLoc)
    