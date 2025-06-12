using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using WTG.ErrorReporting;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class IssueManager : IIssueManager
	{
		private const string IssueManagerUri = nameof(IssueManagerUri);
		private const string IssueManagerKey = nameof(IssueManagerKey);
		
		public async Task ReportToIssueManagerAsync(string activityId, string key, Exception exception, ILog logger, CancellationToken cancellationToken)
		{
			if (logger == null)
			{
				logger = new NoOpLogger();
			}

			logger.Log(activityId, LogLevel.Debug, "Exception reporting to IssueManager", exception);

			try
			{
				logger.Log(activityId, LogLevel.Trace, "Start reporting to IssueManager");
				await ReportToIssueManagerCoreAsync(activityId, key, exception, logger, cancellationToken);
			}
			catch (Exception ex)
			{
				logger.Log(activityId, LogLevel.Error, $"Failed to post to IssueManager, innerException: {ex}", exception);
			}
			finally
			{
				logger.Log(activityId, LogLevel.Trace, "Finished reporting to IssueManager");
			}
		}

		private async Task ReportToIssueManagerCoreAsync(string activityId, string key, Exception exception, ILog logger, CancellationToken cancellationToken)
		{
			var issueManagerUrl = GetConfig(IssueManagerUri);
			var callingSystem = GetConfig(IssueManagerKey);

			if (string.IsNullOrWhiteSpace(issueManagerUrl))
			{
				logger.Log(activityId, LogLevel.Error, "Missing app setting: 'IssueManagerUri', exception won't be reported", exception);
				return;
			}

			if (string.IsNullOrWhiteSpace(callingSystem))
			{
				logger.Log(activityId, LogLevel.Warn, "Missing app setting: 'IssueManagerKey'");
				callingSystem = typeof(IssueManager).Assembly.GetName().Name;
			}

			var builder = GetReportBuilder($"{callingSystem} {key}", exception);
			using (var errorReportingClient = CreateReportClient(issueManagerUrl))
			{
				await errorReportingClient.PostCrashReportAsync(builder, cancellationToken);
			}
		}

		internal virtual IErrorReportingClient CreateReportClient(string uri)
		{
			var serviceUri = new Uri(uri);
			return new ErrorReportingClient(serviceUri);
		}

		internal virtual IOpaqueErrorReport GetReportBuilder(string key, Exception exception)
		{
			return new EnterpriseErrorReportBuilder()
				.SetKey(key)
				.SetRandomErrorReportID()
				.SetTimeOfException(DateTime.Now)
				.SetRootException(exception)
				.SetSubject("eHub Alert")
				.SetExeCreationTime(DateTime.Now);
		}

		internal virtual string GetConfig(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}
	}
}
