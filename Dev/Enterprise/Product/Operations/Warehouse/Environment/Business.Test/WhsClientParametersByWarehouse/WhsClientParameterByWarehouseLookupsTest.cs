using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsClientParameterByWarehouseLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		#region TestOrganisatons

		public void TestOrganisatons()
		{
			var clientParam = Factory.New<WhsClientParameterByWarehouse>();
			var lookups = new WhsClientParameterByWarehouseLookups(clientParam);
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;
			var headers = lookups.Organisations;
			AssertNotNull(headers);
			AssertEquals(typeof(WarehouseClientCollection), headers.GetType());
			AssertEquals("Should not be loaded", 0, headers.Count);
			AssertEquals("Should be cached", headers, lookups.Organisations);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var clientParam = Factory.New<WhsClientParameterByWarehouse>();
			var lookups = new WhsClientParameterByWarehouseLookups(clientParam);
			Factory.New<WhsWarehouse>();
			var warehouses = lookups.Warehouses;
			AssertNotNull(warehouses);
			AssertEquals("Should be cached", warehouses, lookups.Warehouses);
			AssertEquals("Should not be loaded", 0, warehouses.Count);
		}

		#endregion

		#region TestAreas

		public void TestPutawayAreas()
		{
			var clientParam = Factory.New<WhsClientParameterByWarehouse>();
			var lookups = new WhsClientParameterByWarehouseLookups(clientParam);
			var whs1 = Factory.New<WhsWarehouse>();
			var whs2 = Factory.New<WhsWarehouse>();
			AssertEquals("Warehouse is not set, no Areas should be available.", 0, lookups.PutawayAreas.Count);
			AssertEquals("Warehouse is not set, should be no default filter.", false, lookups.PutawayAreas.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"));
			AssertFilterDefaults(lookups.PutawayAreas, WhsAreaCollection.FilterConstants.IsPutawayArea, "Property0");

			clientParam.WY_WW_Whs = whs1.PK;
			AssertContainsExactElementsInAnyOrder("Warehouse is set, only Areas from Whs1 should be available.", whs1.Areas, lookups.PutawayAreas);
			AssertEquals("Warehouse is set, Areas should be defaulted by Whs1.", whs1.PK, lookups.PutawayAreas.FilterBusinessObjectDefaults["Warehouse:Property"].Value);
			AssertFilterDefaults(lookups.PutawayAreas, WhsAreaCollection.FilterConstants.IsPutawayArea, "Property0");
		}

		static void AssertFilterDefaults(WhsAreaCollection collection, string filterName, string propertyName)
		{
			var filterDefault = collection.FilterBusinessObjectDefaults[$"{filterName}:{propertyName}"];
			AssertEquals(propertyName, filterDefault.PropertyName);
		}

		#endregion

		#region TestPutawayAreas_OnlyReturnsPutawayAreas

		public void TestPutawayAreas_OnlyReturnsPutawayAreas()
		{
			var whs1 = Helper.CreateWarehouse("Whs1", "R1");
			var putawayArea = Helper.CreateArea(whs1, "A1", AreaTypes.Codes.FreeStore, false, true);
			var pickingArea = Helper.CreateArea(whs1, "A2", AreaTypes.Codes.FreeStore, true, false);
			var bothArea = Helper.CreateArea(whs1, "A3", AreaTypes.Codes.FreeStore, true, true);
			Factory.Save();

			var clientParam = Factory.New<WhsClientParameterByWarehouse>();
			var lookups = new WhsClientParameterByWarehouseLookups(clientParam);
			clientParam.WY_WW_Whs = whs1.PK;
			AssertCollectionContains("Putaway area must be returned.", putawayArea, lookups.PutawayAreas);
			AssertCollectionNotContains("Picking area must not be returned.", pickingArea, lookups.PutawayAreas);
			AssertCollectionContains("Both area must be returned.", bothArea, lookups.PutawayAreas);
		}

		#endregion

		#region TestReceiveCategories

		public void TestReceiveCategories()
		{
			var categories = new SystemDefinableCodeDescriptionBoolCollection
			{
				{ "RC1", (NoResString)"Receive Category 1", false },
				{ "RC2", (NoResString)"Receive Category 2", false }
			};
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var clientParam = Factory.New<WhsClientParameterByWarehouse>();
			var lookups = new WhsClientParameterByWarehouseLookups(clientParam);

			AssertContainsExactElementsInAnyOrder(categories.GetCodeDescriptionPairList(), lookups.ReceiveCategories.GetCodeDescriptionPairList());
		}

		#endregion
	}
}
