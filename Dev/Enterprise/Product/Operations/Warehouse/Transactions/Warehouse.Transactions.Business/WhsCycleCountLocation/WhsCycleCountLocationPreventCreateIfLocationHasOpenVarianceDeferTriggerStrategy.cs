using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy
	{
	}
	class WhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy
	{
		WhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return businessEntity != null && !businessEntity.IsInDatabase;
		}
	}
}
