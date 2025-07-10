using System.Threading;
using Enterprise.Integration;

namespace Enterprise.Packing.ServiceTasks
{
	public interface IDeletePackingFountainForFinalizedPackingJobsProcessingManager
	{
		void DeletePackingFountainsForFinalizedPackingJobs(ILogger serviceLogger, int batchSize, CancellationToken cancellationToken);
	}
}
