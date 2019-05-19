using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace FunMetrics.Helpers
{
    internal class FunctionsHelper
    {
        public static IConfiguration GetConfig(ExecutionContext context)
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
