namespace HighlighterLib

module HtmlResources =

    open System.Reflection
    open System.IO

    let style = 
        use stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Style.css")
        use s = new StreamReader(stream)
        s.ReadToEnd()

    let script = 
        use stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HighlightingScript.js")
        use s = new StreamReader(stream)
        s.ReadToEnd()