using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InventoryFilterBusinessObject))]
	public class InventoryFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestStatusFilter

		public void TestStatusFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 1, true, false);
			Factory.Save();

			var expectedList = new InventoryStatus();
			var inventoryFilter = GetNewFilterStripBusinessObject();
			AssertContainsExactElementsInAnyOrder(expectedList, ((ModuleFilterWithList)inventoryFilter[InventoryFilterBusinessObject.Schema.Status]).List);
			var inventory3 = receive2.Inventory[0];
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.Property = InventoryStatus.Codes.Available;
			Asserter.AssertMatches("Available", inventoryFilter.Filter, inventory2);

			filter.Property = InventoryStatus.Codes.Putaway;
			Asserter.AssertMatches("Putaway", inventoryFilter.Filter, inventory3);

			filter.Property = InventoryStatus.Codes.Held;
			Asserter.AssertMatches("Held", inventoryFilter.Filter, inventory1);
		}

		#region TestInTransitStatusFilter

		public void TestInTransitStatusFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			var inventory = receive1.Inventory[0];
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, inventory.LocationString, "");
			AssertEquals("Precondition: Inventory should be Available.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Inventory should In-Transit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, transferLine.Inventory[0]);

			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
			var nonSupportUser = Helper.CreateGlbStaff("NS", "notsupport");
			Factory.Save();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.Property = "";

			using (Env.SetTemporaryUserContext(nonSupportUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Asserter.AssertMatches("Does show intransit", inventoryFilter.Filter, inventory2, transferLine.Inventory[0]);
				AssertEquals("Should be able to search inventory with all statuses.", 10, inventoryFilter.InventoryStatuses.Count);
				AssertEquals("Should be able to search inventory with all statuses.", true, inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.InTransit));
			}
		}

		#endregion

		#region TestFilter_InventoryStatuses

		public void TestFilter_InventoryStatuses()
		{
			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();

			AssertEquals("Should return 10 status codes with registry on", 10, inventoryFilter.InventoryStatuses.Count);
			Assert("Should contain AVL", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Available));
			Assert("Should contain HEL", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Held));
			Assert("Should contain PUT", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Putaway));
			Assert("Should contain ARV", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Arrived));
			Assert("Should contain PND", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Pending));
			Assert("Should contain PTA", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.PuttingAway));
			Assert("Should contain REC", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Received));
			Assert("Should contain INT", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.InTransit));
			Assert("Should contain STA", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.Staged));
			Assert("Should contain RTP", inventoryFilter.InventoryStatuses.ContainsCode(InventoryStatus.Codes.ReadyToPack));
		}

		#endregion

		#endregion

		#region TestClientFilter

		public void TestClient()
		{
			var client = Helper.CreateClient("TESTCLIENT1", "TESTCLIENT1");
			Factory.Save();

			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			AssertNull("Precondition", inventoryFilter.Client);

			clientFilter.IsActive = true;
			clientFilter.Property = client.PK;
			AssertEquals("Client property should return the client that was added in the filter", client.PK, inventoryFilter.Client.PK);
		}

		public void TestClientFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("TEST2", "TEST2");
			var client3 = Helper.CreateClient("TEST3", "TEST3");
			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "115", Notify);
			var receive2 = Helper.CreateWhsReceive(client2.PK, data.Whs1.PK, "211", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 10.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2.PK, 16.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1.PK, 12.0m);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("no filter", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = data.Org1.PK;
			Asserter.AssertMatches("filter org1", inventoryFilter.Filter, inventory1, inventory2);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);

			filter.Property = client3.PK;
			Asserter.AssertMatches("clinet3 does not have inventory", inventoryFilter.Filter);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);
		}

		public void TestClientFilterVisibility()
		{
			var inventoryFilter = GetNewFilterStripBusinessObject();

			AssertEquals("Precondition: WhsAllowedClients", true, Env.Security.WhsAllowedClients.IsAllowed);
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			try
			{
				var inventoryFilter1 = GetNewFilterStripBusinessObject();
				var filter1 = (ModuleGuidFilter)inventoryFilter1[InventoryFilterBusinessObject.Schema.Client];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					var inventoryFilter2 = GetNewFilterStripBusinessObject();
					var filter2 = (ModuleGuidFilter)inventoryFilter2[InventoryFilterBusinessObject.Schema.Client];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedClients.IsAllowed = true;
			}
		}

		#endregion

		#region TestWarehouseFilter

		public void TestClientWarehouseFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("warehouse1", "A1", 3, 2);
			var whs2 = Helper.CreateWarehouse("warehouse2", "A2", 3, 2);

			var client1 = Helper.CreateClient("client1", "client1");
			var client2 = Helper.CreateClient("client2", "client2");
			var receive1 = Helper.CreateWhsReceive(client1.PK, whs1.PK, "115", Notify);
			var receive2 = Helper.CreateWhsReceive(client2.PK, whs2.PK, "211", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 10.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2.PK, 16.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1.PK, 12.0m);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			var filterClient = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			var filterWarehouse = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			filterClient.IsActive = true;
			filterWarehouse.IsActive = true;
			filterClient.Property = client1.PK;
			filterWarehouse.Property = whs1.PK;
			Asserter.AssertMatches("two inventory should match", inventoryFilter.Filter, inventory1, inventory2);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);

			filterClient.Property = client1.PK;
			filterWarehouse.Property = whs2.PK;
			Asserter.AssertMatches("no match", inventoryFilter.Filter);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);

			filterClient.Property = client2.PK;
			filterWarehouse.Property = whs1.PK;
			Asserter.AssertMatches("no match", inventoryFilter.Filter);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);

			filterWarehouse.Property = whs2.PK;
			Asserter.AssertMatches("inventory3 should match", inventoryFilter.Filter, inventory3);
			AssertContains("When filtering inventories by client, consider using WI_OH_Client in order to improve performance.", "WI_OH_Client", inventoryFilter.Filter.LiteralTextADO);
		}

		public void TestWarehouseFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30.0m).Inventory[0];
			Factory.Save();

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, 10m, "A");
			adjustment1.FinaliseDocket();
			Factory.Save();

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "2", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, 10m, "A");
			adjustment2.FinaliseDocket();

			Factory.Save();
			var inventory2 = adjustment1.Lines[0].Inventory[0];
			var inventory3 = adjustment2.Lines[0].Inventory[0];
			AssertEquals("Test adjustment data not finalised", true, adjustment1.IsFinalised && adjustment2.IsFinalised);
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var warehouseChangedHitCount = 0;
			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
			inventoryFilter.WarehouseChanged += (sender, e) => warehouseChangedHitCount++;
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			filter.Property = data.Whs1.PK;
			AssertEquals(1, warehouseChangedHitCount);
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = Helper.CreateWarehouse("ABC").PK;
			AssertEquals(2, warehouseChangedHitCount);
			Asserter.AssertMatches("not match", inventoryFilter.Filter);
		}

		public void TestWarehouseFilter_FilterOnlyDocketLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, whs2.DefaultLocation); // assign location from wrong warehouse

			var connection = ((IDbConnected)Factory).Connection;
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsDocketLine_LocationIsInCorrectWarehouse ON WhsDocketLine"); // This test relies on a bad datashape
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(inventoryLine1, inventoryLine2);

			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("When filter is empty, should find all inventories.", inventoryFilter.Filter, inventoryLine1, inventoryLine2);

			filter.Property = data.Whs1.PK;
			Asserter.AssertMatches("When filtering by Whs1, should find all inventories as they are on Receive for Whs1.", inventoryFilter.Filter, inventoryLine1, inventoryLine2);

			filter.Property = whs2.PK;
			Asserter.AssertMatches("When filtering by whs2, should find no inventory as no dockets for whs2 exist.", inventoryFilter.Filter);
		}

		public void TestWarehouseFilterVisibility()
		{
			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();

			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				var inventoryFilter1 = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
				var filter1 = (ModuleGuidFilter)inventoryFilter1[InventoryFilterBusinessObject.Schema.Warehouse];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					var inventoryFilter2 = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
					var filter2 = (ModuleGuidFilter)inventoryFilter2[InventoryFilterBusinessObject.Schema.Warehouse];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		#endregion

		#region TestProductFilter

		#region TestProductFilter

		public void TestProductFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("warehouse1", "A1", 1, 2);
			var whs2 = Helper.CreateWarehouse("warehouse2", "A2", 1, 2);
			Helper.CreateWarehouse("warehouse3", "A3", 1, 2);
			var client1 = Helper.CreateClient("client1", "client1");
			var client2 = Helper.CreateClient("client2", "client2");
			var product1 = Helper.CreateProduct(client1, "product1");
			var product2 = Helper.CreateProduct(client1, "product2");
			var product3 = Helper.CreateProduct(client2, "product3");
			var product4 = Helper.CreateProduct(client2, "product4");
			Factory.Save();

			var inventory1 = Helper.CreateWhsReceiveWithInventory(client1, whs1, "r1", product1, 1m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(client1, whs2, "r2", product1, 1m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(client1, whs2, "r3", product2, 1m).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(client2, whs1, "r4", product3, 1m).Inventory[0];
			var inventory5 = Helper.CreateWhsReceiveWithInventory(client2, whs2, "r5", product3, 1m).Inventory[0];
			var inventory6 = Helper.CreateWhsReceiveWithInventory(client2, whs1, "r6", product4, 1m).Inventory[0];
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filterClient = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			var filterProduct = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Product];
			var filterWarehouse = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			filterClient.IsActive = true;
			filterProduct.IsActive = true;
			filterWarehouse.IsActive = true;
			filterClient.Property = client1.PK;
			filterProduct.Property = product1.PK;
			Asserter.AssertMatches("only two rows are match.", inventoryFilter.Filter, inventory1, inventory2);

			filterClient.Property = client2.PK;
			filterProduct.Property = product3.PK;
			Asserter.AssertMatches("only two rows are match.", inventoryFilter.Filter, inventory4, inventory5);

			filterWarehouse.Property = whs1.PK;
			filterClient.Property = client1.PK;
			filterProduct.Property = product2.PK;
			Asserter.AssertMatches("no match.", inventoryFilter.Filter);

			filterWarehouse.Property = whs2.PK;
			Asserter.AssertMatches("only one row is match.", inventoryFilter.Filter, inventory3);

			filterClient.Property = client2.PK;
			filterProduct.Property = product4.PK;
			Asserter.AssertMatches("no match.", inventoryFilter.Filter);

			filterWarehouse.Property = whs1.PK;
			Asserter.AssertMatches("only one row is match.", inventoryFilter.Filter, inventory6);
		}

		#endregion

		#region TestProductFilter_MultipleProductsWithSameName

		public void TestProductFilter_MultipleProductsWithSameName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WHS2");
			var whs3 = Helper.CreateWarehouse("WHS3");

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "111", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, whs2.PK, "112", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1.PK, whs3.PK, "113", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 6.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2.PK, 5.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1.PK, 2.0m);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filterClient = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Client];
			var filterProduct = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Product];
			filterClient.IsActive = true;
			filterProduct.IsActive = true;
			filterClient.Property = data.Org1.PK;
			filterProduct.Property = data.Part1.PK;
			Asserter.AssertMatches("inventory2 has product2 and not match", inventoryFilter.Filter, inventory1, inventory3);

			data.Part1.OP_PartNum = "TEST";
			data.Part2.OP_PartNum = "TEST";

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			Asserter.AssertMatches("Filter should Grab Matching Product Codes", inventoryFilter.Filter, inventory1, inventory2, inventory3);
		}

		#endregion

		#region TestProductFilter_Validation

		public void TestProductFilter_Validation()
		{
			var client = Helper.CreateClient("CLIENT1");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var productFilter = (ModuleGuidFilter)filter[InventoryFilterBusinessObject.Schema.Product];
			productFilter.IsActive = true;
			productFilter.Property = part.PK; // need to double set so validation will be run on empty value
			productFilter.Property = ZGuid.Empty;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");

			productFilter.Property = part.PK;
			AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertHasError(productFilter.PropertyInfo, "Enter a valid selection.");
			productFilter.Property = ZGuid.Empty; // clean up

			var clientFilter = (ModuleGuidFilter)filter[InventoryFilterBusinessObject.Schema.Client];
			clientFilter.IsActive = true;
			clientFilter.Property = client.PK;
			productFilter.Property = part.PK;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");
		}

		#endregion

		#region TestProductFilter_ReValidation

		public void TestProductFilter_ReValidation()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)filter[InventoryFilterBusinessObject.Schema.Client];
			var productFilter = (ModuleGuidFilter)filter[InventoryFilterBusinessObject.Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;
			FieldInvalidTextMemory.SetInvalidText(productFilter, productFilter.PropertyInfo.Name, "AA");
			productFilter.Property = ZGuid.Missing;
			AssertNoExceptionThrown("No Exception should be thrown for invalid product code.", new AnonymousMethod(() => clientFilter.Property = client.PK));
			AssertEquals("Product PK should not be changed.", ZGuid.Missing, productFilter.Property);
			AssertEquals("Invalid Product code should not be cleared out.", "AA", FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name));
		}

		#endregion

		#region TestProductFilter_NullProduct

		public void TestProductFilter_NullProduct()
		{
			var inventoryFilter = GetNewFilterStripBusinessObject();

			var productFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Product];
			productFilter.IsActive = true;
			productFilter.Property = ZGuid.NewZGuid();

			var inventoryCollection = new WhsInventoryViewCollection(Factory);
			inventoryCollection.Load(inventoryFilter.Filter);
			AssertEquals("There should be no results.", 0, inventoryCollection.Count);
			AssertEquals("This filter should be a no result query.", true, inventoryFilter.Filter.IsNoResultQuery);
		}

		#endregion

		#endregion

		#region TestProductCategoryFilter

		public void TestProductCategoryFilter()
		{
			var warehouse = Helper.CreateWarehouse(InventoryFilterBusinessObject.Schema.Warehouse, "A");
			var client = Helper.CreateClient(InventoryFilterBusinessObject.Schema.Client);

			// product categories and products
			var categoryBeverage = Helper.CreateProductCategory("BEV", "Beverages");
			var categorySoftdrink = helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage);
			var categoryBeer = helper.CreateProductCategory("BEER", "All Beers", categoryBeverage);
			var categoryDarkBeer = helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer);

			var productCoke = Helper.CreateProduct(client, "Coke");
			var relationCoke = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationCoke.OU_OPC_Category = categorySoftdrink.PK;

			var productTea = Helper.CreateProduct(client, "Tea");
			var relationTea = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationTea.OU_OPC_Category = categorySoftdrink.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var relationVB = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationVB.OU_OPC_Category = categoryBeer.PK;

			var productGuinness = Helper.CreateProduct(client, "Guinness");
			var relationGuinness = productGuinness.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationGuinness.OU_OPC_Category = categoryDarkBeer.PK;

			// dockets
			var receiveCoke = Helper.CreateWhsReceive(client, warehouse, "RCoke");
			var inventoryCoke = Helper.CreateWhsReceiveInventoryLine(receiveCoke, productCoke, 10m);
			receiveCoke.AllocateLocationsWithMock();

			var receiveTea = Helper.CreateWhsReceive(client, warehouse, "RTea");
			var inventoryTea = Helper.CreateWhsReceiveInventoryLine(receiveTea, productTea, 10m);
			receiveTea.AllocateLocationsWithMock();

			var receiveVB = Helper.CreateWhsReceive(client, warehouse, "RVB");
			var inventoryVB = Helper.CreateWhsReceiveInventoryLine(receiveVB, productVB, 10m);
			receiveVB.AllocateLocationsWithMock();

			var receiveGuinness = Helper.CreateWhsReceive(client, warehouse, "RGuinness");
			var inventoryGuinness = Helper.CreateWhsReceiveInventoryLine(receiveGuinness, productGuinness, 10m);
			receiveGuinness.AllocateLocationsWithMock();

			Factory.Save();
			Asserter.AddToScope(inventoryCoke, inventoryTea, inventoryVB, inventoryGuinness);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ProductCategory];
			filter.IsActive = true;
			filter.Property = ZGuid.NewZGuid();
			Asserter.AssertMatches("no match.", inventoryFilter.Filter);
			// filter result
			filter.Property = categorySoftdrink.PK;
			Asserter.AssertMatches("Find all soft-drinks.", inventoryFilter.Filter, inventoryCoke, inventoryTea);

			filter.Property = categoryBeer.PK;
			Asserter.AssertMatches("Find all Beers.", inventoryFilter.Filter, inventoryVB, inventoryGuinness);

			filter.Property = categoryDarkBeer.PK;
			Asserter.AssertMatches("Find all Dark Beers.", inventoryFilter.Filter, inventoryGuinness);

			filter.Property = categoryBeverage.PK;
			Asserter.AssertMatches("Find all Beverages.", inventoryFilter.Filter, inventoryCoke, inventoryTea, inventoryVB, inventoryGuinness);

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Find all Beverages.", inventoryFilter.Filter, inventoryCoke, inventoryTea, inventoryVB, inventoryGuinness);
		}

		#endregion

		#region TestPackageToteIDFilter

		public void TestPackageToteIDFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);

			var order1Package1 = packageJob1.Packages.AddNew("PLT");
			order1Package1.KP_PackageID = "o11";

			var order1Package2 = packageJob1.Packages.AddNew("PLT");
			order1Package2.KP_PackageID = "o12";

			Helper.CreatePickNew(order1);
			var order1PickLine1 = orderLine1.PickLines[0];
			var order1PickLine2 = orderLine1.PickLines[1];
			order1PickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			order1PickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var order1Divot1 = Factory.New<PkgPackageItemDivot>();
			order1Divot1.KI_ParentID = order1PickLine1.PK;
			order1Divot1.KI_KP_Package = order1Package1.PK;
			order1Divot1.KI_PackedQty = 10m;
			order1Divot1.KI_ParentTableCode = order1PickLine1.TablePrefix;

			var order1Divot2 = Factory.New<PkgPackageItemDivot>();
			order1Divot2.KI_ParentID = order1PickLine2.PK;
			order1Divot2.KI_KP_Package = order1Package2.PK;
			order1Divot2.KI_PackedQty = 10m;
			order1Divot2.KI_ParentTableCode = order1PickLine1.TablePrefix;
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var order2Package = packageJob2.Packages.AddNew("PLT");
			order2Package.KP_PackageID = "o21";

			Helper.CreatePickNew(order2);
			var order2PickLine1 = orderLine2.PickLines[0];
			order2PickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var divot2 = Factory.New<PkgPackageItemDivot>();
			divot2.KI_ParentID = order2PickLine1.PK;
			divot2.KI_KP_Package = order2Package.PK;
			divot2.KI_PackedQty = 10m;
			divot2.KI_ParentTableCode = order2PickLine1.TablePrefix;
			Factory.Save();

			Asserter.AddToScope(order1PickLine1.Inventory, order1PickLine2.Inventory, order2PickLine1.Inventory);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PackageID];
			filter.IsActive = true;
			filter.Property = "o1";
			Asserter.AssertMatches("Start with 'o1' should return order1PickLine1.Inventory and order1PickLine2.Inventory.", inventoryFilter.Filter, order1PickLine1.Inventory, order1PickLine2.Inventory);

			filter.IsActive = true;
			filter.Property = "o11";
			filter.ComparisonOperator = "exact";
			Asserter.AssertMatches("Equals 'o11' should return only order1PickLine1.Inventory.", inventoryFilter.Filter, order1PickLine1.Inventory);

			filter.IsActive = true;
			filter.Property = "o12";
			Asserter.AssertMatches("Equals 'o12' should return only order1PickLine2.Inventory.", inventoryFilter.Filter, order1PickLine2.Inventory);

			filter.IsActive = true;
			filter.Property = "o21";
			Asserter.AssertMatches("Equals 'o21' should return only order2PickLine1.Inventory.", inventoryFilter.Filter, order2PickLine1.Inventory);

			filter.IsActive = true;
			filter.Property = "o1";
			Asserter.AssertMatches("no match for Equals 'o1'.", inventoryFilter.Filter);
		}

		public void TestPackageToteIDFilterWithReleaseCapturedAttributes()
		{
			var today = ZDate.Today;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");

			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, false);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, false);

			Helper.SetProductAttributeUse(client, product, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Three, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.ExpiryDate, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.PackingDate, true, true);

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);
			var inventory1 = receive1.Inventory[0];
			var receiveline1 = receive1.Lines[0];
			receiveline1.WE_ExpiryDate = today.AddDays(+10);
			receiveline1.WE_PackingDate = today.AddDays(-5);

			var receive2 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product, 20m);
			var inventory2 = receive2.Inventory[0];
			var receiveline2 = receive2.Lines[0];
			receiveline2.WE_ExpiryDate = today.AddDays(+10);
			receiveline2.WE_PackingDate = today.AddDays(-5);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 10m);
			order1.WD_ExternalReference = "1";
			var orderline1 = order1.Lines[0];
			orderline1.WE_ExpiryDate = today.AddDays(+10);
			orderline1.WE_PackingDate = today.AddDays(-5);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			Helper.CreatePickNew(order1);
			var pickLine1 = orderline1.PickLines[0];
			pickLine1.WZ_Units = 5m;
			pickLine1.WZ_WE_InventoryLine = receive1.Lines[0].PK;
			var pickLine2 = orderline1.PickLines.AddNew();
			pickLine2.WZ_Units = 5m;
			pickLine2.WZ_WE_InventoryLine = receive1.Lines[0].PK;
			pickLine1.WZ_ReleaseCapturedPartAttrib1 = "PA1";
			pickLine1.WZ_ReleaseCapturedPartAttrib2 = "PA2";
			pickLine1.WZ_ReleaseCapturedPartAttrib3 = "PA3";
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "PA1";
			pickLine2.WZ_ReleaseCapturedPartAttrib2 = "PA2";
			pickLine2.WZ_ReleaseCapturedPartAttrib3 = "PA3";

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;

			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 20m);
			order2.WD_ExternalReference = "2";
			var orderline2 = order2.Lines[0];
			orderline2.WE_ExpiryDate = today.AddDays(+10);
			orderline2.WE_PackingDate = today.AddDays(-5);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var orderPackage3 = packageJob2.Packages.AddNew("PLT", "o21");
			var orderPackage4 = packageJob2.Packages.AddNew("PLT", "o22");

			Helper.CreatePickNew(order2);
			var pickLine3 = orderline2.PickLines[0];
			pickLine3.WZ_Units = 10m;
			pickLine3.WZ_WE_InventoryLine = receive2.Lines[0].PK;
			var pickLine4 = orderline2.PickLines.AddNew();
			pickLine4.WZ_Units = 10m;
			pickLine4.WZ_WE_InventoryLine = receive2.Lines[0].PK;
			pickLine3.WZ_ReleaseCapturedPartAttrib1 = "PA4";
			pickLine3.WZ_ReleaseCapturedPartAttrib2 = "PA5";
			pickLine3.WZ_ReleaseCapturedPartAttrib3 = "PA6";
			pickLine4.WZ_ReleaseCapturedPartAttrib1 = "PA4";
			pickLine4.WZ_ReleaseCapturedPartAttrib2 = "PA5";
			pickLine4.WZ_ReleaseCapturedPartAttrib3 = "PA6";

			var divot3 = orderPackage3.PackedItemDivots.AddNew();
			divot3.KI_ParentID = pickLine3.PK;
			divot3.KI_PackedQty = pickLine3.WZ_Units;
			divot3.KI_ParentTableCode = pickLine3.TablePrefix;

			var divot4 = orderPackage4.PackedItemDivots.AddNew();
			divot4.KI_ParentID = pickLine4.PK;
			divot4.KI_PackedQty = pickLine4.WZ_Units;
			divot4.KI_ParentTableCode = pickLine4.TablePrefix;
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PackageID];
			filter.IsActive = true;
			filter.Property = "o";
			Asserter.AssertMatches("Start with 'o' should return inventory1 and inventory2.", inventoryFilter.Filter, inventory1, inventory2);
			filter.IsActive = true;
			filter.Property = "o1";
			Asserter.AssertMatches("Start with 'o1' should return only inventory1.", inventoryFilter.Filter, inventory1);
			filter.IsActive = true;
			filter.Property = "o2";
			Asserter.AssertMatches("Start with 'o2' should return only inventory2.", inventoryFilter.Filter, inventory2);
			filter.IsActive = true;
			filter.Property = "o11";
			filter.ComparisonOperator = "exact";
			Asserter.AssertMatches("Equals 'o11' should return only inventory1.", inventoryFilter.Filter, inventory1);
			filter.IsActive = true;
			filter.Property = "o12";
			Asserter.AssertMatches("Equals 'o12' should return only inventory1.", inventoryFilter.Filter, inventory1);
			filter.IsActive = true;
			filter.Property = "o1";
			Asserter.AssertMatches("no match for Equals 'o1'.", inventoryFilter.Filter);
		}

		public void TestPackageToteIDFilterWithReleaseCapturedAttributes_MultiplePackagesOnOneOrder()
		{
			var today = ZDate.Today;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.SetClientAttributeType(client, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m, finalise: false);
			var receiveline1 = receive1.Lines[0];
			var receiveline2 = Helper.CreateWhsReceiveLine(receive1, product, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 20m);
			order1.WD_ExternalReference = "1";
			var orderline1 = order1.Lines[0];

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			Helper.CreatePickNew(order1);
			var pickLine1 = orderline1.PickLines[0];
			var pickLine2 = orderline1.PickLines[1];
			pickLine1.WZ_ReleaseCapturedPartAttrib1 = "1";
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "2";

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;

			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;
			Factory.Save();

			var inventory1 = pickLine1.Inventory;
			var inventory2 = pickLine2.Inventory;
			Asserter.AddToScope(inventory1, inventory2);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PackageID];
			filter.IsActive = true;
			filter.Property = "o";
			Asserter.AssertMatches("Start with 'o' should return inventory1 and inventory2.", inventoryFilter.Filter, inventory1, inventory2);

			filter.IsActive = true;
			filter.Property = "o11";
			filter.ComparisonOperator = "exact";
			Asserter.AssertMatches("Equals 'o11' should return only inventory1.", inventoryFilter.Filter, inventory1);

			filter.IsActive = true;
			filter.Property = "o12";
			Asserter.AssertMatches("Equals 'o12' should return only inventory2.", inventoryFilter.Filter, inventory2);
		}

		#endregion

		#region TestCommittedTypes

		public void TestCommittedTypes()
		{
			var inventoryFilter = (InventoryFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals(3, inventoryFilter.CommittedTypes.Count);
		}

		#endregion

		#region TestCommittedStockFilter

		#region TestCommittedStockFilter

		public void TestCommittedStockFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("warehouse1", "A1", 3, 2);
			var whs2 = Helper.CreateWarehouse("warehouse2", "A2", 3, 2);
			var client1 = Helper.CreateClient("client1", "client1");
			var client2 = Helper.CreateClient("client2", "client2");
			var product1 = Helper.CreateProduct(client1, "product1");
			var product2 = Helper.CreateProduct(client1, "product2");
			var product3 = Helper.CreateProduct(client2, "product3");
			var product4 = Helper.CreateProduct(client2, "product4");

			var inventory1 = Helper.CreateWhsReceiveWithInventory(client1, whs1, "r1", product1, 10m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(client1, whs2, "r2", product1, 10m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(client1, whs1, "r3", product2, 10m).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(client2, whs1, "r4", product3, 10m).Inventory[0];
			var inventory5 = Helper.CreateWhsReceiveWithInventory(client2, whs2, "r5", product3, 10m).Inventory[0];
			var inventory6 = Helper.CreateWhsReceiveWithInventory(client2, whs1, "r6", product4, 10m).Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(client1.PK, whs1.PK, "O1", Notify);
			Helper.CreateWhsOrderLine(order, product1, 5);
			Helper.CreateWhsOrderLine(order, product2, 5);
			Helper.CreatePickNew(order);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Committed];
			filter.IsActive = true;
			filter.Property = "ALL";
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4, inventory5, inventory6);

			filter.Property = "COM";
			Asserter.AssertMatches("2 Inventory committed to Order", inventoryFilter.Filter, inventory1, inventory3);

			filter.Property = "UNC";
			Asserter.AssertMatches("Uncommitted, include inventory from unfinalised receives.", inventoryFilter.Filter, inventory2, inventory4, inventory5, inventory6);
		}

		#endregion

		#region TestCommittedStockFilter_Transfers

		public void TestCommittedStockFilter_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locationA1, "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, part3, 150m, locationA1, "");
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_Picked = Helper.CreateWhsTransferLine(transfer, data.Part2, 15m, "A-1", "A-2");
			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, part3, 20m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit stock.
			transferLine_Picked.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			transferLine_Finalised.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine_Finalised);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, transferLine_Finalised.Inventory[0]);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Committed];
			filter.IsActive = true;
			filter.Property = "ALL";
			Asserter.AssertMatches("Should return all inventories.", inventoryFilter.Filter, inventory1, inventory2, inventory3, transferLine_Finalised.Inventory[0]);

			filter.Property = "COM";
			Asserter.AssertMatches("Should return inventory with committed stock.", inventoryFilter.Filter, inventory1);

			filter.Property = "UNC";
			Asserter.AssertMatches("Should return inventories with no stock committed.", inventoryFilter.Filter, inventory2, inventory3, transferLine_Finalised.Inventory[0]);
		}

		#endregion

		#region TestCommittedStockFilter_Adjustments

		public void TestCommittedStockFilter_Adjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var locationA1 = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locationA1, "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, part3, 150m, locationA1, "");
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var adjustment1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, -10m, locationA1);
			var adjustmentLine_Picked = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part2, -15m, locationA1);
			adjustment1.RunPreSaveValidation(); // to commit stock.
			adjustmentLine_Picked.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today;

			var adjustment2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment2, part3, -20m, locationA1);
			adjustment2.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment2);
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Committed];
			filter.IsActive = true;
			filter.Property = "ALL";
			Asserter.AssertMatches("Should return all inventories.", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = "COM";
			Asserter.AssertMatches("Should return inventory with committed stock.", inventoryFilter.Filter, inventory1);

			filter.Property = "UNC";
			Asserter.AssertMatches("Should return inventories with no stock committed.", inventoryFilter.Filter, inventory2, inventory3);
		}

		#endregion

		#region TestCommittedStockFilter_Picked

		public void TestCommittedStockFilter_Picked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var locationA1 = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1, "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locationA1, "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, part3, 150m, locationA1, "");
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine_Picked = Helper.CreateWhsOrderLine(order1, data.Part2, 15m);
			Helper.CreatePickNew(order1);
			orderLine_Picked.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
			Helper.CreateWhsOrderLine(order2, part3, 20m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order2);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick2);
			Factory.Save();

			var inventory4 = orderLine_Picked.PickLines.Single().InventoryLine.Inventory[0];
			var inventoryFilter = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Committed];
			filter.IsActive = true;
			filter.Property = "ALL";
			Asserter.AssertMatches("Should return all inventories.", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);

			filter.Property = "COM";
			Asserter.AssertMatches("Should return inventory with committed stock.", inventoryFilter.Filter, inventory1, inventory4);

			filter.Property = "UNC";
			Asserter.AssertMatches("Should return inventories with no stock committed.", inventoryFilter.Filter, inventory2, inventory3);
		}

		#endregion

		#endregion

		#region TestPartAttribute1Filter

		public void TestPartAttribute1Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "PR1");
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 2m).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part3, 4m).Inventory[0];
			inventory1.WI_PartAttrib1 = "PA1";
			inventory2.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib3 = "PA3";
			inventory4.WI_PartAttrib1 = "PA1";
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			var filter = (ModuleTextFilter)inventoryFilter["Part Attribute 1"];
			filter.IsActive = true;
			filter.Property = "PA1";
			Asserter.AssertMatches("Should return two inventories.", inventoryFilter.Filter, inventory1, inventory4);
		}

		#endregion

		#region TestSerialNumberFilter

		public void TestSerialNumberFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
			receiveLine1.WE_SerialNumber = "SN1";
			var inventory1 = receiveLine1.Inventory[0];
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 1m);
			receiveLine2.WE_SerialNumber = "SN2";
			var inventory2 = receiveLine2.Inventory[0];
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 1m);
			receiveLine3.WE_SerialNumber = "SN3";
			var inventory3 = receiveLine3.Inventory[0];
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			var filter = (ModuleTextFilter)inventoryFilter["Serial Number"];
			filter.IsActive = true;
			filter.Property = "SN1";
			Asserter.AssertMatches("Should return 1 inventory.", inventoryFilter.Filter, inventory1);
		}

		#endregion

		#region TestSerialNumberFilterPivot_SerialNumber

		public void TestSerialNumberFilterPivot_SerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
				var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m);
				var pivot1 = receiveLine1.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var inventory1 = receiveLine1.Inventory[0];
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m);
				var pivot2 = receiveLine2.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				var inventory2 = receiveLine2.Inventory[0];
				var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 1m);
				var pivot3 = receiveLine3.SerialNumbers.AddNew();
				pivot3.SerialNumberValue = "SX3";
				var inventory3 = receiveLine3.Inventory[0];
				var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 1m);
				var inventory4 = receive4.Inventory[0];
				Factory.Save();
				Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

				var inventoryFilter = GetNewFilterStripBusinessObject();

				var filter = (ModuleTextFilter)inventoryFilter["Serial Number"];
				filter.IsActive = true;
				filter.Property = "SN1";
				Asserter.AssertMatches("Should return 1 inventory.", inventoryFilter.Filter, inventory1);

				filter.Property = "SN2";
				Asserter.AssertMatches("Should return 1 inventory.", inventoryFilter.Filter, inventory2);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				Asserter.AssertMatches("Should return all inventories with serial numbers.", inventoryFilter.Filter, inventory1, inventory2, inventory3);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				Asserter.AssertMatches("Inventories with serial numbers should be excluded.", inventoryFilter.Filter, inventory4);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				filter.Property = "SN";
				Asserter.AssertMatches("Inventories with serial numbers should be excluded.", inventoryFilter.Filter, inventory3, inventory4);
			}
		}

		#endregion

		#region TestPartAttribute2Filter

		public void TestPartAttribute2Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "PR1");
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 2m).Inventory[0];
			inventory1.WI_PartAttrib1 = "PA1";
			inventory2.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib3 = "PA3";
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter["Part Attribute 2"];
			filter.IsActive = true;
			filter.Property = "PA2";
			Asserter.AssertMatches("Should return one inventory.", inventoryFilter.Filter, inventory2);
		}

		#endregion

		#region TestPartAttribute3Filter

		public void TestPartAttribute3Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "PR1");
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 2m).Inventory[0];
			inventory1.WI_PartAttrib1 = "PA1";
			inventory2.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib3 = "PA3";
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter["Part Attribute 3"];
			filter.IsActive = true;
			filter.Property = "PA3";
			Asserter.AssertMatches("Should return one inventory.", inventoryFilter.Filter, inventory3);
		}

		#endregion

		#region TestAttributeCombination

		public void TestAttributeCombination()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "PR1");
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 2m).Inventory[0];
			inventory1.WI_PartAttrib1 = "PA1";
			inventory1.WI_PartAttrib3 = "PA3";
			inventory2.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib1 = "PA1";
			inventory3.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib3 = "PA3";
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)inventoryFilter["Part Attribute 1"];
			var filter2 = (ModuleTextFilter)inventoryFilter["Part Attribute 2"];
			var filter3 = (ModuleTextFilter)inventoryFilter["Part Attribute 3"];
			filter1.IsActive = true;
			filter2.IsActive = true;
			filter3.IsActive = true;

			filter1.Property = "PA1";
			filter3.Property = "PA3";
			Asserter.AssertMatches("Should exclude inventory 2", inventoryFilter.Filter, inventory1, inventory3);

			filter1.Property = "";
			filter2.Property = "PA2";
			filter3.Property = "";
			Asserter.AssertMatches("Should only have inventory 2", inventoryFilter.Filter, inventory2, inventory3);
		}

		#endregion

		#region TestCustomAttributesFilter

		public void TestCustomAttributesFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "PR1");
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 2m).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 2m).Inventory[0];
			var inventory5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 2m).Inventory[0];
			var inventory6 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 2m).Inventory[0];
			inventory1.InDocketLine.WE_CustomAttrib1 = "CA11";
			inventory2.InDocketLine.WE_CustomAttrib4 = "CA41";
			inventory3.InDocketLine.WE_CustomAttrib4 = "CA411";
			inventory4.InDocketLine.WE_CustomAttrib1 = "CA112";
			inventory5.InDocketLine.WE_CustomAttrib4 = "CA41";
			inventory6.InDocketLine.WE_CustomAttrib4 = "CA412";
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filterCustomAttrib1 = (ModuleTextFilter)inventoryFilter["WhsDocketLine.CustomAttrib1"];
			filterCustomAttrib1.IsActive = true;
			filterCustomAttrib1.Property = "CA11";
			Asserter.AssertMatches("Should return two inventories.", inventoryFilter.Filter, inventory1, inventory4);

			filterCustomAttrib1.Property = "CA112";
			Asserter.AssertMatches("Should return one inventory.", inventoryFilter.Filter, inventory4);

			var filterCustomAttrib4 = (ModuleTextFilter)inventoryFilter["WhsDocketLine.CustomAttrib4"];
			filterCustomAttrib4.IsActive = true;
			filterCustomAttrib4.Property = "CA41";
			Asserter.AssertMatches("Both filters are apply should not find a match.", inventoryFilter.Filter);

			inventoryFilter = GetNewFilterStripBusinessObject();
			filterCustomAttrib4 = (ModuleTextFilter)inventoryFilter["WhsDocketLine.CustomAttrib4"];
			filterCustomAttrib4.IsActive = true;
			filterCustomAttrib4.Property = "CA41";
			Asserter.AssertMatches("should find 4 inventories.", inventoryFilter.Filter, inventory2, inventory3, inventory5, inventory6);

			filterCustomAttrib4.Property = "CA412";
			Asserter.AssertMatches("Should return one inventory.", inventoryFilter.Filter, inventory6);

			AssertEquals(FilterCategories.AttributeSearch, inventoryFilter["WhsDocket.CustomAttrib3"].Category);
		}

		#endregion

		#region TestProductStyleFilters()

		public void TestProductStyleFilters()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var nikeStyle = Helper.CreateProductStyle("nikeStyle", "Test Style", data.Org1.PK);

			var colourRed = Helper.CreateProductStyleColour(nikeStyle, "130", "RED");
			var colourGreen = Helper.CreateProductStyleColour(nikeStyle, "140", "GREEN");

			var size5 = Helper.CreateProductStyleSize(nikeStyle, 1, "05");
			var size10 = Helper.CreateProductStyleSize(nikeStyle, 2, "10");

			var addidasStyle = Helper.CreateProductStyle("addidasStyle", "Test Style", data.Org1.PK);

			var colourYellow = Helper.CreateProductStyleColour(addidasStyle, "120", "YELLOW");
			var size2 = Helper.CreateProductStyleSize(addidasStyle, 1, "02");

			var product1 = Helper.CreateProduct(data.Org1, "PRODUCT1");
			var whsProduct1 = WhsProduct.GetWhsProduct(product1);
			whsProduct1.ProductStylePK = nikeStyle.PK;
			whsProduct1.ProductStyleColourPK = colourRed.PK;
			whsProduct1.ProductStyleSizePK = size5.PK;

			var product2 = Helper.CreateProduct(data.Org1, "PRODUCT2");
			var whsProduct2 = WhsProduct.GetWhsProduct(product2);
			whsProduct2.ProductStylePK = nikeStyle.PK;
			whsProduct2.ProductStyleColourPK = colourGreen.PK;
			whsProduct2.ProductStyleSizePK = size10.PK;

			var product3 = Helper.CreateProduct(data.Org1, "PRODUCT3");
			var whsProduct3 = WhsProduct.GetWhsProduct(product3);
			whsProduct3.ProductStylePK = addidasStyle.PK;
			whsProduct3.ProductStyleColourPK = colourYellow.PK;
			whsProduct3.ProductStyleSizePK = size2.PK;

			var productWithNoStyle = Helper.CreateProduct(data.Org1, "PRODUCT4");

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "111", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1.PK, 6);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, product2.PK, 5);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, product3.PK, 5);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, productWithNoStyle.PK, 5);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			Asserter.AssertMatches("No Product Style Selected", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);
			var filterProductStyle = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ProductStyle];
			var filterColourCode = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ColourCode];
			var filterStyleSize = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.StyleSize];
			filterProductStyle.IsActive = true;
			filterColourCode.IsActive = true;
			filterStyleSize.IsActive = true;

			filterProductStyle.Property = nikeStyle.PK;
			Asserter.AssertMatches("Filtering Nike Product Style Only, should show lines 1 and 2 with Nike Style.", inventoryFilter.Filter, inventory1, inventory2);

			filterProductStyle.Property = nikeStyle.PK;
			filterColourCode.Property = colourRed.WSC_Code;
			filterStyleSize.Property = size10.WSZ_Size;
			Asserter.AssertMatches("Filtering Nike Product Style, colour red, size 10, should show no lines", inventoryFilter.Filter);

			filterProductStyle.Property = nikeStyle.PK;
			filterColourCode.Property = colourRed.WSC_Code;
			filterStyleSize.Property = size5.WSZ_Size;
			Asserter.AssertMatches("Filtering Nike Product Style, colour red, size 5, should show line 1", inventoryFilter.Filter, inventory1);

			filterProductStyle.Property = nikeStyle.PK;
			filterColourCode.Property = colourGreen.WSC_Code;
			filterStyleSize.Property = size10.WSZ_Size;
			Asserter.AssertMatches("Filtering Nike Product Style, colour green, size 10, should show line 2", inventoryFilter.Filter, inventory2);

			filterProductStyle.Property = addidasStyle.PK;
			filterColourCode.Property = colourYellow.WSC_Code;
			filterStyleSize.Property = size2.WSZ_Size;
			Asserter.AssertMatches("Filtering Addidas Product Style, colour yellow, size 2, should show line 3", inventoryFilter.Filter, inventory3);

			filterProductStyle.Property = ZGuid.Invalid;
			Asserter.AssertMatches("Invalid Product Style show all lines", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);
		}

		public void TestProductStyleFilters_Classification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var style1 = Helper.CreateProductStyle("style1", "Style1", data.Org1.PK);
			var colourYellow1 = Helper.CreateProductStyleColour(style1, "YEL", "YELLOW");
			var colourPurple1 = Helper.CreateProductStyleColour(style1, "PUR", "PURPLE");
			var size51 = Helper.CreateProductStyleSize(style1, 1, "05");
			var size101 = Helper.CreateProductStyleSize(style1, 2, "10");
			var classificationM1 = Helper.CreateProductStyleClassification(style1, "M", "MALE");
			var classificationF1 = Helper.CreateProductStyleClassification(style1, "F", "FEMALE");

			var style2 = Helper.CreateProductStyle("style2", "Style2", data.Org1.PK);
			var colourYellow2 = Helper.CreateProductStyleColour(style2, "YEL", "YELLOW");
			var colourPurple2 = Helper.CreateProductStyleColour(style2, "PUR", "PURPLE");
			var size52 = Helper.CreateProductStyleSize(style2, 1, "05");
			var size102 = Helper.CreateProductStyleSize(style2, 2, "10");
			var classificationM2 = Helper.CreateProductStyleClassification(style2, "M", "MALE");
			var classificationF2 = Helper.CreateProductStyleClassification(style2, "F", "FEMALE");

			var product1 = Helper.CreateProduct(data.Org1, "PRODUCT1");
			var whsProduct1 = WhsProduct.GetWhsProduct(product1);
			whsProduct1.ProductStylePK = style1.PK;
			whsProduct1.ProductStyleColourPK = colourYellow1.PK;
			whsProduct1.ProductStyleSizePK = size51.PK;
			whsProduct1.ProductStyleClassificationPK = classificationM1.PK;

			var product2 = Helper.CreateProduct(data.Org1, "PRODUCT2");
			var whsProduct2 = WhsProduct.GetWhsProduct(product2);
			whsProduct2.ProductStylePK = style1.PK;
			whsProduct2.ProductStyleColourPK = colourPurple1.PK;
			whsProduct2.ProductStyleSizePK = size101.PK;
			whsProduct2.ProductStyleClassificationPK = classificationM1.PK;

			var product3 = Helper.CreateProduct(data.Org1, "PRODUCT3");
			var whsProduct3 = WhsProduct.GetWhsProduct(product3);
			whsProduct3.ProductStylePK = style2.PK;
			whsProduct3.ProductStyleColourPK = colourYellow2.PK;
			whsProduct3.ProductStyleSizePK = size52.PK;
			whsProduct3.ProductStyleClassificationPK = classificationM2.PK;

			var product4 = Helper.CreateProduct(data.Org1, "PRODUCT4");
			var whsProduct4 = WhsProduct.GetWhsProduct(product4);
			whsProduct4.ProductStylePK = style2.PK;
			whsProduct4.ProductStyleColourPK = colourPurple2.PK;
			whsProduct4.ProductStyleSizePK = size102.PK;
			whsProduct4.ProductStyleClassificationPK = classificationF2.PK;

			var productWithNoStyle = Helper.CreateProduct(data.Org1, "PRODUCT5");

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "111", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1.PK, 6);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, product2.PK, 5);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, product3.PK, 5);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, product4.PK, 5);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive1, productWithNoStyle.PK, 5);

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4, inventory5);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			Asserter.AssertMatches("No Product Style Selected", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4, inventory5);
			var filterProductStyle = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ProductStyle];
			var filterColourCode = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ColourCode];
			var filterClassificationCode = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ClassificationCode];
			var filterStyleSize = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.StyleSize];
			filterProductStyle.IsActive = true;
			filterColourCode.IsActive = true;
			filterClassificationCode.IsActive = true;
			filterStyleSize.IsActive = true;

			filterProductStyle.Property = style1.PK;
			filterClassificationCode.Property = classificationM1.WSS_Code;
			Asserter.AssertMatches("Filtering Style1, classification male, should show lines 1 and 2", inventoryFilter.Filter, inventory1, inventory2);

			filterProductStyle.Property = style1.PK;
			filterClassificationCode.Property = classificationF1.WSS_Code;
			Asserter.AssertMatches("Filtering Style1, classification male, should show no lines", inventoryFilter.Filter);

			filterProductStyle.Property = style1.PK;
			filterColourCode.Property = colourYellow1.WSC_Code;
			filterClassificationCode.Property = classificationM1.WSS_Code;
			Asserter.AssertMatches("Filtering Style1, colour yellow, classification male, should show lines 1", inventoryFilter.Filter, inventory1);

			filterProductStyle.Property = style2.PK;
			Asserter.AssertMatches("Filtering Style2, should show lines 3 and 4", inventoryFilter.Filter, inventory3, inventory4);

			filterProductStyle.Property = ZGuid.Invalid;
			Asserter.AssertMatches("Invalid Product Style show all lines", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4, inventory5);
		}

		public void TestProductStyleFilters_SizeOrderedBySequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var nikeStyle = Helper.CreateProductStyle("nikeStyle", "Test Style", data.Org1.PK);

			var size10 = Helper.CreateProductStyleSize(nikeStyle, 4, "Ten");
			var size5 = Helper.CreateProductStyleSize(nikeStyle, 2, "Five");
			var size1 = Helper.CreateProductStyleSize(nikeStyle, 1, "One");
			var size7 = Helper.CreateProductStyleSize(nikeStyle, 3, "Seven");

			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filterProductStyle = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ProductStyle];
			filterProductStyle.IsActive = true;
			filterProductStyle.Property = nikeStyle.PK;

			var filterStyleSize = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.StyleSize];
			filterStyleSize.IsActive = true;
			AssertArrayEqualsByElements("Collection should be sorted by sequence.", new ZByte[] { 1, 2, 3, 4 }, filterStyleSize.List.ToList<WhsProductStyleSize>().Select(s => s.WSZ_Sequence).ToArray());
		}

		#endregion

		#region TestArrivalDateFilter

		public void TestArrivalDateFilter()
		{
			var today = ZDateTime.Today;

			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 1);
			receive1.WD_ArrivalDate = today.ToOffset();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 1);
			receive2.WD_ArrivalDate = today.AddDays(-7).ToOffset();
			Factory.Save();
			Asserter.AddToScope(receive1.Inventory[0], receive2.Inventory[0]);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ArrivalDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today;
			filter.Property2 = today;
			Asserter.AssertMatches(InventoryFilterBusinessObject.Schema.ArrivalDate, filter, new[] { receive1.Lines[0].Inventory[0] });

			filter.Property1 = today.AddDays(-7);
			filter.Property2 = today.AddDays(-7);
			Asserter.AssertMatches(InventoryFilterBusinessObject.Schema.ArrivalDate, filter, receive2.Lines[0].Inventory[0]);
		}

		#endregion

		#region TestExpiryDateFilter

		public void TestExpiryDateFilter()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			inventory1.WI_ExpiryDate = today;
			inventory2.WI_ExpiryDate = today.AddDays(1);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			Factory.Save();
			AssertEquals(true, receive1.IsFinalised);
			Asserter.AddToScope(inventory1, inventory2);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ExpiryDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.ToZDateTime();
			filter.Property2 = today.ToZDateTime();
			Asserter.AssertMatches("Should find one inventory", inventoryFilter.Filter, inventory1);

			filter.Property1 = today.AddDays(1).ToZDateTime();
			filter.Property2 = today.AddDays(1).ToZDateTime();
			Asserter.AssertMatches("Should find one inventory", inventoryFilter.Filter, inventory2);
		}

		#endregion

		#region TestIsExpiredFilter

		[TestDate(2022, 02, 09)]
		public void TestIsExpiredFilter()
		{
			var today = ZDate.Today;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			inventory1.WI_ExpiryDate = today;
			inventory2.WI_ExpiryDate = today.AddDays(1);
			inventory3.WI_ExpiryDate = today.AddDays(-1);
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.IsExpired];
			filter.IsActive = true;
			filter.Property = InventoryFilterBusinessObject.Schema.IsExpiredTypeCodes.All;
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);

			filter.Property = InventoryFilterBusinessObject.Schema.IsExpiredTypeCodes.Expired;
			Asserter.AssertMatches("Expired", inventoryFilter.Filter, inventory1, inventory3);

			filter.Property = InventoryFilterBusinessObject.Schema.IsExpiredTypeCodes.NotExpired;
			Asserter.AssertMatches("Not Expired", inventoryFilter.Filter, inventory2, inventory4);
		}

		#endregion

		#region TestPackingDateFilter

		public void TestPackingDateFilter()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			inventory1.WI_PackingDate = today;
			inventory2.WI_PackingDate = today.AddDays(1);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PackingDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.ToZDateTime();
			filter.Property2 = today.ToZDateTime();
			Asserter.AssertMatches("Should find one inventory", inventoryFilter.Filter, inventory1);

			filter.Property1 = today.AddDays(1).ToZDateTime();
			filter.Property2 = today.AddDays(1).ToZDateTime();
			Asserter.AssertMatches("Should find one inventory", inventoryFilter.Filter, inventory2);
		}

		#endregion

		#region TestPalletIDFilter

		public void TestPalletIDFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 16.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 12.0m);
			inventory1.WI_PalletID = "A0001";
			inventory2.WI_PalletID = "AB002";
			inventory3.WI_PalletID = "CC003";
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PalletID];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("Should find all inventories", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = "AB";
			Asserter.AssertMatches("Should find one inventory", inventoryFilter.Filter, inventory2);

			filter.Property = "D";
			Asserter.AssertMatches("no match", inventoryFilter.Filter);
		}

		#endregion

		#region TestAddAttributeFilterWhenAlreadyExists

		public void TestAddAttributeFilterWhenAlreadyExists()
		{
			var webClient = Helper.CreateClient("WEB", "WEB");
			var inventoryWebFilter = new InventoryFilterBusinessObject(webClient);

			Helper.SetClientAttributeType(webClient, AttributeNumber.One, true, InventoryFilterBusinessObject.Schema.ExpiryDate);
			Helper.SetClientAttributeType(webClient, AttributeNumber.Two, true, InventoryFilterBusinessObject.Schema.ArrivalDate);
			Helper.SetClientAttributeType(webClient, AttributeNumber.Three, true, InventoryFilterBusinessObject.Schema.PalletID);
			Globals.IsWeb = true;

			var filter = (ModuleDateFilter)inventoryWebFilter[InventoryFilterBusinessObject.Schema.ExpiryDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddMonths(2);
			filter.Property2 = ZDateTime.Today.AddMonths(2);
			new WhsInventoryViewCollection(Factory).Load(inventoryWebFilter.Filter);
			AssertNoExceptionThrown(() =>
			{
				var poke = inventoryWebFilter["Expiry Date - PA1"];
				poke = inventoryWebFilter["Arrival Date - PA2"];
				poke = inventoryWebFilter["Pallet ID - PA3"];
			});
		}

		#endregion

		#region TestBondedEntryKeyFilter

		public void TestBondedEntryKeyFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10.0m, "BEK1-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 16.0m, "BEK1-2");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 12.0m, "AEK2-1");
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.CustomsEntryKey];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = "BEK1-1";
			Asserter.AssertMatches("Only one inventory is match.", inventoryFilter.Filter, inventory1);
		}

		#endregion

		#region TestNumberRangeFilter

		public void TestNumberRangeFilter()
		{
			var inventoryFilter = GetNewFilterStripBusinessObject();

			var filter = (ModuleNumberRangeFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.TouchesTillStocktake];
			filter.Property1 = 99999999999m;
			filter.Property2 = 99999999999m;
			filter.Validation.ValidateAll();

			AssertHasError(filter.Property1Info, "Please enter a value less than or equal to 2,147,483,647.");
			AssertHasError(filter.Property2Info, "Please enter a value less than or equal to 2,147,483,647.");
		}

		#endregion

		#region TestAttributesCanBeSameAsReservedFilterName_CustomLabels

		public void TestAttributesCanBeSameAsReservedFilterName_CustomLabels()
		{
			var client = Helper.CreateClient();
			Factory.Save();
			var ca = client.CustomLabels.AddNew();
			ca.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute1;
			foreach (var propinfo in typeof(InventoryFilterBusinessObject.Schema).GetFields())
			{
				ca.OT_Caption = propinfo.Name;
				Factory.Save();
				TestAttributesCanBeSameAsReservedFilterNameCore(client, propinfo.Name + " - CA");
			}
		}

		public void TestAttributesCanBeSameAsReservedFilterName_Attribute1()
		{
			var client = Helper.CreateClient();
			Factory.Save();
			foreach (var propinfo in typeof(InventoryFilterBusinessObject.Schema).GetFields())
			{
				client.MiscServ.OM_IMPartAttrib1Name = propinfo.Name;
				TestAttributesCanBeSameAsReservedFilterNameCore(client, propinfo.Name + " - PA1");
			}
		}

		public void TestAttributesCanBeSameAsReservedFilterName_Attribute2()
		{
			var client = Helper.CreateClient();
			Factory.Save();
			foreach (var propinfo in typeof(InventoryFilterBusinessObject.Schema).GetFields())
			{
				client.MiscServ.OM_IMPartAttrib2Name = propinfo.Name;
				TestAttributesCanBeSameAsReservedFilterNameCore(client, propinfo.Name + " - PA2");
			}
		}

		public void TestAttributesCanBeSameAsReservedFilterName_Attribute3()
		{
			var client = Helper.CreateClient();
			Factory.Save();
			foreach (var propinfo in typeof(InventoryFilterBusinessObject.Schema).GetFields())
			{
				client.MiscServ.OM_IMPartAttrib3Name = propinfo.Name;
				TestAttributesCanBeSameAsReservedFilterNameCore(client, propinfo.Name + " - PA3");
			}
		}

		void TestAttributesCanBeSameAsReservedFilterNameCore(OrgHeader client, string expected)
		{
			Globals.IsWeb = true;
			try
			{
				var inventoryFilter = GetNewFilterStripBusinessObject(client);
				inventoryFilter = GetNewFilterStripBusinessObject(client);

				AssertNotNull($"{expected}`s filter for attribute is here", inventoryFilter[expected]);
				AssertEquals("Filter category", FilterCategories.AttributeSearch, inventoryFilter[expected].Category);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region TestFilterCommodityCode

		public void TestFilterCommodityCode()
		{
			var whs1 = Helper.CreateWarehouse("1", "A1", 3, 2);
			var whs2 = Helper.CreateWarehouse("2", "A2", 3, 2);
			var client1 = Helper.CreateClient("TEST1", "TEST1");
			var client2 = Helper.CreateClient("TEST2", "TEST2");
			var product1 = Helper.CreateProduct(client1, "1");
			var product2 = Helper.CreateProduct(client2, "2");
			var product3 = Helper.CreateProduct(client1, "3");
			var product4 = Helper.CreateProduct(client2, "4");
			product1.OP_RH_NKCommodityCode = "1";
			product2.OP_RH_NKCommodityCode = "2";
			product3.OP_RH_NKCommodityCode = "3";
			product4.OP_RH_NKCommodityCode = "4";
			Factory.Save();

			var inventory1 = Helper.CreateWhsReceiveWithInventory(client1, whs1, "INW1", product1, 2m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(client2, whs1, "INW2", product2, 3m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(client1, whs2, "INW3", product3, 4m).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(client2, whs2, "INW4", product4, 5m, false, false).Inventory[0];
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Commodity];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);

			filter.Property = "1";
			Asserter.AssertMatches("Should find one inventory.", inventoryFilter.Filter, inventory1);

			filter.Property = "4";
			Asserter.AssertMatches("Should find one inventory.", inventoryFilter.Filter, inventory4);

			filter.Property = "A";
			Asserter.AssertMatches("no match.", inventoryFilter.Filter);
		}

		#endregion

		#region Location Filters

		#region TestLocationAreaNameFilter

		public void TestLocationAreaNameFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW2", data.Part1, 3m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW3", data.Part1, 4m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW4", data.Part1, 5m, false, false);

			var area1 = Helper.CreateArea(data.Whs1, "AREA1", AreaTypes.Codes.FreeStore);
			var area2 = Helper.CreateArea(data.Whs1, "AREA2", AreaTypes.Codes.FreeStore);
			var area3 = Helper.CreateArea(data.Whs1, "STAGING3", AreaTypes.Codes.FreeStore);
			receive1.Inventory[0].Location.WLV_WA_PickingArea = area1.PK;
			receive2.Inventory[0].Location.WLV_WA_PickingArea = area2.PK;
			receive3.Inventory[0].Location.WLV_WA_PickingArea = area3.PK;

			Asserter.AddToScope(receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0], receive4.Inventory[0]);
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PickAreaName];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("Should match 4 Inventorys", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0], receive4.Inventory[0]);

			filter.Property = "AR";
			Asserter.AssertMatches("Should match Inventory with Area Name Starting With AR", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0]);

			filter.Property = "AREA1";
			Asserter.AssertMatches("Should match Inventory with Area Name Starting With AREA1", inventoryFilter.Filter, receive1.Inventory[0]);

			filter.Property = "AREA2";
			filter.ComparisonOperator = "exact";
			Asserter.AssertMatches("Should match Inventory with Area Name exact equal AREA2", inventoryFilter.Filter, receive2.Inventory[0]);

			filter.Property = "STAGING3";
			filter.ComparisonOperator = "not equal";
			Asserter.AssertMatches("Should match Inventory with Area Name  not equal STAGING3", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive4.Inventory[0]);

			filter.Property = "AREA";
			filter.ComparisonOperator = "contains";
			Asserter.AssertMatches("Should match Inventory with Area Name contains AREA", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0]);

			filter.Property = "ST";
			filter.ComparisonOperator = "not starting";
			Asserter.AssertMatches("Should match Inventory with Area Name not starting ST", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive4.Inventory[0]);

			filter.Property = "REA";
			filter.ComparisonOperator = "not contain";
			Asserter.AssertMatches("Should match Inventory with Area Name not contain REA", inventoryFilter.Filter, receive3.Inventory[0], receive4.Inventory[0]);

			filter.Property = "";
			filter.ComparisonOperator = "is blank";
			Asserter.AssertMatches("Should match Inventory with Area Name is blank", inventoryFilter.Filter, receive4.Inventory[0]);

			filter.Property = "";
			filter.ComparisonOperator = "is not blank";
			Asserter.AssertMatches("Should match Inventory with Area Name is not blank", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0]);
		}

		#endregion

		#region TestLocationAreaNameFilter

		public void TestLocationAreaTypeFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW2", data.Part1, 3m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW3", data.Part1, 4m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW4", data.Part1, 5m, false, false);

			var area1 = Helper.CreateArea(data.Whs1, "AREA_DDA", AreaTypes.Codes.DockDoor);
			var area2 = Helper.CreateArea(data.Whs1, "AREA_VAT", AreaTypes.Codes.VATFiscal);
			var area3 = Helper.CreateArea(data.Whs1, "AREA_FREE", AreaTypes.Codes.FreeStore);
			receive1.Inventory[0].Location.WLV_WA_PickingArea = area1.PK;
			receive2.Inventory[0].Location.WLV_WA_PickingArea = area2.PK;
			receive3.Inventory[0].Location.WLV_WA_PickingArea = area3.PK;

			Asserter.AddToScope(receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0], receive4.Inventory[0]);
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.PickAreaType];
			CombineAssertions(() =>
			{
				filter.IsActive = true;
				filter.Property = "";
				Asserter.AssertMatches("Should match 4 Inventories", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0], receive4.Inventory[0]);

				filter.Property = "DDA";
				Asserter.AssertMatches("Should match Inventory with Area Type DDA", inventoryFilter.Filter, receive1.Inventory[0]);

				filter.Property = "VAT";
				Asserter.AssertMatches("Should match Inventory with Area Type VAT", inventoryFilter.Filter, receive2.Inventory[0]);

				filter.Property = "FRE";
				Asserter.AssertMatches("Should match Inventory with Area Type FRE", inventoryFilter.Filter, receive3.Inventory[0]);
			});
		}

		#endregion

		#region TestLocationRowFilter

		public void TestLocationRowFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW2", data.Part1, 3m, false, false).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW3", data.Part1, 4m, false, false).Inventory[0];
			var inventory4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW4", data.Part1, 5m, false, false).Inventory[0];

			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "1B", 1, 4);
			var row3 = Helper.CreateRowAndGenerateLocations(data.Whs1, "2C", 1, 4);
			inventory1.WI_WL = row1.Locations[0].PK;
			inventory2.WI_WL = row2.Locations[0].PK;
			inventory3.WI_WL = row3.Locations[0].PK;

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Row];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("Should match 4 Inventorys", inventoryFilter.Filter, inventory1, inventory2, inventory3, inventory4);

			filter.Property = "1";
			Asserter.AssertMatches("Should match Inventory with Row Starting With 1", inventoryFilter.Filter, inventory1, inventory2);

			filter.Property = "1A";
			Asserter.AssertMatches("Should match Inventory with Row Starting With 1A", inventoryFilter.Filter, inventory1);

			filter.Property = "2C";
			filter.ComparisonOperator = "exact";
			Asserter.AssertMatches("Should match Inventory with Row exact equal 2C", inventoryFilter.Filter, inventory3);

			filter.Property = "2C";
			filter.ComparisonOperator = "not equal";
			Asserter.AssertMatches("Should match Inventory with Row not equal 2C", inventoryFilter.Filter, inventory1, inventory2, inventory4);

			filter.Property = "1";
			filter.ComparisonOperator = "contains";
			Asserter.AssertMatches("Should match Inventory with Row contains 1", inventoryFilter.Filter, inventory1, inventory2);

			filter.Property = "2";
			filter.ComparisonOperator = "not starting";
			Asserter.AssertMatches("Should match Inventory with Row not starting 2", inventoryFilter.Filter, inventory1, inventory2, inventory4);

			filter.Property = "B";
			filter.ComparisonOperator = "not contain";
			Asserter.AssertMatches("Should match Inventory with Row not contain B", inventoryFilter.Filter, inventory1, inventory3, inventory4);

			filter.Property = "";
			filter.ComparisonOperator = "is blank";
			Asserter.AssertMatches("Should match Inventory with Row is blank", inventoryFilter.Filter, inventory4);

			filter.Property = "";
			filter.ComparisonOperator = "is not blank";
			Asserter.AssertMatches("Should match Inventory with Row is not blank", inventoryFilter.Filter, inventory1, inventory2, inventory3);
		}

		#endregion

		#region TestLocationColumnFilter

		public void TestLocationColumnFilter()
		{
			TestColumnLevelTrayFromInventoryLineCore(InventoryFilterBusinessObject.Schema.Column);
		}

		#endregion

		#region TestLocationLevelFilter

		public void TestLocationLevelFilter()
		{
			TestColumnLevelTrayFromInventoryLineCore(InventoryFilterBusinessObject.Schema.Level);
		}

		#endregion

		#region TestLocationTrayFilter

		public void TestLocationTrayFilter()
		{
			TestColumnLevelTrayFromInventoryLineCore(InventoryFilterBusinessObject.Schema.Tray);
		}

		#endregion

		#region TestColumnLevelTrayFromInventoryLineCore

		void TestColumnLevelTrayFromInventoryLineCore(ZString schemaName)
		{
			var whs1 = Helper.CreateWarehouse("warehouse1", "A1", 1, 2);
			var whs2 = Helper.CreateWarehouse("warehouse2", "A2", 1, 2);
			var whs3 = Helper.CreateWarehouse("warehouse3", "A3", 1, 2);

			whs1.WW_LocationColumnsAlpha = true;
			whs1.WW_LocationLevelsAlpha = true;
			whs1.WW_LocationTraysAlpha = true;

			whs3.WW_LocationColumnsZeroBased = true;
			whs3.WW_LocationLevelsZeroBased = true;
			whs3.WW_LocationTraysZeroBased = true;

			Helper.CreateRowAndGenerateLocations(whs1, "A", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(whs2, "B", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(whs3, "C", 2, 2, 2);

			var client = Helper.CreateClient("CLIENT", "CLIENT");
			var part = Helper.CreateProduct(client, "P1");

			var receive1 = Helper.CreateWhsReceive(client, whs1, "INW1", Helper.Notify);
			var receive2 = Helper.CreateWhsReceive(client, whs2, "INW2", Helper.Notify);
			var receive3 = Helper.CreateWhsReceive(client, whs3, "INW3", Helper.Notify);

			Helper.CreateWhsReceiveInventoryLine(receive1, part, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive2, part, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive3, part, 2m);

			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();

			receive1.Inventory[0].WI_PalletID = "receive1.Inventory[0]";
			receive2.Inventory[0].WI_PalletID = "receive2.Inventory[0]";
			receive3.Inventory[0].WI_PalletID = "receive3.Inventory[0]";

			Factory.Save();
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			var inventory3 = receive3.Inventory[0];

			AssertEquals("Pre-condition", "A-A-A-A", inventory1.LocationString);
			AssertEquals("Pre-condition", "A2-1-1", inventory2.LocationString);
			AssertEquals("Pre-condition", "A3-0-0", inventory3.LocationString);
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[schemaName];
			filter.IsActive = true;
			filter.Property = "A";
			Asserter.AssertMatches("search by 'A', should return by inventory in Whs1.", inventoryFilter.Filter, inventory1);

			filter.Property = "1";
			Asserter.AssertMatches("search by '1', should return inventory in Whs2.", inventoryFilter.Filter, inventory2);

			filter.Property = "0";
			Asserter.AssertMatches("search by '0', should return inventory in Whs3.", inventoryFilter.Filter, inventory3);
		}

		#endregion

		#region TestLocationType

		public void TestLocationType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 4m);

			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			receive2.FinaliseDocket();
			receive3.FinaliseDocket();

			var locationType1 = Helper.CreateLocationType("LT1");
			var locationType2 = Helper.CreateLocationType("LT2");
			var locationType3 = Helper.CreateLocationType("LT3");
			inventory1.Location.WLV_WLT_LocationType = locationType1.PK;
			inventory2.Location.WLV_WLT_LocationType = locationType2.PK;
			inventory3.Location.WLV_WLT_LocationType = locationType3.PK;
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.LocationType];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Should return 3 inventory records", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = locationType1.PK;
			Asserter.AssertMatches("Should match Inventory with location Type LT1", inventoryFilter.Filter, inventory1);
		}

		#endregion

		#region TestLocationClass

		public void TestLocationClass()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 3);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW2", data.Part1, 3m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW3", data.Part1, 4m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW4", data.Part1, 5m, false, false);

			var locationType1 = Helper.CreateLocationType("LT1", LocationClasses.Codes.DDL);
			var locationType2 = Helper.CreateLocationType("LT2", "LT2 Test", false, 1, LocationClasses.Codes.FIX);
			var locationType3 = Helper.CreateLocationType("LT3", LocationClasses.Codes.NOR);
			receive1.Inventory[0].Location.WLV_WLT_LocationType = locationType1.PK;
			receive2.Inventory[0].Location.WLV_WLT_LocationType = locationType2.PK;
			receive3.Inventory[0].Location.WLV_WLT_LocationType = locationType3.PK;

			Asserter.AddToScope(receive1.Inventory[0], receive2.Inventory[0], receive3.Inventory[0], receive4.Inventory[0]);
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.LocationClass];
			filter.IsActive = true;
			filter.Property = locationType1.WLT_LocationClass;
			Asserter.AssertMatches("Should match Inventory with location Class DDL", inventoryFilter.Filter, receive1.Inventory[0]);

			filter.Property = locationType2.WLT_LocationClass;
			Asserter.AssertMatches("Should match Inventory with location Class FIX", inventoryFilter.Filter, receive2.Inventory[0]);

			filter.Property = locationType3.WLT_LocationClass;
			Asserter.AssertMatches("Should match Inventory with location Class NOR", inventoryFilter.Filter, receive3.Inventory[0]);

			filter.Property = locationType3.WLT_LocationClass;
			filter.ComparisonOperator = "not equal";
			Asserter.AssertMatches("Should match Inventory with location Class not NOR", inventoryFilter.Filter, receive1.Inventory[0], receive2.Inventory[0], receive4.Inventory[0]);
		}

		#endregion

		#region TestLocationFilter

		public void TestLocationFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW2", data.Part1, 3m, false, false).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW3", data.Part1, 4m, false, false).Inventory[0];

			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "1B", 1, 4);
			var row3 = Helper.CreateRowAndGenerateLocations(data.Whs1, "1C", 1, 4);

			inventory1.WI_WL = row1.Locations[0].PK;
			inventory2.WI_WL = row2.Locations[0].PK;
			inventory3.WI_WL = row2.Locations[0].PK;

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];
			AssertEquals("Category should be Locations.", FilterCategories.Locations, locationFilter.Category);
			AssertEquals("Should be not be published on web.", false, locationFilter.IsPublishedOnWeb);

			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			locationFilter.Property = row1.Locations[0].PK;
			Asserter.AssertMatches("Should match 1 Inventory.", inventoryFilter.Filter, inventory1);
			locationFilter.Property = row2.Locations[0].PK;
			Asserter.AssertMatches("Should match 2 Inventorys.", inventoryFilter.Filter, inventory2, inventory3);
			locationFilter.Property = row3.Locations[0].PK;
			Asserter.AssertMatches("Should match no Inventorys.", inventoryFilter.Filter);
		}

		public void TestLocationFilter_Validator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			inventory.WI_WL = row.Locations[0].PK;

			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];
			locationFilter.IsActive = true;

			locationFilter.Validation.ValidateProperty();
			AssertNoError(locationFilter.PropertyInfo, "Location can not be entered without a warehouse.");
			AssertNoError(locationFilter.PropertyInfo, "Enter a valid selection.");

			locationFilter.Property = data.Whs1.PK;
			AssertHasError(locationFilter.PropertyInfo, "Location can not be entered without a warehouse.");
			AssertHasError(locationFilter.PropertyInfo, "Enter a valid selection.");
		}

		public void TestLocationFilters_NotOnWeb()
		{
			var inventoryFilter = GetNewFilterStripBusinessObject();
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];

			AssertEquals("Location Filter IsPublishedOnWeb shoud be set to false.", false, locationFilter.IsPublishedOnWeb);
		}

		public void TestLocationFilters_ClearedOnWarehouseChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			inventory.WI_WL = row.Locations[0].PK;

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];

			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;
			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			locationFilter.Property = row.Locations[0].PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = ZGuid.Empty;
			AssertEquals("Location Filter should be cleared when warehouse is changed.", ZGuid.Empty, locationFilter.Property);
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		public void TestLocationFilters_ClearedOnWarehouseChange_WhenInactive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			inventory.WI_WL = row.Locations[0].PK;

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];

			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;
			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			locationFilter.Property = row.Locations[0].PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			locationFilter.IsActive = false;
			warehouseFilter.Property = ZGuid.Empty;
			locationFilter.IsActive = true;
			AssertEquals("Location Filter should be cleared when warehouse is changed.", ZGuid.Empty, locationFilter.Property);
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		public void TestLocationFilters_NotClearedOnWarehousePopulationToCorrectValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			inventory.WI_WL = row.Locations[0].PK;

			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];

			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;
			locationFilter.Property = row.Locations[0].PK;
			warehouseFilter.Property = data.Whs1.PK;

			AssertEquals("Location Filter should not have been cleared when warehouse is populated.", row.Locations[0].PK, locationFilter.Property);
		}

		public void TestLocationFilters_InvalidWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INW1", data.Part1, 2m, false, false).Inventory[0];
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "1A", 1, 4);
			inventory.WI_WL = row.Locations[0].PK;

			var whs2 = Helper.CreateWarehouse("W2");
			Factory.Save();

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Location];

			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;

			var whs1Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, data.Whs1.PK));
			var whs2Locations = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs2.PK));
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = data.Whs1.PK;
			AssertContainsExactElementsInAnyOrder(whs1Locations.Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));

			warehouseFilter.Property = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(whs1Locations.Concat(whs2Locations).Select(l => l.PK), locationFilter.List.Cast<WhsLocation>().Select(l => l.PK));
		}

		#endregion

		#endregion

		#region TestReceiveRefFilter

		public void TestReceiveRefFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "111", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "112", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "223", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 10.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2.PK, 16.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1.PK, 12.0m);
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ReceiveReference];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3);
		}

		#endregion

		#region TestReceiveRefFilter_WithTransferredInventory

		public void TestReceiveRefFilter_WithTransferredInventory()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var part = Helper.CreateProduct(client, "PROD1");
			var inventory1 = Helper.CreateStock(whs, client, "ORIGINAL REF", part, 10m, "A-1-1").Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client, whs, "TRANSFER REF");
			Helper.CreateWhsTransferLine(transfer, part, 10m, "A-1-1", "A-1-2");
			transfer.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			var inventory2 = transfer.Lines[0].Inventory[0];
			Asserter.AddToScope(inventory1, inventory2);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ReceiveReference];
			filter.IsActive = true;
			filter.Property = "ORIGINAL REF";
			Asserter.AssertMatches("Should find one inventory.", inventoryFilter.Filter, inventory2);
		}

		public void TestReceiveRefFilter_WithTransferredInventory_TransferredMoreThanOnce()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 4);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var part = Helper.CreateProduct(client, "PROD1");
			var inventory1 = Helper.CreateStock(whs, client, "ORIGINAL REF", part, 10m, "A-1-1").Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client, whs, "TRANSFER REF 1");
			Helper.CreateWhsTransferLine(transfer, part, 10m, "A-1-1", "A-1-2");
			transfer.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			var inventory2 = transfer.Lines[0].Inventory[0];

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ReceiveReference];
			filter.IsActive = true;

			Asserter.AddToScope(inventory1, inventory2);

			filter.Property = "ORIGINAL REF";
			Asserter.AssertMatches("Should find inventory2.", inventoryFilter.Filter, inventory2);

			filter.Property = "TRANSFER REF 1";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);

			var transfer2 = Helper.CreateWhsTransfer(client, whs, "TRANSFER REF 2");
			Helper.CreateWhsTransferLine(transfer2, part, 10m, "A-1-2", "A-1-3");
			transfer2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer2);
			Factory.Save();
			var inventory3 = transfer2.Lines[0].Inventory[0];
			Asserter.AddToScope(inventory3);

			filter.Property = "ORIGINAL REF";
			Asserter.AssertMatches("Should find inventory3.", inventoryFilter.Filter, inventory3);

			filter.Property = "TRANSFER REF 1";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);

			filter.Property = "TRANSFER REF 2";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);

			var transfer3 = Helper.CreateWhsTransfer(client, whs, "TRANSFER REF 3");
			Helper.CreateWhsTransferLine(transfer3, part, 7m, "A-1-3", "A-1-4"); // partially transferred
			transfer3.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer3);
			Factory.Save();
			var inventory4 = transfer3.Lines[0].Inventory[0];
			Asserter.AddToScope(inventory4);

			filter.Property = "ORIGINAL REF";
			Asserter.AssertMatches("Should find inventory3 and inventory4.", inventoryFilter.Filter, inventory3, inventory4);

			filter.Property = "TRANSFER REF 1";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);

			filter.Property = "TRANSFER REF 2";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);

			filter.Property = "TRANSFER REF 3";
			Asserter.AssertMatches("Should find null inventory.", inventoryFilter.Filter);
		}

		#endregion

		#region AssertTextColumnBehaviour

		#region TestPickMethod

		public void TestPickMethod()
		{
			AssertTextColumnBehaviour(InventoryFilterBusinessObject.Schema.PickMethod, WhsLocationViewSchema.WLV_PickMethod);
		}

		#endregion

		#region TestVolumeUQ

		public void TestVolumeUQ()
		{
			AssertTextColumnBehaviour(InventoryFilterBusinessObject.Schema.VolumeUQ, WhsLocationViewSchema.WLV_MaxCubicUnit, Constants.Volume.CubicCentimeters, Constants.Volume.MegaLitre, Constants.Volume.TeaChest);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			AssertTextColumnBehaviour(InventoryFilterBusinessObject.Schema.WeightUQ, WhsLocationViewSchema.WLV_MaxWeightUnit, Constants.Weight.Kilograms, Constants.Weight.Hectograms, Constants.Weight.MetricCarat);
		}

		#endregion

		#region TestLocationStatus

		public void TestLocationStatus()
		{
			AssertTextColumnBehaviour(InventoryFilterBusinessObject.Schema.LocationStatus, WhsLocationViewSchema.WLV_LocationStatus);
		}

		#endregion

		void AssertTextColumnBehaviour(string filtername, SchemaStringColumn column, params ZString[] values)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var whs1 = helper.CreateWarehouse("warehouse1", "A", 2, 2);
			var whs2 = helper.CreateWarehouse("warehouse2", "B", 2, 2);
			var whs3 = helper.CreateWarehouse("warehouse3", "C", 2, 2);

			var client = helper.CreateClient("CLIENT", "CLIENT");
			var part = helper.CreateProduct(client, "P1");

			var receive1 = helper.CreateWhsReceive(client, whs1, "INW1", Notify);
			var receive2 = helper.CreateWhsReceive(client, whs2, "INW2", Notify);
			var receive3 = helper.CreateWhsReceive(client, whs3, "INW3", Notify);
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive1, part, 1m);
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive2, part, 1m);
			var inventory3 = helper.CreateWhsReceiveInventoryLine(receive3, part, 1m);

			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			ZString val1, val2, val3;
			if (values.Length == 0)
			{
				val1 = "PM1";
				val2 = "PM2";
				val3 = "PM3";
			}
			else if (values.Length == 3)
			{
				val1 = values[0];
				val2 = values[1];
				val3 = values[2];
			}
			else
			{
				throw new DeveloperNotificationException("You should provide either 0 or 3 values.");
			}

			receive1.Inventory[0].Location[column] = val1;
			receive2.Inventory[0].Location[column] = val2;
			receive3.Inventory[0].Location[column] = val3;
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[filtername];
			filter.IsActive = true;
			filter.Property = val1;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} value of {1}", column.Name, val1), inventoryFilter.Filter, inventory1);

			filter.Property = val2;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} value of {1}", column.Name, val2), inventoryFilter.Filter, inventory2);

			filter.Property = val3;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} value of {1}", column.Name, val3), inventoryFilter.Filter, inventory3);
		}

		#endregion

		#region TestNumberOfTouchesUntilStockTake

		public void TestNumberOfTouchesUntilStockTake()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 4m);
			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			receive2.FinaliseDocket();
			receive3.FinaliseDocket();

			receive1.Lines[0].Location.WLV_MaximumPickCountBeforeAutomatedStocktake = 6;
			receive2.Lines[0].Location.WLV_MaximumPickCountBeforeAutomatedStocktake = 11;
			receive3.Lines[0].Location.WLV_MaximumPickCountBeforeAutomatedStocktake = 16;

			receive1.Lines[0].Location.WLV_FinalisedPickCount = 1;
			receive2.Lines[0].Location.WLV_FinalisedPickCount = 1;
			receive3.Lines[0].Location.WLV_FinalisedPickCount = 1;

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.TouchesTillStocktake];
			filter.IsActive = true;
			AssertEquals("Precondition", (byte)0, filter.Decimals);

			filter.Property1 = 5;
			Asserter.AssertMatches("Should find one inventory.", inventoryFilter.Filter, inventory1);
		}

		#endregion

		#region Sizes

		#region TestVolumeMax

		public void TestVolumeMax()
		{
			AssertNumberRangeFilterBehaviour(InventoryFilterBusinessObject.Schema.VolumeMax, WhsLocationViewSchema.WLV_MaxCubic, 0.5m, 1m, 1.5m, l => l.WLV_MaxCubicUnit = Enterprise.Core.Constants.Volume.CubicMetres);
		}

		#endregion

		#region TestMaxWeight

		public void TestWeightMax()
		{
			AssertNumberRangeFilterBehaviour(InventoryFilterBusinessObject.Schema.WeightMax, WhsLocationViewSchema.WLV_MaxWeight, 50m, 100m, 150m, l => l.WLV_MaxWeightUnit = Enterprise.Core.Constants.Weight.Kilograms);
		}

		#endregion

		#region TestMaxQuantity

		public void TestMaxQuantity()
		{
			AssertNumberRangeFilterBehaviour(InventoryFilterBusinessObject.Schema.QuantityMax, WhsLocationViewSchema.WLV_MaxQuantity, 50m, 100m, 150m);
		}

		#endregion

		#region AssertNumberRangeFilterBehaviour

		void AssertNumberRangeFilterBehaviour(string filtername, SchemaDecimalColumn column, ZDecimal number1, ZDecimal number2, ZDecimal number3, Action<WhsLocation> additionalSetup = null)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 4m);

			receive1.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			receive2.FinaliseDocket();
			receive3.FinaliseDocket();

			var location1 = inventory1.Location;
			var location2 = inventory2.Location;
			var location3 = inventory3.Location;
			location1[column] = number1;
			location2[column] = number2;
			location3[column] = number3;

			if (additionalSetup != null)
			{
				additionalSetup(location1);
				additionalSetup(location2);
				additionalSetup(location3);
			}

			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)inventoryFilter[filtername];
			filter.IsActive = true;
			filter.Property1 = number1;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} of {1}", column.Name, number1), inventoryFilter.Filter, inventory1);

			filter.Property1 = number2;
			filter.Property2 = number2;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} of {1}", column.Name, number2), inventoryFilter.Filter, inventory2);

			filter.Property1 = number3;
			filter.Property2 = number3;
			Asserter.AssertMatches(string.Format("Should match Inventory with {0} of {1}", column.Name, number3), inventoryFilter.Filter, inventory3);

			filter.Property1 = number2;
			filter.Property2 = number3;
			Asserter.AssertMatches(string.Format("Should match Inventories with {0} between {1} and {2}", column.Name, number2, number3), inventoryFilter.Filter, inventory2, inventory3);
		}

		#endregion

		#endregion

		#region TestHeldCode

		public void TestHeldCode()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var holdCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var holdCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1.PK);
			var holdCode3 = Helper.CreateInventoryHeldCode("CCC", "CCC test", client1.PK);
			var holdCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var holdCode5 = Helper.CreateInventoryHeldCode("CCC2", "CCC test", client2.PK);
			var holdCode6 = Helper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2.PK);
			var product1 = Helper.CreateProduct(client1, "product1");
			var product2 = Helper.CreateProduct(client2, "product2");
			var receive1 = Helper.CreateWhsReceive(client1.PK, warehouse.PK, "111", Notify);
			var receive2 = Helper.CreateWhsReceive(client1.PK, warehouse.PK, "112", Notify);
			var receive3 = Helper.CreateWhsReceive(client2.PK, warehouse.PK, "123", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1.PK, 10.0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, product1.PK, 16.0m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, product1.PK, 20.0m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, product1.PK, 24.0m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive3, product2.PK, 12.0m);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive3, product2.PK, 16.0m);
			var inventory7 = Helper.CreateWhsReceiveInventoryLine(receive3, product2.PK, 20.0m);

			inventory1.OriginalInventoryHeldCode = holdCode1.WHC_Code;
			inventory2.OriginalInventoryHeldCode = holdCode2.WHC_Code;
			inventory3.OriginalInventoryHeldCode = "TST";
			inventory4.OriginalInventoryHeldCode = holdCode3.WHC_Code;
			inventory5.OriginalInventoryHeldCode = holdCode4.WHC_Code;
			inventory6.OriginalInventoryHeldCode = holdCode5.WHC_Code;
			inventory7.OriginalInventoryHeldCode = holdCode6.WHC_Code;
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6, inventory7);
			var inventoryFilter = GetNewFilterStripBusinessObject();

			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.HeldCode];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.Property = "AAA";
			Asserter.AssertMatches("Should return inventory1.", filter, inventory1);

			filter.Property = "BBB";
			Asserter.AssertMatches("Should return inventory2 and inventory5.", filter, inventory2, inventory5);

			filter.Property = "BBB2";
			Asserter.AssertMatches("Should return inventory5.", filter, inventory5);

			filter.Property = "CCC";
			Asserter.AssertMatches("Should return inventory4 and inventory6.", filter, inventory4, inventory6);

			filter.Property = "DDD";
			Asserter.AssertMatches("Should return inventory7.", filter, inventory7);

			filter.Property = "TST";
			Asserter.AssertMatches("Should return inventory3.", filter, inventory3);
		}

		#endregion

		#region TestDateLastTouched

		public void TestDateLastTouched()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("A", "A", 1, 3);
			var receive1 = Helper.CreateWhsReceive(data.Org1, whs, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			receive1.AllocateLocationsWithMock();
			receive1.RunPreSaveValidation();

			inventory1.Location.WLV_LastInventoryChangeDate = today;
			inventory2.Location.WLV_LastInventoryChangeDate = today.AddDays(1);
			inventory3.Location.WLV_LastInventoryChangeDate = today.AddDays(2);

			receive1.FinaliseDocket();
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var todayZDateTime = today.ToZDateTime();
			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateTimeOffsetFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.LastTouchedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = todayZDateTime;
			filter.Property2 = todayZDateTime;
			Asserter.AssertMatches(string.Format("Should match Inventory with last touched date of {0}", today.ToString()), inventoryFilter.Filter, inventory1);

			filter.Property1 = todayZDateTime.AddDays(1);
			filter.Property2 = todayZDateTime.AddDays(1);
			Asserter.AssertMatches(string.Format("Should match Inventory with last touched date of {0}", today.AddDays(1).ToString()), inventoryFilter.Filter, inventory2);

			filter.Property1 = todayZDateTime.AddDays(2);
			filter.Property2 = todayZDateTime.AddDays(2);
			Asserter.AssertMatches(string.Format("Should match Inventory with last touched date of {0}", today.AddDays(2).ToString()), inventoryFilter.Filter, inventory3);
		}

		public void TestDateLastTouched_InDifferentTimeZone()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var today = ZDateTimeOffset.Today; // e.x.: 2019-09-02 00:00:00 +10/+11

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();

			location.WLV_LastInventoryChangeDate = today;
			Factory.Save();

			Asserter.AddToScope(inventory);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNNJG";
			var todayZDateTime = today.ToZDateTime(); // e.g. ZDateTime: 2019-09-02 00:00:00 +08:00, Local ZDateTime: 2019-09-01 22:00:00/21:00:00

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateTimeOffsetFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.LastTouchedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = todayZDateTime;
			filter.Property2 = todayZDateTime;
			Asserter.AssertMatches(string.Format("Should not match Inventory if local time is different with last touched date of {0}", today.ToString()), inventoryFilter.Filter);

			filter.Property1 = todayZDateTime.AddDays(-1);
			filter.Property2 = todayZDateTime;
			Asserter.AssertMatches(string.Format("Should match Inventory with last touched date of {0}", today.AddDays(1).ToString()), inventoryFilter.Filter, inventory);
		}

		#endregion

		#region TestComponentFilters

		public void TestComponentFilters_Product()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var parentProduct2 = Helper.CreateProduct(client, "product2");
			var component1 = Helper.CreateProduct(client, "component1");
			var component2 = Helper.CreateProduct(client, "component2");
			var component3 = Helper.CreateProduct(client, "component3");

			Factory.Save();

			Helper.CreateProductBOM(parentProduct1, component1);
			Helper.CreateProductBOM(parentProduct2, component2);
			Helper.CreateProductBOM(parentProduct2, component3);

			var inventory1 = Helper.CreateWhsReceiveWithInventory(client, whs, "r1", component1, 5m).Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveWithInventory(client, whs, "r2", component2, 5m).Inventory[0];
			var inventory3 = Helper.CreateWhsReceiveWithInventory(client, whs, "r3", component3, 5m).Inventory[0];
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 5m);
			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct2, 5m);

			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder1);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder2);

			workOrder1.Receive.FinaliseDocket();
			workOrder2.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory1 = workOrder1.Receive.Inventory[0];
			var woInventory2 = workOrder2.Receive.Inventory[0];

			Asserter.AddToScope(new[] { woInventory1, woInventory2 });

			var inventoryFilter = GetNewFilterStripBusinessObject();

			// Assert filter on products
			var filter = (ModuleGuidFilter)inventoryFilter["Component Product"];
			filter.IsActive = true;
			filter.Property = component1.PK;
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory1);

			filter.Property = component2.PK;
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory2);

			filter.Property = component3.PK;
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory2);

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
		}

		public void TestComponentFilters_ProductPartAttribute1()
		{
			TestComponentFilters_ProductPartAttributesCore(
				"Component Part Attribute 1",
				AttributeNumber.One,
				(WhsReceiveLine line, string attrib) => line.WE_PartAttrib1 = attrib);
		}

		public void TestComponentFilters_ProductPartAttribute2()
		{
			TestComponentFilters_ProductPartAttributesCore(
				"Component Part Attribute 2",
				AttributeNumber.Two,
				(WhsReceiveLine line, string attrib) => line.WE_PartAttrib2 = attrib);
		}

		public void TestComponentFilters_ProductPartAttribute3()
		{
			TestComponentFilters_ProductPartAttributesCore(
				"Component Part Attribute 3",
				AttributeNumber.Three,
				(WhsReceiveLine line, string attrib) => line.WE_PartAttrib3 = attrib);
		}

		public void TestComponentFilters_ProductSerialNumber()
		{
			TestComponentFilters_ProductPartAttributesCore(
				"Component Serial Number",
				AttributeNumber.Serial,
				(WhsReceiveLine line, string attrib) => line.WE_SerialNumber = attrib);
		}

		public void TestComponentFilters_ProductSerialNumber_Pivot()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestComponentFilters_ProductPartAttributesCore(
				"Component Serial Number",
				AttributeNumber.Serial,
				(WhsReceiveLine line, string attrib) => line.SerialNumbers.AddNew().SerialNumberValue = attrib);
			}
		}

		void TestComponentFilters_ProductPartAttributesCore(string filterString, AttributeNumber attributeNumber, Action<WhsReceiveLine, string> setAttribute)
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");
			var parentProduct2 = Helper.CreateProduct(client, "product2");
			var component2 = Helper.CreateProduct(client, "component2");

			Factory.Save();

			Helper.SetClientAttributeType(client, attributeNumber, true);
			Helper.SetProductAttributeUse(client, component1, attributeNumber, true);

			Helper.CreateProductBOM(parentProduct1, component1);
			Helper.CreateProductBOM(parentProduct2, component2);

			var receive1 = Helper.CreateWhsReceive(client, whs, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, component1, 1m);
			setAttribute(inventory1, "PA1");

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();

			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder1);
			workOrder1.Receive.FinaliseDocket();

			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(client, whs, "R2");
			var inventory2 = Helper.CreateWhsReceiveLine(receive2, component1, 1m);
			setAttribute(inventory2, "PA2");

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			Factory.Save();

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder2);
			workOrder2.Receive.FinaliseDocket();

			Factory.Save();

			var receive3 = Helper.CreateWhsReceive(client, whs, "R3");
			var inventory3 = Helper.CreateWhsReceiveLine(receive3, component2, 1m);

			receive3.AllocateLocationsWithMock();
			receive3.FinaliseDocket();
			Factory.Save();

			var workOrder3 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct2, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder3);
			workOrder3.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory1 = workOrder1.Receive.Inventory[0];
			var woInventory2 = workOrder2.Receive.Inventory[0];
			var woInventory3 = workOrder3.Receive.Inventory[0];

			Asserter.AddToScope(woInventory1, woInventory2, woInventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();

			// Assert filter on products
			var filter = (ModuleTextFilter)inventoryFilter[filterString];
			filter.IsActive = true;

			// Equal
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PA1";
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory1);

			filter.Property = "SomeOtherAttributeValue";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);

			filter.Property = "PA2";
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory2);

			filter.Property = ZString.Empty;
			Asserter.AssertMatches("All rows should match.", inventoryFilter.Filter, woInventory1, woInventory2, woInventory3);

			// Starts With
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "PA";
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
			filter.Property = "AB";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("All rows should match.", inventoryFilter.Filter, woInventory1, woInventory2, woInventory3);

			// Contains
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "A";
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
			filter.Property = "1";
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory1);
			filter.Property = "B";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("All rows should match.", inventoryFilter.Filter, woInventory1, woInventory2, woInventory3);

			// Not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "A";
			Asserter.AssertMatches("Only the inventory without any attribute/serial number should match.", inventoryFilter.Filter, woInventory3);
			filter.Property = "1";
			Asserter.AssertMatches("Inventories with are not '1' and those without attributes/serial numbers should match.", inventoryFilter.Filter, woInventory2, woInventory3);
			filter.Property = "B";
			Asserter.AssertMatches("All inventories should match.", inventoryFilter.Filter, woInventory1, woInventory2, woInventory3);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("All inventories should match.", inventoryFilter.Filter, woInventory1, woInventory2, woInventory3);

			// IsBlank
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Should return inventory does not have attribute/serial number.", inventoryFilter.Filter, woInventory3);

			// IsNotBlank
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("Should return inventories that have attribute/serial number(s).", inventoryFilter.Filter, woInventory1, woInventory2);
		}

		public void TestComponentFilters_ProductPackingDate()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");

			Factory.Save();

			helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(client, component1, AttributeNumber.PackingDate, true);

			Helper.CreateProductBOM(parentProduct1, component1);

			var today = ZDate.Today;

			var receive1 = Helper.CreateWhsReceive(client, whs, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, component1, 1m);
			inventory1.WE_PackingDate = today;

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();

			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder1);
			workOrder1.Receive.FinaliseDocket();

			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(client, whs, "R2");
			var inventory2 = Helper.CreateWhsReceiveLine(receive2, component1, 1m);
			inventory2.WE_PackingDate = today.AddDays(-14);

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			Factory.Save();

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder2);
			workOrder2.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory1 = workOrder1.Receive.Inventory[0];
			var woInventory2 = workOrder2.Receive.Inventory[0];

			Asserter.AddToScope(new[] { woInventory1, woInventory2 });

			var inventoryFilter = GetNewFilterStripBusinessObject();

			// Assert filter on products
			var filter = (ModuleDateFilter)inventoryFilter["Component Packing Date"];
			filter.IsActive = true;

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(1);
			filter.Property2 = today.AddDays(5);

			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today.AddDays(5);
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory1);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-15);
			filter.Property2 = today.AddDays(5);
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-15);
			filter.Property2 = today.AddDays(-10);
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory2);
		}

		public void TestComponentFilters_ProductExpiryDate()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 2);
			var client = Helper.CreateClient("C");
			var parentProduct1 = Helper.CreateProduct(client, "product1");
			var component1 = Helper.CreateProduct(client, "component1");

			Factory.Save();

			helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(client, component1, AttributeNumber.ExpiryDate, true);

			Helper.CreateProductBOM(parentProduct1, component1);

			var today = ZDate.Today;

			var receive1 = Helper.CreateWhsReceive(client, whs, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, component1, 1m);
			inventory1.WE_ExpiryDate = today.AddDays(1);

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();

			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder1);
			workOrder1.Receive.FinaliseDocket();

			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(client, whs, "R2");
			var inventory2 = Helper.CreateWhsReceiveLine(receive2, component1, 1m);
			inventory2.WE_ExpiryDate = today.AddDays(14);

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			Factory.Save();

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(client, whs, parentProduct1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, workOrder2);
			workOrder2.Receive.FinaliseDocket();

			Factory.Save();

			var woInventory1 = workOrder1.Receive.Inventory[0];
			var woInventory2 = workOrder2.Receive.Inventory[0];

			Asserter.AddToScope(new[] { woInventory1, woInventory2 });

			var inventoryFilter = GetNewFilterStripBusinessObject();

			// Assert filter on products
			var filter = (ModuleDateFilter)inventoryFilter["Component Expiry Date"];
			filter.IsActive = true;

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-7);
			filter.Property2 = today;

			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today;
			filter.Property2 = today.AddDays(7);
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory1);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today;
			filter.Property2 = today.AddDays(15);
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(7);
			filter.Property2 = today.AddDays(15);
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory2);
		}

		public void TestComponentFilters_ProductInwardsEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR");
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[0].WLV_WA_PutawayArea = area.PK;

			Helper.CreateProductBOM(data.Part2, data.Part1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, allocateLocations: false, finalise: false);
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.Lines[0].WE_WL = row.Locations[0].PK;
			receive1.Lines[0].CustomsData.WB_EntryKey = "PA1";

			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef1", data.Part2, 5m);
			workOrder1.WD_IsInwardsProcessingJob = true;

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, allocateLocations: false, finalise: false);
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.Lines[0].WE_WL = row.Locations[0].PK;
			receive2.Lines[0].CustomsData.WB_EntryKey = "PA2";

			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef2", data.Part2, 5m);
			workOrder2.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder2.WD_IsInwardsProcessingJob = true;
			workOrder2.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder2);
			workOrder2.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var woInventory1 = workOrder1.Receive.Inventory[0];
			var woInventory2 = workOrder2.Receive.Inventory[0];

			Asserter.AddToScope(new[] { woInventory1, woInventory2 });

			var inventoryFilter = GetNewFilterStripBusinessObject();

			// Assert filter on products
			var filter = (ModuleTextFilter)inventoryFilter["Component Inwards Entry Key"];
			filter.IsActive = true;

			// Equal
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PA1";
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory1);

			filter.Property = "SomeOtherAttributeValue";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);

			filter.Property = "PA2";
			Asserter.AssertMatches("Only one row should match.", inventoryFilter.Filter, woInventory2);

			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);

			// Starts With
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "PA";
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
			filter.Property = "AB";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);

			// Contains
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "A";
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
			filter.Property = "1";
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory1);
			filter.Property = "B";
			Asserter.AssertMatches("No rows should match.", inventoryFilter.Filter);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);

			// Not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "A";
			Asserter.AssertMatches("No row should match.", inventoryFilter.Filter);
			filter.Property = "1";
			Asserter.AssertMatches("One row should match.", inventoryFilter.Filter, woInventory2);
			filter.Property = "B";
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Both rows should match.", inventoryFilter.Filter, woInventory1, woInventory2);
		}

		#endregion

		#region TestCustomsData filters

		#region TestManufacturerFilter

		public void TestManufacturerFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress1 = manufacturer1.MainAddress;

			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress2 = manufacturer2.MainAddress;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec1");
			receive.WD_DocketSubType = "CUS";

			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond1 = Factory.New<WhsBondedWarehouseAttribute>();
			bond1.WB_OA_ManufacturerAddress = manufacturerAddress1.PK;
			bond1.SetParent(inventoryLine1.InDocketLine);

			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			var bond2 = Factory.New<WhsBondedWarehouseAttribute>();
			bond2.WB_OA_ManufacturerAddress = manufacturerAddress2.PK;
			bond2.SetParent(inventoryLine2.InDocketLine);

			var nonCustomsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec2");
			var nonCustomsInventoryLine = Helper.CreateWhsReceiveInventoryLine(nonCustomsReceive, data.Part1, 5m);

			Factory.Save();
			Asserter.AddToScope(inventoryLine1, inventoryLine2, nonCustomsInventoryLine);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.Manufacturer];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("When filter is empty, should find all inventories.", inventoryFilter.Filter, inventoryLine1, inventoryLine2, nonCustomsInventoryLine);

			filter.Property = manufacturer1.PK;
			Asserter.AssertMatches("Should find inventory line 1.", inventoryFilter.Filter, inventoryLine1);

			filter.Property = manufacturer2.PK;
			Asserter.AssertMatches("Should find inventory line 2.", inventoryFilter.Filter, inventoryLine2);

			filter.Property = ZGuid.NewZGuid();
			Asserter.AssertMatches("When filter is other valid Guid, should find null", inventoryFilter.Filter);
		}

		#endregion

		#region TestCountryOfOriginFilter

		public void TestCountryOfOriginFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec1");
			receive.WD_DocketSubType = "CUS";

			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond1 = Factory.New<WhsBondedWarehouseAttribute>();
			bond1.WB_RN_NKCountryOfOrigin = "AU";
			bond1.SetParent(inventoryLine1.InDocketLine);

			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			var bond2 = Factory.New<WhsBondedWarehouseAttribute>();
			bond2.WB_RN_NKCountryOfOrigin = "US";
			bond2.SetParent(inventoryLine2.InDocketLine);

			var product3 = Helper.CreateProduct("P3", data.Org1);
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, product3, 1m);

			var nonCustomsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec2");
			var nonCustomsInventoryLine = Helper.CreateWhsReceiveInventoryLine(nonCustomsReceive, data.Part1, 5m);

			Factory.Save();
			Asserter.AddToScope(inventoryLine1, inventoryLine2, inventoryLine3, nonCustomsInventoryLine);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.CountryOfOrigin];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should find all inventories.", inventoryFilter.Filter, inventoryLine1, inventoryLine2, inventoryLine3, nonCustomsInventoryLine);

			filter.Property = "AU";
			Asserter.AssertMatches("When filter is AU, should find inventory line 1.", inventoryFilter.Filter, inventoryLine1);

			filter.Property = "US";
			Asserter.AssertMatches("When filter is US, should find inventory line 2.", inventoryFilter.Filter, inventoryLine2);

			filter.Property = "UK";
			Asserter.AssertMatches("When filter is UK, should find null", inventoryFilter.Filter);
		}

		#endregion

		#endregion

		#region TestEachModuleFilterCollectionIsDistinct

		public void TestEachModuleFilterCollectionIsDistinct()
		{
			var inventoryFilter = GetNewFilterStripBusinessObject();
			var colours = new GridColourStripBusinessObject(inventoryFilter, null, typeof(WhsInventoryView));
			AssertEquals(0, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(0, colours.ActiveModuleFilters.Count);

			inventoryFilter.ModuleFilters[InventoryFilterBusinessObject.Schema.Status].IsActive = true;
			AssertEquals(1, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(0, colours.ActiveModuleFilters.Count);

			colours.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse].IsActive = true;
			AssertEquals(1, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(1, colours.ActiveModuleFilters.Count);

			inventoryFilter.ModuleFilters[InventoryFilterBusinessObject.Schema.Status].IsActive = false;
			AssertEquals(0, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(1, colours.ActiveModuleFilters.Count);

			colours.ModuleFilters[InventoryFilterBusinessObject.Schema.Status].IsActive = true;
			AssertEquals(0, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(2, colours.ActiveModuleFilters.Count);

			inventoryFilter.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse].IsActive = true;
			AssertEquals(1, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(2, colours.ActiveModuleFilters.Count);

			colours.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse].IsActive = false;
			AssertEquals(1, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(1, colours.ActiveModuleFilters.Count);

			colours.ModuleFilters[InventoryFilterBusinessObject.Schema.Status].IsActive = false;
			AssertEquals(1, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(0, colours.ActiveModuleFilters.Count);

			inventoryFilter.ModuleFilters[InventoryFilterBusinessObject.Schema.Warehouse].IsActive = false;
			AssertEquals(0, inventoryFilter.ActiveModuleFilters.Count);
			AssertEquals(0, colours.ActiveModuleFilters.Count);
		}

		#endregion

		#region TestAddCustomAttributeFilter_WithOverlappingFilterDescription

		public void TestAddCustomAttributeFilter_WithOverlappingFilterDescription()
		{
			Globals.IsWeb = true;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, InventoryFilterBusinessObject.Schema.Warehouse, false);
			Helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute2, InventoryFilterBusinessObject.Schema.Client, false);
			Helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute3, InventoryFilterBusinessObject.Schema.Warehouse, false);

			var inventoryFilter = new InventoryFilterBusinessObject(data.Org1);
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.ReceiveReference];
			filter.IsActive = true;
			AssertNoExceptionThrown(() =>
			{
				filter.Property = ""; // Can be any filter, just to generate filter list
			});

			AssertNotNull(inventoryFilter[InventoryFilterBusinessObject.Schema.Client + " - CA"]);
			AssertNotNull(inventoryFilter[InventoryFilterBusinessObject.Schema.Warehouse + " - CA"]);
		}

		#endregion

		#region TestCustomsEntryKeyFilter

		public void TestCustomsEntryKeyFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10.0m, "BEK1-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 16.0m, "BEK1-2");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 12.0m, "AEK2-1");

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.CustomsEntryKey];
			filter.IsActive = true;
			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should find all inventories.", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = "B";
			Asserter.AssertMatches("should find two inventories.", inventoryFilter.Filter, inventory1, inventory2);

			filter.Property = "BEK1-2";
			Asserter.AssertMatches("should find one inventory.", inventoryFilter.Filter, inventory2);

			filter.Property = "R";
			Asserter.AssertMatches("No match.", inventoryFilter.Filter);
		}

		#endregion

		#region TestEmptyFilter

		public void TestEmptyFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10.0m, "BEK1-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 16.0m, "BEK1-2");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 12.0m, "AEK2-1");
			Factory.Save();
			Asserter.AddToScope(inventory1, inventory2, inventory3);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			Asserter.AssertMatches("When filter is empty, should find all inventories.", inventoryFilter.Filter, inventory1, inventory2, inventory3);
		}

		#endregion

		#region TestPickAreaNameFilterMaxLength
		public void TestPickAreaNameFilterMaxLength()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var description = "Pick Area Name";
				var filter = (ModuleTextFilter)GetNewFilterStripBusinessObject()[description];
				AssertEquals("Max length should be:", WhsAreaSchema.WA_Name.MaxLength, filter.MaxLength);
			}
		}

		#endregion

		#region TestLoggedInOrgIsBeingPassedWhileGettingAttributes

		public void TestLoggedInOrgIsBeingPassedWhileGettingAttributes()
		{
			Enterprise.ZArchitecture.Environment.Globals.IsWeb = true;
			try
			{
				var testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var inventoryFilter = GetNewFilterStripBusinessObject(testLoggedInOrg);

				foreach (var filter in inventoryFilter)
				{
					var isComponentFilter = ComponentFilters.Contains(filter.Code.ToString());
					Assert("Only component attribute filters should exist.", (filter.Category != FilterCategories.AttributeSearch || isComponentFilter));
				}

				var loCA2 = testLoggedInOrg.CustomLabels.AddNew();
				loCA2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.WhsDocket.CustomAttribute2;
				loCA2.OT_Caption = "LoggedInOrgsCA2";

				Factory.Save();
				inventoryFilter = GetNewFilterStripBusinessObject(testLoggedInOrg);

				AssertNotNull("LoggedInOrg`s filter for CustomAttribute2 is here", inventoryFilter["LoggedInOrgsCA2 - CA"]);
				AssertEquals("Filter category", FilterCategories.AttributeSearch, inventoryFilter["LoggedInOrgsCA2 - CA"].Category);
			}
			finally
			{
				Enterprise.ZArchitecture.Environment.Globals.IsWeb = false;
			}
		}

		string[] ComponentFilters
			=> new[]
			{
				InventoryFilterBusinessObject.Schema.ComponentPartAttrib1,
				InventoryFilterBusinessObject.Schema.ComponentPartAttrib2,
				InventoryFilterBusinessObject.Schema.ComponentPartAttrib3,
				InventoryFilterBusinessObject.Schema.ComponentSerial,
				InventoryFilterBusinessObject.Schema.ComponentExpiryDate,
				InventoryFilterBusinessObject.Schema.ComponentPackingDate,
				InventoryFilterBusinessObject.Schema.ComponentEntryKey,
			};

		#endregion

		#region TestClientChangedEvent

		public void TestClientChangedEvent()
		{
			var client = Helper.CreateClient("TESTCLIENT", "TESTCLIENT");
			var product = Helper.CreateProduct(client, "TESTPRODUCT");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var clientFilter = (ModuleGuidFilter)filter.ModuleFilters[InventoryFilterBusinessObject.Schema.Client];
			var productFilter = (ModuleGuidFilter)filter.ModuleFilters[InventoryFilterBusinessObject.Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;
			FieldInvalidTextMemory.SetInvalidText(productFilter, productFilter.PropertyInfo.Name, product.OP_PartNum);
			AssertEquals("Precondition.", product.OP_PartNum, FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name));

			productFilter.Property = ZGuid.Missing;
			clientFilter.Property = client.PK;

			AssertEquals("Invalid Product code should be cleared out.", "", FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name));
		}

		#endregion

		#region TestFiterMaxLength

		public void TestFiterMaxLength()
		{
			var filterObject = GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Inventory filter should contain MaxLength for Row Name.", WhsRowSchema.WR_Name.MaxLength, filterObject.ModuleFilters["Row"].MaxLength);
				AssertEquals("Inventory filter should contain MaxLength for Receive Reference.", WhsDocketSchema.WD_ExternalReference.MaxLength, filterObject.ModuleFilters["Receive Reference"].MaxLength);
				AssertEquals("Inventory filter should contain MaxLength for Location Class.", WhsLocationTypeSchema.WLT_LocationClass.MaxLength, filterObject.ModuleFilters["LocationClass"].MaxLength);
			});
		}

		#endregion

		#region Simple Field Filters

		public void TestSimpleFieldFilters()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec1");
			receive.WD_DocketSubType = "CUS";
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond1 = Factory.New<WhsBondedWarehouseAttribute>();
			bond1.WB_RN_NKCountryOfOrigin = "AU";
			bond1.WB_DeclarationReference = "Declaration Reference Text";
			bond1.SetParent(inventoryLine1.InDocketLine);
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond2 = Factory.New<WhsBondedWarehouseAttribute>();
			bond2.WB_RN_NKCountryOfOrigin = "AU";
			bond2.WB_CustomsDeadline = ZDate.Today;
			bond2.SetParent(inventoryLine2.InDocketLine);
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond3 = Factory.New<WhsBondedWarehouseAttribute>();
			bond3.WB_RN_NKCountryOfOrigin = "AU";
			bond3.WB_InwardStyle = "InwardS";
			bond3.SetParent(inventoryLine3.InDocketLine);
			var inventoryLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond4 = Factory.New<WhsBondedWarehouseAttribute>();
			bond4.WB_RN_NKCountryOfOrigin = "AU";
			bond4.WB_InwardProcedure = "InwardP";
			bond4.SetParent(inventoryLine4.InDocketLine);
			var inventoryLine5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond5 = Factory.New<WhsBondedWarehouseAttribute>();
			bond5.WB_RN_NKCountryOfOrigin = "AU";
			bond5.WB_DeclarationReference = "Declaration Reference Text";
			bond5.WB_CustomsDeadline = ZDate.Today;
			bond5.WB_InwardStyle = "InwardS";
			bond5.WB_InwardProcedure = "InwardP";
			bond5.SetParent(inventoryLine5.InDocketLine);
			Factory.Save();
			Asserter.AddToScope(inventoryLine1, inventoryLine2, inventoryLine3, inventoryLine4, inventoryLine5);

			var inventoryFilter = GetNewFilterStripBusinessObject();
			var declarationReferenceFilter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.DeclarationReference];
			declarationReferenceFilter.IsActive = true;
			declarationReferenceFilter.Property = "Declaration Reference Text";
			var customsDeadlineFilter = (ModuleDateFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.CustomsDeadline];
			customsDeadlineFilter.IsActive = true;
			customsDeadlineFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			customsDeadlineFilter.Property1 = ZDateTime.Today.AddDays(-1);
			customsDeadlineFilter.Property2 = ZDateTime.Today.AddDays(1);
			var inwardStyleFilter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.InwardStyle];
			inwardStyleFilter.IsActive = true;
			inwardStyleFilter.Property = "InwardS";
			var inwardProcedureFilter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.InwardProcedure];
			inwardProcedureFilter.IsActive = true;
			inwardProcedureFilter.Property = "InwardP";

			Asserter.AssertMatches("Only bond5 which has property values on all fields should match InventoryFilter.", inventoryFilter.Filter, inventoryLine5);
		}

		#endregion

		#region TestAllocationKeyFilter

		public void TestAllocationKeyFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			inventory1.WI_AllocationKey = "ABC";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1);
			inventory2.WI_AllocationKey = "DEF";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part1, 1, true, false);
			var inventory3 = receive2.Inventory[0];
			inventory3.WI_AllocationKey = "GHI";
			Factory.Save();

			Asserter.AddToScope(inventory1, inventory2, inventory3);
			var inventoryFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)inventoryFilter[InventoryFilterBusinessObject.Schema.AllocationKey];
			filter.IsActive = true;
			filter.Property = "ABC";
			Asserter.AssertMatches("ABC", inventoryFilter.Filter, inventory1);

			filter.Property = "DEF";
			Asserter.AssertMatches("DEF", inventoryFilter.Filter, inventory2);

			filter.Property = "GHI";
			Asserter.AssertMatches("GHI", inventoryFilter.Filter, inventory3);

			filter.Property = "";
			Asserter.AssertMatches("All", inventoryFilter.Filter, inventory1, inventory2, inventory3);

			filter.Property = "JKL";
			Asserter.AssertMatches("JKL", inventoryFilter.Filter);
		}

		#endregion

		#region Implementation

		protected FilterStripAsserter<WhsInventoryView> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsInventoryView>(Factory, (i) => i.WI_PalletID));
		FilterStripAsserter<WhsInventoryView> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InventoryFilterBusinessObject();
		}

		protected FilterStripBusinessObject GetNewFilterStripBusinessObject(OrgHeader orgHeader)
		{
			return new InventoryFilterBusinessObject(orgHeader);
		}

		#region Helper

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		#endregion
	}
}
