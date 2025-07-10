using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsTransferLine_DeferTriggerStrategy
	{
	}

	class WhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsTransferLine_DeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsTransferLine_DeferTriggerStrategy
	{
		WhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsTransferLine_DeferTriggerStrategy()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity) => !businessEntity.IsInDatabase && !((WhsTransferLine)businessEntity).IsChildTransferLine;
	}
}
