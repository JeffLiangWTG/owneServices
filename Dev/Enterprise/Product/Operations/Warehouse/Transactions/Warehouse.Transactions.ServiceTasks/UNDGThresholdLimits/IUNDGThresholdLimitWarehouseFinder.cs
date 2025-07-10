using System.Collections.Generic;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public interface IUNDGThresholdLimitWarehouseFinder
	{
		IReadOnlyCollection<WhsWarehouse> LoadWarehousesWithDGLimits();
	}
}
