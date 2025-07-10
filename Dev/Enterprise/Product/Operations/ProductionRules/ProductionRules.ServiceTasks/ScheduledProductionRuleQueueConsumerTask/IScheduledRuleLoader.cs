using CargoWise.EntityFramework;

namespace Enterprise.ProductionRules.ServiceTasks
{
	interface IScheduledRuleLoader
	{
		ScheduledRuleLoaderResult GetNextRuleSetToProcess(BusinessObjectFactory factory);
	}
}
