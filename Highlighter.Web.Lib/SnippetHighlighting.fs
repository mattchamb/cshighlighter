namespace Highlighter.Web

module SnippetHighlighting =
    
    open System
    open System.Collections.Generic
    open System.IO
    open HighlighterLib

//    let renderStandalone code = 
//        let analysis = Analysis.analyseFile code
//        let preformattedOutput = Formatting.htmlFormat Formatting.Standalone analysis.ClassifiedTokens
//        Render.SinglePage(preformattedOutput)
