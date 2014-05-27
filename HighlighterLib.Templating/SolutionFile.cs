using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating
{
    public class SolutionFile
    {
        public string RelativePath { get; private set; }
        public string FileName { get; private set; }
        public Formatting.FormattedHtml Contents { get; private set; }

        public SolutionFile(string relativePath, string fileName, Formatting.FormattedHtml contents)
        {
            RelativePath = relativePath;
            FileName = fileName;
            Contents = contents;
        }
    }
}