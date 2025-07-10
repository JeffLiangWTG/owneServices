using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStockOnHandTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship =
				productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Factory.Save();

			var result1 = Load_Report_WhsStockOnHand();
			AssertEquals("ResultSet Count", 3, result1.Count); // should find everything

			var result2 =
				Load_Report_WhsStockOnHand_WithProductCategoryFilter(client.PK, warehouse.PK, categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result2.Count);

			var result3 =
				Load_Report_WhsStockOnHand_WithProductCategoryFilter(client.PK, warehouse.PK, categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result3.Count);

			var result4 =
				Load_Report_WhsStockOnHand_WithProductCategoryFilter(client.PK, warehouse.PK, categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection Load_Report_WhsStockOnHand_WithProductCategoryFilter(ZGuid clientPK,
			ZGuid warehousePK, ZGuid categoryPK)
		{
			string sql = @"select * from WhsStockOnHandReport(@ClientPK, @WarehousePK, null, @ProductCategoryPK)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ClientPK", clientPK, OrgHeaderSchema.PK);
			sqlParams.Add("@WarehousePK", warehousePK, WhsWarehouseSchema.PK);
			sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView_WithProductPKFilter

		public void TestView_WithProductPKFilter()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var productCoke = Helper.CreateProduct(client, "Coke");
			var productVB = Helper.CreateProduct(client, "VB");
			var receive1 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, location1, "PLT01", true, true);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productVB, 30m, location1, "PLT01", true, true);
			var receive3 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productCoke, 200m, location1, "PLT01", true, true);
			var receive4 = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R4", productTea, 10m, location1, "PLT01", true, true);

			Factory.Save();

			var result1 = Load_Report_WhsStockOnHand_WithProductPKFilter(client.PK, warehouse.PK, productVB.PK);
			AssertEquals(1, result1.Count);
			AssertRow(result1[0], receive1.Lines[0], expectedTotalUnits: 130m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);

			var result2 = Load_Report_WhsStockOnHand_WithProductPKFilter(client.PK, warehouse.PK, productCoke.PK);
			AssertEquals(1, result2.Count);
			AssertRow(result2[0], receive3.Lines[0], expectedTotalUnits: 200m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);

			var result3 = Load_Report_WhsStockOnHand_WithProductPKFilter(client.PK, warehouse.PK, productTea.PK);
			AssertEquals(1, result3.Count);
			AssertRow(result3[0], receive4.Lines[0], expectedTotalUnits: 10m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		DynamicBusinessObjectCollection Load_Report_WhsStockOnHand_WithProductPKFilter(ZGuid clientPK, ZGuid warehousePK, ZGuid productPK)
		{
			string sql = @"select * from WhsStockOnHandReport(@ClientPK, @WarehousePK, @ProductPK, null)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ClientPK", clientPK, OrgHeaderSchema.PK);
			sqlParams.Add("@WarehousePK", warehousePK, WhsWarehouseSchema.PK);
			sqlParams.Add("@ProductPK", productPK, OrgSupplierPartSchema.PK);

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView

		public void TestView()
		{
			var receives = SetupData();
			receives[3].Inventory[1].OriginalInventoryHeldCode = "123";
			receives[3].Inventory[1].InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "ABC";
			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("ResultSet Count", 10, result.Count);

			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 111m), receives[0].Inventory[0].InDocketLine, 111m, 0m, 11m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 121m), receives[1].Inventory[0].InDocketLine, 121m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 112m), receives[0].Inventory[1].InDocketLine, 112m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 122m), receives[1].Inventory[1].InDocketLine, 122m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 123m), receives[4].Inventory[0].InDocketLine, 123m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 321m), receives[4].Inventory[1].InDocketLine, 321m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 211m), receives[2].Inventory[0].InDocketLine, 211m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 212m), receives[2].Inventory[1].InDocketLine, 212m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 221m), receives[3].Inventory[0].InDocketLine, 221m, 0m, 0m);
			AssertRow(result.Single(r => (ZDecimal)(r["TotalUnits"]) == 222m), receives[3].Inventory[1].InDocketLine, 222m, 0m, 0m);
		}

		#endregion

		#region TestView_ExcludesZeroUnitPickLines

		[TestDate(2015, 1, 1)]
		public void TestView_ExcludesZeroUnitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(2014, 1, 1);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("It should not consider the order as a movement.", new ZDateTimeOffset(2014, 1, 1),
				result[0]["LastMovementDate"]);
		}

		#endregion

		#region TestView_InTransitInventory_Order

		public void TestView_InTransitInventory_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderPick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var inTransitTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should have 1 Receive Inventory and 1 In-Transit Inventory.", 2, results.Count);

			var availableResult = results.Single(o => o["Status"].ToString() == InventoryStatus.Codes.Available);
			var inTransitResult = results.Single(o => o["Status"].ToString() == InventoryStatus.Codes.InTransit);
			AssertRow(availableResult, inventory.InDocketLine, expectedTotalUnits: 6m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m);
			AssertRow(inTransitResult, inTransitTransferLine, expectedTotalUnits: 4m, expectedCommittedUnits: 4m,
				expectedReservedUnits: 0m);
		}

		#endregion

		#region TestView_InTransitInventory_Transfer

		public void TestView_InTransitInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 6m, location1, location2);
			transferLine1.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer line is Picked.", true, transferLine1.IsPicked);
			Factory.Save();

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should have 1 Receive Inventory and 1 In-Transit Inventory.", 2, results.Count);
			var availableResult = results.Single(o => o["Status"].ToString() == InventoryStatus.Codes.Available);
			var inTransitResult = results.Single(o => o["Status"].ToString() == InventoryStatus.Codes.InTransit);
			AssertRow(availableResult, inventory.InDocketLine, expectedTotalUnits: 4m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m);
			AssertRow(inTransitResult, transferLine1, expectedTotalUnits: 6m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m);
		}

		#endregion

		public void TestSOHView_PuttingAwayInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var now = ZDateTimeOffset.Now;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLT1", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = now;
			Factory.Save();

			AssertEquals(
				$"InventoryStatus of Precondition: Transfer Line should be {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"InventoryStatus of Precondition: Inventory should be {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should have 1 resulting Inventory.", 1, results.Count);
			AssertRow(results[0], transferLine, expectedTotalUnits: 10m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		public void TestSOHView_PutAwayInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLT1", 10m);
			transfer.FinaliseDocket();
			Factory.Save();

			AssertEquals($"InventoryStatus of Precondition: Transfer Line should be {InventoryStatus.Codes.Putaway}.",
				InventoryStatus.Codes.Putaway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"InventoryStatus of Precondition: Inventory should be {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should have 1 Received Inventory.", 1, results.Count);
			var putawayResult = results.Single(o => o["Status"].ToString() == InventoryStatus.Codes.Putaway);
			AssertRow(putawayResult, transferLine, expectedTotalUnits: 10m, expectedCommittedUnits: 0m,
				expectedReservedUnits: 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		#region TestView_StagedInventory

		public void TestView_StagedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("ResultSet Count", 2, result.Count);
			AssertRow(result[0], originalInventory, 12m, 0m, 0m);
			AssertRow(result[1], transferLine, 8m, 8m, 0m);
		}

		#endregion

		#region TestView_ReadyToPackInventory

		public void TestView_ReadyToPackInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("ResultSet Count", 2, result.Count);
			var result0 = result.Single(o => o["Status"].ToString() == InventoryStatus.Codes.Available);
			var result1 = result.Single(o => o["Status"].ToString() == InventoryStatus.Codes.ReadyToPack);
			AssertRow(result0, originalInventory, 12m, 0m, 0m);
			AssertRow(result1, transferLine, 8m, 8m, 0m);
		}

		#endregion

		#region TestView_Transfers

		[TestDate(2014, 01, 14, 5, 5, 0)]
		public void TestView_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var today = ZDateTimeOffset.Today;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine =
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -7m, data.Whs1.FindLocation("A-1"));
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 7m, adjustmentLine.CommittedQuantity);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, "A-1", "A-2");
			var transfer1Line2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 15m, "A-1", "A-2");
			var transfer1Line3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 15m, "A-2", "A-3");
			transfer1Line2.FinaliseDocketLine();
			transfer1.RunPreSaveValidation(); // to commit stock.
			AssertIsFinalisedPrecondition(transfer1Line2);
			AssertEquals(
				"Precondition - make sure that we take stock from finalised transfer line, while transfer is not finalised.",
				15m, transfer1Line2.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 20m, "A-1", "A-3");
			var transfer2Line2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 25m, "A-1", "A-4");
			transfer2.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer2);

			// hack to change finalised dates.
			transfer1Line2.WE_FinalisedDate = today.AddDays(-3);
			transfer2Line1.WE_FinalisedDate = today.AddDays(-2);
			transfer2Line2.WE_FinalisedDate = today.AddDays(-1);
			transfer2.WD_FinalisedDate = today.AddDays(+1);

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("Should be 1 Receive + 3 Transfers transaction.", 4, result.Count);
			AssertLineMatch(receive.Lines[0], result, 40m, 17m, 0m);
			AssertLineMatch(transfer1Line2, result, 15m, 15m, 0m);
			AssertLineMatch(transfer2Line1, result, 20m, 0m, 0m);
			AssertLineMatch(transfer2Line2, result, 25m, 0m, 0m);
		}

		void AssertLineMatch(WhsDocketLine expectedDocketLine, DynamicBusinessObjectCollection results,
			ZDecimal expectedTotalUnits, ZDecimal expectedCommittedUnits, ZDecimal expectedReservedUnits)
		{
			var matchingLine = results.Single(l =>
				(ZGuid)l["ProductPK"] == expectedDocketLine.WE_OP &&
				(ZString)l["Location"] == expectedDocketLine.LocationString);
			AssertRow(matchingLine, expectedDocketLine, expectedTotalUnits, expectedCommittedUnits,
				expectedReservedUnits);
		}

		#endregion

		#region TestView_PutawayTransfers

		public void TestView_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var today = ZDateTime.Today;
			var dockDoorLocation1 = data.Whs1.FindLocation("A-2");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var dockDoorLocation2 = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLoc1 = data.Whs1.FindLocation("A-1");
			var normalLoc2 = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receivedInv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, dockDoorLocation1, "PLT3");

			var receiveWithPutawayTransfer = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var putawayInv = Helper.CreateWhsReceiveInventoryLine(receiveWithPutawayTransfer, data.Part2, 22m,
				dockDoorLocation2, "PLT2");
			Factory.Save();

			var putawayTransfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var putAwayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part2,
				dockDoorLocation2, normalLoc2, "PLT2", 22m);
			putAwayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer2.FinaliseDocketWithoutUserConfirmation();
			putAwayTransferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			Factory.Save();

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should return 1 Receive + 1 Transfers.", 2, results.Count);
			var putAwayResult = results.Single(l => l["Status"].ToString() == InventoryStatus.Codes.Putaway);
			var receivedResult = results.Single(l => l["Status"].ToString() == InventoryStatus.Codes.Received);
			AssertRow(putAwayResult, putAwayTransferLine, 22m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
			AssertRow(receivedResult, receivedInv.InDocketLine, 8m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		#endregion

		#region TestAttribsAndPalletID

		public void TestAttribsAndPalletID()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Slip";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Slop";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Slap";

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, data.Whs1.FindLocation("A"),
				"PALLETID", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			Helper.SetAllCustomLabels(data.Org1,
				new WhsInventoryView.CustomLabelsProvider(inventory).GetCustomFields(data.Org1, Factory), true);
			Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m,
				new ZDateTime(year, 1, 3), new ZDateTime(year, 1, 4), new ZDateTime(year, 1, 5),
				new ZDateTime(year, 1, 6), new ZDateTime(year, 1, 7), true, true, true, true, true, "Blob");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(1, result.Count);
			AssertRow(result[0], inventory.InDocketLine, 11m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		public void TestAttribsAndPalletID_SerialNumber()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A"),
				"PALLETID", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			inventory.WI_SerialNumber = "SN";
			Helper.SetAllCustomLabels(data.Org1,
				new WhsInventoryView.CustomLabelsProvider(inventory).GetCustomFields(data.Org1, Factory), true);
			Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m,
				new ZDateTime(year, 1, 3), new ZDateTime(year, 1, 4), new ZDateTime(year, 1, 5),
				new ZDateTime(year, 1, 6), new ZDateTime(year, 1, 7), true, true, true, true, true, "Blob");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(1, result.Count);
			AssertRow(result[0], inventory.InDocketLine, 1m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		#endregion

		#region TestStockOnHandReport_TotalPallets

		public void TestStockOnHandReport_TotalPallets_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var whs1 = data.Whs1;
			var location1 = whs1.FindLocation("A-1");
			var whs2 = Helper.CreateWarehouse("WHS2");
			var row = Helper.CreateRowAndGenerateLocations(whs2, "B", 4, 1);
			var location2 = row.Locations[0];

			var receive1 = Helper.CreateWhsReceive(data.Org1, whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, location1, "PLT1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 2m, location2, "PLT1");
			var receive3 = Helper.CreateWhsReceive(data.Org1, whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 3m, location1, "PLT2");
			var receive4 = Helper.CreateWhsReceive(data.Org1, whs1, "R4", Notify);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part1, 4m, location1, "");
			var receive5 = Helper.CreateWhsReceive(data.Org1, whs2, "R5", Notify);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive5, data.Part1, 5m, location2, "");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(5, result.Count);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(1m)), receive1.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(2m)), receive2.Lines[0], 2m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(3m)), receive3.Lines[0], 3m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(4m)), receive4.Lines[0], 4m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(5m)), receive5.Lines[0], 5m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 1);
		}

		public void TestStockOnHandReport_TotalPallets_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var org1 = data.Org1;
			var part1 = data.Part1;
			var org2 = Helper.CreateClient("C1");
			var partRelation = Helper.CreateProductClientRelationShip(org2, part1, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			var receive1 = Helper.CreateWhsReceive(org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part1, 2m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receive3 = Helper.CreateWhsReceive(org1, data.Whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, part1, 3m, data.Whs1.FindLocation("A-1"), "PLT2");
			var receive4 = Helper.CreateWhsReceive(org1, data.Whs1, "R4", Notify);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, part1, 4m, data.Whs1.FindLocation("A-1"), "");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(4, result.Count);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(1m)), receive1.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(2m)), receive2.Lines[0], 2m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(3m)), receive3.Lines[0], 3m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(4m)), receive4.Lines[0], 4m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 2);
		}

		public void TestStockOnHandReport_TotalPallets_DifferentAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			inventory1.WI_PartAttrib1 = "001";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			inventory2.WI_PartAttrib1 = "002";

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			inventory3.WI_PartAttrib1 = "003";

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4", Notify);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part2, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			inventory4.WI_PartAttrib1 = "004";

			var receive5 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R5", Notify);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive5, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT3");
			inventory5.WI_PartAttrib1 = "005";

			var receive6 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R6", Notify);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive6, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT4");
			inventory6.WI_PartAttrib1 = "006";

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(6, result.Count);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("001")), receive1.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 1);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("002")), receive2.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 1);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("003")), receive3.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("004")), receive4.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("005")), receive5.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["PartAttrib1"].Equals("006")), receive6.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
		}

		public void TestStockOnHandReport_TotalPallets_DifferentSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			inventory1.WI_SerialNumber = "001";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			inventory2.WI_SerialNumber = "002";

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			inventory3.WI_SerialNumber = "003";

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4", Notify);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part2, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			inventory4.WI_SerialNumber = "004";

			var receive5 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R5", Notify);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive5, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT3");
			inventory5.WI_SerialNumber = "005";

			var receive6 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R6", Notify);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive6, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT4");
			inventory6.WI_SerialNumber = "006";

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(6, result.Count);
			AssertRow(result.First(r => r["SerialNumber"].Equals("001")), receive1.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 1);
			AssertRow(result.First(r => r["SerialNumber"].Equals("002")), receive2.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 1);
			AssertRow(result.First(r => r["SerialNumber"].Equals("003")), receive3.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["SerialNumber"].Equals("004")), receive4.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 1, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["SerialNumber"].Equals("005")), receive5.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
			AssertRow(result.First(r => r["SerialNumber"].Equals("006")), receive6.Lines[0], 1m, 0m, 0m, expectedPalletsPerProduct: 4, expectedPalletsPerLocation: 3);
		}

		public void TestStockOnHandReport_TotalPallets_NonePalletID_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var whs1 = data.Whs1;
			var location1 = whs1.FindLocation("A-1");
			var whs2 = Helper.CreateWarehouse("WHS2");
			var row = Helper.CreateRowAndGenerateLocations(whs2, "B", 4, 1);
			var location2 = row.Locations[0];

			var receive1 = Helper.CreateWhsReceive(data.Org1, whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, location1, "");
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 2m, location2, "");
			var receive3 = Helper.CreateWhsReceive(data.Org1, whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 3m, location1, "");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(2, result.Count);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(2m)), receive2.Lines[0], 2m, 0m, 0m, expectedPalletsPerProduct: 0, expectedPalletsPerLocation: 0);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(4m)), receive3.Lines[0], 4m, 0m, 0m, expectedPalletsPerProduct: 0, expectedPalletsPerLocation: 0); // inventory1 and inventory3 combined
		}

		public void TestStockOnHandReport_TotalPallets_NonePalletID_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var org1 = data.Org1;
			var part1 = data.Part1;
			var org2 = Helper.CreateClient("C1");
			var partRelation = Helper.CreateProductClientRelationShip(org2, part1, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			var receive1 = Helper.CreateWhsReceive(org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part1, 2m, data.Whs1.FindLocation("A-1"), "");
			var receive3 = Helper.CreateWhsReceive(org1, data.Whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, part1, 3m, data.Whs1.FindLocation("A-1"), "");

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(2, result.Count);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(2m)), receive2.Lines[0], 2m, 0m, 0m, expectedPalletsPerProduct: 0, expectedPalletsPerLocation: 0);
			AssertRow(result.Single(r => r["TotalUnits"].Equals(4m)), receive3.Lines[0], 4m, 0m, 0m, expectedPalletsPerProduct: 0, expectedPalletsPerLocation: 0); // inventory1 and inventory3 combined
		}

		#endregion

		#region TestStockOnHandReport_LastMovementDate_WithTransfers

		[TestDate(2014, 1, 1)]
		public void TestStockOnHandReport_LastMovementDate_WithTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 15m, "A-1", "A-2");
			transfer1.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer1Line1);
			AssertIsFinalisedPrecondition(transfer1);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 20m, "A-1", "A-2");
			transfer2Line1.FinaliseDocketLine();
			transfer2Line1.WE_FinalisedDate = new ZDateTimeOffset(2014, 1, 2);
			AssertIsFinalisedPrecondition(transfer2Line1);

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("Should be 1 Receive + 1 group of 2 Transfer transactions.", 2, result.Count);
			AssertEquals("Inventory last movement date should be the second transfer line's finalised date.",
				new ZDateTimeOffset(2014, 1, 2), result[0]["LastMovementDate"]);
			AssertEquals("Transfer should show the second transfer line's finalised date,", new ZDateTimeOffset(2014, 1, 2),
				result[1]["LastMovementDate"]);
		}

		#endregion

		#region TestStockOnHandReport_LastMovementDate_SerialNumber

		[TestDate(2014, 1, 1)]
		public void TestStockOnHandReport_LastMovementDate_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			CreateReceiveLine("SN1");
			CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, "A-1", "A-2");
			transfer1Line1.WE_SerialNumber = "SN1";
			transfer1.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer1Line1);
			AssertIsFinalisedPrecondition(transfer1);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, "A-1", "A-2");
			transfer2Line1.WE_SerialNumber = "SN3";
			transfer2Line1.FinaliseDocketLine();
			transfer2Line1.WE_FinalisedDate = new ZDateTimeOffset(2014, 1, 2);
			AssertIsFinalisedPrecondition(transfer2Line1);

			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("Should be 2 Transfer transactions.", 2, result.Count);
			AssertEquals("Inventory last movement date should be the first transfer finalised date.",
				transfer1Line1.WE_FinalisedDate, result[0]["LastMovementDate"]);
			AssertEquals("Inventory last movement date should be the second transfer finalised date.",
				transfer2Line1.WE_FinalisedDate, result[1]["LastMovementDate"]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestStockOnHandReport_LastMovementDate

		[TestDate(2014, 01, 14, 5, 5, 0)]
		public void TestStockOnHandReport_LastMovementDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1),
				data.Part1, 100m, data.Whs1.DefaultLocation, "PLT1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today.AddDays(-2),
				data.Part1, 150m, data.Whs1.DefaultLocation, "PLT1");

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", today, data.Part1, 150m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var result = Load_Report_WhsStockOnHand();
			AssertEquals("Should rollup all transactions into 1.", 1, result.Count);
			AssertEquals("LastMovementDate", order.WD_FinalisedDate, result[0]["LastMovementDate"]);
		}

		#endregion

		#region TestWhenStockHasDifferentOwnerToProduct

		public void TestWhenStockHasDifferentOwnerToProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var differentClient = Helper.CreateClient("Client2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			// change owner on product
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_OH = differentClient.PK;
			Factory.Save();

			// should return stock even if the owner on product is wrong
			var result = Load_Report_WhsStockOnHand();
			AssertEquals(1, result.Count);

			// just to prevent assertion from blowing up with null reference when it can't find the owner relationship
			relation.OU_OH = data.Org1.PK;
			AssertRow(result[0], receive.Lines[0], 10m, 0m, 0m);
		}

		#endregion

		#region TestUNDGFields

		public void TestUNDGFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			AddUNDGToProduct(data.Part1, "DG1", "SHP1", "CLS1", "SR1", "SR2", "PK1");
			AddUNDGToProduct(data.Part1, "DG2", "SHP2", "CLS2", "SR3", "SR4", "PK2");
			AddUNDGToProduct(data.Part2, "DG3", "SHP3", "CLS3", "SR5", "SR6", "PK3");
			AddUNDGToProduct(data.Part2, "DG1", "SHP1", "CLS1", "SR1", "SR2", "PK1");
			Factory.Save();

			AddUNDGToProduct(data.Part2, "DG3", "SHP3", "CLS3", "SR5", "SR6", "PK3");
			Factory.Save();

			var result = Load_Report_WhsStockOnHand();
			AssertEquals(2, result.Count);

			AssertUNDGItem(result[0], data.Part1, "DG1\r\nDG2", "SHP1\r\nSHP2", "CLS1\r\nCLS2", "SR1\r\nSR3",
				"SR2\r\nSR4", "PK1\r\nPK2");
			AssertUNDGItem(result[1], data.Part2, "DG1\r\nDG3\r\nDG3", "SHP1\r\nSHP3\r\nSHP3", "CLS1\r\nCLS3\r\nCLS3",
				"SR1\r\nSR5\r\nSR5", "SR2\r\nSR6\r\nSR6", "PK1\r\nPK3\r\nPK3");
		}

		public void TestView_ProductWithHugeUNDGCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			for (var i = 0; i < 2001; i++)
			{
				AddUNDGToProduct(data.Part1, "DG11", "SHP1", "CLS1", "SR12", "SR22", "PK1");
			}
			Factory.Save();

			DynamicBusinessObjectCollection result = null;
			AssertNoExceptionThrown(() => result = Load_Report_WhsStockOnHand());
			AssertEquals(1, result.Count);
			Assert("UNDGNumber is not empty", !((ZString)(result[0]["UNDGNumber"])).IsEmpty);
		}

		void AddUNDGToProduct(OrgSupplierPart part, string undgNumber, string shippingName, string imoClass,
			string subRisk1, string subRisk2, string packingGroup)
		{
			var query = new ZQuery(UNDGSubstanceSchema.DG_Code, undgNumber);
			var substance = Factory.LoadTop1<UNDGSubstance>(query) ?? Factory.New<UNDGSubstance>();
			if (substance.DG_Code.IsEmpty)
			{
				substance.DG_Code = undgNumber;
				substance.DG_PSN = shippingName;
				substance.DG_Class = imoClass;
				substance.DG_SubLabel1 = subRisk1;
				substance.DG_SubLabel2 = subRisk2;
				substance.DG_PG = packingGroup;
				substance.DG_UNNO = undgNumber;
			}

			var undg = part.UNDGs.AddNew();

			var subsPivot = Factory.New<UNDGSubstancePivot>();
			subsPivot.DP_UNNO = substance.DG_UNNO;
			subsPivot.DP_Variant = substance.DG_Variant;
			subsPivot.DP_ParentId = undg.PK;
			subsPivot.DP_Standard = "IMO";
			subsPivot.DP_ParentTableCode = substance.TablePrefix;
			subsPivot.DP_IsDefault = true;
		}

		void AssertUNDGItem(DynamicBusinessObject bizO, OrgSupplierPart expectedPart, string expectedUNDGNumber,
			string expectedShippingName, string expectedClass, string expectedSubRisk1, string expectedSubRisk2,
			string expectedPackGroup)
		{
			AssertEquals(expectedPart.PK, bizO["ProductPK"]);
			AssertEquals("UNDGNumber", expectedUNDGNumber, bizO["UNDGNumber"]);
			AssertEquals("UNDGProperShippingName", expectedShippingName, bizO["UNDGProperShippingName"]);
			AssertEquals("UNDGIMOClass", expectedClass, bizO["UNDGIMOClass"]);
			AssertEquals("UNDGSubRisk1", expectedSubRisk1, bizO["UNDGSubRisk1"]);
			AssertEquals("UNDGSubRisk2", expectedSubRisk2, bizO["UNDGSubRisk2"]);
			AssertEquals("UNDGPackingGroup", expectedPackGroup, bizO["UNDGPackingGroup"]);
		}

		#endregion

		#region TestReportShowsDockDoorLocations

		public void TestReportShowsDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var now = ZDateTime.Now;

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-4");
			var dockDoorLocation1 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-3");
			var dockDoorLocation3 = data.Whs1.DefaultOutboundDockDoorLocation;
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1");
			var receivedInventory = Helper
				.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, dockDoorLocation1, "PLT12345").InDocketLine;
			var putawayInventory = Helper
				.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, nonDockDoorLocation1, "PLT135").InDocketLine;
			Factory.Save();

			var results = Load_Report_WhsStockOnHand();
			AssertEquals("Should only have returned the Product Warehouse.", 2, results.Count);
			var putAwayResult = results.Single(l => l["Status"].ToString() == InventoryStatus.Codes.Putaway);
			var receivedResult = results.Single(l => l["Status"].ToString() == InventoryStatus.Codes.Received);
			AssertRow(putAwayResult, putawayInventory, 3m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 1);
			AssertRow(receivedResult, receivedInventory, 1m, 0m, 0m, expectedPalletsPerProduct: 2, expectedPalletsPerLocation: 1);
		}

		#endregion

		#region TestView_FixedWidthLocationWarehouse

		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, warehouse);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, warehouse.FindLocation("LOCZ0040302"));
			Factory.Save();

			var results = Load_Report_WhsStockOnHand();
			var result = results.Single();
			AssertEquals("LOCZ-004-03-02", result["Location"].ToString());
			AssertRow(result, inventory.InDocketLine, 1m, 0m, 0m);
		}

		#endregion

		#region Load_Report_WhsStockOnHand

		DynamicBusinessObjectCollection Load_Report_WhsStockOnHand()
		{
			var sql = @"
select
	*
from
	WhsStockOnHandReport(null, null, null, null)
order by
	WarehouseName,
	Client,
	Product,
	Location,
	SerialNumber";

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		#endregion

		#region SetupData

		WhsReceive[] SetupData()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 2);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var client1 = Helper.CreateClient("1", "1");
			var client2 = Helper.CreateClient("2", "2");
			var part1 = Helper.CreateProduct(client1, "1", OrgPartRelation.RelationshipTypes.Both);
			var part2 = Helper.CreateProduct(client2, "2");
			var part3 = Helper.CreateProduct(client1, "3");

			var part4 = Helper.CreateProduct(client1, "4");
			var category1 = Helper.CreateProductCategory(client1, part4, "Cat1");
			category1.OPC_CategoryDescription = "Category Description 1";
			var part5 = Helper.CreateProduct(client1, "5");
			var category2 = Helper.CreateProductCategory(client1, part5, "Cat2");
			category2.OPC_CategoryDescription = "Category Description 2";

			part1.OP_StockKeepingUnit = "CTN";

			part2.OP_Brand = "PRO BRAND";
			part2.OP_Cubic = 11m;
			part2.OP_Model = "PRO MODEL";
			part2.OP_Weight = 44m;

			part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(client1, OrgPartRelation.RelationshipTypes.Owner).OU_ClientUQ =
				"PLT";

			var partUnit = part1.PartUnits.AddNew();
			partUnit.OF_PackType = "CTN";
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 20;

			part1.OP_LastCost = 3m;
			part1.OP_RX_NKLastWeightedCostCurr = "AUD";
			part2.OP_LastCost = 4m;
			part2.OP_RX_NKLastWeightedCostCurr = "USD";
			part3.OP_LastCost = 5m;
			part3.OP_RX_NKLastWeightedCostCurr = "EUR";

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part3.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			// product 3 is owned by both clients
			var relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part3.PK;
			relation.OU_OH = client2.PK;

			SetClientAttributesType(client1);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client1, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client1, part3);
			SetProductAttributesUse(client2, part3);

			var receive1 = SetupReceive(client1, whs1, "11", part1, 111, part3, 112, true);
			var receive2 = SetupReceive(client1, whs1, "12", part1, 121, part3, 122, false);
			var receive3 = SetupReceive(client2, whs1, "21", part2, 211, part3, 212, true);
			var receive4 = SetupReceive(client2, whs2, "22", part2, 221, part3, 222, false);
			var receive5 = SetupReceive(client1, whs1, "13", part4, 123, part5, 321, false);

			var order1 = Helper.CreateWhsOrder(client1, whs1, "O1");
			var order2 = Helper.CreateWhsOrder(client1, whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, part1, 7m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(receive1.Inventory[0]);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(receive1.Inventory[0]);
			AssertNotNull("Precondition - reserved line created.", reservedPickLine1);
			AssertNotNull("Precondition - reserved line created.", reservedPickLine2);

			reservedPickLine1.WZ_Units = 5m;
			reservedPickLine2.WZ_Units = 6m;
			AssertEquals("Precondition - Original reserved Qty should not have changed.", 10m,
				reservedPickLine1.WZ_OriginalReservedQty);
			AssertEquals("Precondition - Original reserved Qty should not have changed.", 7m,
				reservedPickLine2.WZ_OriginalReservedQty);

			Factory.Save();

			return new[] { receive1, receive2, receive3, receive4, receive5 };
		}

		void SetClientAttributesType(OrgHeader client)
		{
			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
		}

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part1,
			ZDecimal units1, OrgSupplierPart part2, ZDecimal units2, ZBool finaliseDocket)
		{
			var year = ZDateTime.Now.Year;

			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, new ZDate(year, 1, 1), new ZDate(year, 2, 1),
				"PA1", "PA2", "PA3", "BEK");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, new ZDate(year, 1, 1), new ZDate(year, 2, 1),
				"PA1", "PA2", "PA3", "BEK");
			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPutaway", true, receive.IsPuttingAway);

			if (finaliseDocket)
			{
				receive.FinaliseDocket();
				AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
			}

			return receive;
		}

		#endregion

		#region AssertRow

		void AssertRow(DynamicBusinessObject dynamicObject, WhsDocketLine docketLine, ZDecimal expectedTotalUnits,
			ZDecimal expectedCommittedUnits, ZDecimal expectedReservedUnits, int expectedPalletsPerProduct = 0, int expectedPalletsPerLocation = 0)
		{
			var docket = docketLine.Docket;
			var client = docket.Client;
			var miscServ = client.MiscServ;
			var warehouse = docket.Warehouse;
			var supplierPart = docketLine.SupplierPart;
			var partRelation =
				supplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category?.OPC_CategoryCode ?? ZString.Empty;
			var expectedCategoryDescription = category?.OPC_CategoryDescription ?? ZString.Empty;
			var expectedPalletSpaces = (supplierPart.OP_StockKeepingUnitPerPallet > 0)
				? Math.Ceiling(expectedTotalUnits / supplierPart.OP_StockKeepingUnitPerPallet)
				: 0m;
			var expectedCommodityPK = (!supplierPart.OP_RH_NKCommodityCode.IsEmpty)
				? supplierPart.CommodityCode.PK
				: ZGuid.Empty;
			var finalisedDate = (docket.WD_DocketType == DocketType.Codes.Transfer)
				? docketLine.WE_FinalisedDate
				: docket.WD_FinalisedDate;
			var expectedLastMovementDate = (!finalisedDate.IsEmpty)
				? finalisedDate.ToSmallDateTimeFloor()
				: docketLine.WE_AdjustmentArrivalDate.ToSmallDateTimeFloor();

			CombineAssertions(() =>
			{
				AssertEquals("WarehousePK", warehouse.PK, dynamicObject["WarehousePK"]);
				AssertEquals("WarehouseName", warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
				AssertEquals("ClientPK", client.PK, dynamicObject["ClientPK"]);
				AssertEquals("ClientCode", client.OH_Code, dynamicObject["ClientCode"]);
				AssertEquals("Client", client.OH_FullName, dynamicObject["Client"]);
				AssertEquals("Product", supplierPart.OP_PartNum, dynamicObject["Product"]);
				AssertEquals("ProductDesc", supplierPart.OP_Desc, dynamicObject["ProductDesc"]);
				AssertEquals("ProductPK", supplierPart.PK, dynamicObject["ProductPK"]);
				AssertEquals("ProductBrandName", supplierPart.OP_Brand, dynamicObject["ProductBrandName"]);
				AssertEquals("ProductModel", supplierPart.OP_Model, dynamicObject["ProductModel"]);
				AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
				AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
					dynamicObject["ProductCategoryDescription"]);
				AssertEquals("WeightUQ", supplierPart.OP_WeightUQ, dynamicObject["WeightUQ"]);
				AssertEquals("VolumeUQ", supplierPart.OP_CubicUQ, dynamicObject["VolumeUQ"]);
				AssertEquals("ClientUnit", partRelation.OU_ClientUQ, dynamicObject["ClientUnit"]);
				AssertEquals("CommodityCode", supplierPart.OP_RH_NKCommodityCode, dynamicObject["CommodityCode"]);
				AssertEquals("CommodityPK", expectedCommodityPK, dynamicObject["CommodityPK"]);
				AssertEquals("Arrival Date", docketLine.WE_AdjustmentArrivalDate.Date,
					((ZDateTime)dynamicObject["ArrivalDate"]).Date);
				AssertEquals("Status", docketLine.WE_CurrentInventoryStatus, dynamicObject["Status"]);
				AssertEquals("PartAttrib1", docketLine.WE_PartAttrib1, dynamicObject["PartAttrib1"]);
				AssertEquals("PartAttrib2", docketLine.WE_PartAttrib2, dynamicObject["PartAttrib2"]);
				AssertEquals("PartAttrib3", docketLine.WE_PartAttrib3, dynamicObject["PartAttrib3"]);
				AssertEquals("SerialNumber", docketLine.WE_SerialNumber, dynamicObject["SerialNumber"]);
				AssertEquals("WhsDocketLine_CustomAttrib1", docketLine.WE_CustomAttrib1,
					dynamicObject["WhsDocketLine_CustomAttrib1"]);
				AssertEquals("WhsDocketLine_CustomAttrib2", docketLine.WE_CustomAttrib2,
					dynamicObject["WhsDocketLine_CustomAttrib2"]);
				AssertEquals("WhsDocketLine_CustomAttrib3", docketLine.WE_CustomAttrib3,
					dynamicObject["WhsDocketLine_CustomAttrib3"]);
				AssertEquals("WhsDocketLine_CustomAttrib4", docketLine.WE_CustomAttrib4,
					dynamicObject["WhsDocketLine_CustomAttrib4"]);
				AssertEquals("WhsDocketLine_CustomAttrib5", docketLine.WE_CustomAttrib5,
					dynamicObject["WhsDocketLine_CustomAttrib5"]);
				AssertEquals("WhsDocketLine_CustomAttrib6", docketLine.WE_CustomAttrib6,
					dynamicObject["WhsDocketLine_CustomAttrib6"]);
				AssertEquals("WhsDocketLine_CustomDate1", docketLine.WE_CustomDate1,
					dynamicObject["WhsDocketLine_CustomDate1"]);
				AssertEquals("WhsDocketLine_CustomDate2", docketLine.WE_CustomDate2,
					dynamicObject["WhsDocketLine_CustomDate2"]);
				AssertEquals("WhsDocketLine_CustomDate3", docketLine.WE_CustomDate3,
					dynamicObject["WhsDocketLine_CustomDate3"]);
				AssertEquals("WhsDocketLine_CustomDate4", docketLine.WE_CustomDate4,
					dynamicObject["WhsDocketLine_CustomDate4"]);
				AssertEquals("WhsDocketLine_CustomDate5", docketLine.WE_CustomDate5,
					dynamicObject["WhsDocketLine_CustomDate5"]);
				AssertEquals("WhsDocketLine_CustomDecimal1", docketLine.WE_CustomDecimal1,
					dynamicObject["WhsDocketLine_CustomDecimal1"]);
				AssertEquals("WhsDocketLine_CustomDecimal2", docketLine.WE_CustomDecimal2,
					dynamicObject["WhsDocketLine_CustomDecimal2"]);
				AssertEquals("WhsDocketLine_CustomDecimal3", docketLine.WE_CustomDecimal3,
					dynamicObject["WhsDocketLine_CustomDecimal3"]);
				AssertEquals("WhsDocketLine_CustomDecimal4", docketLine.WE_CustomDecimal4,
					dynamicObject["WhsDocketLine_CustomDecimal4"]);
				AssertEquals("WhsDocketLine_CustomDecimal5", docketLine.WE_CustomDecimal5,
					dynamicObject["WhsDocketLine_CustomDecimal5"]);
				AssertEquals("WhsDocketLine_CustomFlag1", docketLine.WE_CustomFlag1,
					dynamicObject["WhsDocketLine_CustomFlag1"]);
				AssertEquals("WhsDocketLine_CustomFlag2", docketLine.WE_CustomFlag2,
					dynamicObject["WhsDocketLine_CustomFlag2"]);
				AssertEquals("WhsDocketLine_CustomFlag3", docketLine.WE_CustomFlag3,
					dynamicObject["WhsDocketLine_CustomFlag3"]);
				AssertEquals("WhsDocketLine_CustomFlag4", docketLine.WE_CustomFlag4,
					dynamicObject["WhsDocketLine_CustomFlag4"]);
				AssertEquals("WhsDocketLine_CustomFlag5", docketLine.WE_CustomFlag5,
					dynamicObject["WhsDocketLine_CustomFlag5"]);
				AssertEquals("WhsDocketLine_CustomTextBlob", docketLine.WE_CustomTextBlob1,
					dynamicObject["WhsDocketLine_CustomTextBlob"]);
				AssertEquals("ExpiryDate", docketLine.WE_ExpiryDate, dynamicObject["ExpiryDate"]);
				AssertEquals("PackingDate", docketLine.WE_PackingDate, dynamicObject["PackingDate"]);
				AssertEquals("PartAttrib1Name", miscServ.OM_IMPartAttrib1Name, dynamicObject["PartAttrib1Name"]);
				AssertEquals("PartAttrib2Name", miscServ.OM_IMPartAttrib2Name, dynamicObject["PartAttrib2Name"]);
				AssertEquals("PartAttrib3Name", miscServ.OM_IMPartAttrib3Name, dynamicObject["PartAttrib3Name"]);
				AssertEquals("StockKeepingUnit", supplierPart.OP_StockKeepingUnit, dynamicObject["StockKeepingUnit"]);
				AssertEquals("PalletID", docketLine.WE_PalletID, dynamicObject["PalletID"]);
				AssertEquals("Pallet Spaces", expectedPalletSpaces, dynamicObject["TotalPalletSpaces"]);
				AssertEquals("TotalPalletsPerProduct", new ZLong(expectedPalletsPerProduct), dynamicObject["TotalPalletsPerProduct"]);
				AssertEquals("TotalPalletsPerLocation", new ZLong(expectedPalletsPerLocation), dynamicObject["TotalPalletsPerLocation"]);
				AssertEquals("InventoryHeldCode", docketLine.WE_WHC_NKCurrentInventoryHeldCode,
					dynamicObject["WhsDocketLine_HeldCode"]);

				var availableToPickUnits = docketLine.IsAvailable
					? expectedTotalUnits - expectedReservedUnits - expectedCommittedUnits
					: 0m;

				AssertEquals("Allocated Units", expectedReservedUnits, dynamicObject["AllocatedUnits"]);
				AssertEquals("Committed Units", expectedCommittedUnits, dynamicObject["CommittedUnits"]);
				AssertEquals("Total Units", expectedTotalUnits, dynamicObject["TotalUnits"]);
				AssertEquals("Available Units", availableToPickUnits, dynamicObject["AvailableUnits"]);

				AssertEquals("Weight", expectedTotalUnits * supplierPart.OP_Weight, dynamicObject["Weight"]);
				AssertEquals("Volume", expectedTotalUnits * supplierPart.OP_Cubic, dynamicObject["Volume"]);

				var clientUnits = partRelation.OU_ClientUQ.IsEmpty
					? ZDecimal.Zero
					: supplierPart.UnitConverter.Convert(expectedTotalUnits, supplierPart.OP_StockKeepingUnit,
						partRelation.OU_ClientUQ);
				AssertEquals("TotalClientUnits", clientUnits, dynamicObject["TotalClientUnits"]);

				var location = docketLine.CurrentLocation;
				AssertEquals("AreaPK", location != null ? location.WLV_WA_PickingArea : ZGuid.Empty,
					dynamicObject["AreaPK"]);
				AssertEquals("AreaName", location != null ? location.PickingArea.WA_Name : ZString.Empty,
					dynamicObject["AreaName"]);
				AssertEquals("LocnRow", location != null ? location.RowName : ZString.Empty, dynamicObject["LocnRow"]);
				AssertEquals("LocnCol", location != null ? location.FormattedColumn : ZString.Empty,
					dynamicObject["LocnCol"]);
				AssertEquals("LocnLevel", location != null ? location.FormattedLevel : ZString.Empty,
					dynamicObject["LocnLevel"]);
				AssertEquals("LocnTray", location != null ? location.FormattedTray : ZString.Empty,
					dynamicObject["LocnTray"]);
				AssertEquals("Location", location != null ? location.WLV_LocationString_UserFriendly : ZString.Empty,
					dynamicObject["Location"]);
				AssertEquals("LocationIndexForSort", WhsSqlViewHelper.GetLocationIndexForSort(location),
					dynamicObject["LocationIndexForSort"]);

				AssertEquals("Total Value", supplierPart.OP_LastCost * expectedTotalUnits, dynamicObject["TotalValue"]);
				AssertEquals("Last Cost", supplierPart.OP_LastCost, dynamicObject["LastCost"]);
				AssertEquals("Currency", supplierPart.OP_RX_NKLastWeightedCostCurr, dynamicObject["Currency"]);
				AssertEquals("LastMovementDate", expectedLastMovementDate.Date,
					((ZDateTimeOffset)dynamicObject["LastMovementDate"]).Date);
			});
		}

		#endregion
	}

	#region WhsSqlViewHelper

	class WhsSqlViewHelper : WhsTestCaseWithFactory
	{
		public static ZInt GetLocationIndexForSort(WhsLocation location)
		{
			return location == null
				? 0
				: ((location.WLV_Column - 1) * location.Row.WR_Levels * location.Row.WR_Trays) +
				  ((location.WLV_Level - 1) * location.Row.WR_Trays) + location.WLV_Tray;
		}
	}

	#endregion
}
