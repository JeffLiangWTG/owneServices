using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentRatingAdaptersProviderTest : TestCaseWithFactory
	{
		#region TestGetAdapters

		public void TestGetAdapters()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adapterProvider = new WhsAdjustmentRatingAdaptersProvider(adjustment);
			var adapters = adapterProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals(1, adapters.Count);
			Assert(adapters[0] is WhsAdjustmentRatingAdapter);
		}

		#endregion
	}
}
