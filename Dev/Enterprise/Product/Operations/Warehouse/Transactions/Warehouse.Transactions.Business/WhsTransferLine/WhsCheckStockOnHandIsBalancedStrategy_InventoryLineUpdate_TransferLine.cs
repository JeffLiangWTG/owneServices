using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => docketLine.WE_DocketLineStatus == DocketLineStatus.Codes.HeldForTransfer || docketLine.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised;
	}
}
