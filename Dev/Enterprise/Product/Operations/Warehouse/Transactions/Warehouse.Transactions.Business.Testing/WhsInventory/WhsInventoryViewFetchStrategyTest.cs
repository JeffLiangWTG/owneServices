using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryViewFetchStrategyTest : TestCaseWithFactory
	{
		#region TestFetchForLoad

		public void TestFetchForLoad()
		{
			var strategy = new WhsInventoryViewFetchStrategy(Factory.New<WhsReceive>().Inventory.AddNew());
			int previousCount = Factory.ActiveTableFetchHints;
			strategy.FetchForLoad();
			AssertEquals("No fetch hints should be added", previousCount, Factory.ActiveTableFetchHints);  // see RowFactory.cs AddFetchHint
		}

		#endregion
	}
}
