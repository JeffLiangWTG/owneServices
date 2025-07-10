using System;
using System.Net;
using System.Threading;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterData.ServiceTask;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SendTradeBalanceTask.Code,
	SendTradeBalanceTask.FriendlyName,
	"SYS",
	typeof(SendTradeBalanceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	IsScheduleReadOnly = true,
	MinimumPeriod = "1month",
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1,
	DefaultScheduleStartAtUtc = "15minutes",
	DefaultScheduleRandomStartOffset = "225minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.MasterData.ServiceTask
{
	public class SendTradeBalanceTask : ServiceProviderImpl
	{
		public const string Code = "TBS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task FriendlyName")]
		public const string FriendlyName = "Credit Report - Trade Balance Service";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				RunWithRetry(true, youMustReactToThisToken);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected virtual int RetryWaitMilliseconds => 600 * 1000;

		protected virtual void RunWithRetry(bool shouldRetry, CancellationToken youMustReactToThisToken)
		{
			try
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();
				TradeInformationHelper.SendTradeInformation(ServiceLogger, youMustReactToThisToken);
			}
			catch (WebException ex)
			{
				if (shouldRetry && !youMustReactToThisToken.IsCancellationRequested)
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"Web exception occur, retry 1 time after 10 minutes. Exception: {ex}"));

					youMustReactToThisToken.WaitHandle.WaitOne(RetryWaitMilliseconds);

					RunWithRetry(false, youMustReactToThisToken);
				}
				else
				{
					LogAndReportError(ex);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogAndReportError(ex);
			}
		}

		void LogAndReportError(Exception ex)
		{
			ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Unhandled exception when sending Trade Balance Information: {ex}"));
			ErrorReporter.ReportOnce("Unhandled exception when sending Trade Balance Information", ex);
		}
	}
}
