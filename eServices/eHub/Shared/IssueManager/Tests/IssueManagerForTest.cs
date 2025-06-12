using System;
using System.Collections.Specialized;
using WTG.ErrorReporting;
using Common.Logging;

namespace CargoWise.eHub.Shared.IssueManagerTests
{
	public class IssueManagerForTesting: IssueManager.IssueManager
	{
		EnterpriseErrorReportBuilder reportBuilder;

		public EnterpriseErrorReportBuilder EnterpriseErrorReportBuilder
		{
			get { return reportBuilder; }
			set { reportBuilder = value;}
		}

		public override void ReportToIssueManager(string subject, Exception exception, ILog logger, NameValueCollection appSettings, bool checkExcluding = true)
		{
			var callingSystem = appSettings["IssueManagerKey"];
			logger.Error(subject, exception);

			if (string.IsNullOrWhiteSpace(callingSystem))
			{
				logger.Warn("Missing app setting: 'IssueManagerKey'");
				callingSystem = callingSystem ?? "";
			}

			try
			{
				var key = $"{callingSystem} exception: {exception.Message}";

				reportBuilder = GetReportBuilder(key, exception);
				reportBuilder.SetSubject(subject);

			}
			catch (Exception ex)
			{
				logger.Error($@"Failed to raise an issue manager issue. Calling System: [{callingSystem}], Subject: [{subject}]]
Original exception: {exception}
Exception: {ex}");
			}
		}

		public EnterpriseErrorReportBuilder GetReportBuilder(string key, Exception exception)
		{
			return reportBuilder = new EnterpriseErrorReportBuilder()
				.SetKey(key)
				.SetRandomErrorReportID()
				.SetTimeOfException(DateTime.Now)
				.SetRootException(exception);
		}
	}
}
