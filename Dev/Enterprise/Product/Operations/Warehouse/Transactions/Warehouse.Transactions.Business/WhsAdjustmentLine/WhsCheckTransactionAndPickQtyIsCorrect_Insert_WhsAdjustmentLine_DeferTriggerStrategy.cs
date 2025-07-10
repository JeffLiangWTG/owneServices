using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsAdjustmentLine_DeferTriggerStrategy
	{
	}

	class WhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsAdjustmentLine_DeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsAdjustmentLine_DeferTriggerStrategy
	{
		WhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsAdjustmentLine_DeferTriggerStrategy()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity) => !businessEntity.IsInDatabase && ((WhsAdjustmentLine)businessEntity).IsAdjustmentOut;
	}
}
