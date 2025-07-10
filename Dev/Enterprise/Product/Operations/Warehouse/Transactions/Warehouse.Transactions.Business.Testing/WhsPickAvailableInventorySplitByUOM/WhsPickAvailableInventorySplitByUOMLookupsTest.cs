namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickAvailableInventorySplitByUOMLookupsTest : WhsPickAvailableInventorySplitBaseLookupsTest<WhsPickAvailableInventorySplitByUOM>
	{
		protected override WhsPickAvailableInventorySplitByUOM CreateInventory() => new WhsPickAvailableInventorySplitByUOM(Factory);
	}
}
