using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	abstract class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert : IDeferTriggerConditionStrategy
	{
		protected WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var result = !businessEntity.IsInDatabase;
			if (result)
			{
				var docketLine = (WhsDocketLine)businessEntity;
				result = docketLine.WE_TransactionQuantity > 0
					&& docketLine.WE_StockOnHand < docketLine.WE_TransactionQuantity
					&& ShouldDeferTriggerCore(docketLine);
			}

			return result;
		}

		protected abstract bool ShouldDeferTriggerCore(WhsDocketLine docketLine);
	}
}
