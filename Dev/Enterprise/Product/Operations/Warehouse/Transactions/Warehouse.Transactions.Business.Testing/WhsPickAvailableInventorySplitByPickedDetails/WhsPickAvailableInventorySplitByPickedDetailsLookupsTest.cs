namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickAvailableInventorySplitByPickedDetailsLookupsTest : WhsPickAvailableInventorySplitBaseLookupsTest<WhsPickAvailableInventorySplitByPickedDetails>
	{
		protected override WhsPickAvailableInventorySplitByPickedDetails CreateInventory() => new WhsPickAvailableInventorySplitByPickedDetails(Factory);
	}
}
