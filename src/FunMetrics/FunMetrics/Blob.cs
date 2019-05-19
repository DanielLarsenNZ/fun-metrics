using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Net.Http;

namespace FunMetrics
{
    public static class Blob
    {
        private static readonly Lazy<CloudBlobClient> _lazyClient = new Lazy<CloudBlobClient>(InitializeCloudBlobClient);
        
        private static CloudBlobClient CloudBlobClient => _lazyClient.Value;

        [FunctionName("blob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Run Blob: {DateTime.UtcNow} XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            var uri = new Uri("https://azurelunchaue.blob.core.windows.net/podcasts/azure-lunch-s1e03.mp3");
            var reference = await CloudBlobClient.GetBlobReferenceFromServerAsync(uri);

            var stream = new MemoryStream();
            await reference.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamContent(stream);

            return new FileStreamResult(stream, reference.Properties.ContentType);

            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static CloudBlobClient InitializeCloudBlobClient()
        {
            //if (_config["StorageConnectionString"] == null)
            //    throw new InvalidOperationException("App Setting \"StorageConnectionString\" is not set.");

            CloudStorageAccount account = 
                CloudStorageAccount.Parse(
                    "DefaultEndpointsProtocol=https;AccountName=azurelunchaue;AccountKey=;EndpointSuffix=core.windows.net");
            return account.CreateCloudBlobClient();
        }

    }
}
