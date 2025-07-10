namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine : WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine()
		{
		}

		protected override bool ShouldDeferTriggerCore(WhsDocketLine docketLine) => ((WhsAdjustmentLine)docketLine).IsAdjustmentIn && docketLine.IsFinalised;
	}
}
