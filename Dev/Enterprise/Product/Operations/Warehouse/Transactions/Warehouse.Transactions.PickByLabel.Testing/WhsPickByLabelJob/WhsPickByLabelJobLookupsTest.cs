namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class WhsPickByLabelJobLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTasks()
		{
			var job = Factory.New<WhsPickByLabelJob>();
			AssertEquals("Should be a no result query, to avoid accidental loads of many records.", true, job.Lookups.Tasks.CompleteFilter.IsNoResultQuery);
		}
	}
}
