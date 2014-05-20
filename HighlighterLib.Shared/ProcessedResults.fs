namespace HighlighterLib.Shared

// We need to share these types between HighligherLib and HighlighterLib.Templating

type SolutionFile = {
    relativePath: string;
    fileName: string;
    contents: string
}

type SolutionFolder = {
    folerName: string;
    isProject: bool;
    subFolders: SolutionFolder seq;
    solutionFiles: SolutionFile seq
}

    
