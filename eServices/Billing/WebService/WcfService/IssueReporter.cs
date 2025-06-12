using Common.Logging;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.WcfService
{
	public static class IssueReporter
	{
        public static void ReportToIssueManager(string subject, Exception exception, ILog logger)
        {
            logger.Error(subject, exception);

            if (string.IsNullOrWhiteSpace(errorReportingUrl))
            {
                logger.Warn("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'");
                return;
            }

            try
            {
                var key = $"Billing WCF service exception: {exception.Message}";

                var reportBuilder = new EnterpriseErrorReportBuilder()
	                .SetSubject(subject)
                    .SetKey(key)
                    .SetRandomErrorReportID()
                    .SetTimeOfException(DateTime.Now)
                    .SetRootException(exception)
                    .SetExeCreationTime(DateTime.Now);

                if (exception.GetType() == typeof(KafkaException))
                {
                    var kafkaEx = (KafkaException)exception;
                    reportBuilder.SetExceptionDescription($"Code: {kafkaEx.Error.Code}\r\nReason: {kafkaEx.Error.Reason}\r\nIsBrokerError: {kafkaEx.Error.IsBrokerError}\r\nIsLocalError: {kafkaEx.Error.IsLocalError}");
                }

                using (var errorReportingClient = GetErrorReportingClient())
                {
                    if (!errorReportingClient.PostCrashReportAsync(reportBuilder).Wait(60000))
                    {
                        logger.Error($@"60 seconds timed out when reporting to issue manager, Subject: {subject}, Uri: {errorReportingUrl}
Original exception: {exception}");
                    }
                    else
                    {
                        logger.Debug($"Logged Issue Manager issue to URL: {errorReportingUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($@"Failed to raise an issue manager issue. Subject: {subject}, Uri: {errorReportingUrl}
Original exception: {exception}
Exception: {ex}");
            }
        }

        internal static Func<IErrorReportingClient> GetErrorReportingClient = () => new ErrorReportingClient(new Uri(errorReportingUrl));
        static string errorReportingUrl => System.Configuration.ConfigurationManager.AppSettings["IssueManagerUri"];
    }
}
