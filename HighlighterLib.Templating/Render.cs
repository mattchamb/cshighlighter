using HighlighterLib.Templating.Models;
using HighlighterLib.Templating.Properties;
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

        public static string SinglePage(string htmlContent)
        {
            var m = new Models.SingleFileModel()
            {
                Stylesheets = Content.GetStyles(),
                PreformattedHtml = htmlContent,
                Javascript = Content.GetScripts()
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

        private static string RenderSingleFile(SingleFileModel m)
        {
            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(HtmlTemplateBase<>)
            };

            using (var service = new TemplateService(config))
            {
                var t = Resources.FormattedSingleFile;
                Razor.SetTemplateService(service);
                return Razor.Parse(t, m);
            }
        }

        
    }
}