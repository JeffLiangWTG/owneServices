using System.Threading;
using Enterprise.MasterFiles.Business.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ADC",
	"Accounting Data Consistency Check Service Task",
	"ACC",
	typeof(DataConsistencyCheckServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]
namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public class DataConsistencyCheckServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public override void RunTask(CancellationToken token)
		{
			var dataConsistencyCheckRunner = new DataConsistencyCheckRunner(ServiceLogger);
			dataConsistencyCheckRunner.PerformChecks(token);
		}
	}
}
