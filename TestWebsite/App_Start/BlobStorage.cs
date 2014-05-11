using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CSHighlighter;

namespace TestWebsite
{
    public class BlobStorage
    {
        

        public static void CreateBlobContainers()
        {
            var client = new BlobClientFactory().CreateBlobClient();
            Storage.getContainer(client, BlobContainers.StandaloneContainer);
            Storage.getContainer(client, BlobContainers.ContentContainer);
            Storage.getContainer(client, BlobContainers.ProjectsContainer);
        }
    }
}