using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface IWhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy
	{
	}

	class WhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy
	{
		WhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return businessEntity != null && !businessEntity.IsInDatabase;
		}
	}
}
