using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsInventoryViewLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestLocations

		public void TestLocations()
		{
			OrgHeader client = Helper.CreateClient();
			WhsWarehouse warehouse1 = Helper.CreateWarehouse("Whs1", "Row1");
			WhsWarehouse warehouse2 = Helper.CreateWarehouse("Whs2", "Row2");
			OrgSupplierPart product = Helper.CreateProduct(client, "P1");

			WhsReceive receive = Helper.CreateWhsReceive(client, warehouse1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product, 7m);

			Factory.Save(); // this will generate the first location for each Warehouse

			AssertCollectionContains(warehouse1.Rows[0].Locations[0], inventory.Lookups.Locations);
			AssertCollectionNotContains(warehouse2.Rows[0].Locations[0], inventory.Lookups.Locations);
		}

		public void TestLocationsWhenDocketIsNull()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "Row1");
			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row2");
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_WD = ZGuid.Empty;
			inventory.WI_WE_InDocketLine = ZGuid.Empty;

			AssertNull("Precondition", inventory.Docket);
			AssertContainsExactElementsInAnyOrder(new[] { warehouse1.DefaultLocation, warehouse1.DefaultOutboundDockDoorLocation, warehouse2.DefaultLocation, warehouse2.DefaultOutboundDockDoorLocation }, inventory.Lookups.Locations);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;
			AssertNotNull(Inventory.Lookups.Clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), Inventory.Lookups.Clients.GetType());
			AssertEquals("Collection should not be loaded", 0, Inventory.Lookups.Clients.Count);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			Factory.New<WhsWarehouse>();
			AssertNotNull(Inventory.Lookups.Warehouses);
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), Inventory.Lookups.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, Inventory.Lookups.Warehouses.Count);
		}

		#endregion

		#region TestSupplierParts

		public void TestSupplierParts()
		{
			var client = Helper.CreateClient("CLIENT");
			Helper.SetClientAttributeType(client, AttributeNumber.One, true);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);

			var inventory = Factory.New<WhsInventoryView>();
			AssertNotNull(inventory.Lookups.SupplierParts);
			AssertEquals(typeof(WhsOrgSupplierPartCollection), inventory.Lookups.SupplierParts.GetType());
			AssertEquals("Collection should not be loaded", 0, inventory.Lookups.SupplierParts.Count);

			inventory.WI_OH_Client = client.PK;
			AssertEquals(true, inventory.Lookups.SupplierParts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));

			inventory.WI_OP = ZGuid.Invalid;
			inventory.WI_OP_PartNum = "NewProduct";
			inventory.WI_OP_Desc = "NewDescription";
			inventory.WI_UnitsUQ = "PLT";
			inventory.CommodityCode = "1234";
			inventory.WI_PartAttrib1 = "PA1";
			inventory.WI_PartAttrib2 = "PA2";
			inventory.WI_PartAttrib3 = "PA3";
			inventory.WI_ExpiryDate = ZDate.Today;
			inventory.WI_PackingDate = ZDate.Today;

			var newPart = inventory.Lookups.SupplierParts.AddNew();
			AssertEquals("NewDescription", newPart.OP_Desc);
			AssertEquals("PLT", newPart.OP_StockKeepingUnit);
			AssertEquals("", newPart.OP_RH_NKCommodityCode);
			AssertEquals(false, newPart.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, newPart.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, newPart.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, newPart.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, newPart.RelatedOrganisations[0].OU_UsePackingDate);

			inventory.SetupSupplierPart(newPart);
			AssertEquals("1234", newPart.OP_RH_NKCommodityCode);
			AssertEquals(true, newPart.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(true, newPart.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals("Should be false, because client does not have this attribute set.", false, newPart.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(true, newPart.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(true, newPart.RelatedOrganisations[0].OU_UsePackingDate);
		}

		#endregion

		#region TestPackTypes

		public void TestPackTypes()
		{
			AssertNotNull(Inventory.Lookups.PackTypesWithStandardUnits);
			Assert(Inventory.Lookups.PackTypesWithStandardUnits is CodeDescriptionPairList);
		}

		#endregion

		#region TestPossibleInventory

		public void TestPossibleInventory()
		{
			var data = new TestDataForInventory(Factory, null);
			data.CreateMultiWarehouseClientProductInventory();

			var lookups = data.Line213.Lookups;
			var inventoryCollection = lookups.PossibleInventory;
			inventoryCollection.Load();

			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 1" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 2" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 3" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Row" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Column" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Level" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventoryCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Tray" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(15, inventoryCollection.Count);
		}

		#endregion

		#region TestBondedEntryLines

		public void TestBondedEntryLines()
		{
			var data = new TestDataForBondedEntries(Factory);
			Factory.Save(); // needed because Lookups.BondedEntryLines uses a diff factory

			WhsInventoryView line = data.Receive.Inventory[0];
			WhsInventoryViewLookups lookups = line.Lookups;
			WhsBondedWarehouseAttributeCollection bondList = lookups.BondedEntryLines;
			bondList.Load();

			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Customs Entry Key" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(line.WI_OP, bondList.FilterBusinessObjectDefaults["Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.Docket.WD_WW_Whs, bondList.FilterBusinessObjectDefaults["Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.Docket.WD_OH_Client, bondList.FilterBusinessObjectDefaults["Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.InDocketLine.CustomsData.WB_EntryKey, bondList.FilterBusinessObjectDefaults["Customs Entry Key" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(3, bondList.Count);
		}

		#endregion

		#region TestCommodityCodes

		public void TestCommodityCodes()
		{
			AssertNotNull(Inventory.Lookups.CommodityCodes);
			AssertEquals(typeof(RefCommodityCodeCollection), Inventory.Lookups.CommodityCodes.GetType());
		}

		#endregion

		#region TestPutawayAreas

		public void TestAreas()
		{
			var receive = Factory.New<WhsReceive>();
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_WD = receive.PK;

			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");

			var area1 = Helper.CreateArea(whs1, "AREA1", AreaTypes.Codes.FreeStore);
			var area2 = Helper.CreateArea(whs1, "AREA2", AreaTypes.Codes.FreeStore);
			var area3 = Helper.CreateArea(whs2, "AREA3", AreaTypes.Codes.FreeStore);

			var areasCollection1 = inventory.Lookups.PutawayAreas;
			AssertEquals("When no Warehouse is set, Area collection should be empty.", 0, areasCollection1.Count);

			receive.WD_WW_Whs = whs1.PK;
			var areasCollection2 = inventory.Lookups.PutawayAreas;
			AssertEquals("When Warehouse is set, Area collection should contain the Areas on that Warehouse.", true, areasCollection2.Contains(area1));
			AssertEquals("When Warehouse is set, Area collection should contain the Areas on that Warehouse.", true, areasCollection2.Contains(area2));
			AssertEquals("When Warehouse is set, Area collection should not contain Areas from other Warehouses.", false, areasCollection2.Contains(area3));
		}

		#region TestPutawayAreas

		public void TestPutawayAreas()
		{
			var receive = Factory.New<WhsReceive>();
			var inventory = Factory.New<WhsInventoryView>();
			inventory.WI_WD = receive.PK;

			var whs = Helper.CreateWarehouse("WHS1");

			var pickingArea = Helper.CreateArea(whs, "AREA1", AreaTypes.Codes.FreeStore, true, false);
			var putawayArea = Helper.CreateArea(whs, "AREA2", AreaTypes.Codes.FreeStore, false, true);
			var pickingAndPutawayArea = Helper.CreateArea(whs, "AREA3", AreaTypes.Codes.FreeStore, true, true);

			receive.WD_WW_Whs = whs.PK;
			var areasCollection = inventory.Lookups.PutawayAreas;

			AssertEquals("Area collection should contain the putaway only area.", true, areasCollection.Contains(putawayArea));
			AssertEquals("Area collection should contain the picking and putaway area.", true, areasCollection.Contains(pickingAndPutawayArea));
			AssertEquals("Area collection should not contain picking only area.", false, areasCollection.Contains(pickingArea));
		}

		#endregion

		#endregion

		#region TestInventoryHeldCodeCollection

		public void TestInventoryHeldCodeCollection()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("ABC", "DESC");
			var holdCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1.PK);
			var holdCode3 = helper.CreateInventoryHeldCode("CCC", "CCC test", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var holdCode5 = helper.CreateInventoryHeldCode("CCC2", "CCC test", client2.PK);

			var inventoryView = Factory.New<WhsInventoryView>();
			inventoryView.WI_OH_Client = client1.PK;
			var heldCodeCollection2 = inventoryView.Lookups.InventoryHeldCodeCollection.ToArray();
			AssertEquals(8, heldCodeCollection2.Length);
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "SHORT" && c.Description == "Short Picked"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "HEL" && c.Description == "Held"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "DAM" && c.Description == "Damaged"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "ABC" && c.Description == "DESC"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => string.IsNullOrEmpty(c.Code) && c.Description == "None"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "BBB" && c.Description == "BBB for client 1"));
			AssertNotNull(heldCodeCollection2.SingleOrDefault(c => c.Code == "CCC" && c.Description == "CCC test"));

			inventoryView.WI_OH_Client = client2.PK;
			var heldCodeCollection3 = inventoryView.Lookups.InventoryHeldCodeCollection.ToArray();
			AssertEquals(8, heldCodeCollection3.Length);
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "SHORT" && c.Description == "Short Picked"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "HEL" && c.Description == "Held"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "DAM" && c.Description == "Damaged"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "ABC" && c.Description == "DESC"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => string.IsNullOrEmpty(c.Code) && c.Description == "None"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "BBB2" && c.Description == "BBB for client 2"));
			AssertNotNull(heldCodeCollection3.SingleOrDefault(c => c.Code == "CCC2" && c.Description == "CCC test"));
		}

		#endregion

		#region Implementation

		protected WhsInventoryView Inventory
		{
			get { return inventory ?? (inventory = Factory.New<WhsInventoryView>()); }
		}

		WhsInventoryView inventory;

		#endregion
	}
}
