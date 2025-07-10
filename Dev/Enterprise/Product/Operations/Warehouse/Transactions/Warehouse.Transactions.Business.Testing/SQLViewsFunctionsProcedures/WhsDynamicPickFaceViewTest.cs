using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using WDPViewSql = CargoWise.Database.TestFramework.ObjectModel.WhsDynamicPickFaceView;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDynamicPickFaceViewTest : WhsTestCaseWithFactory
	{
		#region Unique PK

		public void TestView_PK_ByLocationProductClient_ShouldBeUnique()
		{
			var (data, locationA1, locationA2, org1, org2) = SetLocationsPartsOrgsDynamic();

			var resultRows = LoadView();

			AssertExhaustiveMatch(locationA1, data.Part1, org1, resultRows[0]);
			AssertExhaustiveMatch(locationA1, data.Part1, org2, resultRows[1]);
			AssertExhaustiveMatch(locationA1, data.Part2, org1, resultRows[2]);
			AssertExhaustiveMatch(locationA1, data.Part2, org2, resultRows[3]);
			AssertExhaustiveMatch(locationA2, data.Part1, org1, resultRows[4]);
			AssertExhaustiveMatch(locationA2, data.Part1, org2, resultRows[5]);
			AssertExhaustiveMatch(locationA2, data.Part2, org1, resultRows[6]);
			AssertExhaustiveMatch(locationA2, data.Part2, org2, resultRows[7]);

			AssertEquals("Should return 8 rows", 8, resultRows.Count);
			AssertEquals("8 combines should return 8 unique PK", 8, resultRows.Select(x => (ZGuid)x[WhsDynamicPickFaceViewSchema.Constants.PK]).Distinct().Count());
		}

		#endregion

		#region Test all fields

		public void TestView_AllFields()
		{
			var (data, locationA, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));
			data.Part1.OP_Desc = "Product 1";
			Helper.CreateRow(data.Whs1, "B");
			Helper.CreateRow(data.Whs1, "C");
			Factory.Save();

			var locationB = data.Whs1.FindLocation("B");
			var locationC = data.Whs1.FindLocation("C");

			Helper.CreateABCCategory(data.Part1, data.Org1, data.Whs1, "C", DateTimeOffset.Now, DateTimeOffset.Now);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 16, locationA, palletID: "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 999, locationB, palletID: "");

			Factory.Save();

			var transferIncoming = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transferIncoming, data.Part1, 999, locationB, locationA);
			var transferCommitted = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transferCommitted, data.Part1, 4, locationA, locationC);
			transferIncoming.RunPreSaveValidation();
			transferCommitted.RunPreSaveValidation();
			Factory.Save();

			var collection = LoadView();
			AssertEquals("One inventory entered, so 1 records should be found.", 1, collection.Count);

			var matcher = new ExhaustiveMatcher(collection[0]);
			matcher.AddColumnMatcher("WDP_PK", collection[0]["WDP_PK"]);    // Cannot be tested
			matcher.AddColumnMatcher("WDP_WarehouseCode", "1");
			matcher.AddColumnMatcher("WDP_AreaName", "DYNAMIC");
			matcher.AddColumnMatcher("WDP_LocationString", "A");
			matcher.AddColumnMatcher("WDP_LocationType", "DLC");
			matcher.AddColumnMatcher("WDP_WL_Location", locationA.PK);

			matcher.AddColumnMatcher("WDP_ProductCode", "P1");
			matcher.AddColumnMatcher("WDP_ProductDesc", "Product 1");
			matcher.AddColumnMatcher("WDP_OP_Product", data.Part1.PK);

			matcher.AddColumnMatcher("WDP_ClientCode", "111");

			matcher.AddColumnMatcher("WDP_AvailableToPick", 12M);
			matcher.AddColumnMatcher("WDP_Committed", 4M);
			matcher.AddColumnMatcher("WDP_Incoming", 999M);
			matcher.AddColumnMatcher("WDP_TotalQuantity", 16M);

			matcher.AddColumnMatcher("WDP_ABCCategory", "C");

			matcher.AddColumnMatcher("WDP_IsAssigned", true);

			matcher.AssertAll();
		}

		#endregion

		#region Test LocationString UserFriendly

		public void TestView_DPFLocationWithFixedWidth()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.WW_LocationsHaveLeadingZeros = true;
			data.Whs1.WW_LocationColumnsFixedWidth = 2;
			data.Whs1.WW_LocationLevelsFixedWidth = 2;
			data.Whs1.WW_LocationTraysFixedWidth = 1;
			Factory.Save();

			var locationA11 = data.Whs1.FindLocation("A0101");
			Helper.CreateDynamicPF(data.Whs1, locationA11, "DynamicPFArea1");

			Factory.Save();
			AssertEquals("Precondition: User friendly Location String is correct.", "A-01-01", locationA11.WLV_LocationString_UserFriendly);
			AssertEquals("Precondition: Location String is correct.", "A0101", locationA11.WLV_LocationString);

			var dynamicPickFace = WDPViewSql.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("Should return user friendly Location String.", "A-01-01", dynamicPickFace.WDP_LocationString);
		}

		#endregion

		#region Empty locations

		public void TestView_EmptyLocation_ShouldReturnOnlyWhenInDynamicPFArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA11 = data.Whs1.FindLocation("A-1-1");
			var locationA12 = data.Whs1.FindLocation("A-1-2");
			var locationA21 = data.Whs1.FindLocation("A-2-1");

			var dynamicPFArea1 = Helper.CreateDynamicPF(data.Whs1, locationA11, "DynamicPFArea1");
			locationA12.WLV_WLT_LocationType = locationA11.WLV_WLT_LocationType;
			locationA12.WLV_WA_PickingArea = dynamicPFArea1.PK;

			Helper.CreateDynamicPF(data.Whs1, locationA21, "DynamicPFArea2");

			Factory.Save();

			var result = LoadView();
			AssertContainsExactElementsInAnyOrder(
				"Should contain only three: excluding A-2-2",
				new WhsLocation[] {
					locationA11,
					locationA12,
					locationA21,
				}.Select(x => x.ToLocationString()),
				result.Select(x => (ZString)x["WDP_LocationString"]));
		}

		public void TestView_EmptyDPFLocationWithoutAssignedProducts_ShouldReturnNullProductWithZeroQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var location = data.Whs1.FindLocation("A");
			Helper.CreateDynamicPF(data.Whs1, location);
			Factory.Save();

			var resultRows = LoadView();
			AssertEquals("Should return one row", 1, resultRows.Count);
			AssertExhaustiveMatch(location, product: null, client: null, resultRows[0]);
		}

		public void TestView_EmptyDPFLocation_ShouldReturnsZeroQuantityForEachProductClient()
		{
			var (data, locationA1, locationA2, org1, org2) = SetLocationsPartsOrgsDynamic();

			var resultRows = LoadView();

			AssertExhaustiveMatch(locationA1, data.Part1, org1, resultRows[0]);
			AssertExhaustiveMatch(locationA1, data.Part1, org2, resultRows[1]);
			AssertExhaustiveMatch(locationA1, data.Part2, org1, resultRows[2]);
			AssertExhaustiveMatch(locationA1, data.Part2, org2, resultRows[3]);
			AssertExhaustiveMatch(locationA2, data.Part1, org1, resultRows[4]);
			AssertExhaustiveMatch(locationA2, data.Part1, org2, resultRows[5]);
			AssertExhaustiveMatch(locationA2, data.Part2, org1, resultRows[6]);
			AssertExhaustiveMatch(locationA2, data.Part2, org2, resultRows[7]);
		}

		#endregion

		#region Product IsAssigned

		public void TestView_DPFLocationWithStock_ForAssignedProducts_ShouldFlagIsAssigned()
		{
			var (data, _, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 8M);

			Factory.Save();

			var wdp = WDPViewSql.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("Should be IsAssigned = true", expected: true, wdp.WDP_IsAssigned);
		}

		public void TestView_DPFLocationWithStock_ForProductsAssignedToOtherArea_ShouldNotFlagIsAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateDynamicPF(data.Whs1, locationA1, "DynamicPFArea1");
			var dynamicPFArea2 = Helper.CreateDynamicPF(data.Whs1, data.Whs1.FindLocation("A-2"), areaName: "DynamicPFArea2");

			var part1Params = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			part1Params.W3_WA_DynamicPickFaceArea = dynamicPFArea2.PK;

			// Do not use CreateWhsReceiveWithInventory to test because it has precondition checks against location not within the product's designated dynamic area
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 8M, locationA1);
			receive.FinaliseDocket();
			Factory.Save();

			var wdp = WDPViewSql.ShallowLoadFromDB(TestConnection);
			AssertEquals("First location is A-1", expected: "A-1", wdp[0].WDP_LocationString);
			AssertEquals("A-1 should have IsAssigned = false", expected: false, wdp[0].WDP_IsAssigned);

			AssertEquals("Second location is A-2", expected: "A-2", wdp[1].WDP_LocationString);
			AssertEquals("A-2 should be IsAssigned = true", expected: true, wdp[1].WDP_IsAssigned);
		}

		public void TestView_DPFLocationWithStock_ForUnassignedProducts_ShouldNotFlagIsAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			Helper.CreateDynamicPF(data.Whs1, location);

			// Do not use CreateWhsReceiveWithInventory to test because it has precondition checks against non-dynamic products
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 8M, location);
			receive.FinaliseDocket();
			Factory.Save();

			var wdp = WDPViewSql.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("Should be IsAssigned = false", expected: false, wdp.WDP_IsAssigned);
		}

		public void TestView_DPFLocationWithoutStock_ForAssignedProduct_ShouldFlagIsAssigned()
		{
			SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));
			Factory.Save();
			var wdp = WDPViewSql.ShallowLoadFromDB(TestConnection).Single();
			AssertEquals("Should be IsAssigned = true", expected: true, wdp.WDP_IsAssigned);
		}

		#endregion

		#region ABC Category

		public void TestView_AbcCategory_SingleRecord()
		{
			var (data, location, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));

			var abcCategory = CreateABCCategory(data.Whs1, data.Org1, data.Part1, "A", ZDateTimeOffset.Today);
			Factory.Save();

			var result = LoadView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is A.", abcCategory.WJ_Category, result[0]["WDP_ABCCategory"]);
			AssertExhaustiveMatch(location, data.Part1, data.Org1, result[0]);
		}

		public void TestView_AbcCategory_MultipleRecords_ShouldReturnMostRecent()
		{
			var (data, location, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));

			CreateABCCategory(data.Whs1, data.Org1, data.Part1, "A", ZDateTimeOffset.Today.AddDays(-1));
			CreateABCCategory(data.Whs1, data.Org1, data.Part1, "B", ZDateTimeOffset.Today);
			Factory.Save();

			var result = LoadView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is the most recent", "B", result[0]["WDP_ABCCategory"]);
			AssertExhaustiveMatch(location, data.Part1, data.Org1, result[0]);
		}

		public void TestView_AbcCategory_NoCategoryFound()
		{
			var (data, location, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));

			Factory.Save();

			var result = LoadView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is empty.", string.Empty, result[0]["WDP_ABCCategory"]);
			AssertExhaustiveMatch(location, data.Part1, data.Org1, result[0]);
		}

		#endregion

		#region GroupBy Location Product Client tests

		public void TestView_InventoryByLocationProductClient_ShouldReturnRecordForEachCombination()
		{
			var (data, locationA1, locationA2, org1, org2) = SetLocationsPartsOrgsDynamic();

			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "Receive1", data.Part1, 001, locationA1, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "Receive2", data.Part1, 002, locationA1, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "Receive3", data.Part2, 004, locationA1, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "Receive4", data.Part2, 008, locationA1, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "Receive5", data.Part1, 016, locationA2, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "Receive6", data.Part1, 032, locationA2, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "Receive7", data.Part2, 064, locationA2, palletID: "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "Receive8", data.Part2, 128, locationA2, palletID: "");

			Factory.Save();

			var resultRows = LoadView();

			AssertEquals("Should return 8 rows", 8, resultRows.Count);
			AssertExhaustiveMatch(locationA1, data.Part1, org1, availableToPick: 001, 0, 0, totalQuantity: 001, resultRows[0]);
			AssertExhaustiveMatch(locationA1, data.Part1, org2, availableToPick: 002, 0, 0, totalQuantity: 002, resultRows[1]);
			AssertExhaustiveMatch(locationA1, data.Part2, org1, availableToPick: 004, 0, 0, totalQuantity: 004, resultRows[2]);
			AssertExhaustiveMatch(locationA1, data.Part2, org2, availableToPick: 008, 0, 0, totalQuantity: 008, resultRows[3]);
			AssertExhaustiveMatch(locationA2, data.Part1, org1, availableToPick: 016, 0, 0, totalQuantity: 016, resultRows[4]);
			AssertExhaustiveMatch(locationA2, data.Part1, org2, availableToPick: 032, 0, 0, totalQuantity: 032, resultRows[5]);
			AssertExhaustiveMatch(locationA2, data.Part2, org1, availableToPick: 064, 0, 0, totalQuantity: 064, resultRows[6]);
			AssertExhaustiveMatch(locationA2, data.Part2, org2, availableToPick: 128, 0, 0, totalQuantity: 128, resultRows[7]);
		}

		public void TestView_LocationAssignedToBothClientsWithStockFromOnlyOne_CombinationTest()
		{
			var (data, locationA1, locationA2, org1, org2) = SetLocationsPartsOrgsDynamic();
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "Receive1", data.Part1, 8, locationA1, "");
			Factory.Save();

			var resultRows = LoadView();

			AssertEquals("Should return 8 rows", 8, resultRows.Count);
			AssertExhaustiveMatch(locationA1, data.Part1, org1, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[0]);
			AssertExhaustiveMatch(locationA1, data.Part1, org2, availableToPick: 8, 0, 0, totalQuantity: 8, resultRows[1]);
			AssertExhaustiveMatch(locationA1, data.Part2, org1, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[2]);
			AssertExhaustiveMatch(locationA1, data.Part2, org2, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[3]);
			AssertExhaustiveMatch(locationA2, data.Part1, org1, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[4]);
			AssertExhaustiveMatch(locationA2, data.Part1, org2, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[5]);
			AssertExhaustiveMatch(locationA2, data.Part2, org1, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[6]);
			AssertExhaustiveMatch(locationA2, data.Part2, org2, availableToPick: 0, 0, 0, totalQuantity: 0, resultRows[7]);
		}

		#endregion

		#region Quantities aggregation tests

		public void TestView_SameLocationProductClient_FinalizedReceived_ShouldBeTotalQuantity()
		{
			var (data, location, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 8M, location, palletID: "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 16M, location, palletID: "");
			Factory.Save();

			var resultRows = LoadView();
			AssertEquals("Should return one row", 1, resultRows.Count);
			AssertExhaustiveMatch(location, data.Part1, data.Org1, availableToPick: 24, 0, 0, totalQuantity: 24, resultRows[0]);
		}

		public void TestView_SameLocationProductClient_AvailableToPickPlusCommitted_ShouldBeTotalQuantity()
		{
			var (data, location, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 8);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 16);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 4M);
			Helper.CreatePickNew(order);
			Factory.Save();

			var resultRows = LoadView();
			AssertEquals("Should return one row", 1, resultRows.Count);
			AssertExhaustiveMatch(location, data.Part1, data.Org1, availableToPick: 20, committed: 4, incoming: 0, totalQuantity: 24, resultRows[0]);
		}

		public void TestView_SameLocationProductClient_UnfinalizedTransfer_ShouldBeIncoming()
		{
			var (data, locationA, _) = SetLocationAPart1Org1Dynamic(new TestDataSimpleEnvironment(Factory));
			Helper.CreateRow(data.Whs1, "B");
			Factory.Save();
			var locationB = data.Whs1.FindLocation("B");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 50, locationB, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 8m, locationB, locationA);
			transfer1.RunPreSaveValidation();
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer2");
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 16m, locationB, locationA);
			transfer2.RunPreSaveValidation();
			Factory.Save();

			var resultRows = LoadView();
			AssertEquals("Should return one row", 1, resultRows.Count);
			AssertExhaustiveMatch(locationA, data.Part1, data.Org1, 0, 0, incoming: 24, 0, resultRows[0]);
		}

		#endregion

		#region Implementation

		(TestDataSimpleEnvironment data, WhsLocation a1, WhsLocation a2, OrgHeader org1, OrgHeader org2) SetLocationsPartsOrgsDynamic()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org1 = data.Org1;
			org1.OH_Code = "Org1";
			var org2 = Helper.CreateClient("Org2");
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, locationA1);
			locationA2.WLV_WA_PickingArea = dynamicPFArea.PK;
			locationA2.WLV_WLT_LocationType = locationA1.WLV_WLT_LocationType;

			var part1 = data.Part1;
			part1.OP_PartNum = "Part1";
			var part2 = data.Part2;
			part2.OP_PartNum = "Part2";

			var w3Part1Org1 = Helper.CreateProductParamsByWhsAndClient(part1, org1, data.Whs1);
			w3Part1Org1.W3_WA_DynamicPickFaceArea = dynamicPFArea.PK;
			var w3Part2Org2 = Helper.CreateProductParamsByWhsAndClient(part2, org2, data.Whs1);
			w3Part2Org2.W3_WA_DynamicPickFaceArea = dynamicPFArea.PK;
			var w3Part1Org2 = Helper.CreateProductParamsByWhsAndClient(part1, org2, data.Whs1);
			w3Part1Org2.W3_WA_DynamicPickFaceArea = dynamicPFArea.PK;
			var w3Part2Org1 = Helper.CreateProductParamsByWhsAndClient(part2, org1, data.Whs1);
			w3Part2Org1.W3_WA_DynamicPickFaceArea = dynamicPFArea.PK;

			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductClientRelationShip(org2, data.Part2);

			Factory.Save();

			return (data, locationA1, locationA2, org1, org2);
		}

		(TestDataSimpleEnvironment data, WhsLocation locationA, WhsArea dynamicPFArea) SetLocationAPart1Org1Dynamic(TestDataSimpleEnvironment data)
		{
			var location = data.Whs1.FindLocation("A");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, location);
			var part1Params = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			part1Params.W3_WA_DynamicPickFaceArea = dynamicPFArea.PK;
			return (data, location, dynamicPFArea);
		}

		WhsABCCategory CreateABCCategory(WhsWarehouse whs, OrgHeader org, OrgSupplierPart product, string category, ZDateTimeOffset analysisDateTo)
		{
			var whsABCCategory = Factory.NewWithValidTestData<WhsABCCategory>();
			whsABCCategory.WJ_WW_Warehouse = whs.PK;
			whsABCCategory.WJ_OP_Product = product.PK;
			whsABCCategory.WJ_OH_Client = org.PK;
			whsABCCategory.WJ_Category = category;
			whsABCCategory.WJ_AnalysisDateTo = analysisDateTo;
			return whsABCCategory;
		}

		DynamicBusinessObjectCollection LoadView(string orderByColumns = "WDP_LocationString,WDP_ProductCode,WDP_ClientCode")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var queryText = string.Format($"SELECT * FROM dbo.WhsDynamicPickFaceView ORDER BY {orderByColumns} ASC");
			result.Load(queryText);
			return result;
		}

		void AssertExhaustiveMatch(WhsLocation location, OrgSupplierPart product, OrgHeader client, DynamicBusinessObject testedObject)
		{
			AssertExhaustiveMatch(location, product, client, 0, 0, 0, 0, testedObject);
		}

		void AssertExhaustiveMatch(
			WhsLocation location,
			OrgSupplierPart product,
			OrgHeader client,
			ZDecimal availableToPick,
			ZDecimal committed,
			ZDecimal incoming,
			ZDecimal totalQuantity,
			DynamicBusinessObject testedObject)
		{
			var matcher = new ExhaustiveMatcher(testedObject);
			matcher.AddColumnMatcher("WDP_PK", testedObject["WDP_PK"]);
			matcher.AddColumnMatcher("WDP_WarehouseCode", location.Warehouse.WW_WarehouseCode);
			matcher.AddColumnMatcher("WDP_AreaName", location.PickingArea.WA_Name);
			matcher.AddColumnMatcher("WDP_LocationString", location.WLV_LocationString_UserFriendly);
			matcher.AddColumnMatcher("WDP_WL_Location", location.PK);
			matcher.AddColumnMatcher("WDP_LocationType", location.WLV_LocationTypeCode);

			matcher.AddColumnMatcher("WDP_ProductCode", product?.OP_PartNum ?? string.Empty);
			matcher.AddColumnMatcher("WDP_ProductDesc", product?.OP_Desc ?? string.Empty);
			matcher.AddColumnMatcher("WDP_OP_Product", product?.PK ?? ZGuid.Empty);

			matcher.AddColumnMatcher("WDP_ClientCode", client?.OH_Code ?? string.Empty);

			matcher.AddColumnMatcher("WDP_AvailableToPick", availableToPick);
			matcher.AddColumnMatcher("WDP_Committed", committed);
			matcher.AddColumnMatcher("WDP_Incoming", incoming);
			matcher.AddColumnMatcher("WDP_TotalQuantity", totalQuantity);

			matcher.AddColumnMatcher("WDP_ABCCategory", ABCCategory(location, product, client));

			matcher.AddColumnMatcher("WDP_IsAssigned", IsAssigned(location, product, client));

			matcher.AssertAll();
		}

		bool IsAssigned(WhsLocation location, OrgSupplierPart product, OrgHeader client)
		{
			var result = false;
			if (product != null && client != null)
			{
				var w3Query = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_WW, location.WLV_WW_Whs);
				w3Query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OP, product.PK);
				w3Query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, client.PK);
				var w3 = Factory.LoadTop1<WhsProductParamsByWhsAndClient>(w3Query);

				result = location.WLV_PickingAreaType == AreaTypes.Codes.DynamicPickFace
					&& w3 != null && location.WLV_WA_PickingArea == w3.W3_WA_DynamicPickFaceArea;
			}
			return result;
		}

		string ABCCategory(WhsLocation location, OrgSupplierPart product, OrgHeader client)
		{
			var result = string.Empty;
			if (product != null && client != null)
			{
				var wjQuery = new ZQuery(WhsABCCategorySchema.WJ_WW_Warehouse, location.WLV_WW_Whs);
				wjQuery.AddToFilter(WhsABCCategorySchema.WJ_OP_Product, product.PK);
				wjQuery.AddToFilter(WhsABCCategorySchema.WJ_OH_Client, client.PK);
				wjQuery.OrderBy = $"{WhsABCCategorySchema.WJ_AnalysisDateTo.Name} DESC";
				var wj = Factory.LoadTop1<WhsABCCategory>(wjQuery);
				result = wj?.WJ_Category ?? string.Empty;
			}

			return result;
		}

		#endregion
	}
}
