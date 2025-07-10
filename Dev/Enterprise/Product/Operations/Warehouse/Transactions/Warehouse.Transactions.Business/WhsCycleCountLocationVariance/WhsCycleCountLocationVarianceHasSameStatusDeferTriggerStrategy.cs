using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy
	{
	}
	class WhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy
	{
		WhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			if (businessEntity != null)
			{
				if (businessEntity.IsInDatabase)
				{
					return businessEntity.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.Name.Equals(WhsCycleCountLocationVariance.Schema.WCC_Status) && p.HasChanges);
				}
				else
				{
					return true;
				}
			}
			return false;
		}
	}
}
