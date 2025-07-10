using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"PFR",
	"Create Transfers for Pick replenishment",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.PickFaceReplenishmentServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour"
	)]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class PickFaceReplenishmentServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new PickFaceReplenishmentProcessingManager(ServiceLogger);
			processingManager.CreateTransfersForPickfaceReplenishment();
		}

		#endregion
	}
}
