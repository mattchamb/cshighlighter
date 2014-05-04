using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating.Models
{
    public class SingleFileModel
    {
        public IEnumerable<string> Stylesheets { get; set; }
        public IEnumerable<Uri> StylesheetUris { get; set; }

        public string PreformattedHtml { get; set; }

        public IEnumerable<string> Javascript { get; set; }
        public IEnumerable<Uri> JavascriptUris { get; set; }

        public SingleFileModel()
        {
            Stylesheets = Enumerable.Empty<string>();
            Javascript = Enumerable.Empty<string>();
            StylesheetUris = Enumerable.Empty<Uri>();
            JavascriptUris = Enumerable.Empty<Uri>();
            PreformattedHtml = string.Empty;
        }
    }
}