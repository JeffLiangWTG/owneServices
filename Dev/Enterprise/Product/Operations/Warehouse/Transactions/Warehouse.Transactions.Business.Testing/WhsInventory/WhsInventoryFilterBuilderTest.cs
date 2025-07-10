using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryFilterBuilderTest : WhsTestCaseWithFactory
	{
		#region TestBuildFilter_TransferFrom_NotAdded

		public void TestBuildFilter_TransferFrom_NotAdded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 18m, dockDoorLocation2, "B", false, false);
			Factory.Save();

			var filterBuilderRegTrue1 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, data.Whs1, null, dockDoorLocation1, "A", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty, InventoryStatus.Codes.Received);
			var filterBuilderRegTrue2 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, data.Whs1, null, dockDoorLocation2, "B", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty, InventoryStatus.Codes.Received);

			CombineAssertions(() =>
			{
				AssertEquals("filterBuilderRegTrue1 WhsInventoryView Count is correct", 0, Factory.Load<WhsInventoryView>(filterBuilderRegTrue1).Length);
				AssertEquals("filterBuilderRegTrue2 WhsInventoryView Count is correct", 1, Factory.Load<WhsInventoryView>(filterBuilderRegTrue2).Length);
				AssertEquals("dockDoorLocation2 TotalUnits is correct", 18m, Factory.Load<WhsInventoryView>(filterBuilderRegTrue2).Single().WI_TotalUnits);
			});
		}

		#endregion

		#region TestBuildFilter_MostCombinations

		[SnailTest]
		public void TestBuildFilter_MostCombinations()
		{
			// this test was copied almost verbatim from the original Calculate test for deleted WhsInventoryQuery class
			var today = ZDate.Today;
			var notificationBuffer = new TestNotificationBuffer();
			var data = new TestDataForInventory(Factory, notificationBuffer);
			data.CreateMultiWarehouseClientProductInventory();

			var adjustment11 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			adjustment11.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateAdjustmentLine(adjustment11, data.Line111, -5m);
			CreateAdjustmentLine(adjustment11, data.Line112, -10m);
			adjustment11.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment11);

			var adjustment12 = Helper.CreateWhsAdjustment(data.Org1, data.Whs2, "AD2", Notify);
			adjustment12.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateAdjustmentLine(adjustment12, data.Line121, -15m);
			CreateAdjustmentLine(adjustment12, data.Line122, -20m);
			adjustment12.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment12);

			var adjustment21 = Helper.CreateWhsAdjustment(data.Org2, data.Whs1, "AD3", Notify);
			adjustment21.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateAdjustmentLine(adjustment21, data.Line211, -25m);
			CreateAdjustmentLine(adjustment21, data.Line212, -30m);
			CreateAdjustmentLine(adjustment21, data.Line213, -35m);
			CreateAdjustmentLine(adjustment21, data.Line214, -40m);
			CreateAdjustmentLine(adjustment21, data.Line215, -45m);
			CreateAdjustmentLine(adjustment21, data.Line216, -50m);
			CreateAdjustmentLine(adjustment21, data.Line217, -55m);
			adjustment21.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment21);

			var adjustment22 = Helper.CreateWhsAdjustment(data.Org2, data.Whs2, "AD4", Notify);
			adjustment22.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateAdjustmentLine(adjustment22, data.Line221, -60m);
			CreateAdjustmentLine(adjustment22, data.Line222, -65m);
			adjustment22.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment22);
			Factory.Save();

			// total of 455 units removed from initial 1400
			var query1 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query1, "Query for all stock", 1045m);

			var query2 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query2, "Query for Org1", 350m);

			var query3 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query3, "Query for Org2", 695m);

			var query4 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query4, "Query for Org1, Part1", 180m);

			var query5 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part2, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query5, "Query for Org1, Part2", 170m);

			var query6 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part1, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query6, "Query for Org2, Part1", 215m);

			var query7 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part2, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query7, "Query for Org2, Part2", 480m);

			var query8 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part2, null, null, null, "", "BEK11-2", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query8, "Query for Org1, Part2, Entry BEK11-2", 90m);

			var query9 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, data.Whs1.FindLocation("A-1"), "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query9, "Query for Location1", 605m);

			var query10 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, data.Whs2.FindLocation("A"), "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query10, "Query for Location2", 340m);

			var query11 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, null, null, data.Whs1.FindLocation("A-1"), "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query11, "Query for Org1, Part1, Location1", 95m);

			var query12 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part2, null, null, data.Whs2.FindLocation("A"), "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query12, "Query for Org2, Part2, Location2", 35m);

			var query13 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "BEK11-1", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query13, "Query for BondedEntryKey BEK11-1", 95m);

			var query14 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "BEK11-2", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query14, "Query for BondedEntryKey BEK11-2", 90m);

			var query15 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "BEK11-", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query15, "Query for ALL EntryKey BEK11", 185m);

			var query16 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", today.AddMonths(1), ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query16, "Query for Expiry Date", 415m);

			var query17 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, today.AddMonths(-1), "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query17, "Query for Packing Date", 420m);

			var query18 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query18, "Query for Attrib1", 325m);

			var query19 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query19, "Query for Attrib2", 330m);

			var query20 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "PA3", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query20, "Query for Attrib3", 335m);

			var query21 = WhsInventoryFilterBuilder.BuildFilter(null, null, data.Whs1, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query21, "Query for Whs1", 705m);

			var query22 = WhsInventoryFilterBuilder.BuildFilter(null, null, data.Whs2, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query22, "Query for Whs2", 340m);

			var query23 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, data.Whs1.Areas[2], null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query23, "Query for Area in Whs1", 605m);

			var query24 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, data.Whs2.Areas[2], null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query24, "Query for Area in Whs2", 340m);

			var query25 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, data.Whs1, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query25, "Query for Org1, Part1, Whs1", 95m);

			var query26 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part2, data.Whs1, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query26, "Query for Org1, Part2, Whs1", 90m);

			var query27 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part1, data.Whs1, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query27, "Query for Org2, Part1, Whs1", 75m);

			var query28 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part2, data.Whs1, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query28, "Query for Org2, Part2, Whs1", 445m);

			var query29 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, data.Whs2, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query29, "Query for Org1, Part1, Whs2", 85m);

			var query30 = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part2, data.Whs2, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query30, "Query for Org1, Part2, Whs2", 80m);

			var query31 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part1, data.Whs2, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query31, "Query for Org2, Part1, Whs2", 140m);

			var query32 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part2, data.Whs2, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query32, "Query for Org2, Part2, Whs2", 35m);

			var query33 = WhsInventoryFilterBuilder.BuildFilter(data.Org2, data.Part2, data.Whs1, null, null, "", "BEK21-2", today.AddMonths(1), today.AddMonths(-1), "PA1", "PA2", "PA3", "", ZDateTimeOffset.Empty);
			AssertQueryResults(query33, "Query for all attributes", 70m);
		}

		static WhsAdjustmentLine CreateAdjustmentLine(WhsAdjustment adjustment, WhsInventoryView inventoryToAdjust, decimal qtyToAdjustOut)
		{
			var adjustmentLine = (WhsAdjustmentLine)adjustment.CreateDocketLineFromInventory(inventoryToAdjust);
			adjustmentLine.WE_TransactionQuantity = qtyToAdjustOut;
			adjustmentLine.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;

			return adjustmentLine;
		}

		#endregion

		#region TestBuildFilter_SerialNumber

		public void TestBuildFilter_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.BrettsBirthday;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Factory.New<WhsReceiveLine>();
			line.WE_WD = receive.PK;
			line.WE_OP = data.Part1.PK;
			line.WE_WL = data.Whs1.DefaultLocation.PK;
			line.WE_TransactionQuantity = 1m;
			line.WE_PalletID = "PLTID1";
			Helper.SetDocketLineAttributes(line, expiryDate, packingDate, "PA1", "PA2", "PA3", "SN", "BEK");

			var query = WhsInventoryFilterBuilder.BuildFilter(data.Org1, data.Part1, data.Whs1, null, data.Whs1.DefaultLocation, line.WE_PalletID, line.WE_BondedEntryKey, line.WE_ExpiryDate, line.WE_PackingDate, line.WE_PartAttrib1, line.WE_PartAttrib2, line.WE_PartAttrib3, line.WE_SerialNumber, ZDateTimeOffset.Empty);
			var inventoryCollection = new[] { line.Inventory[0] };
			AssertContainsExactElementsInAnyOrder(inventoryCollection, Factory.Load<WhsInventoryView>(query));
		}

		public void TestBuildFilter_SerialNumber_Addition()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stockInv = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Stock");
			Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber = "SN1";
			Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber = "SN2";
			Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber = "SN3";
			Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber = "SN4";
			stockInv.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(stockInv);
			Factory.Save();

			var invQueryAll = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(invQueryAll, "Precondition: query for all stock", 4m);

			var invQuerySN1 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN1, "Query for SN1", 1m);

			var invQuerySN2 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN2, "Query for SN2", 1m);

			var invQuerySN3 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN3", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN3, "Query for SN3", 1m);

			var invQuerySN4 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN4", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN4, "Query for SN4", 1m);
		}

		public void TestBuildFilter_SerialNumber_AdjustOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var stockInv = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Stock");
			var stockInv1 = Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation);
			stockInv1.WE_SerialNumber = "SN1";
			var stockInv2 = Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation);
			stockInv2.WE_SerialNumber = "SN2";
			var stockInv3 = Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation);
			stockInv3.WE_SerialNumber = "SN3";
			var stockInv4 = Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation);
			stockInv4.WE_SerialNumber = "SN4";
			var stockInv5 = Helper.CreateWhsAdjustmentLine(stockInv, data.Part1, 1m, data.Whs1.DefaultLocation);
			stockInv5.WE_SerialNumber = "SN5";
			stockInv.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(stockInv);
			Factory.Save();

			var invQueryAll = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "", ZDateTimeOffset.Empty);
			AssertQueryResults(invQueryAll, "Precondition: query for all stock", 5m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			CreateAdjustmentLine(adjustment, stockInv3.Inventory[0], -1m);
			CreateAdjustmentLine(adjustment, stockInv5.Inventory[0], -1m);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			var invQuerySN1 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN1, "Query for SN1", 1m);

			var invQuerySN2 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN2, "Query for SN2", 1m);

			var invQuerySN3 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN3", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN3, "Query for SN3", 0m);

			var invQuerySN4 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN4", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN4, "Query for SN4", 1m);

			var invQuerySN5 = WhsInventoryFilterBuilder.BuildFilter(null, null, null, null, null, "", "", ZDate.Empty, ZDate.Empty, "", "", "", "SN5", ZDateTimeOffset.Empty);
			AssertQueryResults(invQuerySN5, "Query for SN5", 0m);
		}

		#endregion

		#region Implementation

		void AssertQueryResults(ZQuery query, string assertionMessage, decimal expectedAvailableUnits)
		{
			var inventories = Factory.Load<WhsInventoryView>(query);
			AssertEquals(assertionMessage, expectedAvailableUnits, inventories.Sum(i => i.WI_TotalUnits - i.CommittedQuantityIncludingUnfinalisedReceipt));
		}

		#endregion
	}
}
