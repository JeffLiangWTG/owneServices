using CargoWise.Billing.API;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin
{
    public static class ErrorReportingClientExtension
    {
        public static void ReportToIssueManager(this IErrorReportingClient errorReportingClient, string subject, Exception exception, ILogger logger, bool useSubjectAsKey = false)
        {
            logger.LogError(exception, subject);

            var errorReportingUrl = errorReportingClient.ServiceUri.ToString();
			if (string.IsNullOrWhiteSpace(errorReportingUrl))
            {
                logger.LogWarning("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'");
                return;
            }

            try
            {
                var key = useSubjectAsKey ? subject : $"eHub Billing exception: {exception.Message}";

                var reportBuilder = new EnterpriseErrorReportBuilder()
                    .SetKey(key)
                    .SetRandomErrorReportID()
                    .SetTimeOfException(DateTime.Now)
                    .SetRootException(exception)
                    .SetExeCreationTime(DateTime.Now);

                if (exception is ValidationException validationEx)
                {
	                reportBuilder.SetExceptionDescription(string.Join(Environment.NewLine, validationEx.Errors.Select(error => "  " + error)));
                }

				if (!errorReportingClient.PostCrashReportAsync(reportBuilder).Wait(60000))
				{
					logger.LogError($@"60 seconds timed out when reporting to issue manager, Subject: {subject}, Uri: {errorReportingUrl}
Original exception: {exception}");
				}
				else
				{
					logger.LogDebug($"Logged Issue Manager issue to URL: {errorReportingUrl}");
				}
			}
            catch (Exception ex)
            {
                logger.LogError($@"Failed to raise an issue manager issue. Subject: {subject}, Uri: {errorReportingUrl}
Original exception: {exception}
Exception: {ex}");
            }
        }
    }
}
