using HighlighterLib.Templating.Models;
using HighlighterLib;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace HighlighterLib.Templating
{
    public static class Render
    {
        private const string CssResourceName = "Style";
        private const string JsResourceName = "HightlightingScript";

        static Render()
        {
            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(HtmlTemplateBase<>)
            };

            var service = new TemplateService(config);
            Razor.SetTemplateService(service);
            Razor.Compile(Resources.FormattedSingleFile(), "singleFile");
            Razor.Compile(Resources.Directory(), "directory");
        }

        public static string SinglePage(Formatting.FormattedHtml htmlContent)
        {
            var m = new Models.SingleFileModel()
            {
                Stylesheets = new[] { htmlContent.Stylesheet },
                PreformattedHtml = htmlContent.Html,
                Javascript = new[] { htmlContent.Javascript }
            };
            return RenderSingleFile(m);
        }

        public static string SinglePage(string htmlContent, IEnumerable<Uri> cssPaths, IEnumerable<Uri> jsPaths)
        {
            var m = new Models.SingleFileModel()
            {
                StylesheetUris = cssPaths,
                PreformattedHtml = htmlContent,
                JavascriptUris = jsPaths
            };
            return RenderSingleFile(m);
        }

        public static string Directory(SolutionFolder solution)
        {
            return Razor.Run("directory", solution);
        }

        private static string RenderSingleFile(SingleFileModel m)
        {
            return Razor.Run("singleFile", m);
        }
        
    }
}