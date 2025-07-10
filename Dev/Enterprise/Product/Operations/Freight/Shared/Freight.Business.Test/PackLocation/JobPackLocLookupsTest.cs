using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobPackLocLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWarehouses()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WH1");
			Factory.Save();

			PackLocation loc = Factory.New<PackLocation>();
			Assert("Warehouses.Count > 0", loc.Lookups.Warehouses.Count > 0);
		}
	}
}
