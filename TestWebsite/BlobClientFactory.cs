using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TestWebsite
{
    public class BlobClientFactory
    {
        public CloudBlobClient CreateBlobClient()
        {
            var conn = ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(conn);
            return storageAccount.CreateCloudBlobClient();
        }
    }
}