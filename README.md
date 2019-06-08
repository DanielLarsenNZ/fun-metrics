# Functions for Metrics

Prototypes and examples of Azure Functions for recording metrics. For example, how many times a file
is downloaded.

> Work in Progress! 👷‍

## TODO

1. ✅ ~~Tracking cookie~~
1. Short-circuit if-modified-since
1. Add ETag to cached file name
1. Cache content type, etag, lastmodified to save a metadata call to Blob service

## Links and references

Monitor Azure Functions <https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring>

Application Insights API for custom events and metrics <https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics>

Track custom operations with Application Insights .NET SDK <https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking>

### Counting Blob downloads

<https://github.com/djhmateer/AzureFunctionBlobDownloadCount/blob/master/davemtest/ProcessLogs.cs>

Enabling (classic) storage account logging <https://docs.microsoft.com/en-us/rest/api/storageservices/enabling-storage-logging-and-accessing-log-data#HowtoenableStorageLoggingusingtheWindowsAzureManagementPortal>
