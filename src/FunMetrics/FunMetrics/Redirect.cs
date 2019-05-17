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
            _config = GetConfig(context);
            _log = log;

            string url = req.Query["url"];

            using (var operation = _telemetryClient.StartOperation<RequestTelemetry>($"GET {url}"))
            {
                if (!IsWhitelisted(url))
                {
                    operation.Telemetry.ResponseCode = "404";
                    return new NotFoundResult();
                }

                log.LogInformation($"Redirect: 302 {url}");

                // Track Event

                var uri = new Uri(url);
                string urlNoQuery = url.Split('?')[0];

                var properties = new Dictionary<string, string> {
                        { "Url", url },
                        { "Uri.AbsolutePath", uri.AbsolutePath },
                        { "Uri.UrlNoQuery", urlNoQuery},
                        { "Referer", req.Headers["Referer"] }
                    };

                var queries = uri.Query.Substring(1).Split('&');

                foreach (var query in queries)
                {
                    if (string.IsNullOrEmpty(query)) continue;
                    var parts = query.Split('=');
                    properties.TryAdd($"Url.Query.{parts[0]}", parts.Length > 1 ? parts[1] : null);
                }

                _telemetryClient.TrackEvent(
                    "FunMetrics.Redirect",
                    properties: properties);

                _telemetryClient.TrackEvent(
                    urlNoQuery,
                    properties: properties);

                _telemetryClient.TrackEvent(
                    uri.AbsolutePath,
                    properties: properties);

                operation.Telemetry.ResponseCode = "302";

                return new RedirectResult(url, false);
            }
        }

        private static bool IsWhitelisted(string url)
        {
            if (string.IsNullOrEmpty(_config["WhitelistedUrlsStartsWith"]))
            {
                _log.LogError("Redirect.IsWhitelisted: App Setting \"WhitelistedUrlsStartsWith\" cannot be null or empty.");
            }

            return RedirectHelper.IsWhitelisted(url, _config["WhitelistedUrlsStartsWith"]);
        }

        private static IConfiguration GetConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
#if DEBUG
               .SetBasePath(context.FunctionAppDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
#endif
               .AddEnvironmentVariables()
               .Build();

            return config;
        }

    }
}
