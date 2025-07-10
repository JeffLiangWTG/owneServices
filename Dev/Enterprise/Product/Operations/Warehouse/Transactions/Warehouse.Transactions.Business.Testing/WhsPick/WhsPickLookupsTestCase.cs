using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsPickLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		public void TestWarehouses()
		{
			Pick = GetNewBusinessObject();
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), Pick.Lookups.Warehouses.GetType());
			AssertNotNull(Pick.Lookups.Warehouses);
		}

		public void TestOrderFindBoxList()
		{
			Pick = GetNewBusinessObject();
			Factory.New<WhsOrder>();
			var collection = Pick.Lookups.OrderFindBoxList;

			AssertEquals(true, collection is WhsOrderCollectionForPicking);

			AssertNotNull(collection);
			AssertEquals(0, collection.Count);
			AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Pick Option" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Order Status" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			Pick.WP_WW_Whs = ZGuid.NewZGuid();
			Pick.WP_PickOption = WhsPickOption.Codes.Manual;
			collection = Pick.Lookups.OrderFindBoxList;
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Pick Option" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestConsignees()
		{
			Pick = GetNewBusinessObject();
			AssertNotNull(Pick.Lookups.Consignees);
		}

		public void TestPickOptions()
		{
			Pick = GetNewBusinessObject();
			AssertNotNull(Pick.Lookups.PickOptions);
		}

		#region TestDockDoorLocations

		public void TestDockDoorLocations()
		{
			var ddlLocationType = Helper.CreateLocationType("XYZ", LocationClasses.Codes.DDL);
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);

			var dockDoorFilterKey = WhsLocationCollection.FilterSchema.DockDoorLocation + ":Property0";
			var warehouseFilterKey = WhsLocationCollection.FilterSchema.Warehouse + ":Property";

			var pick = Factory.New<WhsPick>();
			AssertEquals("Dock Door location filter should be used.", true, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should not be used.", false, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults.ContainsDefaultFor(warehouseFilterKey));

			pick.WP_WW_Whs = whs1.PK;
			AssertEquals("Dock Door location filter should be used.", true, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs1 and used.", whs1.PK, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);

			pick.WP_WW_Whs = whs2.PK;
			AssertEquals("Dock Door location filter should be used.", true, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs2 and used.", whs2.PK, pick.Lookups.DockDoorLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);
		}

		#endregion

		#region TestDynamicPickAreas

		public void TestDynamicPickAreas()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row1", 2, 2);

			var areaB = Factory.New<WhsArea>();
			areaB.WA_AreaType = AreaTypes.Codes.FreeStore;
			areaB.WA_IsPickingArea = true;
			areaB.WA_WW_Whs = warehouse1.PK;
			areaB.WA_Name = "areaB";

			var areaDynamicWhs2 = Factory.New<WhsArea>();
			areaDynamicWhs2.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamicWhs2.WA_IsPickingArea = true;
			areaDynamicWhs2.WA_WW_Whs = warehouse2.PK;
			areaDynamicWhs2.WA_Name = "areaDynamicwhs2";

			var areaDynamic = Factory.New<WhsArea>();
			areaDynamic.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamic.WA_IsPickingArea = true;
			areaDynamic.WA_WW_Whs = warehouse1.PK;
			areaDynamic.WA_Name = "areaDynamic";

			var putawayArea = Factory.New<WhsArea>();
			putawayArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			putawayArea.WA_IsPickingArea = false;
			putawayArea.WA_IsPutawayArea = true;
			putawayArea.WA_WW_Whs = warehouse1.PK;
			putawayArea.WA_Name = "putawayArea";

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = warehouse1.PK;
			var collection = pick.Lookups.DynamicPickAreas;
			AssertEquals("Collection count correct", 1, collection.Count);
			AssertEquals("Area correct", "areaDynamic", collection[0].WA_Name);
		}

		#endregion

		#region TestPackingStationLocations

		public void TestPackingStationLocations()
		{
			Helper.CreateLocationType("XYZ", LocationClasses.Codes.PST);
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);

			var packingStationFilterKey = WhsLocationCollection.FilterSchema.PackingStationLocation + ":Property0";
			var warehouseFilterKey = WhsLocationCollection.FilterSchema.Warehouse + ":Property";

			var pick = Factory.New<WhsPick>();
			AssertEquals("Packing Station location filter should be used.", true, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults[packingStationFilterKey].Value);
			AssertEquals("Warehouse filter should not be used.", false, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults.ContainsDefaultFor(warehouseFilterKey));

			pick.WP_WW_Whs = whs1.PK;
			AssertEquals("Packing Station location filter should be used.", true, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults[packingStationFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs1 and used.", whs1.PK, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);

			pick.WP_WW_Whs = whs2.PK;
			AssertEquals("Packing Station location filter should be used.", true, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults[packingStationFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs1 and used.", whs2.PK, pick.Lookups.PackingStationLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);
		}

		#endregion

		#region Implementation

		protected virtual WhsPick GetNewBusinessObject()
		{
			return Factory.New<WhsPick>();
		}

		protected WhsPick Pick;

		#endregion
	}
}
