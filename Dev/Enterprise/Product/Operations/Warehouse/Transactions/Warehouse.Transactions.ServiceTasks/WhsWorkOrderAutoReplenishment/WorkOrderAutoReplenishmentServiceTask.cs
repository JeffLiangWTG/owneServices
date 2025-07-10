using System.Threading;
using CargoWise.EntityFramework;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"RWO",
	"Automatically create Replenishment Work Orders for Bill of Material Products",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.WhsWorkOrderAutoReplenishmentServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour"
	)]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsWorkOrderAutoReplenishmentServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var factory = new BusinessObjectFactory();
			var processingManager = new WorkOrderAutoReplenishmentProcessingManager(factory, ServiceLogger);
			processingManager.CreateWorkOrderForReplenishment();
		}

		#endregion
	}
}
