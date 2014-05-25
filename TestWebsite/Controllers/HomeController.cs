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
using System.Net;
using System.IO;

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

        [HttpGet]
        public ActionResult SubmitZipUrl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitZipUrl(ZipLocationModel model)
        {

            var fileName = Guid.NewGuid().ToString("N");
            var downloadTo = Path.Combine(Server.MapPath("~/App_Data/"), fileName + ".zip");
            var zipDir = Path.Combine(Server.MapPath("~/App_Data/"), fileName);

            try
            {
                var contentContainer = Storage.getContainer(_blobClient, BlobContainers.ContentContainer);
                var uris = CSHighlighter.Content.getOrUploadLatestContent(contentContainer);
                
                if (!Directory.Exists(Server.MapPath("~/App_Data/")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/"));
                }

                new WebClient().DownloadFile(model.ZipUrl, downloadTo);
                using (var zip = Ionic.Zip.ZipFile.Read(downloadTo))
                {
                    zip.ExtractAll(zipDir);
                }
                var solution = Directory.EnumerateFiles(zipDir, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
                var resultSoln = SolutionParsing.analyseSolution(solution);
                var renderedContent = Highlighting.renderSolution(resultSoln, uris.Style, uris.Script);

                var projContainer = Storage.getContainer(_blobClient, BlobContainers.ProjectsContainer);

                foreach (var item in renderedContent)
                {
                    var path = fileName + "/" + item.RelativePath;
                    Storage.storeBlob(projContainer, path, Storage.BlobContents.NewHtml(item.Content));
                }
                model.Directory = projContainer.Uri.AbsoluteUri + "/" + fileName + "/" + "Directory.html";
                model.ProjectId = fileName;
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(downloadTo))
                    {
                        System.IO.File.Delete(downloadTo);
                    }
                    if (Directory.Exists(zipDir))
                    {
                        Directory.Delete(zipDir, true);
                    }
                } catch (Exception ex)
                {
                    model.Exception = ex.ToString();
                }
            }
            
            return View(model);
        }

        private Uri FormatAndUploadStandalone(string code)
        {
            var standaloneHighlighting = Highlighting.renderStandalone(code);
            
            var container = Storage.getContainer(_blobClient, BlobContainers.StandaloneContainer);
            var fileName = Guid.NewGuid().ToString("N") + ".html";
            var loc = Storage.storeBlob(container, fileName, Storage.BlobContents.NewHtml(standaloneHighlighting));
            return loc;
        }

        [HttpGet]
        public ActionResult ViewProject(string projectId)
        {
            if(string.IsNullOrWhiteSpace(projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View(new BrowseProjectModel()
                {
                    DirectoryUrl = string.Format("{0}/projects/{1}/Directory.html", _blobClient.BaseUri.ToString(), projectId),
                    SourceUrl = ""
                });
        }
                
    }
}