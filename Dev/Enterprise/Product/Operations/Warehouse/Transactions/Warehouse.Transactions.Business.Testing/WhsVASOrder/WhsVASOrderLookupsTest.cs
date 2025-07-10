using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsVASOrderLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestAreas

		public void TestAreas()
		{
			var vasOrder = GetNewVASOrder();
			AssertEquals(typeof(WhsAreaCollection), vasOrder.Lookups.Areas.GetType());
			AssertNotNull(vasOrder.Lookups.Areas);

			var whs = Factory.New<WhsWarehouse>();
			vasOrder.WarehousePK = whs.PK;
			AssertEquals(whs, vasOrder.Lookups.Areas.Relationship.Master);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			var vasOrder = GetNewVASOrder();
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), vasOrder.Lookups.Clients.GetType());
			AssertNotNull(vasOrder.Lookups.Clients);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var vasOrder = GetNewVASOrder();
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), vasOrder.Lookups.Warehouses.GetType());
			AssertNotNull(vasOrder.Lookups.Warehouses);
		}

		#endregion

		#region Implementation

		WhsVASOrder GetNewVASOrder()
		{
			return Factory.New<WhsVASOrder>();
		}

		#endregion
	}
}
