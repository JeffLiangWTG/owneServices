namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => ((WhsAdjustmentLine)docketLine).IsAdjustmentIn && docketLine.IsFinalised;
	}
}
