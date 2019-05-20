using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunMetrics.Helpers
{
    internal class InsightsHelper
    {
        public static void TrackEvents(TelemetryClient client, string eventName, string url, HttpRequest req)
        {
            var uri = new Uri(url);
            string urlNoQuery = url.Split('?')[0];

            var properties = new Dictionary<string, string>
            {
                { "Url", url },
                { "Uri.AbsolutePath", uri.AbsolutePath },
                { "Uri.UrlNoQuery", urlNoQuery},
                { "Referer", req.Headers["Referer"] },
                { "User-Agent", req.Headers["User-Agent"] }
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

            foreach (var header in req.Headers)
            {
                // don't track these headers
                if (new[] 
                {
                    "Accept-Encoding",
                    "Connection",
                    "Accept",
                    "Cache-Control",
                    "Range",
                    "Referer",
                    "User-Agent",
                    "Cookie",
                    "Upgrade-Insecure-Requests"
                }.Contains(header.Key)) continue;

                properties.TryAdd($"Request.Header.{header.Key}", header.Value);
            }

            client.Context.User.Id = GetTrackingId(req);
            client.Context.Session.Id = GetSessionId(req);

            client.TrackEvent(
                $"{eventName} {uri.AbsolutePath}",
                properties: properties);

            //client.TrackEvent(
            //    urlNoQuery,
            //    properties: properties);

            //client.TrackEvent(
            //    uri.AbsolutePath,
            //    properties: properties);
        }

        private static string GetTrackingId(HttpRequest req)
        {
            const string TrackingIdCookieName = "trackingid";
            string id = req.Cookies[TrackingIdCookieName];
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                req.HttpContext.Response.Cookies.Append(TrackingIdCookieName, id,
                    new CookieOptions { Expires = DateTime.Now.AddYears(1), SameSite = SameSiteMode.None });
            }

            return id;
        }

        private static string GetSessionId(HttpRequest req)
        {
            const string SessionIdCookieName = "sessionid";
            string id = req.Cookies[SessionIdCookieName];
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                req.HttpContext.Response.Cookies.Append(SessionIdCookieName, id,
                    new CookieOptions { SameSite = SameSiteMode.None });
            }

            return id;
        }
    }
}
