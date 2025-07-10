using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WhsPutawayLocationCacheServiceTask.Code,
	"Warehouse Putaway Location Cache Maintenance Service Task",
	"WHS",
	typeof(WhsPutawayLocationCacheServiceTask),
	MinimumPeriod = "15Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsPutawayLocationCacheServiceTask : ServiceProviderImpl
	{
		public const string Code = "WPC";

		public override void RunTask(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();

			ServiceLogger.Log(LogType.Information, "WhsPutawayLocationCache Update Service Task started."); // Service Task Logging
			ObjectFactory.Get<IPutawayLocationCacheUpdater>().UpdatePutawayLocationCache(factory, token);
			ServiceLogger.Log(LogType.Information, "WhsPutawayLocationCache Update Service Task completed."); // Service Task Logging
		}
	}
}
