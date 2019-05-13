using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunMetrics.Helpers
{
    internal static class RedirectHelper
    {
        public static bool IsWhitelisted(string url, string whitelist)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(whitelist)) return false;
            return (whitelist.Split(';').Any(w => url.StartsWith(w)));
        }
    }
}
