using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy

	interface IWhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy { }
	class WhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy : WhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy, IWhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy
	{
		WhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy() { }

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsTransferLine transferLine && !transferLine.IsChildTransferLine;
		}
	}

	#endregion
}
