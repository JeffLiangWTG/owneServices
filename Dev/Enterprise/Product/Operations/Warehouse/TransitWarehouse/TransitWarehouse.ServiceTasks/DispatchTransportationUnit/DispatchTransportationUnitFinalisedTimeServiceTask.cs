using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("DFT", "Set Finalized Time of Dispatch Transportation Unit", "WHS",
	typeof(Enterprise.Warehouse.Transit.ServiceTasks.DispatchTransportationUnitFinalisedTimeServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]
namespace Enterprise.Warehouse.Transit.ServiceTasks
{
	public class DispatchTransportationUnitFinalisedTimeServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var processingManager = new DispatchTransportationUnitFinalisedTimeProcessingManager(ServiceLogger);
			processingManager.SetFinalizedIfRequired();
		}

		#endregion
	}
}
