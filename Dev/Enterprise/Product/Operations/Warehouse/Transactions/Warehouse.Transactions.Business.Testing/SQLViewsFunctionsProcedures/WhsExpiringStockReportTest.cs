using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsExpiringStockReportTest : WhsTestCaseWithFactory
	{
		#region TestView_InTransitInventory_Order

		public void TestView_InTransitInventory_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDate.Today;
			var expiryDate = today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Helper.CreatePickNew(order);
			var orderPickLine = Helper.CreateWhsPickLine(orderLine, inventory, 100m);

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderPickLine, ZDateTimeOffset.Now);
			orderLine.PickLines.Single();
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals("Should include In-Transit inventory.", 1, results.Length);
			AssertLineMatch(transferLine, results, 100m, today);
		}

		#endregion

		#region TestView_InTransitInventory_Transfer

		public void TestView_InTransitInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDate.Today;
			var expiryDate = today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m,
				data.Whs1.FindLocation("A-2"), expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var nonInTransitTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var inTransitTransferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "");
			nonInTransitTransferLine.WE_ExpiryDate = expiryDate;
			inTransitTransferLine.WE_ExpiryDate = expiryDate;
			inTransitTransferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line should be in transit.", true,
				inTransitTransferLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
			Factory.Save();

			var results1 = LoadSQLFunction(today);
			AssertEquals("Should include In-Transit inventory but not unpicked Transfer Line.", 3, results1.Length);
			AssertLineMatch(inventory1.InDocketLine, results1, 10, today);
			AssertLineMatch(inventory2.InDocketLine, results1, 0, today);
			AssertLineMatch(inTransitTransferLine, results1, 0, today);

			inTransitTransferLine.LocationString = "A-2";
			Factory.Save();

			var results2 = LoadSQLFunction(today);
			AssertEquals("Should include In-Transit inventory but not unpicked Transfer Line.", 3, results2.Length);
			AssertLineMatch(inventory1.InDocketLine, results2, 10, today);
			AssertLineMatch(inventory2.InDocketLine, results2, 0, today);
			AssertLineMatch(inTransitTransferLine, results2, 0, today);
		}

		#endregion

		#region TestView_PuttingAwayInventory_Transfer

		public void TestView_PuttingAwayInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDateTimeOffset.Today;
			var date = today.Date;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory =
				Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			inventory.WI_ExpiryDate = date;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1,
				nonDockDoorLocation, "PLT1", 15m);
			transferLine.WE_ExpiryDate = date;
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = today;
			Factory.Save();

			AssertEquals(
				$"InventoryStatus of Precondition: Transfer Line should be {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"InventoryStatus of Precondition: Inventory should be {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var results1 = LoadSQLFunction(date);
			AssertEquals("Should include putting away inventory.", 1, results1.Length);
		}

		#endregion

		#region TestView_OnlyShowsPutawayReceiveLines

		public void TestView_OnlyShowsPutawayReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDate.Today;
			var expiryDate = today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, null, expiryDate,
				ZDate.Empty, "", "", "", "");
			AssertEquals("Precondition - ensure Receive is *not* finalised.", false, receive.IsFinalised);
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals("Should include Putaway inventory but not pending inventory.", 1, results.Length);
			AssertLineMatch(inventory1.InDocketLine, results, 0m, today);
		}

		#endregion

		#region TestView_PutawayReceiveLines_WithAttributes

		public void TestView_PutawayReceiveLines_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDate.Today;
			var expiryDate = today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "Blue", "Large", "Smashed", "");
			inventory1.WI_SerialNumber = "SER";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, expiryDate,
				ZDate.Empty, "Green", "Small", "Broken", "");
			inventory2.WI_SerialNumber = "SER3";
			AssertEquals("Precondition - ensure Receive is *not* finalised.", false, receive.IsFinalised);
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals("Should include Putaway inventory but not pending inventory.", 1, results.Length);
			AssertLineMatch(inventory1.InDocketLine, results, 0m, today);
		}

		#endregion

		#region TestView_RunningDate

		public void TestView_RunningDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDate.Today;
			var expiryDate = today.AddDays(2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			AssertEquals("Precondition - ensure Receive is *not* finalised.", false, receive.IsFinalised);
			Factory.Save();

			var results1 = LoadSQLFunction(today);
			AssertEquals("Should not include inventory, is not expired yet.", 0, results1.Length);

			var results2 = LoadSQLFunction(today.AddDays(2));
			AssertEquals("Should include inventory.", 1, results2.Length);
			AssertLineMatch(inventory1.InDocketLine, results2, 0m, today.AddDays(2));

			var results3 = LoadSQLFunction(today.AddDays(1));
			AssertEquals("Should not include inventory, is not expired yet.", 0, results3.Length);
		}

		#endregion

		#region TestView_ShowsStagedLines

		public void TestView_ShowsStagedLines()
		{
			var today = ZDate.Today;
			var expiryDate = today;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation,
				expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], inventory, 10m);
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals("Should include staged inventory.", 2, results.Length);
			AssertLineMatch(inventory.InDocketLine, results, 0m, today);
			AssertLineMatch(transferLine, results, 10m, today);
		}

		#endregion

		#region TestView_WithProductCateogryFilter

		public void TestView_WithProductCateogryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
				MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;
			Helper.SetProductAttributeUse(client, productTea, AttributeNumber.ExpiryDate, true);

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;
			Helper.SetProductAttributeUse(client, productCoke, AttributeNumber.ExpiryDate, true);

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;
			Helper.SetProductAttributeUse(client, productVB, AttributeNumber.ExpiryDate, true);

			var receive1 = Helper.CreateWhsReceive(client, warehouse, "R1");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, productVB, 100m, warehouse.DefaultLocation,
				ZDate.Today, ZDate.Empty, "", "", "", "");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, productCoke, 200m, warehouse.DefaultLocation,
				ZDate.Today, ZDate.Empty, "", "", "", "");
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive1, productTea, 10m, warehouse.DefaultLocation,
				ZDate.Today, ZDate.Empty, "", "", "", "");

			Factory.Save();

			var result1 = LoadView_WithProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WithProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = string.Empty;

			var sqlParams = new ZSqlParameterCollection();
			if (categoryPK.IsEmpty)
			{
				sql = $"SELECT * FROM WhsExpiringStockReport(@CurrentDate, null)";
			}
			else
			{
				sql = $"SELECT * FROM WhsExpiringStockReport(@CurrentDate, @ProductCategoryPK)";
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			sqlParams.Add("@CurrentDate", ZDate.Today, WhsDocketLineSchema.WE_ExpiryDate);

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region ReportWithRealTimeUnits

		public void TestReportOnlyConsidersUnpickedAndCommittedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var today = ZDateTimeOffset.Today;
			var expiryDate = today.Date;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 200m,
				data.Whs1.FindLocation("A-2"), expiryDate, ZDate.Empty, "", "", "", "");
			receive1.FinaliseDocket();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			receive2.WD_ArrivalDate = today.AddDays(3);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 300m,
				data.Whs1.FindLocation("A-2"), expiryDate, ZDate.Empty, "", "", "", "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: "MAN");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			var picker = Helper.CreatePickNew(order1);
			var availableInventories1 =
				picker.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			availableInventories1.Single(p => p.LocationString == "A-1").Allocate = true;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: "MAN");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 52m);
			var picker2 = Helper.CreatePickNew(order2);
			var availableInventories2 =
				picker2.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availableInventory2 = availableInventories2.Single(p => p.LocationString == "A-2");
			availableInventory2.Allocate = true;
			Helper.SetPickedDate(availableInventory2, today);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3", pickOption: "MAN");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 2m);
			var reservedPickLine = orderLine3.ReserveStockIfAbleTo(inventory3, 2m);
			((IBusinessObjectInternals)reservedPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 2m;
			AssertEquals("Precondition: Stock is reserved.", 2m, reservedPickLine.ReservedQuantity);
			Factory.Save();

			var inTransitTransferLine = availableInventory2.PickLines.ElementAt(0).InventoryLine;

			var results = LoadSQLFunction(today.Date);

			AssertEquals("Should find 4 inventories.", 4, results.Length);
			AssertLineMatch(inventory1.InDocketLine, results, 15m, today.Date);
			AssertLineMatch(inventory2.InDocketLine, results, 0m, today.Date);
			AssertLineMatch(inventory3.InDocketLine, results, 0m, today.Date);
			AssertLineMatch(inTransitTransferLine, results, 52m, today.Date); // Committed (unpicked) in the dock door
		}

		#endregion

		#region TestView_Transfers

		public void TestView_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var category = Helper.CreateProductCategory(data.Org1, data.Part1, "Cat1");
			category.OPC_CategoryDescription = "Product Category 1";

			var today = ZDate.Today;
			var expiryDate = today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), expiryDate, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m,
				data.Whs1.FindLocation("A-2"), expiryDate, ZDate.Empty, "", "", "", "");
			inventory2.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine =
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -7m, data.Whs1.FindLocation("A-1"));
			adjustmentLine.WE_ExpiryDate = expiryDate;
			adjustmentLine.RunPreSaveValidation(); // commit stock
			AssertEquals("Precondition: Stock is committed.", 7m, adjustmentLine.CommittedQuantity);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			transferLine1.WE_ExpiryDate = expiryDate;
			transferLine2.WE_ExpiryDate = expiryDate;
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);

			transfer.RunPreSaveValidation(); // to commit stock.
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals("Should find 2 receive inventory and one transferred inventory.", 3, results.Length);
			AssertLineMatch(inventory1.InDocketLine, results, 17m, today);

			AssertLineMatch(transferLine2, results, 0m, today);
		}

		#endregion

		#region TestView_FixedWidthLocationWarehouse

		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			Factory.Save();

			var today = ZDate.Today;
			var expiryDate = today;
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m,
				warehouse.FindLocation("LOCZ0040302"), expiryDate, ZDate.Empty, "", "", "", "");
			AssertEquals("Precondition - ensure Receive is *not* finalised.", false, receive.IsFinalised);
			Factory.Save();

			var results = LoadSQLFunction(today);
			AssertEquals(1, results.Length);
			AssertLineMatch(inventory.InDocketLine, results, 0m, today);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObject[] LoadSQLFunction(ZDate date)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			var sql = $"SELECT * FROM WhsExpiringStockReport(@CurrentDate, null)";
			sqlParams.Add("@CurrentDate", date, WhsDocketLineSchema.WE_ExpiryDate);
			result.Load(sql, sqlParams);

			return result.ToArray();
		}

		#endregion

		#region AssertLineMatch

		void AssertLineMatch(WhsDocketLine expectedDocketLine, DynamicBusinessObject[] results,
			ZDecimal expectedCommittedUnits, ZDateTime today)
		{
			var docket = expectedDocketLine.Docket;
			var client = docket.Client;
			var part = expectedDocketLine.SupplierPart;
			var partRelation =
				part.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			var commodityPK = (part.CommodityCode != null) ? part.CommodityCode.PK : ZGuid.Empty;
			var expectedTotalPallets = (part.OP_StockKeepingUnitPerPallet != 0)
				? expectedDocketLine.WE_StockOnHand / part.OP_StockKeepingUnitPerPallet
				: 0m;
			var expectedExpired = (today >= expectedDocketLine.WE_ExpiryDate) ? "Expired" : "";
			var expectedDaysLeft = (expectedDocketLine.WE_ExpiryDate - today).Days;
			var arrivalDate = expectedDocketLine.WE_AdjustmentArrivalDate;
			var expectedArrivalDate = arrivalDate.Date;
			var availableUnits = expectedDocketLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit
								 || expectedDocketLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.PuttingAway
				? 0m
				: expectedDocketLine.WE_StockOnHand - expectedCommittedUnits;

			var actualInventory = results.Single(d => (ZGuid)d["PK"] == expectedDocketLine.PK);
			CombineAssertions(() =>
			{
				AssertEquals("WarehousePK", docket.WD_WW_Whs, actualInventory["WarehousePK"]);
				AssertEquals("WarehouseName", docket.Warehouse.WW_WarehouseName, actualInventory["WarehouseName"]);
				AssertEquals("ClientPK", docket.WD_OH_Client, actualInventory["ClientPK"]);
				AssertEquals("ClientCode", client.OH_Code, actualInventory["ClientCode"]);
				AssertEquals("Client", client.OH_FullName, actualInventory["Client"]);
				AssertEquals("Product", part.OP_PartNum, actualInventory["Product"]);
				AssertEquals("ProductDesc", part.OP_Desc, actualInventory["ProductDesc"]);
				AssertEquals("ProductBrandName", part.OP_Brand, actualInventory["ProductBrandName"]);
				AssertEquals("ProductModel", part.OP_Model, actualInventory["ProductModel"]);
				AssertEquals("ProductPK", expectedDocketLine.WE_OP, actualInventory["ProductPK"]);
				AssertEquals("ExpiryDate", expectedDocketLine.WE_ExpiryDate, actualInventory["ExpiryDate"]);
				AssertEquals("CommodityCode", part.OP_RH_NKCommodityCode, actualInventory["CommodityCode"]);
				AssertEquals("CommodityPK", commodityPK, actualInventory["CommodityPK"]);
				AssertEquals("ArrivalDate", expectedArrivalDate, actualInventory["ArrivalDate"]);
				AssertEquals("Status", expectedDocketLine.WE_CurrentInventoryStatus, actualInventory["Status"]);
				AssertEquals("PackingDate", expectedDocketLine.WE_PackingDate,
					new ZDateTime(actualInventory["PackingDate"]));
				AssertEquals("PartAttrib1", expectedDocketLine.WE_PartAttrib1, actualInventory["PartAttrib1"]);
				AssertEquals("PartAttrib1Name", client.MiscServ.OM_IMPartAttrib1Name,
					actualInventory["PartAttrib1Name"]);
				var expectedAttribute2 = string.IsNullOrEmpty(expectedDocketLine.WE_PartAttrib2)
					? ""
					: $"Attribute 2: {expectedDocketLine.WE_PartAttrib2}";
				AssertEquals("PartAttrib2", expectedAttribute2, actualInventory["PartAttrib2"]);
				var expectedAttribute3 = string.IsNullOrEmpty(expectedDocketLine.WE_PartAttrib3)
					? ""
					: $"Attribute 3: {expectedDocketLine.WE_PartAttrib3}";
				AssertEquals("PartAttrib3", expectedAttribute3, actualInventory["PartAttrib3"]);
				var expectedSerial = string.IsNullOrEmpty(expectedDocketLine.WE_SerialNumber)
					? ""
					: $"Serial Number: {expectedDocketLine.WE_SerialNumber}";
				AssertEquals("SerialNumber", expectedSerial, actualInventory["SerialNumber"]);
				AssertEquals("Location", expectedDocketLine.CurrentLocation?.WLV_LocationString_UserFriendly ?? "", actualInventory["Location"]);
				AssertEquals("TotalPallets", expectedTotalPallets, actualInventory["TotalPallets"]);
				AssertEquals("TotalUnits", expectedDocketLine.WE_StockOnHand, actualInventory["TotalUnits"]);
				AssertEquals("CommittedUnits", expectedCommittedUnits, actualInventory["CommittedUnits"]);
				AssertEquals("AvailableUnits", availableUnits, actualInventory["AvailableUnits"]);
				AssertEquals("StockKeepingUnit", part.OP_StockKeepingUnit, actualInventory["StockKeepingUnit"]);
				AssertEquals("Expired", expectedExpired, actualInventory["Expired"]);
				AssertEquals("DaysLeft", expectedDaysLeft, actualInventory["DaysLeft"]);
				AssertEquals("HeldCode", expectedDocketLine.WE_WHC_NKCurrentInventoryHeldCode,
					actualInventory["HeldCode"]);
				AssertEquals("ProductCategoryCode", expectedCategoryCode, actualInventory["ProductCategoryCode"]);
				AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
					actualInventory["ProductCategoryDescription"]);
			});
		}

		#endregion
	}
}
