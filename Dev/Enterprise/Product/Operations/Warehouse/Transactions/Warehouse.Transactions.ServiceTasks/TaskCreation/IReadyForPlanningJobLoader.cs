using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	interface IReadyForPlanningJobLoader
	{
		AppLockedItem<WhsReadyForPlanningJobsView> GetNextJobToProcess(BusinessObjectFactory factory);
	}
}
