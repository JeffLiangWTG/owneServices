using Common.Logging;

using log4netRef = log4net;

using System;

using CargoWise.eHub.DataModel.eHubTransactions;
using WTG.ErrorReporting;
using System.Linq;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace CargoWise.eHub.Shared.IssueManager
{
	public class IssueManager
	{
		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();

		internal static MemoryCache Cache = new MemoryCache("IssueManagerCache");

		public virtual void ReportToIssueManager(string subject, Exception exception, ILog logger, NameValueCollection appSettings, bool checkExcluding = true)
		{
			var callingSystem = appSettings["IssueManagerKey"];
			var issueManagerURL = appSettings["IssueManagerUri"];
			var cacheExpiration = int.TryParse(appSettings["IssueManagerCacheExpiration"], out int r) ? r : 300;

			logger.Error($"[IssueManager] {subject}", exception);

			if (string.IsNullOrWhiteSpace(appSettings["IssueManagerCacheExpiration"]))
			{
				logger.Warn("Missing app setting: 'IssueManagerCacheExpiration', using default value of 300 seconds");
			}

			if (Cache.AddOrGetExisting(subject, exception, DateTimeOffset.UtcNow.AddSeconds(cacheExpiration)) != null)
			{
				logger.Debug("Rate limit exceeded, skipping.");
				return;
			}

			if (checkExcluding && AlertExcluded(exception, ex => logger.Error("Caught exception when checking AlertExcluded", ex)))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(issueManagerURL))
			{
				logger.Error("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'");
				return;
			}

			if (string.IsNullOrWhiteSpace(callingSystem))
			{
				logger.Warn("Missing app setting: 'IssueManagerKey'");
				callingSystem = callingSystem ?? "";
			}

			try
			{
				var key = $"{callingSystem} exception: {exception.Message}";

				var reportBuilder = GetReportBuilder(key, exception);
				reportBuilder.SetSubject(subject ?? string.Empty);

				Uri serviceUri = new Uri(issueManagerURL);
				using (var errorReportingClient = new ErrorReportingClient(serviceUri))
				{
					if (!errorReportingClient.PostCrashReportAsync(reportBuilder).Wait(60000))
					{
						logger.Error($@"60 seconds timed out when reporting to issue manager, Calling System: {callingSystem}, Subject: {subject}, Uri: {issueManagerURL}
Original exception: {exception}");
					}
					else
					{
						logger.Debug($"Logged Issue Manager issue to URL: {issueManagerURL}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error($@"Failed to raise an issue manager issue. Calling System: [{callingSystem}], Subject: [{subject}], Uri: [{issueManagerURL}]
Original exception: {exception}
Exception: {ex}");
			}
		}

		public virtual void ReportToIssueManager(string subject, Exception exception, log4netRef.ILog logger, NameValueCollection appSettings, bool checkExcluding = true)
		{
			var callingSystem = appSettings["IssueManagerKey"];
			var issueManagerURL = appSettings["IssueManagerUri"];
			var cacheExpiration = int.TryParse(appSettings["IssueManagerCacheExpiration"], out int r) ? r : 300;

			logger.Error(subject, exception);

			if (string.IsNullOrWhiteSpace(appSettings["IssueManagerCacheExpiration"]))
			{
				logger.Warn("Missing app setting: 'IssueManagerCacheExpiration', using default value of 300 seconds");
			}

			if (Cache.AddOrGetExisting(subject, exception, DateTimeOffset.UtcNow.AddSeconds(cacheExpiration)) != null)
			{
				logger.Debug("Rate limit exceeded, skipping.");
				return;
			}

			if (checkExcluding && AlertExcluded(exception, ex => logger.Error("Caught exception when checking AlertExcluded", ex)))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(issueManagerURL))
			{
				logger.Error("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'");
				return;
			}

			if (string.IsNullOrWhiteSpace(callingSystem))
			{
				logger.Warn("Missing app setting: 'IssueManagerKey'");
				callingSystem = callingSystem ?? "";
			}

			try
			{
				var key = $"{callingSystem} exception: {exception.Message}";

				var reportBuilder = GetReportBuilder(key, exception);
				reportBuilder.SetSubject(subject);

				Uri serviceUri = new Uri(issueManagerURL);
				using (var errorReportingClient = new ErrorReportingClient(serviceUri))
				{
					if (!errorReportingClient.PostCrashReportAsync(reportBuilder).Wait(60000))
					{
						logger.Error($@"60 seconds timed out when reporting to issue manager, Calling System: {callingSystem}, Subject: {subject}, Uri: {issueManagerURL}
Original exception: {exception}");
					}
					else
					{
						logger.Debug($"Logged Issue Manager issue to URL: {issueManagerURL}");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error($@"Failed to raise an issue manager issue. Calling System: [{callingSystem}], Subject: [{subject}], Uri: [{issueManagerURL}]
Original exception: {exception}
Exception: {ex}");
			}
		}

		private EnterpriseErrorReportBuilder GetReportBuilder(string key, Exception exception)
		{
			return new EnterpriseErrorReportBuilder()
					.SetKey(key)
					.SetRandomErrorReportID()
					.SetTimeOfException(DateTime.Now)
					.SetRootException(exception);
		}

		private bool AlertExcluded(Exception ex, Action<Exception> logError)
		{
			try
			{
				using (var context = GetContext())
				{
					var codeMapValue = context.eHubCodeMapValues.FirstOrDefault(x =>
						x.eHubCodeMapKey.eHubCodeSet.eHubTransformationSet.TS_Name == "eHub Error Alert Service" &&
						x.eHubCodeSetResult.CR_Name == "Send To Issue Manager" &&
						x.eHubCodeMapKey.eHubCodeSet.eHubClient_Recipient.CC_ID == "eHub" &&
						x.eHubCodeMapKey.eHubCodeSet.eHubClient_Sender.CC_ID == "eHub" &&
						x.eHubCodeMapKey.CK_Key1Value == "" &&
						x.eHubCodeMapKey.CK_Key2Value == "" &&
						x.eHubCodeMapKey.CK_Key3Value == ex.Message);

					if ((codeMapValue?.CV_OutputCode ?? "") == "N")
					{
						return true;
					}
				}
			}
			catch (Exception innerException)
			{
				logError(innerException);
			}

			return false;
		}
	}
}
