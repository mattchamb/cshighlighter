using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating.Models
{
    public class SingleFileModel
    {
        public IEnumerable<string> Stylesheets { get; set; }

        public string PreformattedHtml { get; set; }

        public string Javascript { get; set; }
    }
}