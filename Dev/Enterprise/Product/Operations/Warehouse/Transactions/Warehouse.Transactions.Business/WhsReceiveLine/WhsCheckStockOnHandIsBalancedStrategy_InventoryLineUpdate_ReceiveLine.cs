using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => docketLine.WE_DocketLineStatus != DocketLineStatus.Codes.Cancelled;
	}
}
