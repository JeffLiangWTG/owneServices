using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsProductParamsByWhsAndClientLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestPickGroups

		public void TestPickGroups()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var productParams = Params;
				AssertEquals(typeof(PickGroupCollection), productParams.Lookups.PickGroups.GetType());
				AssertEquals(1, productParams.Lookups.PickGroups.Count);

				var pickGroupFromLookups = (PickGroup)productParams.Lookups.PickGroups[0];
				AssertEquals(new ZShort(1), pickGroupFromLookups.PickSequence);
				AssertEquals("Desc", pickGroupFromLookups.Description);
			}
		}

		#endregion

		#region TestHeaders

		public void TestHeaders()
		{
			OrgHeaderCollection headers = Lookups.Headers;
			AssertNotNull(headers);
			AssertEquals("Should be cached", headers, Lookups.Headers);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			Factory.New<WhsWarehouse>();
			WhsWarehouseCollection warehouses = Lookups.Warehouses;
			AssertNotNull(warehouses);
			AssertEquals("Should be cached", warehouses, Lookups.Warehouses);
			AssertEquals("Should not be loaded", 0, warehouses.Count);
		}

		#endregion

		#region TestStagingLocationsBOM

		public void TestStagingLocationsBOM()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var row1 = Helper.CreateRowAndGenerateLocations(whs1, "R1", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs2, "R2", 2, 1, 1);
			Factory.Save();

			var prodParams = Factory.New<WhsProductParamsByWhsAndClient>();
			AssertEquals("When no warehouse is set, Locations collection should be empty.", 0, prodParams.Lookups.StagingLocationsBOM.Count);
			Assert("No filter default for warehouse should be added if no Warehouse is set.", !prodParams.Lookups.StagingLocationsBOM.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"));

			var locationsInWhs1 = whs1.Rows.SelectMany(r => r.Locations).ToArray();
			var locationsInWhs2 = whs2.Rows.SelectMany(r => r.Locations).ToArray();

			prodParams.W3_WW = whs1.PK;
			AssertContainsExactElementsInAnyOrder(locationsInWhs1, prodParams.Lookups.StagingLocationsBOM);
			AssertEquals("Locations should be filtered by warehouse", whs1.PK, prodParams.Lookups.StagingLocationsBOM.FilterBusinessObjectDefaults["Warehouse:Property"].Value);

			prodParams.W3_WW = whs2.PK;
			AssertContainsExactElementsInAnyOrder(locationsInWhs2, prodParams.Lookups.StagingLocationsBOM);
			AssertEquals("Locations should be filtered by warehouse", whs2.PK, prodParams.Lookups.StagingLocationsBOM.FilterBusinessObjectDefaults["Warehouse:Property"].Value);
		}

		public void TestPutawayAreas()
		{
			var whs = Factory.New<WhsWarehouse>();

			var pickingArea = Helper.CreateArea(whs, "A1", "", true, false);
			var putawayArea = Helper.CreateArea(whs, "A2", "", false, true);
			var pickingAndPutawayArea = Helper.CreateArea(whs, "A3", "", true, true);

			var prodParams = Factory.New<WhsProductParamsByWhsAndClient>();
			AssertEquals("When no warehouse is set, Areas collection should be empty.", 0, prodParams.Lookups.PutawayAreas.Count);
			Assert("No filter default for warehouse should be added if no Warehouse is set.", !prodParams.Lookups.PutawayAreas.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"));

			prodParams.W3_WW = whs.PK;
			AssertContainsExactElementsInAnyOrder(new[] { putawayArea, pickingAndPutawayArea }, prodParams.Lookups.PutawayAreas.Where(a => a.WA_Name != "DEFAULT"));
			AssertEquals("Areas should be filtered by warehouse", whs.PK, prodParams.Lookups.PutawayAreas.FilterBusinessObjectDefaults["Warehouse:Property"].Value);
		}

		#endregion

		#region TestPackTypes

		public void TestPackTypes()
		{
			AssertEquals(new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(), Lookups.PackTypes);
		}

		#endregion

		#region TestStockTakeCycles

		public void TestStockTakeCycles()
		{
			AssertNotNull(Lookups.StockTakeCycles);
		}

		#endregion

		#region TestPutawayGroups

		public void TestPutawayGroups()
		{
			var putawayGroups = Lookups.PutawayGroups;
			AssertNotNull(putawayGroups);
			AssertEquals("Should be cached", putawayGroups, Lookups.PutawayGroups);
		}

		#endregion

		#region Implementation

		protected WhsProductParamsByWhsAndClient Params
		{
			get { return prodParams ?? (prodParams = Factory.New<WhsProductParamsByWhsAndClient>()); }
		}

		WhsProductParamsByWhsAndClientLookups Lookups
		{
			get { return Params.Lookups; }
		}

		WhsProductParamsByWhsAndClient prodParams;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
