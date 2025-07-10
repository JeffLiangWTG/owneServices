using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsPickLineLookupsTestCase : BusinessObjectLookupsTestCase
	{
		public void TestTasks()
		{
			var pickLine = Factory.New<WhsPickLine>();
			AssertEquals("Should be a no result query, to avoid accidental loads of many records.", true, pickLine.Lookups.Tasks.CompleteFilter.IsNoResultQuery);
		}
	}
}
