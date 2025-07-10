using System.Threading;
using Enterprise.Integration;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public interface IUNDGThresholdLimitNotificationProcessor
	{
		void NotifyWarehouseManagersOfExceededUNDGLimits(CancellationToken token, WhsWarehouse warehouse, ILogger logger);
	}
}
