using System;
using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ABC",
	"ABC Analysis Calculation",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.ABCAnalysisServiceTask),
	MinimumPeriod = "1week",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday }
	)]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class ABCAnalysisServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new ABCAnalysisProcessingManager(ServiceLogger);
			processingManager.DoABCAnalysis();
		}
	}
}
