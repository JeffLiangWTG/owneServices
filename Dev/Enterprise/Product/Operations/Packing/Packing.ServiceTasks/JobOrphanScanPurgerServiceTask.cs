using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

// Category PKG = Packing
[assembly: HostedService(
	"APP",
	"Anonymous Packages Purger",
	"PKG",
	typeof(Enterprise.Packing.ServiceTasks.JobOrphanScanPurgerServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day"
	)]

namespace Enterprise.Packing.ServiceTasks
{
	public class JobOrphanScanPurgerServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processingManager = new ProcessingManager(ServiceLogger);
			processingManager.PurgeOldJobOrphanScans();
		}
	}
}
