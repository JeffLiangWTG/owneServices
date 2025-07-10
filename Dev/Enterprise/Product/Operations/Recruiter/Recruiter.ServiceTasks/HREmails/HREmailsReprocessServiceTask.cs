using System.Globalization;
using System.Linq;
using System.Threading;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Recruiter.ServiceTasks.HREmailsReprocessServiceTask.Code,
	"Human Resources Email Reprocess Service Task",
	"HRM",
	typeof(Enterprise.Recruiter.ServiceTasks.HREmailsReprocessServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "2minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Recruiter.ServiceTasks
{
	public class HREmailsReprocessServiceTask : ServiceProviderImpl
	{
		public const string Code = "HRR";

		[HostedServiceRequirement]
		public static string CheckDaxtraEnabled()
		{
			if (RecruiterDataRegistry.Instance.DaxtraEnable.Value)
			{
				return string.Empty;
			}

			return (NoResString)"This service requires Daxtra to be enabled"; // information for logging only.
		}

		public override void RunTask(CancellationToken cancellationToken)
		{
			using var daxtraResumeParser = new DaxtraResumeParser();
			using (Env.Instance.SuspendBranchAccessError())
			{
				var updater = GetUpdater(daxtraResumeParser);
				updater.Progress += Updater_Progress;
				updater.Finished += Updater_Finished;
				updater.ProcessEmailsBacklog(cancellationToken);
			}
		}

		protected virtual ApplicationDocumentsUpdater GetUpdater(DaxtraResumeParser daxtraResumeParser)
		{
			return new ApplicationDocumentsUpdater(daxtraResumeParser);
		}

		void Updater_Finished(object sender, ApplicationDocumentsUpdater.UpdateFinishedEventArgs e)
		{
			if (e.Errors.Any())
			{
				Log(LogType.Error, Res.GetString("A1C1A6BF-DF97-4838-B9F4-345F6B1D7CA8", "Process completed with errors.")
					+ System.Environment.NewLine
					+ string.Join(System.Environment.NewLine, e.Errors));
			}
			else
			{
				Log(LogType.Information, Res.GetString("4C0CB823-20CE-4025-B38B-39B6578168F0", "Process completed."));
			}

			Log(LogType.Information, Res.GetString("AC801FDB-FA04-423C-912F-075F14CF009D",
				"Processed applications: {0}\r\nBatches processed: {1}\r\nDocuments saved: {2}",
				e.Stats.ItemsTotal, e.Stats.BatchesProcessed, e.Stats.DocumentsSaved));
		}

		void Updater_Progress(object sender, ApplicationDocumentsUpdater.ApplicationDocumentsProgressEventArgs e)
		{
			var log =
				Res.GetString("42BDA82C-81C7-46DC-A6B8-E9AE7E8B5599", "Processing applications {0} of {1}. Processing batches {2} of {3}. Batches sent {4}.",
				e.Stats.ItemsProcessed, e.Stats.ItemsTotal,
				e.Stats.BatchesProcessed, e.Stats.BatchesTotal,
				e.Stats.BatchesSent);

			Log(LogType.Information, log);
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			ServiceLogger.Log(logType, string.Format(CultureInfo.InvariantCulture, format, args));
		}
	}
}
