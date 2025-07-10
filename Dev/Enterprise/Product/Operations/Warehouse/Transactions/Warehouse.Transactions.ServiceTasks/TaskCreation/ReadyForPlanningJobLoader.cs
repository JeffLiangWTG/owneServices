using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class ReadyForPlanningJobLoader : IReadyForPlanningJobLoader
	{
		public AppLockedItem<WhsReadyForPlanningJobsView> GetNextJobToProcess(BusinessObjectFactory factory)
		{
			const string AppLockKey = nameof(ReadyForPlanningJobLoader) + "_" + nameof(AppLockKey);
			const int BatchSize = 1;

			Argument.NotNull(factory, nameof(factory));

			var jobQuery = new ZDBOnlyQuery(typeof(WhsReadyForPlanningJobsView)) { MaximumRows = 1 };
			var loadedJob = factory.LoadWithApplocks<WhsReadyForPlanningJobsView>(AppLockKey, jobQuery, BatchSize);
			return loadedJob.ItemsWithLocks.SingleOrDefault();
		}
	}
}
