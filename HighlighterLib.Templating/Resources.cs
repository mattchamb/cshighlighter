using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;

namespace HighlighterLib.Templating
{
    public static class Resources
    {
        /*
            "HighlighterLib.Templating.Views.View.cshtml"
            "HighlighterLib.Templating.Views.Directory.cshtml"
            "HighlighterLib.Templating.Views.SourcePageTemplate.cshtml"
            "HighlighterLib.Templating.Views.SingleUpload.FormattedSingleFile.cshtml"
        */
        public static string FormattedSingleFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var reader = new StreamReader(assembly.GetManifestResourceStream("HighlighterLib.Templating.Views.SingleUpload.FormattedSingleFile.cshtml")))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Directory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var reader = new StreamReader(assembly.GetManifestResourceStream("HighlighterLib.Templating.Views.Directory.cshtml")))
            {
                return reader.ReadToEnd();
            }
        }
    }
}