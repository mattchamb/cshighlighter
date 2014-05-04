using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestWebsite.Models;
using CSHighlighter;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TestWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly CloudBlobClient _blobClient;
        public HomeController()
        {
            var conn = CloudConfigurationManager.GetSetting("StorageConnection");

            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

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
                var model = FormatAndUpload(code);
                return Redirect(model.AbsoluteUri);
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



        private Uri FormatAndUpload(string code)
        {
            var source = new Analysis.SourceInput("", code);
            var v = Analysis.analyseFile(source);

            var s = Formatting.htmlFormat(v);
            
            var container = Storage.getContainer(_blobClient, "standalone");
            var loc = Storage.storeBlob(container, Guid.NewGuid().ToString("N") + ".html", Storage.BlobContents.NewHtml(HighlighterLib.Templating.Render.SinglePage(s)));

            return loc;
        }
                
    }
}