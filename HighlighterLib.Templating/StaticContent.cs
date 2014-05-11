using HighlighterLib.Templating.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating
{
    public static class StaticContent
    {
        public static string[] GetScripts()
        {
            return new[] {
                Resources.HightlightingScript
            };
        }

        public static string[] GetStyles()
        {
            return new[] {
                Resources.Style
            };
        }

        public static string GetHighlightingScript()
        {
            return Resources.HightlightingScript;
        }

        public static string GetHighlightingStyle()
        {
            return Resources.Style;
        }
    }
}