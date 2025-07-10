using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => docketLine.WE_DocketLineStatus != DocketLineStatus.Codes.Cancelled;
	}
}
