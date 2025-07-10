using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsStockPickFaceReplenishmentReportTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var locations = warehouse.Rows.Single(r => r.WR_Name == "A").Locations;
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
				MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
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

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(productVB), client, locations[0], 5m, 20m, 2m);
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(productCoke), client, locations[1], 5m, 20m);
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(productTea), client, locations[2], 5m, 20m);

			Factory.Save();

			var result1 = LoadView_ProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_ProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_ProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_ProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_ProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"select * from WhsStockPickFaceReplenishmentReport(null, null, null)"
				: @"select * from WhsStockPickFaceReplenishmentReport(null, null, @ProductCategoryPK)";
			var sqlParams = new ZSqlParameterCollection();
			if (!categoryPK.IsEmpty)
			{
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView

		public void TestView()
		{
			var warehouse1 = Helper.CreateWarehouse("1", "A", 5, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 5, 1);
			var locations1 = warehouse1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locations2 = warehouse2.Rows.Single(r => r.WR_Name == "B").Locations;
			var client1 = Helper.CreateClient("CLIENT1", "CLIENT 1");
			var client2 = Helper.CreateClient("CLIENT2", "CLIENT 2");
			var part1 = Helper.CreateProduct(client1, "P1");
			Helper.CreateProductClientRelationShip(client2, part1);
			var part2 = Helper.CreateProduct(client1, "P2");
			var part3 = Helper.CreateProduct(client2, "P3");
			var part4_Inactive = Helper.CreateProduct(client2, "IN_PR");
			part4_Inactive.OP_IsActive = false;

			part1.OP_Brand = "PRO BRAND";
			part1.OP_Model = "PRO MODEL";

			var pickFace11 =
				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(part1), client1, locations1[0], 10m, 20m, 2m);
			var pickFace12 =
				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(part1), client1, locations1[1], 10m, 20m);
			var pickFace21 =
				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(part2), client1, locations2[0], 5m,
					20m); // won't require replenishment (lots of stock)
			var pickFace31 =
				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(part3), client2, locations1[0], 5m,
					20m); // no inventory to replenish from
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(part4_Inactive), client2, locations2[1], 5m,
				20m); // inactive product, shouldn't display

			var inventoryCol = SetupData(warehouse1, warehouse2, client1, client2, part1, part2, part3, locations1,
				locations2);
			var resultLoadedFromView = LoadReportData();
			AssertEquals(14, resultLoadedFromView.Count);

			inventoryCol[4].InDocketLine.WE_StockOnHand =
				20m; // Inventory11_5 and Inventory_6 will be rolled up into the same line, 10+10=20 QuantityAvaivable

			// Tests for same PickFace but different inventories, ordered by ExpiryDate, PackingDate, ArrivalDate, InventoryLocation. 5m is how much stock currently in PickFaceLocation
			var availableQtyInPickFace11 = 5m;
			AssertCorrectData(pickFace11, inventoryCol[4].InDocketLine, resultLoadedFromView[0],
				availableQtyInPickFace11, 20m);
			AssertCorrectData(pickFace11, inventoryCol[6].InDocketLine, resultLoadedFromView[1],
				availableQtyInPickFace11, 10m);
			AssertCorrectData(pickFace11, inventoryCol[7].InDocketLine, resultLoadedFromView[2],
				availableQtyInPickFace11, 10m);
			AssertCorrectData(pickFace11, inventoryCol[8].InDocketLine, resultLoadedFromView[3],
				availableQtyInPickFace11, 10m);
			AssertCorrectData(pickFace11, inventoryCol[10].InDocketLine, resultLoadedFromView[4],
				availableQtyInPickFace11, 10m);
			AssertCorrectData(pickFace11, inventoryCol[9].InDocketLine, resultLoadedFromView[5],
				availableQtyInPickFace11, 8m);

			// Tests for same PickFace but different inventories, ordered by ExpiryDate, PackingDate, ArrivalDate, InventoryLocation. 4m is how much stock currently in PickFaceLocation
			var availableQtyInPickFace12 = 4m;
			AssertCorrectData(pickFace12, inventoryCol[4].InDocketLine, resultLoadedFromView[6],
				availableQtyInPickFace12, 20m);
			AssertCorrectData(pickFace12, inventoryCol[6].InDocketLine, resultLoadedFromView[7],
				availableQtyInPickFace12, 10m);
			AssertCorrectData(pickFace12, inventoryCol[7].InDocketLine, resultLoadedFromView[8],
				availableQtyInPickFace12, 10m);
			AssertCorrectData(pickFace12, inventoryCol[8].InDocketLine, resultLoadedFromView[9],
				availableQtyInPickFace12, 10m);
			AssertCorrectData(pickFace12, inventoryCol[10].InDocketLine, resultLoadedFromView[10],
				availableQtyInPickFace12, 10m);
			AssertCorrectData(pickFace12, inventoryCol[9].InDocketLine, resultLoadedFromView[11],
				availableQtyInPickFace12, 8m);

			// Tests for PickFace that does not require replenishment. Pickface should have null Location properties.
			AssertPickFace(pickFace21, resultLoadedFromView[12], 10m);
			var quantityAvailable = (ZDecimal)resultLoadedFromView[12]["QuantityAvailable"];
			var replenishMin = (ZDecimal)resultLoadedFromView[12]["ReplenishMin"];
			AssertEquals(true, quantityAvailable > replenishMin);
			AssertEquals("", resultLoadedFromView[12]["PickFromLocation"]);

			// Tests for PickFace that has no stock to be replenished from. Such Pick faces has Zero, Empty or null inventory properties so check PickFace properties only.
			AssertPickFace(pickFace31, resultLoadedFromView[13], 1m);
			AssertEquals("This line should have had 0 units because it has no stock to replenish.", "",
				resultLoadedFromView[13]["PickFromLocation"]);
		}

		#endregion

		#region TestView_Transfers

		public void TestView_Transfers()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1,
					data.Whs1.FindLocation("A-1"), 100m, 200m);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
					data.Whs1.FindLocation("A-1"), "");

				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
				var adjustmentLine =
					Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -7m, data.Whs1.FindLocation("A-1"));
				adjustmentLine.RunPreSaveValidation();
				AssertEquals("Precondition: Stock is committed.", 7m, adjustmentLine.CommittedQuantity);

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-2", "A-3");
				transferLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(transferLine);

				transfer.RunPreSaveValidation(); // to commit stock.
				Factory.Save();

				var results = LoadReportData();
				AssertEquals("Should find 1 receive inventory and one transferred inventory.", 1, results.Count);
				// Only 10 Units available in Location A-2, as transferLine3 has 5 units committed.
				AssertCorrectData(pickFace, transferLine, results[0], 85m, 10m);
			}
		}

		#endregion

		#region TestView_WithCommittedUnits

		public void TestView_WithCommittedUnits()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = Helper.CreateGlbStaff("STF", "Staff");
				var data = new TestDataSimpleEnvironment(Factory, 6, 1);
				var locationWithNoAllocatedInv = data.Whs1.FindLocation("A-1");
				var locationWithAllocatedInv = data.Whs1.FindLocation("A-2");
				var locationWithPickerName = data.Whs1.FindLocation("A-3");
				var locationWithPickerNameAndDate1 = data.Whs1.FindLocation("A-4");
				var locationWithReservedInventory = data.Whs1.FindLocation("A-5");
				var locationWithPickerNameAndDate2 = data.Whs1.FindLocation("A-6");
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithNoAllocatedInv, replenishMin: 1m,
					replenishMax: 2m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithAllocatedInv, replenishMin: 1m,
					replenishMax: 2m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithPickerName, replenishMin: 1m,
					replenishMax: 2m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithPickerNameAndDate1, replenishMin: 1m,
					replenishMax: 2m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithReservedInventory, replenishMin: 1m,
					replenishMax: 2m);
				Helper.CreateProductPickFace(data.Part1, data.Org1, locationWithPickerNameAndDate2, replenishMin: 2m,
					replenishMax: 4m);

				// create a receive, order and pick
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithNoAllocatedInv);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithAllocatedInv);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerName);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithPickerNameAndDate1);
				var reservedInventory =
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationWithReservedInventory);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, locationWithPickerNameAndDate2);
				receive.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive);

				var reservedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", Notify);
				var reservedOrderLine = Helper.CreateWhsOrderLine(reservedOrder, data.Part1, 2m);
				var reservedPickLine = reservedOrderLine.ReserveStockIfAbleTo(reservedInventory);
				AssertEquals("Precondition: Stock is reserved.", 2m, reservedPickLine.ReservedQuantity);

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
				Helper.CreateWhsOrderLine(order, data.Part1, 14m);

				var pick = Helper.CreatePickNew(order);
				var availableInventories = ((WhsPickOrderedInventory)pick.OrderedInventories.Single()).AvailableInventories
					.Cast<WhsPickAvailableInventory>();

				// allocate available inventory
				availableInventories.Single(a => a.Location == locationWithNoAllocatedInv);
				var avlInvAllocatedStock = availableInventories.Single(a => a.Location == locationWithAllocatedInv);
				var avlInvWithPickerName = availableInventories.Single(a => a.Location == locationWithPickerName);
				var avlInvWithPickerNameAndDate1 =
					availableInventories.Single(a => a.Location == locationWithPickerNameAndDate1);
				var avlInvWithPickerNameAndDate2 =
					availableInventories.Single(a => a.Location == locationWithPickerNameAndDate2);
				SetAvailableInventory(avlInvAllocatedStock, true, null, ZDateTime.Empty);
				SetAvailableInventory(avlInvWithPickerName, true, staff, ZDateTime.Empty);
				SetAvailableInventory(avlInvWithPickerNameAndDate1, true, staff, ZDateTime.Now);

				avlInvWithPickerNameAndDate2.PickLineQuantity = 1m;
				Helper.SetAssignToPk(avlInvWithPickerNameAndDate2, staff.PK);
				Helper.SetPickedDate(avlInvWithPickerNameAndDate2, ZDateTimeOffset.Now);
				Factory.Save();

				var viewResultWithPickReplenishmentTurnedOn = LoadReportData();
				AssertEquals("Should find 5 receive inventory.", 6, viewResultWithPickReplenishmentTurnedOn.Count);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[0], "A-1", 2m);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[1], "A-2", 2m);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[2], "A-3", 2m);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[3], "A-4", 0m);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[4], "A-5", 2m);
				AssertPickReplenishmentResult(viewResultWithPickReplenishmentTurnedOn[5], "A-6", 3m);
			}
		}

		void SetAvailableInventory(WhsPickAvailableInventory availableInventory, bool allocateInventory,
			GlbStaff picker, ZDateTime pickedDate)
		{
			availableInventory.Allocate = allocateInventory;
			Helper.SetAssignToPk(availableInventory, picker?.PK ?? ZGuid.Empty);
			Helper.SetPickedDate(availableInventory, new ZDateTimeOffset(pickedDate));
		}

		void AssertPickReplenishmentResult(DynamicBusinessObject viewResult, ZString expectedLocation,
			ZDecimal expectedQuantityAvailable)
		{
			AssertEquals(expectedLocation, (ZString)viewResult["PickFaceLocation"]);
			AssertEquals(expectedQuantityAvailable, (ZDecimal)viewResult["QuantityAvailable"]);
		}

		#endregion

		#region TestView_FixedWidthLocationWarehouse

		public void TestView_FixedWidthLocationWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			Factory.Save();

			var pickFaceLocation = warehouse.FindLocation("LOCZ0040302");
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, pickFaceLocation, 10m, 20m, 2m);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 5m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R2", data.Part1, 50m, warehouse.FindLocation("LOCZ0030201"), "");
			Factory.Save();

			var result = LoadReportData().Single();
			AssertEquals("LOCZ-004-03-02", result["PickFaceLocation"]);
			AssertEquals("LOCZ-003-02-01", result["PickFromLocation"]);
		}

		#endregion

		#region Asserts

		void AssertCorrectData(WhsPickFace pickFace, WhsDocketLine docketLine, DynamicBusinessObject dynamicBizO,
			ZDecimal expectedAvailableQuantity, ZDecimal expectedPickFromAvailableQuantity)
		{
			var docket = docketLine.Docket;
			AssertEquals(pickFace.WF_OH_Client, docket.WD_OH_Client);
			AssertEquals(pickFace.WF_OP, docketLine.WE_OP);

			AssertPickFace(pickFace, dynamicBizO, expectedAvailableQuantity);
			AssertInventory(docketLine, dynamicBizO, expectedPickFromAvailableQuantity);
		}

		void AssertPickFace(WhsPickFace pickFace, DynamicBusinessObject dynamicBizO, ZDecimal expectedAvailableQuantity)
		{
			AssertEquals(pickFace.LocationString, dynamicBizO["PickFaceLocation"]);
			AssertEquals(WhsSqlViewHelper.GetLocationIndexForSort(pickFace.Location),
				dynamicBizO["PickFaceLocationIndexForSort"]);
			AssertEquals(pickFace.Location.Row.WR_Name, dynamicBizO["PickFaceLocnRow"]);
			AssertEquals(pickFace.Location.WLV_WA_PickingArea, dynamicBizO["AreaPK"]);
			AssertEquals(pickFace.Location.PickingArea.WA_Name, dynamicBizO["AreaName"]);
			AssertEquals(pickFace.Location.LocationType.WLT_Code, dynamicBizO["LocationType"]);
			AssertEquals(pickFace.Location.WLV_MaxWeight, dynamicBizO["LocationMaxWeight"]);
			AssertEquals(pickFace.Location.WLV_MaxWeightUnit, dynamicBizO["LocationMaxWeightUQ"]);
			AssertEquals(pickFace.Location.WLV_MaxCubic, dynamicBizO["LocationMaxCubic"]);
			AssertEquals(pickFace.Location.WLV_MaxCubicUnit, dynamicBizO["LocationMaxCubicUQ"]);
			AssertEquals(pickFace.WF_OP, dynamicBizO["ProductPK"]);
			AssertEquals(pickFace.SupplierPart.OP_PartNum, dynamicBizO["ProductCode"]);
			AssertEquals(pickFace.SupplierPart.OP_Desc, dynamicBizO["ProductDescription"]);
			AssertEquals(pickFace.SupplierPart.OP_Brand, dynamicBizO["ProductBrandName"]);
			AssertEquals(pickFace.SupplierPart.OP_Model, dynamicBizO["ProductModel"]);
			AssertEquals(pickFace.WF_ReplenishMinimum, dynamicBizO["ReplenishMin"]);
			AssertEquals(pickFace.WF_ReplenishMaximum, dynamicBizO["ReplenishMax"]);
			AssertEquals(pickFace.WF_ReplenishmentMultiple, dynamicBizO["ReplenishMultiple"]);
			AssertEquals(expectedAvailableQuantity, dynamicBizO["QuantityAvailable"]);
			AssertEquals(pickFace.SupplierPart.OP_StockKeepingUnit, dynamicBizO["QuantityAvailableUQ"]);

			var client = pickFace.Client;
			var partRelation =
				pickFace.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			AssertEquals(expectedCategoryCode, dynamicBizO["ProductCategoryCode"]);

			var expectedCommodityPK = (!pickFace.SupplierPart.OP_RH_NKCommodityCode.IsEmpty)
				? pickFace.SupplierPart.CommodityCode.PK
				: ZGuid.Empty;
			AssertEquals(expectedCommodityPK, dynamicBizO["CommodityPK"]);
			AssertEquals(pickFace.SupplierPart.OP_RH_NKCommodityCode, dynamicBizO["CommodityCode"]);
			AssertEquals(pickFace.WF_OH_Client, dynamicBizO["ClientPK"]);
			AssertEquals(pickFace.Client.OH_Code, dynamicBizO["ClientCode"]);
			AssertEquals(pickFace.Client.OH_FullName, dynamicBizO["ClientName"]);
			AssertEquals(pickFace.Location.Row.WR_WW_Whs, dynamicBizO["WarehousePK"]);
			AssertEquals(pickFace.Location.Row.Warehouse.WW_WarehouseCode, dynamicBizO["WarehouseCode"]);
			AssertEquals(pickFace.Location.Row.Warehouse.WW_WarehouseName, dynamicBizO["WarehouseName"]);
		}

		void AssertInventory(WhsDocketLine docketLine, DynamicBusinessObject dynamicBizO,
			ZDecimal expectedPickFromAvailableQuantity)
		{
			AssertEquals(docketLine.LocationString, dynamicBizO["PickFromLocation"]);
			AssertEquals(WhsSqlViewHelper.GetLocationIndexForSort(docketLine.Location),
				dynamicBizO["PickFromLocationIndexForSort"]);
			AssertEquals(docketLine.Location.Row.WR_Name, dynamicBizO["PickFromLocnRow"]);
			AssertEquals(expectedPickFromAvailableQuantity, dynamicBizO["PickFromQuantityAvailable"]);
			AssertEquals(docketLine.SupplierPart.OP_StockKeepingUnit, dynamicBizO["PickFromQuantityAvailableUQ"]);
			AssertEquals(docketLine.WE_ExpiryDate, dynamicBizO["PickFromExpiryDate"]);
			AssertEquals(docketLine.WE_PackingDate, dynamicBizO["PickFromPackingDate"]);
			AssertEquals(docketLine.WE_AdjustmentArrivalDate.Date, dynamicBizO["PickFromArrivalDate"]);
			AssertEquals(docketLine.Docket.WD_OH_Client, dynamicBizO["PickFromClient"]);
			AssertEquals(docketLine.WE_OP, dynamicBizO["PickFromPart"]);
			AssertEquals(docketLine.Warehouse.PK, dynamicBizO["PickFromWarehousePK"]);
		}

		#endregion

		#region SetupData

		WhsInventoryViewCollection SetupData(WhsWarehouse warehouse1, WhsWarehouse warehouse2, OrgHeader client1,
			OrgHeader client2, OrgSupplierPart part1, OrgSupplierPart part2, OrgSupplierPart part3,
			WhsLocationCollection locations1, WhsLocationCollection locations2)
		{
			var inventoryCol = new WhsInventoryViewCollection(Factory);
			var category1 = Helper.CreateProductCategory(client1, part1, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";
			var category2 = Helper.CreateProductCategory(client1, part2, "Cat2");
			category2.OPC_CategoryDescription = "Category 2";

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				if (attribNo != AttributeNumber.Serial)
				{
					Helper.SetClientAttributeType(client1, attribNo, true);
					Helper.SetClientAttributeType(client2, attribNo, true);
					Helper.SetProductAttributeUse(client1, part1, attribNo, true);
					Helper.SetProductAttributeUse(client2, part1, attribNo, true);
				}
			}

			var rowToHaveEmptyLocation = Helper.CreateRowAndGenerateLocations(warehouse1, "C", 1, 1);
			var emptyLocation = rowToHaveEmptyLocation.Locations[0];
			var receiveToBeCancelledOut = Helper.CreateWhsReceive(client1, warehouse1, "Receive");
			Helper.CreateWhsReceiveInventoryLine(receiveToBeCancelledOut, part1, 1m, emptyLocation,
				ZDate.Today.AddDays(1), ZDate.Today.AddDays(1), "PA1", "PA2", "PA3", "BEK");
			receiveToBeCancelledOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receiveToBeCancelledOut);

			var orderToReduceInventoryToZero = Helper.CreateWhsOrderWithOrderLine(client1, warehouse1, "O1", part1, 1m);
			Factory.Save();
			Helper.CreatePickNew(true, true, orderToReduceInventoryToZero);

			var receive11 = Helper.CreateWhsReceive(client1, warehouse1, "R11", Notify);
			var receive12 = Helper.CreateWhsReceive(client1, warehouse2, "R12", Notify);
			var receive21 = Helper.CreateWhsReceive(client2, warehouse1, "R21", Notify);

			receive11.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-1);
			receive12.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-2);
			receive21.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-3);

			//Receive11
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 1m, ZDate.Today.AddDays(+3),
				ZDate.Today.AddDays(-3), locations1[0]));
			var inventory11_2 = CreateWhsReceiveInventoryLine(receive11, part1, 4m, ZDate.Today.AddDays(+2),
				ZDate.Today.AddDays(-2), locations1[0]);
			inventoryCol.Add(inventory11_2);
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 4m, ZDate.Today.AddDays(+1),
				ZDate.Today.AddDays(-1), locations1[1]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part2, 8m, locations1[2]));

			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+1),
				ZDate.Today.AddDays(-3), locations1[2]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+1),
				ZDate.Today.AddDays(-3), locations1[2]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+2),
				ZDate.Today.AddDays(-2), locations1[2]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+2),
				ZDate.Today.AddDays(-2), locations1[3]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+2),
				ZDate.Today.AddDays(-1), locations1[3]));
			var inventory11_10 = CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+3),
				ZDate.Today.AddDays(-2), locations1[3]);
			inventoryCol.Add(inventory11_10);
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part1, 10m, ZDate.Today.AddDays(+3),
				ZDate.Today.AddDays(-3), locations1[3]));

			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part2, 10m, locations1[4]));
			inventoryCol.Add(CreateWhsReceiveInventoryLine(receive11, part2, 10m, locations1[3]));

			//Receive12
			CreateWhsReceiveInventoryLine(receive12, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations2[3]); // Inventory from different Warehouse shouldn't be included
			CreateWhsReceiveInventoryLine(receive12, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations2[4]);
			CreateWhsReceiveInventoryLine(receive12, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations2[2]);

			CreateWhsReceiveInventoryLine(receive12, part2, 10m, locations2[0]);
			CreateWhsReceiveInventoryLine(receive12, part2, 10m, locations2[2]);
			CreateWhsReceiveInventoryLine(receive12, part2, 10m, locations2[3]);

			//Receive21
			CreateWhsReceiveInventoryLine(receive21, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations1[4]); // Inventory from different Client shouldn't be included
			CreateWhsReceiveInventoryLine(receive21, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations1[3]);
			CreateWhsReceiveInventoryLine(receive21, part1, 10m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-1),
				locations1[2]);

			CreateWhsReceiveInventoryLine(receive21, part3, 1m, locations1[0]);

			receive11.FinaliseDocket();
			receive12.FinaliseDocket();
			receive21.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive11);
			AssertIsFinalisedPrecondition(receive12);
			AssertIsFinalisedPrecondition(receive21);

			var order = Helper.CreateWhsOrder(client1, warehouse1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part1, 2m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory11_2);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory11_10);
			((IBusinessObjectInternals)reservedPickLine1).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;
			((IBusinessObjectInternals)reservedPickLine2).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;

			Factory.Save();
			return inventoryCol;
		}

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units,
			ZDate expiryDate, ZDate packingDate, WhsLocation location)
		{
			var inventory = CreateWhsReceiveInventoryLine(receive, part, units, location);
			Helper.SetInventoryAttributes(inventory, expiryDate, packingDate, "PA1", "PA2", "PA3", "");
			return inventory;
		}

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units,
			WhsLocation location)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			inventory.WI_WL = location.PK;
			return inventory;
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadReportData()
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			#region Sql query

			var sql = @"
							select
								*
							from
								WhsStockPickFaceReplenishmentReport(null, null, null)
							order by
								ProductCode,
								PickFaceLocation,
								PickFromExpiryDate,
								PickFromPackingDate,
								PickFromArrivalDate,
								PickFromLocation
							asc";

			#endregion

			result.Load(sql);

			return result;
		}

		#endregion
	}
}
