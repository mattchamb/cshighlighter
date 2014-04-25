using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestWebsite.Models;

namespace TestWebsite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
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
    } 
}";
            var v = Analysis.analyseCode(code);
            
            var s = Formatting.htmlFormat(v);
            return View(new CodeModel
                {
                    Code = s
                });
        }
                
    }
}