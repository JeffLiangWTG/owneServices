using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationTypeReportTest : WhsTestCaseWithFactory
	{
		#region TestView_FiltersInTransitLines

		public void TestView_FiltersInTransitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString, "");
			var inTransitTransferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, inventory.LocationString, "");
			transfer.RunPreSaveValidation(); // to commit inventory
			AssertEquals("Precondition: Transfer Line should be in transit.", false,
				inTransitTransferLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);

			inTransitTransferLine.PickedTime = CargoWise.Types.ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line should be in transit.", true,
				inTransitTransferLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
			Factory.Save();

			var results = LoadView(data.Whs1);
			AssertEquals("WhsLocationTypeReport should not return In Transit (INT) lines.", 2, results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null, 0, 0, 0);
			AssertData(results[1], data.Whs1, data.Whs1.DefaultLocation, data.Part1, 2, 2, 0);
		}

		#endregion

		#region TestView_FiltersPuttingAwayLines

		public void TestView_FiltersPuttingAwayLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receiveWithDockDoor = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part1, 10m, dockDoorLocation, "12345");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m,
				data.Whs1.DefaultLocation, "1");

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "12345", 10m);
			putawayLine.RunPreSaveValidation();
			putawayLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Transfer line inventory status should be changed to Putting Away.",
				InventoryStatus.Codes.PuttingAway, putawayLine.WE_CurrentInventoryStatus);

			var results = LoadView(data.Whs1);
			AssertEquals("LocationInventorySummaryReport should not return put away (PTA) lines.", 3, results.Count);
			AssertData(results[0], data.Whs1, dockDoorLocation, null, 0, 0, 0);
			AssertData(results[1], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null, 0, 0, 0);
			AssertData(results[2], data.Whs1, nonDockDoorLocation, data.Part1, 100, 0, 100);
		}

		#endregion

		#region TestView_ShowsStagedLines

		public void TestView_ShowsStagedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = LoadView(data.Whs1);
			AssertEquals("WhsLocationTypeReport should return staged lines.", 2, results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultLocation, data.Part1, 10m, 0m, 10m);
			AssertData(results[1], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, data.Part1, 10m, 10m, 0m);
		}

		#endregion

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

			var result1 = LoadView_WithProductCategory(warehouse.PK, categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(warehouse.PK, categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(warehouse.PK, categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid warehousePK, ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = @"select * from WhsLocationTypeReport(@WarehousePK, ";
			sql = sql + (categoryPK.IsEmpty ? "null)" : "@ProductCategoryPK)");

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", warehousePK, WhsWarehouseSchema.PK);
			if (!categoryPK.IsEmpty)
			{
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region TestWhsLocationTypeReport_ProductCategory

		public void TestWhsLocationTypeReport_ProductCategory()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);
			var products = SetupData(whs1, whs2);

			var result1 = LoadView(whs1);
			AssertEquals(3, result1.Count);
			AssertRow_ClientProductAndCategory(result1[0], null, "", "");
			AssertRow_ClientProductAndCategory(result1[1], products[0], "Cat1", "Category 1");
			AssertRow_ClientProductAndCategory(result1[2], products[1], "", "");

			var result2 = LoadView(whs2);
			AssertEquals(3, result2.Count);
			AssertRow_ClientProductAndCategory(result2[0], null, "", "");
			AssertRow_ClientProductAndCategory(result2[1], products[2], "Cat2", "Category 2");
			AssertRow_ClientProductAndCategory(result2[2], products[3], "", "");
		}

		OrgSupplierPartCollection SetupData(WhsWarehouse whs1, WhsWarehouse whs2)
		{
			var client1 = Helper.CreateClient("C1");
			Factory.Save();

			var loc1 = whs1.FindLocation("A-1");
			var whs1Loc2 = whs1.FindLocation("A-2");
			var whs2Loc1 = whs2.FindLocation("A-1");
			var whs2Loc2 = whs2.FindLocation("A-2");

			var product1 = Helper.CreateProduct(client1, "P1");
			var product2 = Helper.CreateProduct(client1, "P2");
			var product3 = Helper.CreateProduct(client1, "P3");
			var product4 = Helper.CreateProduct(client1, "P4");

			SetLocationAndProductData(loc1, product1, ZDateTimeOffset.Now, "WH 1-1", "NOR", "LT1", "PM1", 1, 1.1m, 10m,
				Constants.Volume.TeaChest, Constants.Weight.Kilograms);
			SetLocationAndProductData(whs1Loc2, product2, ZDateTimeOffset.Now.AddDays(-1), "WH 1-2", "NOR", "LT2",
				"PM2", 2, 2.2m, 20m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			SetLocationAndProductData(whs2Loc1, product3, ZDateTimeOffset.Now.AddDays(-2), "WH 2-1", "NOR", "LT3",
				"PM3", 3, 3.3m, 30m, Constants.Volume.CubicCentimeters, Constants.Weight.PoundsTroy);
			SetLocationAndProductData(whs2Loc2, product4, ZDateTimeOffset.Now.AddDays(-3), "WH 2-2", "NOR", "LT4",
				"PM4", 4, 4.4m, 40m, Constants.Volume.CubicMetres, Constants.Weight.ShortTons);

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client1, whs1, "TEST1");
			var receive2 = Helper.CreateWhsReceive(client1, whs2, "TEST2");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1, 1);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, product2, 1);
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive2, product3, 1);
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive2, product4, 1);

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive2.IsFinalised);

			var category1 = Helper.CreateProductCategory(client1, product1, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";

			var category2 = Helper.CreateProductCategory(client1, product3, "Cat2");
			category2.OPC_CategoryDescription = "Category 2";

			Factory.Save();

			var products = new OrgSupplierPartCollection(Factory);
			products.Add(product1);
			products.Add(product2);
			products.Add(product3);
			products.Add(product4);
			return products;
		}

		void AssertRow_ClientProductAndCategory(DynamicBusinessObject result, OrgSupplierPart part,
			ZString categoryCode, ZString categoryDescription)
		{
			AssertEquals(part?.OP_PartNum ?? "", result["ProductCode"]);
			AssertEquals(categoryCode, result["ProductCategoryCode"]);
			AssertEquals(categoryDescription, result["ProductCategoryDescription"]);
		}

		#endregion

		#region TestWhsLocationTypeReport_MultipleReceivesOfOneProduct

		public void TestWhsLocationTypeReport_MultipleReceivesOfOneProduct()
		{
			var org1 = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			Factory.Save();

			var loc1 = whs1.FindLocation("A");
			var product1 = Helper.CreateProduct(org1, "P1");

			SetLocationAndProductData(loc1, product1, ZDateTimeOffset.Now, "WH 1-1", "NOR", "LT1", "PM1", 1, 2m, 10m,
				Constants.Volume.TeaChest, Constants.Weight.Kilograms);
			product1.OP_Cubic = .5;
			product1.OP_Weight = 1;

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(org1, whs1);
			receive1.WD_ExternalReference = "TEST1";
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1, 2, loc1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org1, whs1);
			receive2.WD_ExternalReference = "TEST2";
			var line21 = Helper.CreateWhsReceiveInventoryLine(receive2, product1, 2, loc1);
			var line22 = Helper.CreateWhsReceiveInventoryLine(receive2, product1, 2, loc1);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(org1, whs1, "REF1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product1, 1m);
			Helper.CreatePickNew(order1);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_WE_InventoryLine = receive1.Inventory[0].PK;

			var order2 = Helper.CreateWhsOrder(org1, whs1, "REF2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, product1, 1m);
			var orderLine22 = Helper.CreateWhsOrderLine(order2, product1, 1m);
			Helper.CreatePickNew(order2);
			var pickLine21 = orderLine21.PickLines[0];
			pickLine21.WZ_WE_InventoryLine = receive2.Inventory[0].PK;
			var pickLine22 = orderLine22.PickLines[0];
			pickLine22.WZ_WE_InventoryLine = receive2.Inventory[1].PK;

			Factory.Save();

			var result = LoadView(whs1);
			AssertEquals("Precondition", 2, result.Count);

			AssertEquals("", result[0]["ProductCode"]); // should be empty

			AssertEquals(product1.OP_PartNum, result[1]["ProductCode"]);
			AssertEquals(6m, result[1]["StockOnHand"]);
			AssertEquals(3m, result[1]["AvailableUnits"]);
			AssertEquals(0.6m, result[1]["WeightUsed"]); // Percentage of Location's Weight Max that has been used
			AssertEquals(1.5m, result[1]["CubicUsed"]); // Percentage of Location's Cubic Max that has been used
			AssertEquals(3m, result[1]["CommittedUnits"]);
		}

		#endregion

		#region TestReportWhsLocationTypeReport

		public void TestReportWhsLocationTypeReport()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);
			var products = SetUpDataForView(whs1, whs2);

			var results1 = LoadView(whs1);
			AssertEquals("Should only have returned the Product Warehouse.", 3, results1.Count);
			AssertData(results1[0], whs1, whs1.DefaultOutboundDockDoorLocation, null, 0, 0, 0);
			AssertData(results1[1], whs1, whs1.FindLocation("A-1"), products[0]);
			AssertData(results1[2], whs1, whs1.FindLocation("A-2"), products[1]);

			var results2 = LoadView(whs2);
			AssertEquals("Should only have returned the Product Warehouse.", 3, results1.Count);
			AssertData(results2[0], whs2, whs2.DefaultOutboundDockDoorLocation, null, 0, 0, 0);
			AssertData(results2[1], whs2, whs2.FindLocation("A-1"), products[2]);
			AssertData(results2[2], whs2, whs2.FindLocation("A-2"), products[3]);
		}

		public void TestReportWhsLocationTypeReport_FixedWidthLocationWarehouse()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 2, 1, 1);
			Factory.Save();

			var org = Helper.CreateClient();
			Factory.Save();

			var loc1 = warehouse.FindLocation("LOCZ001");
			var loc2 = warehouse.FindLocation("LOCZ002");

			var product1 = Helper.CreateProduct(org, "P1");
			var product2 = Helper.CreateProduct(org, "P2");

			SetLocationAndProductData(loc1, product1, ZDateTimeOffset.Now, "WH 1-1", "NOR", "LT1", "PM1", 1, 1.1m, 10m,
				Constants.Volume.TeaChest, Constants.Weight.Kilograms);
			SetLocationAndProductData(loc2, product2, ZDateTimeOffset.Now.AddDays(-1), "WH 1-2", "NOR", "LT2",
				"PM2", 2, 2.2m, 20m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org, warehouse);
			receive.WD_ExternalReference = "TEST1";
			Helper.CreateWhsReceiveInventoryLine(receive, product1, 1, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive, product2, 1, loc2);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var results = LoadView(warehouse);
			AssertEquals("Should have returned 3 results.", 3, results.Count);
			AssertData(results[0], warehouse, warehouse.DefaultOutboundDockDoorLocation, null, 0, 0, 0);
			AssertData(results[1], warehouse, loc1, product1);
			AssertData(results[2], warehouse, loc2, product2);
		}

		#endregion

		#region TestLocationTypeReport_ShowsDockDoorLocations

		public void TestLocationTypeReport_ShowsDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation1 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-3");
			var dockDoorLocation3 = data.Whs1.FindLocation("A-4");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			dockDoorLocation3.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1");
			var receivedDDL = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, dockDoorLocation1, "1235");
			var directPutaway = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, nonDockDoorLocation, "12345");

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW2");
			var pickedReceivedDDL = Helper.CreateWhsReceiveLine(receive2, data.Part1, 5m, dockDoorLocation2, "145");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation2, nonDockDoorLocation, "145", 5m);
			putawayTransferLine.RunPreSaveValidation();
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WE_CurrentInventoryStatus is correct", InventoryStatus.Codes.Received,
				receivedDDL.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: WE_CurrentInventoryStatus is correct", InventoryStatus.Codes.Putaway,
				directPutaway.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: WE_CurrentInventoryStatus is correct", InventoryStatus.Codes.Received,
				pickedReceivedDDL.WE_CurrentInventoryStatus);

			var results = LoadViewForDockDoorLocations();
			AssertEquals("Should only have returned the Product Warehouse.", 5, results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null, 0m, 0m, 0m);
			AssertData(results[1], data.Whs1, nonDockDoorLocation, data.Part1, expectedStockOnHand: 3,
				expectedCommittedUnits: 0, expectedAvailableUnits: 0);
			AssertData(results[2], data.Whs1, dockDoorLocation1, data.Part1, expectedStockOnHand: 1,
				expectedCommittedUnits: 0, expectedAvailableUnits: 1);
			AssertData(results[3], data.Whs1, dockDoorLocation2, null, 0m, 0m, 0m);
			AssertData(results[4], data.Whs1, dockDoorLocation3, null, 0m, 0m, 0m);
		}

		DynamicBusinessObjectCollection LoadViewForDockDoorLocations()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"select	*
from	WhsLocationTypeReport(null, null)
order by AreaName, LocationRow, LocationColumn";
			result.Load(sql);
			return result;
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs = null, ZDateTime time = new ZDateTime(),
			string name = "",
			string status = "", string type = "", string method = "", int count = 0, decimal cubic = 0,
			decimal weight = 0,
			string cUnit = "", string wUnit = "", OrgSupplierPart part = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var statements = new List<string>();
			var sql = @"select	*
from	WhsLocationTypeReport(null, null)
where";
			AddStatementsToList(whs, time, name, status, type, method, count, cubic, weight, cUnit, wUnit, part,
				statements);
			sql += $@"{AddStatmentsToSql(statements)}
order by ProductCode, LocationRow, LocationColumn";
			result.Load(sql);
			return result;
		}

		#region AddStatementsToList

		void AddStatementsToList(WhsWarehouse whs, ZDateTime time, string name, string status, string type,
			string method, int count, decimal cubic, decimal weight, string cUnit, string wUnit, OrgSupplierPart part,
			List<string> statements)
		{
			if (whs != null)
			{
				statements.Add(CreateStatement("WarehousePK", $"'{whs.PK}'"));
			}

			if (time.IsValid)
			{
				statements.Add(CreateStatement("LastTouched", time));
			}

			if (!string.IsNullOrEmpty(name))
			{
				statements.Add(CreateStatement("AreaName", name));
			}

			if (!string.IsNullOrEmpty(status))
			{
				statements.Add(CreateStatement("LocationStatus", status));
			}

			if (!string.IsNullOrEmpty(type))
			{
				statements.Add(CreateStatement("LocationType", type));
			}

			if (!string.IsNullOrEmpty(method))
			{
				statements.Add(CreateStatement("LocationMethod", method));
			}

			if (count != 0)
			{
				statements.Add(CreateStatement("TouchesToStocktake", count));
			}

			if (cubic != 0)
			{
				statements.Add(CreateStatement("MaxCubic", cubic));
			}

			if (weight != 0)
			{
				statements.Add(CreateStatement("MaxWeight", weight));
			}

			if (!string.IsNullOrEmpty(cUnit))
			{
				statements.Add(CreateStatement("MaxCubicUnit", cUnit));
			}

			if (!string.IsNullOrEmpty(wUnit))
			{
				statements.Add(CreateStatement("MaxWeightUnit", wUnit));
			}

			if (part != null)
			{
				statements.Add(CreateStatement("OP_PK", part.PK));
			}
		}

		#region CreateStatement

		string CreateStatement(string field, object value)
		{
			return $@"
{field} = {value}";
		}

		#endregion

		#endregion

		#region AddStatmentsToSql

		string AddStatmentsToSql(List<string> statements)
		{
			var result = "";
			var needsAnd = false;
			foreach (var s in statements)
			{
				result += s;
				if (needsAnd)
				{
					result += " and";
				}
				else
				{
					needsAnd = true;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Implementation

		OrgSupplierPartCollection SetUpDataForView(WhsWarehouse whs1, WhsWarehouse whs2)
		{
			var org = Helper.CreateClient();
			Factory.Save();

			var loc1 = whs1.FindLocation("A-1");
			var whs1Loc2 = whs1.FindLocation("A-2");
			var whs2Loc1 = whs2.FindLocation("A-1");
			var whs2Loc2 = whs2.FindLocation("A-2");

			var product1 = Helper.CreateProduct(org, "P1");
			var product2 = Helper.CreateProduct(org, "P2");
			var product3 = Helper.CreateProduct(org, "P3");
			var product4 = Helper.CreateProduct(org, "P4");

			SetLocationAndProductData(loc1, product1, ZDateTimeOffset.Now, "WH 1-1", "NOR", "LT1", "PM1", 1, 1.1m, 10m,
				Constants.Volume.TeaChest, Constants.Weight.Kilograms);
			SetLocationAndProductData(whs1Loc2, product2, ZDateTimeOffset.Now.AddDays(-1), "WH 1-2", "NOR", "LT2",
				"PM2", 2, 2.2m, 20m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			SetLocationAndProductData(whs2Loc1, product3, ZDateTimeOffset.Now.AddDays(-2), "WH 2-1", "NOR", "LT3",
				"PM3", 3, 3.3m, 30m, Constants.Volume.CubicCentimeters, Constants.Weight.PoundsTroy);
			SetLocationAndProductData(whs2Loc2, product4, ZDateTimeOffset.Now.AddDays(-3), "WH 2-2", "NOR", "LT4",
				"PM4", 4, 4.4m, 40m, Constants.Volume.CubicMetres, Constants.Weight.ShortTons);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(org, whs1);
			receive1.WD_ExternalReference = "TEST1";
			var receive2 = Helper.CreateWhsReceive(org, whs2);
			receive2.WD_ExternalReference = "TEST2";
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, product1, 1);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, product2, 1);
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive2, product3, 1);
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive2, product4, 1);

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var products = new OrgSupplierPartCollection(Factory);
			products.AddRange(product1, product2, product3, product4);
			return products;
		}

		void SetLocationAndProductData(WhsLocation location, OrgSupplierPart product, ZDateTimeOffset invChangeTime,
			ZString name, ZString status, ZString type, ZString method, ZInt count, ZDecimal cubic, ZDecimal weight,
			string cUnit, string wUnit)
		{
			var locationType = Helper.CreateLocationType(type);

			location.WLV_LastInventoryChangeDate = invChangeTime;
			location.PickingArea.WA_Name = name;
			location.WLV_LocationStatus = status;
			location.WLV_WLT_LocationType = locationType.PK;
			location.WLV_PickMethod = method;
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = count;
			location.WLV_MaxCubic = cubic;
			location.WLV_MaxWeight = weight;
			location.WLV_MaxCubicUnit = cUnit;
			location.WLV_MaxWeightUnit = wUnit;
			product.OP_Cubic = cubic * (ZDecimal).99;
			product.OP_CubicUQ = cUnit;
			product.OP_Weight = weight * (ZDecimal).99;
			product.OP_WeightUQ = wUnit;
		}

		void AssertData(DynamicBusinessObject row, WhsWarehouse expectedWarehouse, WhsLocation expectedLocation,
			OrgSupplierPart expectedProduct, decimal expectedStockOnHand = 1m, decimal expectedCommittedUnits = 0m,
			decimal expectedAvailableUnits = 1m)
		{
			CombineAssertions(() =>
			{
				AssertEquals("WarehousePK", expectedWarehouse.PK, row["WarehousePK"]);
				AssertEquals("WarehouseName", expectedWarehouse.WW_WarehouseName, row["WarehouseName"]);
				AssertEquals("WarehouseCode", expectedWarehouse.WW_WarehouseCode, row["WarehouseCode"]);
				AssertEquals("RowName", expectedLocation.RowName, row["LocationRow"]);
				var locationType = Factory.Load<WhsLocationType>(expectedLocation.WLV_WLT_LocationType);
				AssertEquals("LocationType", locationType.WLT_Code, row["LocationType"]);
				AssertEquals("LocationStatus", expectedLocation.WLV_LocationStatus, row["LocationStatus"]);
				AssertEquals("PickMethod", expectedLocation.WLV_PickMethod, row["PickMethod"]);
				AssertEquals("MaxWeight", expectedLocation.WLV_MaxWeight, row["MaxWeight"]);
				AssertEquals("MaxCubic", expectedLocation.WLV_MaxCubic, row["MaxCubic"]);
				AssertEquals("AreaName", expectedLocation.PickingArea.WA_Name, row["AreaName"]);
				AssertEquals("AreaType", expectedLocation.PickingArea.WA_AreaType, row["AreaType"]);
				AssertEquals("Location", expectedLocation.WLV_LocationString_UserFriendly, row["Location"]);
				AssertEquals("StockOnHand", expectedStockOnHand, row["StockOnHand"]);
				AssertEquals("CommittedUnits", expectedCommittedUnits, row["CommittedUnits"]);
				AssertEquals("AvailableUnits", expectedAvailableUnits, row["AvailableUnits"]);

				if (expectedProduct != null)
				{
					AssertEquals("ProductPK", expectedProduct.PK, row["ProductPK"]);
				}
				else
				{
					AssertEquals(ZGuid.Empty, row["ProductPK"]);
				}
			});
		}

		#endregion
	}
}
