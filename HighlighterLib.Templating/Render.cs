using HighlighterLib.Templating.Models;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating
{
    public class Render
    {
        private const string CssPath = "./Content/Style.css";
        private const string JsPath = "./Scripts/HightlightingScript.js";

        public static string SinglePage(string htmlContent)
        {
            var m = new Models.SingleFileModel()
            {
                Stylesheets = new string[] { File.ReadAllText(CssPath) },
                PreformattedHtml = htmlContent,
                Javascript = File.ReadAllText(JsPath)
            };
            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(HtmlTemplateBase<>)
            };

            using (var service = new TemplateService(config))
            {
                var t = File.ReadAllText("./Views/SingleUpload/FormattedSingleFile.cshtml");
                Razor.SetTemplateService(service);
                return Razor.Parse(t, m);
            }
        }
    }
}