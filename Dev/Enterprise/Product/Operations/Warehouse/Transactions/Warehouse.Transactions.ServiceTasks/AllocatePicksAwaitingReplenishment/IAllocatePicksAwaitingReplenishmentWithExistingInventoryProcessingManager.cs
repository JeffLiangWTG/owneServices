using System.Threading;
using Enterprise.Integration;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public interface IAllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager
	{
		void AllocateAwaitingReplenishmentPicks(ILogger logger, CancellationToken token);
	}
}
