using FunMetrics.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FunMetrics
{
    public class Redirect
    {
        private static IConfiguration _config = null;
        private static ILogger _log = null;

        private readonly TelemetryClient _telemetryClient;

        /// Using dependency injection will guarantee that you use the same configuration for telemetry collected automatically and manually.
        public Redirect(TelemetryConfiguration telemetryConfiguration)
        {
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("redirect")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            _config = FunctionsHelper.GetConfig(context);
            _log = log;

            string url = req.Query["url"];

            if (!IsWhitelisted(url))
            {
                return new NotFoundResult();
            }

            log.LogInformation($"Redirect: 302 {url}");

            InsightsHelper.TrackEvents(_telemetryClient, "FunMetrics.Redirect", url, req);

            return new RedirectResult(url, false);
        }

        private static bool IsWhitelisted(string url)
        {
            if (string.IsNullOrEmpty(_config["WhitelistedUrlsStartsWith"]))
            {
                _log.LogError("Redirect.IsWhitelisted: App Setting \"WhitelistedUrlsStartsWith\" cannot be null or empty.");
            }

            return RedirectHelper.IsWhitelisted(url, _config["WhitelistedUrlsStartsWith"]);
        }
    }
}
