namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsCycleCountLocationLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		public void TestTasks()
		{
			var cycleCountLocation = Factory.New<WhsCycleCountLocation>();
			AssertEquals("Should be a no result query, to avoid accidental loads of many records.", true, cycleCountLocation.Lookups.Tasks.CompleteFilter.IsNoResultQuery);
		}
	}
}
