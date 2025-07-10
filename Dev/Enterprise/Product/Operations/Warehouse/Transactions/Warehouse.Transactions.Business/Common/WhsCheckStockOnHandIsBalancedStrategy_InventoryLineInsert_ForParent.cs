using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent : IDeferTriggerConditionStrategy, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var result = !businessEntity.IsInDatabase;

			if (result)
			{
				var docketLine = (WhsDocketLine)businessEntity;
				result = docketLine.WE_WE_ParentDocketLine.IsValid && !docketLine.WE_IsOriginalInventory;
			}

			return result;
		}
	}
}
