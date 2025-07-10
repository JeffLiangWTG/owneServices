using System.Threading;
using CargoWise.Application;
using ServiceManager.Integration.ServiceTasks.CW;

// Category PKG = Packing
[assembly: HostedService(
	Enterprise.Packing.ServiceTasks.DeletePackingFountainForFinalizedPackingJobs.Code,
	Enterprise.Packing.ServiceTasks.DeletePackingFountainForFinalizedPackingJobs.Description,
	Enterprise.Packing.ServiceTasks.DeletePackingFountainForFinalizedPackingJobs.Category,
	typeof(Enterprise.Packing.ServiceTasks.DeletePackingFountainForFinalizedPackingJobs),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day",
	IsMandatory = true,
	ActiveByDefault = true)]

namespace Enterprise.Packing.ServiceTasks
{
	public class DeletePackingFountainForFinalizedPackingJobs : ServiceProviderImpl
	{
		public const string Code = "PJF";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task description")]
		public const string Description = "Delete Packing Fountains Of Finalized Package Jobs";
		public const string Category = "PKG";
		const int BatchSize = 500;

		public override void RunTask(CancellationToken cancellationToken)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				ObjectFactory.Get<IDeletePackingFountainForFinalizedPackingJobsProcessingManager>().DeletePackingFountainsForFinalizedPackingJobs(ServiceLogger, BatchSize, cancellationToken);
			}
		}
	}
}
