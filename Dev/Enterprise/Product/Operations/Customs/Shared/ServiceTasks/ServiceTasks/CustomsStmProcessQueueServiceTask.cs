using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class CustomsStmProcessQueueServiceTask : NudgeCustomsServiceTask
	{
		public const string ServiceTaskCategory = "CUS";

		protected override void RunMainTask(CancellationToken token)
		{
			using (DisposableEnvironment.ForBranch(GlbCompany.CurrentCompany.FirstActiveBranch?.PK.ToGuid() ?? Env.CurrentBranchPK))
			{
				var processor = GetProcessQueueProcessor(Logger);
				processor.ExecuteBatch(token);
				if (processor.NeedToNudge)
				{
					Nudge();
				}
			}
		}

		protected abstract BaseCustomsStmProcessQueueBatchProcessor GetProcessQueueProcessor(LoggingInformation logger);
	}
}
