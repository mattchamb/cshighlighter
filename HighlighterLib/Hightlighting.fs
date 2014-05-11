namespace CSHighlighter

module Hightlighting =
    
    open HighlighterLib.Templating

    let renderStandalone code = 
        let analysis = Analysis.analyseFile code
        let preformattedOutput = Formatting.htmlFormat Formatting.Standalone analysis.ClassifiedTokens
        Render.SinglePage(preformattedOutput)

    let renderStandaloneSharedContent code cssLoc jsLoc =
        let analysis = Analysis.analyseFile code
        let preformattedOutput = Formatting.htmlFormat Formatting.Standalone analysis.ClassifiedTokens
        Render.SinglePage(preformattedOutput, cssLoc, jsLoc)
    