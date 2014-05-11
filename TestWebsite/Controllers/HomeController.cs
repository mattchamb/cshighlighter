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
using System.Configuration;

namespace TestWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly CloudBlobClient _blobClient;
        public HomeController()
        {
            var blobClientFactory = new BlobClientFactory();
            _blobClient = blobClientFactory.CreateBlobClient();
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
                var blobUri = FormatAndUploadStandalone(code);
                //return View(new CodeModel() { FrameUrl = blobUri.AbsoluteUri });
                return Redirect(blobUri.AbsoluteUri);
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

        private Uri FormatAndUploadStandalone(string code)
        {
            var standaloneHighlighting = Hightlighting.renderStandalone(code);
            
            var container = Storage.getContainer(_blobClient, BlobContainers.StandaloneContainer);
            var fileName = Guid.NewGuid().ToString("N") + ".html";
            var loc = Storage.storeBlob(container, fileName, Storage.BlobContents.NewHtml(standaloneHighlighting));
            return loc;
        }
                
    }
}