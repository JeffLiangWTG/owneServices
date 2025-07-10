using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineLookupsTest<TDocket, TDocketLine> : WhsBusinessObjectLookupsTestCase
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		#region TestPutawayAreas

		public void TestPutawayAreas()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("W1");
			var pickingArea = Helper.CreateArea(whs, "A1", "", true, false);
			var putawayArea = Helper.CreateArea(whs, "A2", "", false, true);
			var pickingAndPutawayArea = Helper.CreateArea(whs, "A3", "", true, true);

			var docketLine = GetNewDocketLine();
			var docket = GetNewWhsDocket(org, whs, docketLine);
			var lookups = docketLine.Lookups;

			AssertEquals("Area collection should contain the putaway only area.", true, lookups.PutawayAreas.Contains(putawayArea));
			AssertEquals("Area collection should contain the picking and putaway area.", true, lookups.PutawayAreas.Contains(pickingAndPutawayArea));
			AssertEquals("Area collection should not contain picking only area.", false, lookups.PutawayAreas.Contains(pickingArea));
		}

		#endregion

		#region TestAreas_WithDeletedDocket

		public void TestAreas_WithDeletedDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLine();
			var docket = GetNewWhsDocket(data.Org1, data.Whs1, docketLine);
			AssertNotNull(docketLine.Lookups.Areas);

			docketLine.Docket.Delete();
			AssertNotNull(docketLine.Lookups.Areas);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			var line1 = GetNewDocketLine();
			AssertNotNull(line1.Lookups.Clients);

			var client1 = Helper.CreateClient("1");
			var client2 = Helper.CreateClient("2");
			var client3 = Helper.CreateClient("3");
			client3.OH_IsWarehouseClient = false;

			var line2 = GetNewDocketLine();
			var clientList = line2.Lookups.Clients;
			AssertEquals("Collection should not be loaded.", 0, clientList.Count);

			clientList.Load();
			AssertCollectionContains("Collection should loaded all clients.", client1, clientList);
			AssertCollectionContains("Collection should loaded all clients.", client2, clientList);
			AssertCollectionNotContains("Collection should not contain organisations that are not whs clients.", client3, clientList);
		}

		#endregion

		#region TestTasks

		public void TestTasks()
		{
			var line = GetNewDocketLine();
			AssertEquals("Should be a no result query, to avoid accidental loads of many records.", true, line.Lookups.Tasks.CompleteFilter.IsNoResultQuery);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var line1 = GetNewDocketLine();
			AssertNotNull(line1.Lookups.Warehouses);

			var whs1 = Helper.CreateWarehouse("1");
			var whs2 = Helper.CreateWarehouse("2");

			var line2 = GetNewDocketLine();
			var whsList = line2.Lookups.Warehouses;
			AssertEquals("Collection should not be loaded.", 0, whsList.Count);

			whsList.Load();
			AssertContainsExactElementsInAnyOrder("Collection should loaded all warehouses.", new[] { whs1, whs2 }, whsList);
		}

		#endregion

		#region TestPackTypes

		public void TestPackTypes_NoProduct()
		{
			var docketLine = GetNewDocketLine();
			AssertContainsExactElementsInAnyOrder(docketLine.Lookups.PackTypes, LookupsHelper.PackTypesWithStandardUnits(Factory));
		}

		public void TestPackTypes_WithProduct()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient("TestClient");

			var product = Helper.CreateProduct("PROD", client);
			product.OP_StockKeepingUnit = Constants.PkgUnit.Box;
			var partUnit1 = Helper.CreateProductUnit(product, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 10);
			var partUnit2 = Helper.CreateProductUnit(product, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 20);

			var order = Helper.CreateWhsOrder(client, warehouse);
			var line = Helper.CreateWhsOrderLine(order, product, 10);

			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Unit, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet },
				line.Lookups.PackTypes.GetAllCodes());
		}

		#endregion

		#region TestSupplierPartsWithNoDocket

		public void TestSupplierPartsWithNoDocket()
		{
			var line = Factory.New<WhsReceiveLine>();
			var parts = (WhsOrgSupplierPartCollection)line.Lookups.SupplierParts;
			parts.Load();
			AssertEquals(0, parts.Count);
		}

		#endregion

		#region TestSupplierPartsWithDocket

		public void TestSupplierPartsWithDocket()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");
			var order = Helper.CreateWhsOrder(client, warehouse);
			var line = Helper.CreateWhsOrderLine(order, product, 10);

			Factory.Save();
			var parts = line.Lookups.SupplierParts;
			parts.Load();
			AssertEquals(1, parts.Count);
			AssertEquals("P1", parts[0].OP_PartNum);
			AssertEquals(true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));

			var newPart = parts.AddNew();
			AssertNotNull("Should default client as the owner.", newPart.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner));
		}

		#endregion

		#region TestSupplierPartsWithDocketDeletedObject

		public void TestSupplierPartsWithDocketDeletedObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var line = order.Lines.Single();

			Factory.Save();

			AssertNotNull(line.Lookups.SupplierParts);
			line.Docket.Delete();
			AssertNull(line.Lookups.SupplierParts);
		}

		#endregion

		#region TestBondedEntryLines

		public void TestBondedEntryLines()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save(); // needed because Lookups.BondedEntryLines uses a diff factory

			WhsDocketLine line = data.Receive.Lines[0];
			WhsDocketLineLookups lookups = line.Lookups;
			WhsBondedWarehouseAttributeCollection bondList = lookups.BondedEntryLines;
			bondList.Load();

			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, bondList.FilterBusinessObjectDefaults.ContainsDefaultFor("Customs Entry Key" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(line.WE_OP, bondList.FilterBusinessObjectDefaults["Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.Docket.WD_WW_Whs, bondList.FilterBusinessObjectDefaults["Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.Docket.WD_OH_Client, bondList.FilterBusinessObjectDefaults["Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(line.CustomsData.WB_EntryKey, bondList.FilterBusinessObjectDefaults["Customs Entry Key" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			AssertEquals(3, bondList.Count);
		}

		#endregion

		#region TestSchema

		public void TestSchema()
		{
			AssertEquals("WI_WW_Whs", WhsDocketLineLookups.Schema.WI_WW_Whs);
			AssertEquals("WI_OH_Client", WhsDocketLineLookups.Schema.WI_OH_Client);
			AssertEquals("WI_OP", WhsDocketLineLookups.Schema.WI_OP);
			AssertEquals("WI_LocationRow", WhsDocketLineLookups.Schema.WI_LocationRow);
			AssertEquals("WI_LocationColumn", WhsDocketLineLookups.Schema.WI_LocationColumn);
			AssertEquals("WI_LocationLevel", WhsDocketLineLookups.Schema.WI_LocationLevel);
			AssertEquals("WI_LocationTray", WhsDocketLineLookups.Schema.WI_LocationTray);
		}

		#endregion

		#region TestLocations

		public void TestLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1");
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0], "");

			var docketLine = GetNewDocketLine();
			docketLine.WE_OP = data.Part1.PK;
			if (NeedWE_WL)
			{
				docketLine.WE_WL = locations[1].PK;
			}
			if (NeedWE_WL_TransferFrom)
			{
				docketLine.WE_WL_TransferFrom = locations[0].PK;
			}
			docketLine.WE_TransactionQuantity = 1m;
			var docket = GetNewWhsDocket(data.Org1, data.Whs1, docketLine);
			docket.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			AssertCollectionContains(locations[0], docketLine.Lookups.Locations);
			AssertCollectionContains(locations[1], docketLine.Lookups.Locations);
			AssertCollectionNotContains(warehouse.DefaultLocation, docketLine.Lookups.Locations);
		}

		#endregion

		#region TestInventoryHeldCodeCollection

		public void TestInventoryHeldCodeCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			var holdCode1 = Helper.CreateInventoryHeldCode("ABC", "DESC");
			var holdCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", data.Org1.PK);
			var holdCode3 = Helper.CreateInventoryHeldCode("CCC", "CCC test", data.Org1.PK);
			var holdCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var holdCode5 = Helper.CreateInventoryHeldCode("CCC2", "CCC2 test", client2.PK);

			var docketLine = GetNewDocketLine();
			var docket = GetNewWhsDocket(data.Org1, data.Whs1, docketLine);
			var lookups = docketLine.Lookups;
			var heldCodeCollection = lookups.InventoryHeldCodeCollection.ToArray();
			AssertEquals(8, heldCodeCollection.Length);
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "SHORT" && c.Description == "Short Picked"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "HEL" && c.Description == "Held"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "DAM" && c.Description == "Damaged"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "ABC" && c.Description == "DESC"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => string.IsNullOrEmpty(c.Code) && c.Description == "None"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "BBB" && c.Description == "BBB for client 1"));
			AssertNotNull(heldCodeCollection.SingleOrDefault(c => c.Code == "CCC" && c.Description == "CCC test" && (ZGuid)c.PK == holdCode3.PK));
		}

		#endregion

		#region TestInventoryStatuses

		public void TestInventoryStatuses()
		{
			TestInventoryStatusesCore();
		}

		protected virtual void TestInventoryStatusesCore()
		{
			var line = GetNewDocketLine();
			AssertContainsExactElementsInAnyOrder(new InventoryStatus(), line.Lookups.InventoryStatuses);
		}

		#endregion

		#region TestConsignees

		public void TestConsignees()
		{
			var client = Helper.CreateClient("CLIENT1");
			var consignee1 = Helper.CreateClient("CONSIGNEE1");
			var consignee2 = Helper.CreateClient("CONSIGNEE2");
			consignee1.OH_IsConsignee = true;
			consignee2.OH_IsConsignee = true;

			var docket = GetNewWhsDocket(client, Helper.CreateWarehouse("WHS"));
			var docketLine = docket.Lines.AddNew();
			var consignees = docketLine.Lookups.Consignees;
			consignees.Load();
			AssertCollectionNotContains("Client should not be found.", client, consignees);
			AssertCollectionContains("All consignees should be found.", consignee1, consignees);
			AssertCollectionContains("All consignees should be found.", consignee2, consignees);
		}

		#endregion

		#region Methods for creating Dockets and creating / attaching DocketLines

		protected virtual TDocket GetNewWhsDocket()
		{
			return Factory.New<TDocket>();
		}

		protected virtual TDocketLine GetNewDocketLine()
		{
			return Factory.New<TDocketLine>();
		}

		protected virtual TDocket GetNewWhsDocket(OrgHeader org, WhsWarehouse whs, TDocketLine docketLine)
		{
			var docket = GetNewWhsDocket(org, whs);
			docket.Lines.Add(docketLine);
			return docket;
		}

		protected virtual TDocket GetNewWhsDocket(OrgHeader org, WhsWarehouse whs)
		{
			// this creates a new docket that can be saved to the database
			var docket = GetNewWhsDocket();
			if (org != null)
			{
				docket.WD_OH_Client = org.PK;
			}

			if (whs != null)
			{
				docket.WD_WW_Whs = whs.PK;
			}

			return docket;
		}

		protected virtual bool NeedWE_WL
		{
			get { return false; }
		}

		protected virtual bool NeedWE_WL_TransferFrom
		{
			get { return false; }
		}

		#endregion
	}
}
