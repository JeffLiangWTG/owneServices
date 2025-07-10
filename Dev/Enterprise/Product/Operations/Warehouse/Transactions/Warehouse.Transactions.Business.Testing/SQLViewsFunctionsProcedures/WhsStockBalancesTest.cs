using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStockBalancesTest : WhsTestCaseWithFactory
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

			var result1 = LoadView_WithProductCategory(ZDateTime.Today, categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(ZDateTime.Today, categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(ZDateTime.Today, categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WithProductCategory(ZDateTime.Today, ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZDateTime date, ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = string.Empty;
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Date", date, WhsDocketSchema.WD_FinalisedDate);
			if (categoryPK.IsEmpty)
			{
				sql = "SELECT * FROM WhsStockBalancesReport(@Date, null) order by WarehouseName, ClientCode, ProductCode";
			}
			else
			{
				sql = "SELECT * FROM WhsStockBalancesReport(@Date, @ProductCategoryPK) order by WarehouseName, ClientCode, ProductCode";
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView

		[TestDate(2011, 1, 2)]
		public void TestView()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			SetupData(data);
			var result1 = Load_Report_WhsStockBalances(new ZDateTime(2011, 1, 1));
			AssertEquals("ResultSet Count", 0, result1.Count);

			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2010, 1, 5),
				data.Part1, 100m, location, "");

			var location2 = data.Whs1.FindLocation("A-2");
			var category = Helper.CreateProductCategory(data.Org1, data.Part2, "Cat1");
			category.OPC_CategoryDescription = "Product Category 1";
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2010, 1, 5),
				data.Part2, 50m, location2, "");

			Factory.Save();

			var result2 = Load_Report_WhsStockBalances(new ZDateTime(2011, 1, 1));
			AssertEquals("ResultSet Count", 2, result2.Count);

			AssertRow(result2[0], receive.Inventory[0], 100m);
			AssertRow(result2[1], receive2.Inventory[0], 50m);
			AssertEquals("LocationIndexForSort", WhsSqlViewHelper.GetLocationIndexForSort(location),
				result2[0]["LocationIndexForSort"]);
		}

		[TestDate(2022, 11, 11)]
		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			warehouse.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", new ZDateTimeOffset(2022, 11, 5), data.Part1, 100m, warehouse.FindLocation("LOCZ0040302"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R2", new ZDateTimeOffset(2022, 11, 5), data.Part2, 50m, warehouse.FindLocation("LOCZ0030201"), "");
			Factory.Save();

			var result = Load_Report_WhsStockBalances(new ZDateTime(2022, 11, 11));
			AssertEquals("ResultSet Count", 2, result.Count);
			AssertEquals("Location", "LOCZ-004-03-02", result[0]["Location"]);
			AssertRow(result[0], receive1.Inventory[0], 100m);
			AssertEquals("Location", "LOCZ-003-02-01", result[1]["Location"]);
			AssertRow(result[1], receive2.Inventory[0], 50m);
		}

		void AssertRow(DynamicBusinessObject dynamicObject, WhsInventoryView inventory, ZDecimal expectedTotalUnits)
		{
			var docketLine = inventory.InDocketLine;
			var docket = docketLine.Docket;
			var supplierPart = inventory.SupplierPart;
			var partRelation =
				supplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(inventory.Client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			var expectedPalletSpaces = (supplierPart.OP_StockKeepingUnitPerPallet > 0)
				? Math.Ceiling(expectedTotalUnits / supplierPart.OP_StockKeepingUnitPerPallet)
				: 0m;
			var expectedCommodityPK = (!supplierPart.OP_RH_NKCommodityCode.IsEmpty)
				? supplierPart.CommodityCode.PK
				: ZGuid.Empty;

			AssertEquals("WarehousePK", inventory.Warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", inventory.Warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientPK", inventory.Client.PK, dynamicObject["ClientPK"]);
			AssertEquals("ClientCode", inventory.Client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("ClientName", inventory.Client.OH_FullName, dynamicObject["ClientName"]);
			AssertEquals("ProductPK", supplierPart.PK, dynamicObject["ProductPK"]);
			AssertEquals("ProductCode", supplierPart.OP_PartNum, dynamicObject["ProductCode"]);
			AssertEquals("ProductDesc", supplierPart.OP_Desc, dynamicObject["ProductDesc"]);
			AssertEquals("ProductBrandName", supplierPart.OP_Brand, dynamicObject["ProductBrandName"]);
			AssertEquals("ProductModel", supplierPart.OP_Model, dynamicObject["ProductModel"]);
			AssertEquals("CommodityPK", expectedCommodityPK, dynamicObject["CommodityPK"]);
			AssertEquals("CommodityCode", supplierPart.OP_RH_NKCommodityCode, dynamicObject["CommodityCode"]);
			AssertEquals("Currency", supplierPart.OP_RX_NKLastWeightedCostCurr, dynamicObject["Currency"]);
			AssertEquals("StockKeepingUnit", supplierPart.OP_StockKeepingUnit, dynamicObject["StockKeepingUnit"]);
			AssertEquals("ClientUnit", partRelation.OU_ClientUQ, dynamicObject["ClientUnit"]);
			AssertEquals("UnitWeight", supplierPart.OP_Weight, dynamicObject["UnitWeight"]);
			AssertEquals("WeightUQ", supplierPart.OP_WeightUQ, dynamicObject["WeightUQ"]);
			AssertEquals("UnitVolume", supplierPart.OP_Cubic, dynamicObject["UnitVolume"]);
			AssertEquals("VolumeUQ", supplierPart.OP_CubicUQ, dynamicObject["VolumeUQ"]);
			AssertEquals("Last Cost", supplierPart.OP_LastCost, dynamicObject["LastCost"]);

			var location = inventory.Location;
			AssertEquals("LocnRow", location != null ? location.RowName : ZString.Empty, dynamicObject["LocnRow"]);
			AssertEquals("LocnCol", location != null ? location.FormattedColumn : ZString.Empty,
				dynamicObject["LocnCol"]);
			AssertEquals("LocnLevel", location != null ? location.FormattedLevel : ZString.Empty,
				dynamicObject["LocnLevel"]);
			AssertEquals("LocnTray", location != null ? location.FormattedTray : ZString.Empty,
				dynamicObject["LocnTray"]);
			AssertEquals("Location", inventory.LocationString, dynamicObject["Location"]);
			AssertEquals("LocationIndexForSort", WhsSqlViewHelper.GetLocationIndexForSort(location),
				dynamicObject["LocationIndexForSort"]);
			AssertEquals("AreaPK", location != null ? location.WLV_WA_PickingArea : ZGuid.Empty,
				dynamicObject["AreaPK"]);
			AssertEquals("AreaName", inventory.LocationPickAreaName, dynamicObject["AreaName"]);

			AssertEquals("PartAttrib1", inventory.WI_PartAttrib1, dynamicObject["PartAttrib1"]);
			AssertEquals("PartAttrib2", inventory.WI_PartAttrib2, dynamicObject["PartAttrib2"]);
			AssertEquals("PartAttrib3", inventory.WI_PartAttrib3, dynamicObject["PartAttrib3"]);
			AssertEquals("SerialNumber", inventory.WI_SerialNumber, dynamicObject["SerialNumber"]);

			AssertEquals("Quantity", expectedTotalUnits, dynamicObject["Quantity"]);
			var clientUnits = partRelation.OU_ClientUQ.IsEmpty
				? ZDecimal.Zero
				: supplierPart.UnitConverter.Convert(expectedTotalUnits, supplierPart.OP_StockKeepingUnit,
					partRelation.OU_ClientUQ);
			AssertEquals("ClientQuantity", clientUnits, dynamicObject["ClientQuantity"]);
			AssertEquals("PalletSpaces", expectedPalletSpaces, dynamicObject["PalletSpaces"]);
			AssertEquals("Weight", expectedTotalUnits * supplierPart.OP_Weight, dynamicObject["TotalWeight"]);
			AssertEquals("Volume", expectedTotalUnits * supplierPart.OP_Cubic, dynamicObject["TotalVolume"]);
			AssertEquals("Total Value", supplierPart.OP_LastCost * expectedTotalUnits, dynamicObject["TotalValue"]);

			// product category
			AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				dynamicObject["ProductCategoryDescription"]);
		}

		void SetupData(TestDataSimpleEnvironment data)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Rec1", data.Part1, 20m,
				data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();
		}

		#endregion

		#region TestView_SerialNumber

		public void TestView_SerialNumber()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var location = data.Whs1.FindLocation("A-1");
			var firstDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5);
			var secondDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 18);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_ArrivalDate = firstDate;
			var inv1 = CreateInventory(receive1, "SER1");
			var inv2 = CreateInventory(receive1, "SER2");
			var inv3 = CreateInventory(receive1, "SER3");
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = secondDate;
			var inv4 = CreateInventory(receive2, "SER4");
			var inv5 = CreateInventory(receive2, "SER5");
			var inv6 = CreateInventory(receive2, "SER6");
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var result1 = Load_Report_WhsStockBalances(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6));
			AssertEquals("ResultSet Count", 3, result1.Count);
			AssertRow(result1[0], inv1, 1m);
			AssertRow(result1[1], inv2, 1m);
			AssertRow(result1[2], inv3, 1m);

			var result2 = Load_Report_WhsStockBalances(new ZDateTime(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 19));
			AssertEquals("ResultSet Count", 6, result2.Count);
			AssertRow(result2[0], inv1, 1m);
			AssertRow(result2[1], inv2, 1m);
			AssertRow(result2[2], inv3, 1m);
			AssertRow(result2[3], inv4, 1m);
			AssertRow(result2[4], inv5, 1m);
			AssertRow(result2[5], inv6, 1m);

			WhsInventoryView CreateInventory(WhsReceive receive, string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
				line.WE_PartAttrib1 = "PA1";
				line.WE_SerialNumber = serialNumber;

				return line.Inventory[0];
			}
		}

		#endregion

		#region TestView_PickedButNotFinalisedOrder

		public void TestView_PickedButNotFinalisedOrder()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 40m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m);
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines();
			pickLines.Single(pl => pl.WZ_Units == 60m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 5);
			AssertEquals("Precondition: 60 picked units.", 60m,
				pickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 40 units committed but unpicked.", 40m,
				pickLines.Where(pl => !pl.IsPicked).Sum(pl => pl.WZ_Units));

			pickLines.Single(pl => pl.WZ_Units == 40m).WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 10);
			AssertEquals("Precondition: 100 picked units.", 100m,
				pickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 0 units committed but unpicked.", 0m,
				pickLines.Where(pl => !pl.IsPicked).Sum(pl => pl.WZ_Units));
			Factory.Save();

			var result1 = Load_Report_WhsStockBalances(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed at the entered date", 0, result1.Count);

			// Add received quantity
			var result2 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 2));
			AssertEquals("ResultSet Count", 1, result2.Count);
			AssertRow(result2[0], receive1.Inventory[0], 100m);

			// Should show 100 units, 40 in the location, 60 in transit (but represented in the src location).
			var result3 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, result3.Count);
			AssertRow(result3[0], receive1.Inventory[0], 100m);

			// Should show 100 units, 0 in the location, 100 in transit (but represented in the src location).
			var results4 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 11));
			AssertEquals("ResultSet Count", 1, results4.Count);
			AssertRow(results4[0], receive1.Inventory[0], 100m);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 15);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			// Should show 100 units as the stock removal considers the order finalised date
			var results5 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 11));
			AssertEquals("ResultSet Count", 1, results5.Count);
			AssertRow(results5[0], receive1.Inventory[0], 100m);

			// Should have no inventory left as there is no stock after the order was finalised
			var results6 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 16));
			AssertEquals("No inventory existed at the entered date.", 0, results6.Count);
		}

		#endregion

		#region TestFunction_PickedButNotFinalisedOrder_WithFinalisedDockDoorTransfer

		public void TestFunction_PickedButNotFinalisedOrder_WithFinalisedDockDoorTransfer()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receiveFinalisedDate = new ZDateTime(year, 1, 1);
			var receiveFinalisedDateOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(receiveFinalisedDate);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = receiveFinalisedDateOffset;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line, create In-Transit Transfer
			var transferLine =
				Helper.PickAndMakeInTransitTransfer(pick.GetAllPickLines().Single(), receiveFinalisedDateOffset.AddDays(1));
			var transfer = transferLine.Docket;
			Factory.Save();

			// Finalise transfer, pick/finalise the dock door stock
			transferLine.FinaliseDocketLine();
			transferLine.WE_FinalisedDate = receiveFinalisedDateOffset.AddDays(2);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			order.WD_FinalisedDate = receiveFinalisedDateOffset.AddDays(3);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results1 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(-1));
			AssertEquals("No inventory existed before entered date, so no records should be found.", 0, results1.Count);

			// Add received quantity
			var results2 = Load_Report_WhsStockBalances(receiveFinalisedDate);
			AssertEquals("Precondition - only one inventory grouped line.", 1, results2.Count);
			AssertRow(results2.Single(), receive.Inventory[0], 20m);

			// Should not show stock reduction for Dock Door Pick
			var results3 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(1));
			AssertEquals("ResultSet Count", 1, results3.Count);
			AssertRow(results3.Single(), receive.Inventory[0], 20m);

			// After increase in dock door - should still show as before
			var results4 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(2));
			AssertEquals("ResultSet Count", 1, results4.Count);
			AssertRow(results4.Single(), receive.Inventory[0], 20m);

			// Reduced after finalising pick
			var results5 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(3));
			AssertEquals("ResultSet Count", 1, results5.Count);
			AssertRow(results5.Single(), receive.Inventory[0], 12m);
		}

		#endregion

		#region TestFunction_PickedButNotFinalisedWorkOrder

		public void TestFunction_PickedButNotFinalisedWorkOrder()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var receiveFinalisedDate = new ZDateTime(year, 1, 1);
			receive.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(receiveFinalisedDate);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomBike, 10m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			var pickedDate = new ZDateTime(year, 1, 5);
			var pickedDateOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(pickedDate);
			foreach (var pickLine in workOrderLine.ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, pickedDateOffset);
			}

			Factory.Save();

			var result1 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(-1));
			AssertEquals("No inventory existed at the entered date", 0, result1.Count);

			// Add in received quantity
			var result2 = Load_Report_WhsStockBalances(receiveFinalisedDate.AddDays(1));
			AssertEquals("ResultSet Count", 2, result2.Count);
			AssertRow(result2.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(result2.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			// Should show 30 units, 0 in the location, 30 in transit (but represented in the src location).
			var result3 = Load_Report_WhsStockBalances(pickedDate.AddDays(1));
			AssertEquals("ResultSet Count", 2, result3.Count);
			AssertRow(result3.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(result3.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			var workOrderFinalisedDate = new ZDateTime(year, 1, 7);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(workOrderFinalisedDate);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			// Should show 30 units as the stock removal considers the order finalised date
			var result4 = Load_Report_WhsStockBalances(workOrderFinalisedDate.AddDays(-1));
			AssertEquals("ResultSet Count", 2, result4.Count);
			AssertRow(result4.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(result4.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			// Should have no inventory left as there is no stock after the order was finalised
			var result5 = Load_Report_WhsStockBalances(workOrderFinalisedDate.AddDays(1));
			AssertEquals("No inventory existed at the entered date", 0, result5.Count);
		}

		#endregion

		#region TestFunction_PickedButNotFinalisedWorkOrder_WithFinalisedDockDoorTransfer

		public void TestFunction_PickedButNotFinalisedWorkOrder_WithFinalisedDockDoorTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bomBike, 8m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			// Pick the line / create In-Transit Transfer
			WhsTransfer transfer = null;
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
				transfer = transfer ?? transferLine.Docket;
			}

			Factory.Save();

			// Finalise transfer, pick/finalise the dock door stock
			transfer.FinaliseDocket();

			var resultsBeforeTransferFinalisation = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 3));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2,
				resultsBeforeTransferFinalisation.Count);
			AssertRow(resultsBeforeTransferFinalisation.Single(r => (ZString)r["ProductCode"] == "FRAME"),
				frameInventory.Inventory[0], 10m);
			AssertRow(resultsBeforeTransferFinalisation.Single(r => (ZString)r["ProductCode"] == "WHEEL"),
				wheelInventory.Inventory[0], 20m);

			var resultsAfterTransferFinalisation = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 5));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2,
				resultsAfterTransferFinalisation.Count);
			AssertRow(resultsAfterTransferFinalisation.Single(r => (ZString)r["ProductCode"] == "FRAME"),
				frameInventory.Inventory[0], 10m);
			AssertRow(resultsAfterTransferFinalisation.Single(r => (ZString)r["ProductCode"] == "WHEEL"),
				wheelInventory.Inventory[0], 20m);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 7);
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			// Add received quantity
			var results2 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 1));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results2.Count);
			AssertRow(results2.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(results2.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			// Should still appear in location after picked by Dock Door Transfer
			var results3 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 3));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results3.Count);
			AssertRow(results3.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(results3.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			// Should still appear in location after In-Transit stock created by Dock Door Transfer
			var results4 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 5));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results4.Count);
			AssertRow(results4.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 10m);
			AssertRow(results4.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 20m);

			// Reduce stock by finalising pick
			var results5 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 8));
			AssertEquals("Precondition - should be two grouped inventory lines.", 2, results5.Count);
			AssertRow(results5.Single(r => (ZString)r["ProductCode"] == "FRAME"), frameInventory.Inventory[0], 2m);
			AssertRow(results5.Single(r => (ZString)r["ProductCode"] == "WHEEL"), wheelInventory.Inventory[0], 4m);
		}

		#endregion

		#region TestView_PickedButUnfinalisedInnerTransfers

		public void TestView_PickedButUnfinalisedInnerTransfers()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, locationA1, "");
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, locationA1, "");
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 110m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // commit transfer line
			AssertEquals("Precondition - committed 110 units to transfer.", 110m,
				transferLine.QtyCommittedIncludingMatchingLines);
			transferLine.PickedTime = new ZDateTimeOffset(year, 1, 5);

			// Make sure we use the pick line's values
			transferLine.MatchingLines[0].PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 7);
			Factory.Save();

			var result1 = Load_Report_WhsStockBalances(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed at the entered date", 0, result1.Count);

			// Add received qty
			var result2 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 2));
			AssertEquals("ResultSet Count", 1, result2.Count);
			AssertRow(result2[0], receive1.Inventory[0], 200m);

			// Should show 200 units, 100 in the location, 100 in transit (but represented in the src location).
			var result3 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, result3.Count);
			AssertRow(result3[0], receive1.Inventory[0], 200m);

			// Should show 200 units, 90 in the location, 110 in transit (but represented in the src location).
			var result4 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 8));
			AssertEquals("ResultSet Count", 1, result4.Count);
			AssertRow(result4[0], receive1.Inventory[0], 200m);

			transfer.FinaliseDocket();
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferLine.MatchingLines[0].WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			// Add in transferred quantity
			var result5 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 11));
			AssertEquals("ResultSet Count", 2, result5.Count);
			AssertRow(result5.Single(r => (ZString)r["Location"] == "A-1"), receive1.Inventory[0], 90m);
			AssertRow(result5.Single(r => (ZString)r["Location"] == "A-2"), transferLine.Inventory[0], 110m);
		}

		#endregion

		#region TestView_PickedButUnfinalisedInnerTransfers_WithUnPickedLinesOnTransfer

		public void TestView_PickedButUnfinalisedInnerTransfers_WithUnPickedLinesOnTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var nonInTransitTransferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString, "");
			var inTransitTransferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, inventory.LocationString, "");
			AssertEquals("Precondition: Transfer Line should not be in transit.", InventoryStatus.Codes.Available,
				inTransitTransferLine.WE_CurrentInventoryStatus);
			transfer.RunPreSaveValidation();
			inTransitTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line should be in transit.", InventoryStatus.Codes.InTransit,
				inTransitTransferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = Load_Report_WhsStockBalances(ZDateTime.Empty);
			var result = results.Single();
			AssertEquals("Quantity", 10m, result["Quantity"]);
		}

		#endregion

		#region TestView_PickedButUnfinalisedInterWhsTransfers

		public void TestView_PickedButUnfinalisedInterWhsTransfers()
		{
			var year = ZDateTime.Now.Year;
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");

			var receive =
				Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 100m, whs1.FindLocation("A-1"), "");
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var transferInterWhsSource =
				Helper.CreateWhsTransfer(client, whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A-1", whs2.PK, "B");
			transferInterWhsSource.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 15m,
				transferInterWhsSourceLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsSourceLine.PickedTime = new ZDateTimeOffset(year, 1, 5);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs2, "TR3", Notify, TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine =
				Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A-1", whs1.PK, "B");
			transferInterWhsDest.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 25m,
				transferInterWhsDestLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsDestLine.PickedTime = new ZDateTimeOffset(year, 1, 5);
			Factory.Save();

			var result1 = Load_Report_WhsStockBalances(new ZDateTime(year - 1, 12, 30));
			AssertEquals("No inventory existed at the entered date", 0, result1.Count);

			// Add received qty
			var result2 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 2));
			AssertEquals("ResultSet Count", 1, result2.Count);
			AssertRow(result2[0], receive.Inventory[0], 100m);

			// Should show 100 units, 60 in the location, 40 in transit (but represented in the src location).
			var result3 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 6));
			AssertEquals("ResultSet Count", 1, result3.Count);
			AssertRow(result3[0], receive.Inventory[0], 100m);

			transferInterWhsSourceLine.FinaliseDocketLine();
			transferInterWhsDestLine.FinaliseDocketLine();
			transferInterWhsSourceLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsSourceLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsDestLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			transferInterWhsDestLine.ChildTransferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 10);
			Factory.Save();
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine);
			AssertIsFinalisedPrecondition(transferInterWhsDestLine);
			Factory.Save();

			var result5 = Load_Report_WhsStockBalances(new ZDateTime(year, 1, 11));
			AssertEquals("ResultSet Count", 2, result5.Count);
			AssertRow(result5.Single(r => (ZString)r["Location"] == "A-1"), receive.Inventory[0], 60m);
			AssertRow(result5.Single(r => (ZString)r["Location"] == "B"), transferInterWhsDestLine.Inventory[0], 40m);
		}

		#endregion

		#region TestView_WithPickFromMultipleLocations

		[TestDate(2016, 1, 30)]
		public void TestView_WithPickFromMultipleLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var whs2 = Helper.CreateWarehouse("2", "B", 20, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, location1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, location2);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			var location3 = whs2.FindLocation("B-1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			var inventory21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 100m, location3);
			receive2.FinaliseDocket();
			AssertEquals(true, receive2.IsFinalised);
			Factory.Save();
			//	Whs1	A-1		100m
			//			A-2		100m
			//	Whs2	B-1		100m

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondtion", 2, pick1.OrderedInventories[0].AvailableInventories.Count);
			var availableInventory1 = pick1.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>()
				.Single(a => a.Location == location1);
			availableInventory1.PickLineQuantity = 10m;
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);
			//	Whs1	A-1		100m	-10
			//			A-2		100m
			//	Whs2	B-1		100m

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR2", WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondtion", 2, pick2.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory21 = pick2.OrderedInventories[0].AvailableInventories
				.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location1);
			var availableInventory22 = pick2.OrderedInventories[0].AvailableInventories
				.Cast<WhsPickAvailableInventory>().Single(a => a.Location == location2);

			availableInventory21.PickLineQuantity = 5m;
			availableInventory22.PickLineQuantity = 5m;

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();

			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick2);
			//	Whs1	A-1		100m	-10		-5
			//			A-2		100m			-5
			//	Whs2	B-1		100m

			// inter-warehouse transfer, from Whs1 "A-1" 20m -> whs2 "B-1";
			//								  Whs1 "A-2" 10m -> whs2 "B-1";
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, whs2, "TR1", Notify, TransferType.Codes.InterWhsDest);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 20m, "A-1", data.Whs1.PK, "B-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, "A-2", data.Whs1.PK, "B-1");
			var laterDate = ZDateTimeOffset.Today.AddDays(3);
			transferLine1.PickedTime = laterDate;
			transferLine2.PickedTime = laterDate;
			transfer1.FinaliseDocket();
			AssertEquals(true, transfer1.IsFinalised);

			transfer1.WD_FinalisedDate = laterDate;
			foreach (var child in transfer1.ChildTransfers)
			{
				child.WD_FinalisedDate = laterDate;
				foreach (var line in child.Lines)
				{
					line.WE_FinalisedDate = laterDate;
				}
			}

			transferLine1.WE_FinalisedDate = laterDate;
			transferLine2.WE_FinalisedDate = laterDate;
			Factory.Save();
			//	Whs1	A-1		100m	-10		-5		(85)	-20
			//			A-2		100m			-5		(95)	-10
			//	Whs2	B-1		100m					(100)	+30

			var result = Load_Report_WhsStockBalances(new ZDateTime(2016, 1, 30));
			AssertEquals(3, result.Count);

			var result1 = result.Single(r => r["Location"].ToString() == "A-1");
			AssertEquals(85m, result1["Quantity"]);

			var result2 = result.Single(r => r["Location"].ToString() == "A-2");
			AssertEquals(95m, result2["Quantity"]);

			var result3 = result.Single(r => r["Location"].ToString() == "B-1");
			AssertEquals(100m, result3["Quantity"]);

			result = Load_Report_WhsStockBalances(ZDateTime.Today.AddDays(-1));
			AssertEquals(0, result.Count);
		}

		#endregion

		#region TestView_NoDivideByZeroExceptionDueToPalletConversion

		[TestDate(2015, 03, 27)]
		public void TestView_NoDivideByZeroExceptionDueToPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "CAS", 0.0001);
			Helper.CreateProductUnit(data.Part1, "CAS", "PLT", 0.0001);
			// so after all rounding we will get 0 PLT in a UNT

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part1, 1m);
			Factory.Save();

			var result = Load_OrgSupplierPartUnitsPerPallet(data.Part1.PK);
			AssertEquals("Precondition: OrgSupplierPartUnitsPerPallet should return 0", 0m,
				result[0]["NumPalletsPerUnit"]);

			AssertNoExceptionThrown("No Division by zero exception should be thrown",
				() => Load_Report_WhsStockBalances(ZDateTime.Today));
			AssertEquals(1, Load_Report_WhsStockBalances(ZDateTime.Today).Count);
		}

		DynamicBusinessObjectCollection Load_OrgSupplierPartUnitsPerPallet(ZGuid productPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"SELECT UnitsPerPallet as NumPalletsPerUnit from dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) as tableResult";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductPK", productPK, WhsDocketLineSchema.WE_OP);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView_PutawayTransfers

		public void TestView_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Factory.Save();

			var results1 = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("Unfinalised receives should not be shown.", 0, results1.Count);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1,
				data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT-1", 10m);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var results2 = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("Putaway Transfers should not be shown.", 0, results2.Count);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			var results3 = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("Picked Putaway Transfers should not be shown.", 0, results3.Count);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var results4 = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("Putaway Transfers should not be shown.", 0, results4.Count);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receive.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(-1);
			Factory.Save();

			var results5 = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("Only putaway transfers should be shown as it has the stock on hand.", 1, results5.Count);

			var result = results5.Single(r => r["Location"].ToString() == data.Whs1.DefaultLocation.ToLocationString());
			AssertEquals("Stock balances should show the current stock of 10 Units.", 10m, result["Quantity"]);

			var results6 = Load_Report_WhsStockBalances(ZDateTime.Now.AddDays(-3));
			AssertEquals("Nothing should exist for the past.", 0, results6.Count);
		}

		#endregion

		#region TestReportIgnoresCustomAttributes

		[TestDate(2011, 1, 2)]
		public void TestReportIgnoresCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3);
			order.Lines[0].WE_CustomAttrib1 = "TestValue";
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var result = Load_Report_WhsStockBalances(new ZDateTime(2011, 1, 1));

			AssertEquals("There should be no stock before receive", 0, result.Count);
		}

		#endregion

		public void TestReturnStocksTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_OrigLocationNotReturnLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var returnTransfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			var returnTransferLine = returnTransfer.Lines.Single();
			returnTransferLine.WE_WL = data.Whs1.FindLocation("A-2").PK;
			Factory.Save();

			AssertNotEquals(receiveLine.WE_WL, returnTransferLine.WE_WL);

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_OrigLocationNotReturnLocation_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var returnTransfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			var returnTransferLine = returnTransfer.Lines.Single();
			returnTransferLine.WE_WL = data.Whs1.FindLocation("A-2").PK;

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			Assert(returnTransfer.IsFinalised);
			Factory.Save();

			AssertNotEquals(receiveLine.WE_WL, returnTransferLine.WE_WL);

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("2 should exist for future.", 2, resultsFuture.Count);
			var resultLoc1 = resultsFuture.Where(r => (ZString)r["Location"] == "A-1").Single();
			AssertEquals(80m, resultLoc1["Quantity"]);
			var resultLoc2 = resultsFuture.Where(r => (ZString)r["Location"] == "A-2").Single();
			AssertEquals(20m, resultLoc2["Quantity"]);
		}

		public void TestReturnStocksTransfer_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			Assert(!transfer.IsFinalised);

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert(transfer.IsFinalised);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_PartialReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresentBeforePickFinalised = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresentBeforePickFinalised.Count);
			AssertResult(resultsPresentBeforePickFinalised.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFutureBeforePickFinalised = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFutureBeforePickFinalised.Count);
			AssertResult(resultsFutureBeforePickFinalised.Single(), data.Part1.PK, "A-1", 100m);

			// Had to create additional factory for synchronisation with database
			// This will not happen in real business cases due to common factory in GUI
			var otherFactory = new BusinessObjectFactory();
			var loadOrder = otherFactory.Load<WhsOrder>(order.PK);
			var loadPick = otherFactory.Load<WhsPick>(pick.PK);
			loadOrder.FinaliseDocketWithoutUserConfirmation();
			loadPick.FinalisePick();
			otherFactory.Save();

			Assert(loadPick.IsFinalised);

			var resultsPresentAfterPickFinalised = Load_Report_WhsStockBalances(ZDateTime.Now, otherFactory);
			AssertEquals("1 should exist for present.", 1, resultsPresentAfterPickFinalised.Count);
			AssertResult(resultsPresentAfterPickFinalised.Single(), data.Part1.PK, "A-1", 90m);

			var resultsresultsPresentAfterPickFinalised = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20), otherFactory);
			AssertEquals("1 should exist for future.", 1, resultsresultsPresentAfterPickFinalised.Count);
			AssertResult(resultsresultsPresentAfterPickFinalised.Single(), data.Part1.PK, "A-1", 90m);
		}

		public void TestReturnStocksTransfer_PartialReturn_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			Assert(!transfer.IsFinalised);

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert(transfer.IsFinalised);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_MultiStageTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_MultiStageTransfer_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			Assert(!transfer.IsFinalised);

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert(transfer.IsFinalised);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_MultiStageTransfer_PartialReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestReturnStocksTransfer_MultiStageTransfer_Finalised_PartialReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			Assert(!transfer.IsFinalised);

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert(transfer.IsFinalised);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 100m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 100m);
		}

		public void TestAdjustOutQtyMet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 80m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 80m);
		}

		public void TestAdjustOutQtyMet_PartialAdjustOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 90m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 90m);
		}

		public void TestAdjustOutQtyMet_MultiStageTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 80m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 80m);
		}

		public void TestAdjustOutQtyMet_MultiStageTransfer_PartialReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit,
					ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location =
					i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today.AddDays(-1);
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 10,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true,
				result.IsSuccess);
			Factory.Save();

			var resultsPast = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(-20));
			AssertEquals("Nothing should exist for the past.", 0, resultsPast.Count);

			var resultsPresent = Load_Report_WhsStockBalances(ZDateTime.Now);
			AssertEquals("1 should exist for present.", 1, resultsPresent.Count);
			AssertResult(resultsPresent.Single(), data.Part1.PK, "A-1", 90m);

			var resultsFuture = Load_Report_WhsStockBalances(ZDateTime.Now.AddYears(+20));
			AssertEquals("1 should exist for future.", 1, resultsFuture.Count);
			AssertResult(resultsFuture.Single(), data.Part1.PK, "A-1", 90m);
		}

		void AssertResult(DynamicBusinessObject result, ZGuid expectedProductPK, string expectedLocation,
			ZDecimal expectedQty)
		{
			AssertEquals("ProductPK", expectedProductPK, result["ProductPK"]);
			AssertEquals("Location", expectedLocation, result["Location"]);
			AssertEquals("Quantity", expectedQty, result["Quantity"]);
		}

		#region Load_Report_WhsStockBalances

		DynamicBusinessObjectCollection Load_Report_WhsStockBalances(ZDateTime date, BusinessObjectFactory factory = null)
		{
			var result = new DynamicBusinessObjectCollection(factory ?? Factory);
			var sql = "SELECT * FROM WhsStockBalancesReport(@Date, null) order by WarehouseName, ClientCode, ProductCode";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Date", date, WhsDocketSchema.WD_FinalisedDate);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion
	}
}
