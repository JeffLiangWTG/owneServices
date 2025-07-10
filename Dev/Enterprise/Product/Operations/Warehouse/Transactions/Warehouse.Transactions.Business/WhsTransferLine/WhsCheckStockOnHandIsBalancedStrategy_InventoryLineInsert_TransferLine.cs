using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => docketLine.WE_DocketLineStatus == DocketLineStatus.Codes.HeldForTransfer || docketLine.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised;
	}
}
