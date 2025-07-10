using System.Threading;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public interface IWhsAutoRateQueuedInvoicesProcessSynchronously
	{
		void Process(IAutoRatingServiceLogger logger, CancellationToken token);
	}
}
