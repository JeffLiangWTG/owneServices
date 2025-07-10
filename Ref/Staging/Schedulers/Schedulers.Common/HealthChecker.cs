using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Quartz;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class HealthChecker : IJob
	{
		public Task Execute(IJobExecutionContext context)
		{
			using (var repo = new StagingRepository(DbConnectionStringManager.StagingConnectionString))
			using (var errorReport = new ErrorReportingWrapper())
			{
				CheckBlockedTriggersWithoutFiredTrigger(repo, errorReport);
			}
			return Task.CompletedTask;
		}

		public static void CheckBlockedTriggersWithoutFiredTrigger(IStagingRepository repo, IErrorReportingWrapper errorReporting)
		{
			// See JobStoreSupport.ClusterRecover : whenever the cluster restarts or fails, it will try to recover
			// jobs which are in the middle of being executed (i.e. BLOCKED STATE). It will check the fired triggers first
			// then recover job state based on fired triggers. If for some reasons, a trigger is in the blocked state and
			// does not the corresponding fired trigger, it will be in BLOCKED state forver. This check is meant to report
			// the abnormal status as soon as possible so we can understand why fired states are deleted.
			// Update: As long as their is a fired trigger for the same job, it's valid state,
			// so condition to filter bad records should be check if there is a BLOCKED trigger when there is no fired trigger for that job
			var triggers = (from tr in repo.Get<QRTZ_TRIGGERS>()
						   join fr in repo.Get<QRTZ_FIRED_TRIGGERS>() on tr.JOB_NAME equals fr.JOB_NAME into gr
						   from g in gr.DefaultIfEmpty()
						   where g == null && tr.TRIGGER_STATE == "BLOCKED"
						   select tr).ToArray();
			if (triggers.Length > 0)
			{
				errorReporting.AppendDescription($"Invalid quartz triggers state: BLOCKED without fired triggers :  {string.Join(",", triggers.Select(x => x.JOB_NAME))}");
				errorReporting.PostCrashReport();
			}

		}
	}
}
