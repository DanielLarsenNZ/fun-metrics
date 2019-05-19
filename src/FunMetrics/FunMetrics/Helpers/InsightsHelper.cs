using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunMetrics.Helpers
{
    internal class InsightsHelper
    {
        public static void TrackEvents(TelemetryClient client, string eventName, string url, HttpRequest req)
        {
            var uri = new Uri(url);
            string urlNoQuery = url.Split('?')[0];

            var properties = new Dictionary<string, string> {
                    { "Url", url },
                    { "Uri.AbsolutePath", uri.AbsolutePath },
                    { "Uri.UrlNoQuery", urlNoQuery},
                    { "Referer", req.Headers["Referer"] }
                };

            if (!string.IsNullOrWhiteSpace(uri.Query))
            {
                // record query string params as properties
                var queries = uri.Query.Substring(1).Split('&');

                foreach (var query in queries)
                {
                    if (string.IsNullOrEmpty(query)) continue;
                    var parts = query.Split('=');
                    properties.TryAdd($"Url.Query.{parts[0]}", parts.Length > 1 ? parts[1] : null);
                }
            }

            client.TrackEvent(
                eventName,
                properties: properties);

            client.TrackEvent(
                urlNoQuery,
                properties: properties);

            client.TrackEvent(
                uri.AbsolutePath,
                properties: properties);
        }
    }
}
