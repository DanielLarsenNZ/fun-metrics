using FunMetrics.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FunMetrics
{
    public class Blob
    {
        private static IConfiguration _config = null;
        private static readonly Lazy<CloudBlobClient> _lazyClient = new Lazy<CloudBlobClient>(InitializeCloudBlobClient);

        private static CloudBlobClient CloudBlobClient => _lazyClient.Value;

        private readonly TelemetryClient _telemetryClient;

        /// Using dependency injection will guarantee that you use the same configuration for telemetry collected automatically and manually.
        public Blob(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("blob")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "head", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            _config = FunctionsHelper.GetConfig(context);

            string path = req.Query["path"];

            log.LogInformation($"FunMetrics.Blob.Run: {req.Method} {path}");

            if (string.IsNullOrWhiteSpace(path))
            {
                log.LogError("Querystring param \"path\" is null, empty or missing.");
                return new NotFoundResult();
            }
            if (path.StartsWith('/') || !path.Contains('/'))
            {
                log.LogError("Querystring param \"path\" is malformed. Must be a relative path to a blob file, with no leading \"/\".");
                return new NotFoundResult();
            }

            var uri = new Uri($"{CloudBlobClient.BaseUri}{path}");

            log.LogInformation($"FunMetrics.Blob.Run: {req.Method} {uri}");

            ICloudBlob blob;

            try
            {
                blob = await CloudBlobClient.GetBlobReferenceFromServerAsync(uri);
            }
            catch (StorageException ex)
            {
                log.LogError(ex, "Returning 404 / Not found.");
                return new NotFoundResult();
            }
            
            if (req.Method == HttpMethods.Head)
            {
                // HEAD - Return empty body but with headers as if GET
                req.HttpContext.Response.Headers["ETag"] = blob.Properties.ETag;
                req.HttpContext.Response.ContentLength = blob.Properties.Length;
                req.HttpContext.Response.ContentType = blob.Properties.ContentType;

                if (blob.Properties.LastModified.HasValue)
                {
                    req.HttpContext.Response.Headers["Last-Modified"] = blob.Properties.LastModified.Value.ToString("r");
                }

                return new OkResult();
            }

            // GET - get blob and stream to client
            InsightsHelper.TrackEvents(_telemetryClient, "FunMetrics.Blob", uri.ToString(), req);

            Stream stream = null;

            // try local cache
            string filePath = $"D:\\Local\\{path.Replace("/", "__")}";
            if (File.Exists(filePath))
            {
                // retry three times
                for (int i = 0; i < 3; i++)
                {
                    // get from local cache
                    try
                    {
                        stream = new FileStream(filePath, FileMode.Open);
                        log.LogInformation($"FunMetrics.Blob.Run: Cache HIT {filePath}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Could not open file \"{filePath}\" on attempt {i + 1}.");
                        await Task.Delay(10);
                    }
                }
            }

            if (stream == null)
            {
                // get from Blob
                log.LogInformation($"FunMetrics.Blob.Run: Cache MISS {filePath}");
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                
                // save to cache
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Could not cache file \"{filePath}\".");
                }
            }

            stream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(stream, blob.Properties.ContentType)
            {
                EnableRangeProcessing = true,
                EntityTag = new EntityTagHeaderValue(blob.Properties.ETag),
                LastModified = blob.Properties.LastModified
            };
        }

        private static CloudBlobClient InitializeCloudBlobClient()
        {
            if (_config["Blob.StorageConnectionString"] == null)
                throw new InvalidOperationException("App Setting \"Blob.StorageConnectionString\" is not set.");

            var account = CloudStorageAccount.Parse(_config["Blob.StorageConnectionString"]);
            return account.CreateCloudBlobClient();
        }
    }
}
