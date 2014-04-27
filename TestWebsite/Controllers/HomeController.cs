using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestWebsite.Models;
using CSHighlighter;

namespace TestWebsite.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(string code = null)
        {
            if(code == null)
                return View();
            try
            {
                return View(RenderCodeToModel(code));
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
                return View(new CodeModel()
                    {
                        ExceptionMessage = ex.ToString()
                    });
            }
        }

        [HttpGet]
        public ActionResult Example()
        {
            var code = @"
using System;

namespace TopLevel
{ 
    class Foo 
    {
        // Something something comment
        private int someField = 0;
        private string someStringField = ""asdf"";
        public Foo(int x)
        {
            var a = 10;
            a = 3456;
            this.someField = x;
        }

        /* multiline on a single line */
        /*****
        *
        * Multiline on multi lines.
        *
        ******/
        public bool Test()
        {
            #region SomeRegion
            return someField == 0 && Test();
            #endregion
        }
        private class SubClass<TSomething>
        {
            public TSomething Value { get; set; }
            public SubClass(TSomething ts)
            {
                Value = ts;
            }
        }
    }
}";
            return View(RenderCodeToModel(code));
        }

        private CodeModel RenderCodeToModel(string code)
        {
            var source = new Analysis.SourceInput("", code);
            var v = Analysis.analyseFile(source);

            var s = Formatting.htmlFormat(v);
            //var hoverCss = Formatting.generateCss(v);
            return new CodeModel
            {
                CodeElements = s,
                HoverCssClasses = ""
            };
        }
                
    }
}