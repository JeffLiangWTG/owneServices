using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsAdHocServiceJobLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestClients

		public void TestClients()
		{
			var adhocServiceJob = GetNewAdHocServiceJob();
			var lookups = adhocServiceJob.Lookups;
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), lookups.Clients.GetType());
			AssertNotNull(lookups.Clients);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var adhocServiceJob = GetNewAdHocServiceJob();
			var lookups = adhocServiceJob.Lookups;
			AssertEquals(typeof(WhsWarehouseCollectionForProductWarehouse), lookups.Warehouses.GetType());
			AssertNotNull(lookups.Warehouses);
		}

		public void TestWarehouses_ContainsProductWarehousesOnly()
		{
			var warehouseTransit = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouseTransit.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var warehouseProduct = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouseProduct.WW_WarehouseType = WarehouseTypes.Codes.Product;

			Factory.Save();

			var adhocServiceJob = GetNewAdHocServiceJob();
			var lookups = adhocServiceJob.Lookups;
			AssertNotNull(lookups.Warehouses);
			lookups.Warehouses.Load();
			AssertCollectionContains("Should contains Product warehouse.", warehouseProduct, lookups.Warehouses);
			AssertCollectionNotContains("Should NOT contains Transit warehouse.", warehouseTransit, lookups.Warehouses);
		}

		public void TestWarehouses_ContainsVirtualAndNonVirtualWarehouses()
		{
			var virtualWarehouse = Helper.CreateWarehouse("Virtual warehouse");
			virtualWarehouse.WW_IsVirtualWarehouse = true;
			virtualWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var nonVirtualWarehouse = Helper.CreateWarehouse("Non-virtual warehouse");
			nonVirtualWarehouse.WW_IsVirtualWarehouse = false;
			nonVirtualWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			Factory.Save();

			var adHocServiceJob = GetNewAdHocServiceJob();
			var lookups = adHocServiceJob.Lookups;
			AssertNotNull(lookups.Warehouses);
			lookups.Warehouses.Load();
			AssertContainsExactElementsInAnyOrder("Should contain virtual AND non-virtual warehouses.", lookups.Warehouses, new WhsWarehouse[] { nonVirtualWarehouse, virtualWarehouse });
		}

		#endregion

		#region Implementation

		WhsAdHocServiceJob GetNewAdHocServiceJob()
		{
			return Factory.New<WhsAdHocServiceJob>();
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
