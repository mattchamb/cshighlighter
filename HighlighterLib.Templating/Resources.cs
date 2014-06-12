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
            "Highlighter.Web.Templating.Views.View.cshtml"
            "Highlighter.Web.Templating.Views.Directory.cshtml"
            "Highlighter.Web.Templating.Views.SourcePageTemplate.cshtml"
            "Highlighter.Web.Templating.Views.SingleUpload.FormattedSingleFile.cshtml"
        */
        public static string FormattedSingleFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Highlighter.Web.Templating.Views.SingleUpload.FormattedSingleFile.cshtml"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Directory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Highlighter.Web.Templating.Views.Directory.cshtml"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}