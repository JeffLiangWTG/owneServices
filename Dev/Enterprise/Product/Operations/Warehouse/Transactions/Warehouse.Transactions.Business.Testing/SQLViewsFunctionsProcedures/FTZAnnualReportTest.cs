using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class FTZAnnualReportTest : WhsTestCaseWithFactory
	{
		#region ColumnNames

		static class ColumnNames
		{
			public const string TotalItemsReceived = "TotalItemsReceived";
			public const string TotalnumberOfCountriesOfOrigin = "TotalNumberOfCountriesOfOrigin";
			public const string BeginningValueForDomesticMerchandise = "BeginningValueForDomesticMerchandise";
			public const string EndingValueForDomesticMerchandise = "EndingValueForDomesticMerchandise";
			public const string BeginningValueForForeignMerchandise = "BeginningValueForForeignMerchandise";
			public const string EndingValueForForeignMerchandise = "EndingValueForForeignMerchandise";
			public const string DomesticMerchandiseReceived = "DomesticMerchandiseReceived";

			public const string DomesticMerchandiseReceivedFromAnotherUSFTZs =
				"DomesticMerchandiseReceivedFromAnotherUSFTZ";

			public const string ForeignMerchandiseReceived = "ForeignMerchandiseReceived";

			public const string ForeignMerchandiseReceivedFromAnotherUSFTZs =
				"ForeignMerchandiseReceivedFromAnotherUSFTZ";

			public const string ReceivedPrevilegedMerchandiseFromAnotherUSFTZ =
				"ReceivedPrevilegedMerchandiseFromAnotherUSFTZ";

			public const string ReceivedPrevilegedForeignMerchandise = "ReceivedPrevilegedForeignMerchandise";

			public const string ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ =
				"ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ";

			public const string ReceivedNonPrevilegedMerchandise = "ReceivedNonPrevilegedMerchandise";
			public const string OrderedVFDForDomesticMarket = "OrderedVFDForDomesticMarket";
			public const string OrderedVFDForInternationalMarket = "OrderedVFDForInternationalMarket";
			public const string OrderedVFDForOtherUSFTZs = "OrderedVFDForOtherUSFTZs";
			public const string Category = "Category";
			public const string Value = "Value";
			public const string CountryOfOrigin = "CountryOfOrigin";
		}

		#endregion

		#region TestFTZReportColumns

		public void TestFTZReportColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("W2");
			SetupFTZWarehouse(data.Whs1);
			SetupFTZWarehouse(whs2);

			var productA = data.Part1;
			var productB = data.Part2;

			// Finalised Receive in Warehouse W1
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", productA, 100, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 01, 01)), 100,
				"A", "AU", ZoneStatusList.Codes.Domestic, true);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R2", productB, 50, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 01, 01)),
				50, "B", "NZ", ZoneStatusList.Codes.Domestic, false);

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", productA, 10, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 02, 10)),
				200, "C", "LK", ZoneStatusList.Codes.PrivilegedForeign, false);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", productB, 5, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 05, 10)),
				100, "D", "IN", ZoneStatusList.Codes.NonPrivilegedForeign, true);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", productB, 5, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 10)),
				100, "E", "LK", ZoneStatusList.Codes.ZoneRestricted, false);

			// Unfinalised Receive in Warehouse W1
			var unfinalisedReceive = CreateCustomReceive(data.Org1, whs1, "R7");
			CreateCustomsReceiveLine(unfinalisedReceive, productA, 2, 2, "A", "AU", ZoneStatusList.Codes.Domestic,
				whs1.DefaultLocation, false, "");
			AssertEquals("Precondition - Receive must not be finalised.", false, unfinalisedReceive.IsFinalised);
			Factory.Save();

			// Finalised Receive In Warehouse W2
			CreateCustomsReceiveWithInventory(whs2, data.Org1, "RW21", productA, 2, new ZDateTimeOffset(2015, 01, 01), 10, "A",
				"CH", ZoneStatusList.Codes.Domestic, true);

			// Finalised order in Warehouse W1
			var orderInWhs1 = CreateCustomOrder(data.Org1, whs1, "O1");
			CreateCustomsOrderLine(orderInWhs1, productA, 3, 100m, 100m, "A",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(orderInWhs1, productB, 1, 50m, 50m, "D",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(orderInWhs1, productA, 1, 10m, 10m, "C",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);
			Factory.Save();
			CreatePickAndFinalise(orderInWhs1, new ZDate(2015, 12, 31));

			// Unfinalised Order in Warehouse W1
			var unfinalisedOrder = CreateCustomOrder(data.Org1, whs1, "O2");
			CreateCustomsOrderLine(unfinalisedOrder, productA, 2, 100m, 100m, "A",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(unfinalisedOrder, productB, 1, 50m, 50m, "D",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(unfinalisedOrder, productA, 1, 10m, 10m, "C",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);
			Factory.Save();

			// Finalised order in Warehouse W2
			var orderInWhs2 = CreateCustomOrder(data.Org1, whs2, "O3");
			CreateCustomsOrderLine(orderInWhs2, productA, 1, 100m, 100m, "A",
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			Factory.Save();
			CreatePickAndFinalise(orderInWhs2, new ZDate(2015, 12, 31));

			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalItemsReceived, 2);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalnumberOfCountriesOfOrigin, 4);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.BeginningValueForDomesticMerchandise, 150m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.BeginningValueForForeignMerchandise, 0m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.EndingValueForDomesticMerchandise, 147m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.EndingValueForForeignMerchandise, 360m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.DomesticMerchandiseReceived, 50m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.DomesticMerchandiseReceivedFromAnotherUSFTZs, 100m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ForeignMerchandiseReceived, 300m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ForeignMerchandiseReceivedFromAnotherUSFTZs, 100m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ReceivedNonPrevilegedMerchandise, 0m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ, 100m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ReceivedPrevilegedForeignMerchandise, 200m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.ReceivedPrevilegedMerchandiseFromAnotherUSFTZ, 0m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.OrderedVFDForDomesticMarket, 3m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.OrderedVFDForInternationalMarket, 20m);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.OrderedVFDForOtherUSFTZs, 20m);
		}

		#endregion

		#region TestFTZReportColumns_RoundedTo2DecimalPlaces

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_DomesticMerchandiseReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromAnotherFTZWhs: false);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("DomesticMerchandiseReceived should be correct.", 6.67m,
				results[0][ColumnNames.DomesticMerchandiseReceived]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_DomesticMerchandiseReceivedFromAnotherUSFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromAnotherFTZWhs: true);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("DomesticMerchandiseReceivedFromAnotherUSFTZ should be correct.", 6.67m,
				results[0][ColumnNames.DomesticMerchandiseReceivedFromAnotherUSFTZs]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_BeginningAndEnding_ValueForDomesticMerchandise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromAnotherFTZWhs: false);

			var resultsSameYear =
				Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("BeginningValueForDomesticMerchandise for year should be correct.", 0m,
				resultsSameYear[0][ColumnNames.BeginningValueForDomesticMerchandise]);
			AssertEquals("EndingValueForDomesticMerchandise for year should be correct.", 6.67m,
				resultsSameYear[0][ColumnNames.EndingValueForDomesticMerchandise]);

			var resultsYearPlus1 =
				Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year + 1, 01, 01), new ZDate(year + 1, 12, 31));
			AssertEquals("BeginningValueForDomesticMerchandise for year+1 should be correct.", 6.67m,
				resultsYearPlus1[0][ColumnNames.BeginningValueForDomesticMerchandise]);
			AssertEquals("EndingValueForDomesticMerchandise for year+1 should be correct.", 6.67m,
				resultsYearPlus1[0][ColumnNames.EndingValueForDomesticMerchandise]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ReceivedPrevilegedForeignMerchandise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromAnotherFTZWhs: false);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ReceivedPrevilegedForeignMerchandise should be correct.", 6.67m,
				results[0][ColumnNames.ReceivedPrevilegedForeignMerchandise]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ReceivedPrevilegedMerchandiseFromAnotherUSFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromAnotherFTZWhs: true);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ReceivedPrevilegedMerchandiseFromAnotherUSFTZ should be correct.", 6.67m,
				results[0][ColumnNames.ReceivedPrevilegedMerchandiseFromAnotherUSFTZ]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ReceivedNonPrevilegedMerchandise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromAnotherFTZWhs: false);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ReceivedNonPrevilegedMerchandise should be correct.", 6.67m,
				results[0][ColumnNames.ReceivedNonPrevilegedMerchandise]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromAnotherFTZWhs: true);

			var results2015 =
				Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ should be correct.", 6.67m,
				results2015[0][ColumnNames.ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ForeignMerchandiseReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year, zoneStatus: "",
				isFromAnotherFTZWhs: false);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ForeignMerchandiseReceived should be correct.", 6.67m,
				results[0][ColumnNames.ForeignMerchandiseReceived]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_ForeignMerchandiseReceivedFromAnotherUSFTZs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year, zoneStatus: "",
				isFromAnotherFTZWhs: true);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("ForeignMerchandiseReceivedFromAnotherUSFTZs should be correct.", 6.67m,
				results[0][ColumnNames.ForeignMerchandiseReceivedFromAnotherUSFTZs]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_BeginningAndEnding_ValueForForeignMerchandise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryData(data.Whs1, data.Org1, data.Part1, finalisedYear: year, zoneStatus: "",
				isFromAnotherFTZWhs: false);

			var resultsSameYear =
				Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("BeginningValueForForeignMerchandise for year should be correct.", 0m,
				resultsSameYear[0][ColumnNames.BeginningValueForForeignMerchandise]);
			AssertEquals("EndingValueForForeignMerchandise for year should be correct.", 6.67m,
				resultsSameYear[0][ColumnNames.EndingValueForForeignMerchandise]);

			var resultsYearPlus1 =
				Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year + 1, 01, 01), new ZDate(year + 1, 12, 31));
			AssertEquals("BeginningValueForForeignMerchandise for year+1 should be correct.", 6.67m,
				resultsYearPlus1[0][ColumnNames.BeginningValueForForeignMerchandise]);
			AssertEquals("EndingValueForForeignMerchandise for year+1 should be correct.", 6.67m,
				resultsYearPlus1[0][ColumnNames.EndingValueForForeignMerchandise]);
		}

		void SetupInventoryData(WhsWarehouse whs, OrgHeader org, OrgSupplierPart part, int finalisedYear,
			ZString zoneStatus, bool isFromAnotherFTZWhs)
		{
			var adjustmentIn = Helper.CreateWhsAdjustment(org, whs, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, part, 6, whs.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 10;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 6;
			adjustmentInLine.CustomsData.WB_ZoneStatus = zoneStatus;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = isFromAnotherFTZWhs;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = whs.GetWarehouseBranchDateTimeOffset(new ZDateTime(finalisedYear, 02, 02));
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(org, whs, "A2");
			adjustmentOut.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentOutLine =
				Helper.CreateWhsAdjustmentLine(adjustmentOut, part, -2, whs.DefaultLocationInBondedArea);
			adjustmentOutLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentOutLine.CustomsData.WB_EntryKey = "A";
			adjustmentOutLine.CustomsData.WB_ValueForDuty = 10;
			adjustmentOutLine.CustomsData.WB_BondedWhsQty = 6;
			adjustmentOutLine.CustomsData.WB_ZoneStatus = zoneStatus;
			adjustmentOutLine.CustomsData.WB_IsFromAnotherFTZWhs = isFromAnotherFTZWhs;
			adjustmentOutLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentOutLine.WE_BondedEntryKey = "A";
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = whs.GetWarehouseBranchDateTimeOffset(new ZDateTime(finalisedYear, 02, 02));
			AssertIsFinalisedPrecondition(adjustmentOut);
			Factory.Save();
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_OrderedVFDForDomesticMarket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryAndOrderData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("OrderedVFDForDomesticMarket should be correct.", 6.67m,
				results[0][ColumnNames.OrderedVFDForDomesticMarket]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_OrderedVFDForInternationalMarket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryAndOrderData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("OrderedVFDForInternationalMarket should be correct.", 6.67m,
				results[0][ColumnNames.OrderedVFDForInternationalMarket]);
		}

		public void TestFTZReportColumns_RoundedTo2DecimalPlaces_OrderedVFDForOtherUSFTZs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			SetupInventoryAndOrderData(data.Whs1, data.Org1, data.Part1, finalisedYear: year,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("OrderedVFDForOtherUSFTZs should be correct.", 6.67m,
				results[0][ColumnNames.OrderedVFDForOtherUSFTZs]);
		}

		void SetupInventoryAndOrderData(WhsWarehouse whs, OrgHeader org, OrgSupplierPart part, int finalisedYear,
			ZString outwardType)
		{
			var receive = Helper.CreateWhsReceive(org, whs, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine =
				Helper.CreateWhsReceiveInventoryLine(receive, part, 6m, receive.Warehouse.DefaultLocationInBondedArea);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = "A";
			receiveLine.CustomsData.WB_ValueForDuty = 10;
			receiveLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			receiveLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			receiveLine.CustomsData.WB_OutwardType = "";
			receiveLine.CustomsData.WB_BondedWhsQty = 6;
			receiveLine.InDocketLine.WE_BondedEntryKey = "A";
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = receive.Warehouse.GetWarehouseBranchDateTimeOffset(new ZDateTime(finalisedYear, 01, 01));
			Factory.Save();

			var order = Helper.CreateWhsOrder(org, whs, "O1");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;

			var orderLine = Helper.CreateWhsOrderLine(order, part, 4m);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = "A";
			orderLine.CustomsData.WB_ValueForDuty = 10;
			orderLine.CustomsData.WB_OutwardType = outwardType;
			orderLine.CustomsData.WB_BondedWhsQty = 6;
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			orderLine.WE_BondedEntryKey = "A";

			Factory.Save();
			CreatePickAndFinalise(order, new ZDate(finalisedYear, 01, 01));
		}

		#endregion

		#region TestTotalItemsReceived

		public void TestTotalItemsReceived()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var org2 = Helper.CreateClient("O2");
			var whs1 = data.Whs1;

			var productA = data.Part1;
			var productAOrg2 =
				Helper.CreateProduct(data.Part1.OP_PartNum,
					org2); // received with same product code for different client
			var productB = data.Part2;
			var productC = Helper.CreateProduct("C", data.Org1);
			var productD = Helper.CreateProduct("D", data.Org1);
			var productE = Helper.CreateProduct("E", data.Org1); // didn't receive in W1

			// In Warehouse W1
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", productA, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 01, 01)), 1);
			CreateCustomsReceiveWithInventory(whs1, org2, "R2", productAOrg2, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 01, 01)), 1);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", productB, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 06, 10)), 1);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", productB, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01)), 1);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", productC, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31)), 1);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R6", productD, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2017, 01, 30)), 1);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R7", productD, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2017, 01, 31)), 1);

			AssertFTZAnnualReportValue(whs1, 2014, ColumnNames.TotalItemsReceived, 0);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalItemsReceived, 4); // A, B, C, E
			AssertFTZAnnualReportValue(whs1, 2016, ColumnNames.TotalItemsReceived, 1); // B
			AssertFTZAnnualReportValue(whs1, 2017, ColumnNames.TotalItemsReceived, 1); // D
		}

		public void TestTotalItemsReceivedViaAdjustmentIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			SetupFTZWarehouse(whs1);
			whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var whs2 = Helper.CreateWarehouse("W2");
			SetupFTZWarehouse(whs2);
			whs2.DefaultLocation.WLV_WA_PickingArea = whs2.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var productA = data.Part1;
			var productB = data.Part2;
			var productC = Helper.CreateProduct("C", data.Org1);
			var productD = Helper.CreateProduct("D", data.Org1);
			var productE = Helper.CreateProduct("E", data.Org1); // didn't receive in W1

			// In Warehouse W1
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", productA, new ZDate(2015, 01, 01), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", productB, new ZDate(2015, 06, 10), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", productB, new ZDate(2016, 01, 01), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A4", productC, new ZDate(2015, 12, 31), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A5", productD, new ZDate(2017, 01, 31), 1, 1);

			// In Warehouse W2
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AW21", productE, new ZDate(2015, 01, 01), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AW22", productB, new ZDate(2015, 06, 10), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AW23", productB, new ZDate(2016, 01, 01), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AW24", productC, new ZDate(2015, 12, 31), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AW25", productD, new ZDate(2017, 01, 31), 1, 1);

			AssertFTZAnnualReportValue(whs1, 2014, ColumnNames.TotalItemsReceived, 0);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalItemsReceived, 3);
			AssertFTZAnnualReportValue(whs1, 2016, ColumnNames.TotalItemsReceived, 1);
			AssertFTZAnnualReportValue(whs1, 2017, ColumnNames.TotalItemsReceived, 1);
		}

		public void TestTotalItemsReceivedViaAdjustmentInAndOut_IgnoreAdjustmentOuts()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var productA = data.Part1;
			var productB = data.Part2;

			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", productA, new ZDate(2015, 01, 01), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", productB, new ZDate(2015, 06, 10), 1, 1);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", productB, new ZDate(2015, 06, 30), -1,
				1); // should be ignored

			var unfinalisedAdjustment = Helper.CreateWhsAdjustment(data.Org1, whs1, "A4");
			unfinalisedAdjustment.WD_DocketSubType = OrderType.Codes.Customs;
			CreateCustomsAdjustmentLine(unfinalisedAdjustment, productA, 50, 50, "", whs1.DefaultLocation);
			AssertEquals("Precondition - Adjustment must not be finalised.", false, unfinalisedAdjustment.IsFinalised);
			Factory.Save();

			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalItemsReceived, 2);
		}

		#endregion

		#region TestTotalNumberOfCountriesOfOrigin

		public void TestTotalNumberOfCountriesOfOrigin()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			// In Warehouse W1
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 01, 01)), 1,
				countryOfOrigin: "CN");
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R2", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 06, 10)), 1,
				countryOfOrigin: "AU");
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31)), 1,
				countryOfOrigin: "CN");
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01)), 1,
				countryOfOrigin: "LK");
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2017, 01, 31)), 1,
				countryOfOrigin: "AU");
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R6", data.Part1, 1, whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2017, 02, 20)), 1,
				countryOfOrigin: ""); // we need to consider empty country of origin as a separate group

			AssertFTZAnnualReportValue(whs1, 2014, ColumnNames.TotalnumberOfCountriesOfOrigin, 0);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalnumberOfCountriesOfOrigin, 2);
			AssertFTZAnnualReportValue(whs1, 2016, ColumnNames.TotalnumberOfCountriesOfOrigin, 1);
			AssertFTZAnnualReportValue(whs1, 2017, ColumnNames.TotalnumberOfCountriesOfOrigin, 2);
		}

		public void TestTotalNumberOfCountriesOfOrigin_AdjustmentsIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			SetupFTZWarehouse(whs1);
			whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var whs2 = Helper.CreateWarehouse("W2");
			SetupFTZWarehouse(whs2);
			whs2.DefaultLocation.WLV_WA_PickingArea = whs2.Areas.Single(a => a.WA_AreaType == "BON").PK;

			// In Warehouse W1
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", data.Part1, new ZDate(2015, 01, 01), 1, 1, "A",
				"CN");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", data.Part1, new ZDate(2015, 06, 10), 1, 1, "B",
				"AU");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", data.Part1, new ZDate(2015, 12, 31), 1, 1, "C",
				"CN");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A4", data.Part1, new ZDate(2016, 01, 01), 1, 1, "D",
				"LK");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A5", data.Part1, new ZDate(2017, 01, 31), 1, 1, "E",
				"AU");

			// In Warehouse W2
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AWHS21", data.Part1, new ZDate(2015, 01, 01), 1, 1,
				"A", "NZ");
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AWHS22", data.Part1, new ZDate(2015, 01, 31), 1, 1,
				"B", "TH");
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AWHS23", data.Part1, new ZDate(2016, 06, 10), 1, 1,
				"C", "LK");
			CreateCustomsAdjustmentsWithInventory(whs2, data.Org1, "AWHS24", data.Part1, new ZDate(2017, 01, 31), 1, 1,
				"D", "AU");

			AssertFTZAnnualReportValue(whs1, 2014, ColumnNames.TotalnumberOfCountriesOfOrigin, 0);
			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalnumberOfCountriesOfOrigin, 2);
			AssertFTZAnnualReportValue(whs1, 2016, ColumnNames.TotalnumberOfCountriesOfOrigin, 1);
			AssertFTZAnnualReportValue(whs1, 2017, ColumnNames.TotalnumberOfCountriesOfOrigin, 1);
		}

		public void TestTotalNumberOfCountriesOfOrigin_AdjustmentsInAndOut()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			// In Warehouse W1
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", data.Part1, new ZDate(2015, 01, 01), 1, 1, "A",
				"CN");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", data.Part1, new ZDate(2015, 06, 10), 1, 1, "B",
				"AU");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", data.Part1, new ZDate(2015, 12, 31), 1, 1, "C",
				"CN");
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A4", data.Part1, new ZDate(2015, 12, 31), -1, 1,
				"B", "AU");

			AssertFTZAnnualReportValue(whs1, 2015, ColumnNames.TotalnumberOfCountriesOfOrigin, 2);
		}

		#endregion

		#region TestMerchandiseInBeginningAndEndOfTheYears

		#region TestMerchandise_FinalisedAllInPast

		public void TestMerchandise_FinalisedAllInPast()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var part1InZoneD = data.Part1;
			var part2InZoneD = data.Part2;
			var partInZoneP = Helper.CreateProduct(data.Org1, "C");
			var partInZoneN = Helper.CreateProduct(data.Org1, "D");
			var partInZoneZ = Helper.CreateProduct(data.Org1, "E");

			// In Warehouse W1
			var receiveInWhs1 = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receiveInWhs1, part1InZoneD, 1, 1, zoneStatus: ZoneStatusList.Codes.Domestic);
			CreateCustomsReceiveLine(receiveInWhs1, partInZoneP, 1, 2, zoneStatus: ZoneStatusList.Codes.ZoneRestricted);
			CreateCustomsReceiveLine(receiveInWhs1, part2InZoneD, 1, 3, zoneStatus: ZoneStatusList.Codes.Domestic);
			CreateCustomsReceiveLine(receiveInWhs1, partInZoneN, 1, 4,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign);
			CreateCustomsReceiveLine(receiveInWhs1, partInZoneZ, 1, 5, zoneStatus: ZoneStatusList.Codes.ZoneRestricted);
			FinaliseDocketAndSave(receiveInWhs1, new ZDate(2015, 12, 31));

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 4, 4, 11, 11);
		}

		#endregion

		#region TestMerchandise_FinalisedAllInPast_OrderOutAtTheBeginningOfTheYear

		public void TestMerchandise_FinalisedAllInPast_OrderOutAtTheBeginningOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 0, 0, 0);
		}

		public void TestMerchandise_FinalisedAllInPast_OrderOutAtTheBeginningOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 5, 5, 55, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAllInPast_OrderOutInTheMiddleOfTheYear

		public void TestMerchandise_FinalisedAllInPast_OrderOutInTheMiddleOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 0, 90, 0);
		}

		public void TestMerchandise_FinalisedAllinPast_OrderOutInTheMiddleOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 5, 90, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAllinPast_OrderOutAtTheEndOfTheYear

		public void TestMerchandise_FinalisedAllinPast_OrderOutAtTheEndOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 0, 90, 0);
		}

		public void TestMerchandise_FinalisedAllinPast_OrderOutAtTheEndOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 5, 90, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAllinPast_OrderOutInTheNextYear

		public void TestMerchandise_FinalisedAllinPast_OrderOutInTheNextYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		TestDataSimpleEnvironment GetDataSimpleEnviornmentWithFTZWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			return data;
		}

		public void TestMerchandise_FinalisedAllinPast_OrderOutInTheNextYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2015, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheBeginningOfTheYear

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear()
		{
			AssertReceivingMerchandise(
				receiveFinalisedDate: new ZDate(2016, 01, 01),
				domBeginVFD: 10,
				domEndVFD: 10,
				fgnBeginVFD: 90,
				fgnEndVFD: 90);
		}

		void AssertReceivingMerchandise(ZDate receiveFinalisedDate, int domBeginVFD, int domEndVFD, int fgnBeginVFD,
			int fgnEndVFD)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var partA = data.Part1;

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, zoneStatus: ZoneStatusList.Codes.Domestic);
			CreateCustomsReceiveLine(receive, partA, 4, 20, zoneStatus: ZoneStatusList.Codes.ZoneRestricted);
			CreateCustomsReceiveLine(receive, partA, 6, 30, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign);
			CreateCustomsReceiveLine(receive, partA, 8, 40, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(receiveFinalisedDate);
			Factory.Save();

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, domBeginVFD, domEndVFD, fgnBeginVFD, fgnEndVFD);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheBeginningOfTheYear

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheBeginningOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 0, 0, 0);
		}

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheBeginningOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 5, 5, 55, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheMiddleOfTheYear

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheMiddleOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 0, 90, 0);
		}

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheMiddleOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 5, 90, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheEndOfTheYear

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheEndOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 0, 90, 0);
		}

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutAtTheEndOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 5, 90, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheNextYear

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheNextYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		public void TestMerchandise_FinalisedAtTheBeginningOfTheYear_OrderOutInTheNextYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 1, 1));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheMiddleOfTheYear

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear()
		{
			AssertReceivingMerchandise(
				receiveFinalisedDate: new ZDate(2016, 06, 30),
				domBeginVFD: 0,
				domEndVFD: 10,
				fgnBeginVFD: 0,
				fgnEndVFD: 90);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheMiddleOfTheYear

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheMiddleOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 07, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 0, 0, 0);
		}

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheMiddleOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 07, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 5, 0, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutAtTheEndOfTheYear

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutAtTheEndOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 0, 0, 0);
		}

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutAtTheEndOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 5, 0, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheNextYear

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheNextYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 10, 0, 90);
		}

		public void TestMerchandise_FinalisedAtTheMiddleOfTheYear_OrderOutInTheNextYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 06, 30));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 10, 0, 90);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheEndOfTheYear

		public void TestMerchandise_FinalisedAtTheEndOfTheYear()
		{
			AssertReceivingMerchandise(
				receiveFinalisedDate: new ZDate(2016, 12, 31),
				domBeginVFD: 0,
				domEndVFD: 10,
				fgnBeginVFD: 0,
				fgnEndVFD: 90);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutAtTheEndOfTheYear

		public void TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutAtTheEndOfTheYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 0, 0, 0);
		}

		public void TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutAtTheEndOfTheYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 5, 0, 55);
		}

		#endregion

		#region TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutInTheNextYear

		public void TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutInTheNextYear_NoApportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			CreateCustomsOrder_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 10, 0, 90);
		}

		public void TestMerchandise_FinalisedAtTheEndOfTheYear_OrderOutInTheNextYear_Apportionment()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			CreateCustomsReceive_10Domestic_90Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2016, 12, 31));
			CreateCustomsOrder_5Domestic_35Foreign(data.Whs1, data.Org1, data.Part1, new ZDate(2017, 01, 01));
			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 0, 10, 0, 90);
		}

		#endregion

		#region TestMerchandise_FinalisedNextYear

		public void TestMerchandise_FinalisedNextYear()
		{
			AssertReceivingMerchandise(
				receiveFinalisedDate: new ZDate(2017, 01, 01),
				domBeginVFD: 0,
				domEndVFD: 0,
				fgnBeginVFD: 0,
				fgnEndVFD: 0);
		}

		#endregion

		#region TestMerchandise_Transfers

		public void TestMerchandise_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			SetupFTZWarehouse(data.Whs1);
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;
			var bondedArea = Helper.CreateArea(whs1, "BONDED", Environment.CodeLists.AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			location2.WLV_WA_PickingArea = bondedArea.PK;
			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_WA_PutawayArea = bondedArea.PK;
			location3.WLV_WA_PickingArea = bondedArea.PK;
			var location4 = data.Whs1.FindLocation("A-2-2");
			location4.WLV_WA_PutawayArea = bondedArea.PK;
			location4.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, zoneStatus: ZoneStatusList.Codes.Domestic,
				location: location1);
			CreateCustomsReceiveLine(receive, partB, 4, 20, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				location: location2);
			CreateCustomsReceiveLine(receive, partB, 6, 30, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				location: location3);
			CreateCustomsReceiveLine(receive, partB, 8, 40, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				location: location3);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			CreateCustomsTransferLine(transfer, partA, 1, 5, ZoneStatusList.Codes.Domestic, location1, location4);
			CreateCustomsTransferLine(transfer, partB, 1, 5, ZoneStatusList.Codes.ZoneRestricted, location2, location4);
			CreateCustomsTransferLine(transfer, partB, 2, 10, ZoneStatusList.Codes.PrivilegedForeign, location3,
				location4);
			transfer.FinaliseDocketWithoutUserConfirmation();
			transfer.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 02));
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		public void TestMerchandise_ReleaseTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			SetupFTZWarehouse(data.Whs1);
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;
			var bondedArea = Helper.CreateArea(whs1, "BONDED", Environment.CodeLists.AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			location2.WLV_WA_PickingArea = bondedArea.PK;
			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_WA_PutawayArea = bondedArea.PK;
			location3.WLV_WA_PickingArea = bondedArea.PK;
			var location4 = data.Whs1.FindLocation("A-2-2");
			location4.WLV_WA_PutawayArea = bondedArea.PK;
			location4.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, zoneStatus: ZoneStatusList.Codes.Domestic,
				location: location1);
			CreateCustomsReceiveLine(receive, partB, 4, 20, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				location: location2);
			CreateCustomsReceiveLine(receive, partB, 6, 30, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				location: location3);
			CreateCustomsReceiveLine(receive, partB, 8, 40, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				location: location3);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, whs1, "O1");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLineForDomestic = CreateCustomsOrderLine(order, partA, 1, 10m, 2m);
			var orderLineForForeign = CreateCustomsOrderLine(order, partB, 1, 30m, 6m);
			var orderLine = CreateCustomsOrderLine(order, partA, 1, 10m, 2m);
			Factory.Save();

			var permitService = CreatePermitService(order);
			using (ObjectFactory.Substitute(permitService.Object))
			{
				Helper.CreatePickNew(order);
			}

			var pickLineForDomestic = orderLineForDomestic.PickLines.Single();
			var pickLineForForeign = orderLineForForeign.PickLines.Single();
			var transferLineForDomestic = Helper.PickAndMakeInTransitTransfer(pickLineForDomestic, ZDateTimeOffset.Now);
			var transferLineForForeign = Helper.PickAndMakeInTransitTransfer(pickLineForForeign, ZDateTimeOffset.Now);
			transferLineForDomestic.FinaliseDocketLine();
			transferLineForForeign.FinaliseDocketLine();
			Assert(transferLineForDomestic.IsFinalised);
			Assert(transferLineForForeign.IsFinalised);
			Factory.Save();

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 100, 100);
		}

		#endregion

		#region TestMerchandise_Adjustments

		public void TestMerchandise_Adjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			SetupFTZWarehouse(data.Whs1);

			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;

			var bondedArea = whs1.Areas.Single(a => a.WA_AreaType == "BON");
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_WA_PickingArea = bondedArea.PK;
			location1.WLV_WA_PutawayArea = bondedArea.PK;

			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WA_PickingArea = bondedArea.PK;
			location2.WLV_WA_PutawayArea = bondedArea.PK;

			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_WA_PickingArea = bondedArea.PK;
			location3.WLV_WA_PutawayArea = bondedArea.PK;

			var location4 = data.Whs1.FindLocation("A-2-2");
			location4.WLV_WA_PickingArea = bondedArea.PK;
			location4.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, zoneStatus: ZoneStatusList.Codes.Domestic,
				location: location1);
			CreateCustomsReceiveLine(receive, partB, 4, 20, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				location: location2);
			CreateCustomsReceiveLine(receive, partB, 6, 30, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				location: location3);
			CreateCustomsReceiveLine(receive, partB, 8, 40, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				location: location3);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			CreateCustomsAdjustmentLine(adjustment, partA, -1, -5, ZoneStatusList.Codes.Domestic, location1); // VFD -5
			CreateCustomsAdjustmentLine(adjustment, partA, 2, 10, ZoneStatusList.Codes.ZoneRestricted,
				location2); // VFD should now have to be entered correctly for adjust in lines
			CreateCustomsAdjustmentLine(adjustment, partB, -3, -15, ZoneStatusList.Codes.PrivilegedForeign,
				location3); // VFD -15
			CreateCustomsAdjustmentLine(adjustment, partB, 2, 10, ZoneStatusList.Codes.NonPrivilegedForeign,
				location3); // VFD should now have to be entered correctly for adjust in lines
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 02));
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 5, 90, 95);
		}

		#endregion

		#region TestMerchandise_OrderOutButNotFinalised

		public void TestMerchandise_OrderOutButNotFinalised()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, zoneStatus: ZoneStatusList.Codes.Domestic);
			CreateCustomsReceiveLine(receive, partB, 4, 20, zoneStatus: ZoneStatusList.Codes.ZoneRestricted);
			CreateCustomsReceiveLine(receive, partB, 6, 30, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign);
			CreateCustomsReceiveLine(receive, partB, 8, 40, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, whs1, "O1");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			CreateCustomsOrderLine(order, partA, 2, 10, 2);
			CreateCustomsOrderLine(order, partB, 4, 20, 4);
			CreateCustomsOrderLine(order, partB, 6, 30, 6);
			CreateCustomsOrderLine(order, partB, 8, 40, 8);
			Factory.Save();

			var permitService = CreatePermitService(order);
			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Precondition - Order must not be finalised.", false, order.IsFinalised);
				AssertEquals("Precondition - Pick must not be finalised.", false, pick.IsFinalised);
				Factory.Save();
			}

			AssertBeginningAndEndingMerchandiseValues(2016, data.Whs1, 10, 10, 90, 90);
		}

		#endregion

		void CreateCustomsReceive_10Domestic_90Foreign(WhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part,
			ZDate finalisedDate)
		{
			var receive1 = CreateCustomReceive(org, warehouse, "R1");
			CreateCustomsReceiveLine(receive1, part, 2, 10, entryKey_Domestic,
				zoneStatus: ZoneStatusList.Codes.Domestic);
			FinaliseDocketAndSave(receive1, finalisedDate);
			var receive2 = CreateCustomReceive(org, warehouse, "R2");
			CreateCustomsReceiveLine(receive2, part, 4, 20, entryKey_ZoneRestricted,
				zoneStatus: ZoneStatusList.Codes.ZoneRestricted); // foreign
			FinaliseDocketAndSave(receive2, finalisedDate);
			var receive3 = CreateCustomReceive(org, warehouse, "R3");
			CreateCustomsReceiveLine(receive3, part, 6, 30, entryKey_PForeign,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign); // foreign
			FinaliseDocketAndSave(receive3, finalisedDate);
			var receive4 = CreateCustomReceive(org, warehouse, "R4");
			CreateCustomsReceiveLine(receive4, part, 8, 40, entryKey_NPForeign,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign); // foreign
			FinaliseDocketAndSave(receive4, finalisedDate);
		}

		void CreateCustomsOrder_10Domestic_90Foreign(WhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part,
			ZDate finalisedDate)
		{
			CreateCustomsOrderForMerchandiseTest(warehouse, org, part, finalisedDate, false);
		}

		void CreateCustomsOrder_5Domestic_35Foreign(WhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part,
			ZDate finalisedDate)
		{
			CreateCustomsOrderForMerchandiseTest(warehouse, org, part, finalisedDate, true);
		}

		void CreateCustomsOrderForMerchandiseTest(WhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part,
			ZDate finalisedDate, bool orderPortion)
		{
			var order = CreateCustomOrder(org, warehouse, "O1");
			CreateCustomsOrderLine(order, part, orderPortion ? 1 : 2, 10, 2, entryKey_Domestic);
			CreateCustomsOrderLine(order, part, orderPortion ? 1 : 4, 20, 4, entryKey_ZoneRestricted);
			CreateCustomsOrderLine(order, part, orderPortion ? 2 : 6, 30, 6, entryKey_PForeign);
			CreateCustomsOrderLine(order, part, orderPortion ? 4 : 8, 40, 8, entryKey_NPForeign);
			Factory.Save();

			CreatePickAndFinalise(order, finalisedDate);
		}

		const string entryKey_Domestic = "DOMEntyKey";
		const string entryKey_ZoneRestricted = "ZREntyKey";
		const string entryKey_PForeign = "PForeignEntryKey";
		const string entryKey_NPForeign = "NPForeignEntryKey";

		#endregion

		#region TestMerchandiseMovements

		#region TestMerchandiseReceived

		#region TestMerchandiseReceived_Domestic

		public void TestMerchandiseReceived_Domestic_NotFromOtherFTZs()
		{
			AssertMerchandiseReceived_Domestic(false, ColumnNames.DomesticMerchandiseReceived);
		}

		public void TestMerchandiseReceived_Domestic_FromOtherFTZs()
		{
			AssertMerchandiseReceived_Domestic(true, ColumnNames.DomesticMerchandiseReceivedFromAnotherUSFTZs);
		}

		void AssertMerchandiseReceived_Domestic(bool isFromOtherUSFTZs, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", partA, 1m, new ZDateTimeOffset(2015, 12, 31), 1,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);

			var receive = CreateCustomReceive(data.Org1, whs1, "R2");
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", partA, 6, new ZDateTimeOffset(2016, 06, 30), 5,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", partA, 7, new ZDateTimeOffset(2016, 12, 31), 6,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", partA, 8, new ZDateTimeOffset(2017, 01, 01), 7,
				zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 16m);
		}

		#endregion

		#region TestMerchandiseReceived_ForeignStatus

		public void TestMerchandiseReceived_ForeignStatus_NotFromOtherFTZs()
		{
			AssertMerchandiseReceived_Foreign(false, ColumnNames.ForeignMerchandiseReceived);
		}

		public void TestMerchandiseReceived_ForeignStatus_FromOtherFTZs()
		{
			AssertMerchandiseReceived_Foreign(true, ColumnNames.ForeignMerchandiseReceivedFromAnotherUSFTZs);
		}

		void AssertMerchandiseReceived_Foreign(bool isFromOtherUSFTZs, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", partA, 1m, new ZDateTimeOffset(2015, 12, 31), 1,
				zoneStatus: ZoneStatusList.Codes.ZoneRestricted, isFromOtherUSFTZs: isFromOtherUSFTZs);

			var receive = CreateCustomReceive(data.Org1, whs1, "R2");
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", partA, 6, new ZDateTimeOffset(2016, 06, 30), 5,
				zoneStatus: ZoneStatusList.Codes.ZoneRestricted, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", partA, 7, new ZDateTimeOffset(2016, 12, 31), 6,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", partA, 8, new ZDateTimeOffset(2017, 01, 01), 7,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 23m);
		}

		#endregion

		#region TestMerchandiseReceived_Previleged

		public void TestMerchandiseReceived_Previleged_IsFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Privileged(isFromOtherUSFTZs: true,
				columnName: ColumnNames.ReceivedPrevilegedMerchandiseFromAnotherUSFTZ);
		}

		public void TestMerchandiseReceived_Previleged_IsNotFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Privileged(isFromOtherUSFTZs: false,
				columnName: ColumnNames.ReceivedPrevilegedForeignMerchandise);
		}

		void AssertMerchandiseReceived_Privileged(bool isFromOtherUSFTZs, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", partA, 1m, new ZDateTimeOffset(2015, 12, 31), 1,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);

			var receive = CreateCustomReceive(data.Org1, whs1, "R2");
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", partA, 6, new ZDateTimeOffset(2016, 06, 30), 5,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", partA, 7, new ZDateTimeOffset(2016, 12, 31), 6,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", partA, 8, new ZDateTimeOffset(2017, 01, 01), 7,
				zoneStatus: ZoneStatusList.Codes.PrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 16m);
		}

		#endregion

		#region TestMerchandiseReceived_NonPrevileged

		public void TestMerchandiseReceived_NonPrevileged_IsFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_NonPrevileged(isFromOtherUSFTZs: true,
				columnName: ColumnNames.ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ);
		}

		public void TestMerchandiseReceived_NonPrevileged_IsNotFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_NonPrevileged(isFromOtherUSFTZs: false,
				columnName: ColumnNames.ReceivedNonPrevilegedMerchandise);
		}

		void AssertMerchandiseReceived_NonPrevileged(bool isFromOtherUSFTZs, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", partA, 1m, new ZDateTimeOffset(2015, 12, 31), 1,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);

			var receive = CreateCustomReceive(data.Org1, whs1, "R2");
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partA, 2, 2, zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, zoneStatus: ZoneStatusList.Codes.ZoneRestricted,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 4, 4, zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 5, 5, zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R3", partA, 6, new ZDateTimeOffset(2016, 06, 30), 5,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R4", partA, 7, new ZDateTimeOffset(2016, 12, 31), 6,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R5", partA, 8, new ZDateTimeOffset(2017, 01, 01), 7,
				zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign, isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 15m);
		}

		#endregion

		#region TestMerchandiseReceived_Adjustment

		#region TestMerchandiseReceived_Adjustments_Domestic_Adjustments

		public void TestMerchandiseReceived_Adjustments_Domestic_NotFromOtherFTZs()
		{
			AssertMerchandiseReceived_Adjustments_Domestic(false, ColumnNames.DomesticMerchandiseReceived);
		}

		public void TestMerchandiseReceived_Adjustments_Domestic_FromOtherFTZs()
		{
			AssertMerchandiseReceived_Adjustments_Domestic(true,
				ColumnNames.DomesticMerchandiseReceivedFromAnotherUSFTZs);
		}

		void AssertMerchandiseReceived_Adjustments_Domestic(bool isFromOtherUSFTZs, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON");
			whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", partA, new ZDate(2015, 12, 31), 1m, 1,
				entryKey: "A", zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);

			var receive = CreateCustomReceive(data.Org1, whs1, "R2");
			CreateCustomsReceiveLine(receive, partA, 2, 2, entryKey: "B", zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, entryKey: "B", zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsReceiveLine(receive, partB, 3, 3, entryKey: "B", zoneStatus: ZoneStatusList.Codes.Domestic,
				isFromOtherUSFTZs: !isFromOtherUSFTZs);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", partB, new ZDate(2016, 06, 30), -2m, 2,
				entryKey: "B", zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", partA, new ZDate(2016, 12, 31), 1m, 1,
				entryKey: "E", zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A4", partA, new ZDate(2016, 12, 31), -1m, 1,
				entryKey: "E", zoneStatus: ZoneStatusList.Codes.Domestic, isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 3m);
		}

		#endregion

		#region TestMerchandiseReceived_Adjustment_ForeignStatus

		public void TestMerchandiseReceived_Adjustment_ForeignStatus_NotFromOtherFTZs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(false, ColumnNames.ForeignMerchandiseReceived, 30);
		}

		public void TestMerchandiseReceived_Adjustment_ForeignStatus_FromOtherFTZs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(true, ColumnNames.ForeignMerchandiseReceivedFromAnotherUSFTZs,
				30);
		}

		#endregion

		#region TestMerchandiseReceived_Adjustment_Previleged

		public void TestMerchandiseReceived_Adjustment_Previleged_IsFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(isFromOtherUSFTZs: true,
				columnName: ColumnNames.ReceivedPrevilegedMerchandiseFromAnotherUSFTZ, expectedVFDValue: 14);
		}

		public void TestMerchandiseReceived_Adjustment_Previleged_IsNotFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(isFromOtherUSFTZs: false,
				columnName: ColumnNames.ReceivedPrevilegedForeignMerchandise, expectedVFDValue: 14);
		}

		#endregion

		#region TestMerchandiseReceived_Adjustment_NonPrevileged

		public void TestMerchandiseReceived_Adjustment_NonPrevileged_IsFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(isFromOtherUSFTZs: true,
				columnName: ColumnNames.ReceivedNonPrevilegedMerchandiseFromAnotherUSFTZ, expectedVFDValue: 6);
		}

		public void TestMerchandiseReceived_Adjustment_NonPrevileged_IsNotFromAnotherFTZWhs()
		{
			AssertMerchandiseReceived_Adjustment_Foreign(isFromOtherUSFTZs: false,
				columnName: ColumnNames.ReceivedNonPrevilegedMerchandise, expectedVFDValue: 6);
		}

		#endregion

		void AssertMerchandiseReceived_Adjustment_Foreign(bool isFromOtherUSFTZs, string columnName,
			ZDecimal expectedVFDValue)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;
			whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A1", partA, new ZDate(2016, 01, 31), 10, 20,
				entryKey: "A", zoneStatus: ZoneStatusList.Codes.ZoneRestricted, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A2", partA, new ZDate(2016, 06, 30), -5, 10,
				entryKey: "A", zoneStatus: ZoneStatusList.Codes.ZoneRestricted, isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A3", partA, new ZDate(2016, 07, 31), 5, 10,
				entryKey: "B", zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A4", partA, new ZDate(2016, 10, 01), -2, 4,
				entryKey: "B", zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A5", partA, new ZDate(2016, 12, 31), 8, 16,
				entryKey: "C", zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A6", partA, new ZDate(2016, 12, 31), -1, 2,
				entryKey: "C", zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A7", partA, new ZDate(2017, 01, 01), -2, 4,
				entryKey: "C", zoneStatus: ZoneStatusList.Codes.PrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);
			CreateCustomsAdjustmentsWithInventory(whs1, data.Org1, "A8", partA, new ZDate(2017, 10, 01), -2, 4,
				entryKey: "B", zoneStatus: ZoneStatusList.Codes.NonPrivilegedForeign,
				isFromOtherUSFTZs: isFromOtherUSFTZs);

			AssertFTZAnnualReportValue(whs1, 2016, columnName, expectedVFDValue);
		}

		#endregion

		#endregion

		#region TestMerchandiseForwardered(Ordered)

		public void TestMerchandiseOrdered_ForUSMarket()
		{
			AssertMerchandiseOrdered(WhsBondedWarehouseAttributeOutwardType.Codes.CNN,
				ColumnNames.OrderedVFDForDomesticMarket);
		}

		public void TestMerchandiseOrdered_Exports()
		{
			AssertMerchandiseOrdered(WhsBondedWarehouseAttributeOutwardType.Codes.EXS,
				ColumnNames.OrderedVFDForInternationalMarket);
		}

		public void TestMerchandiseOrdered_ToOtherUSFTZs()
		{
			AssertMerchandiseOrdered(WhsBondedWarehouseAttributeOutwardType.Codes.TOF,
				ColumnNames.OrderedVFDForOtherUSFTZs);
		}

		void AssertMerchandiseOrdered(string outwardType, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;

			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R1", partA, 100, new ZDateTimeOffset(2015, 11, 30), 100);
			CreateCustomsReceiveWithInventory(whs1, data.Org1, "R2", partB, 50, new ZDateTimeOffset(2016, 01, 01), 100);

			CreateCustomsOrderWithOrderLine(whs1, data.Org1, "O2", partA, 1, 100m, new ZDate(2015, 12, 31), 100m,
				outwardType: outwardType);

			var order = Helper.CreateWhsOrder(data.Org1, whs1, "O3");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;

			CreateCustomsOrderLine(order, partA, 2, 100m, 100m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(order, partB, 4, 100m, 50m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(order, partA, 2, 100m, 100m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(order, partB, 4, 100m, 50m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(order, partA, 2, 100m, 100m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);
			CreateCustomsOrderLine(order, partB, 4, 100m, 50m,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);

			Factory.Save();
			CreatePickAndFinalise(order, new ZDate(2016, 01, 01));

			CreateCustomsOrderWithOrderLine(whs1, data.Org1, "O4", partA, 7, 100m, new ZDate(2016, 06, 30), 100m,
				outwardType: outwardType); // VFD value should be 7m
			CreateCustomsOrderWithOrderLine(whs1, data.Org1, "O5", partA, 8, 100m, new ZDate(2016, 12, 31), 100m,
				outwardType: outwardType); // VFD value should be 7m
			CreateCustomsOrderWithOrderLine(whs1, data.Org1, "O6", partA, 9, 100m, new ZDate(2017, 01, 01), 100m,
				outwardType: outwardType); // VFD value should be 9m

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 25m);
		}

		#endregion

		#region TestMerchandiseOrderedButNotFinalised

		public void TestMerchandiseOrderedButNotFinalised_ForUSMarket()
		{
			AssertMerchandiseOrderedButNotFinalised(WhsBondedWarehouseAttributeOutwardType.Codes.CNN,
				ColumnNames.OrderedVFDForDomesticMarket);
		}

		public void TestMerchandiseOrderedButNotFinalised_Exports()
		{
			AssertMerchandiseOrderedButNotFinalised(WhsBondedWarehouseAttributeOutwardType.Codes.EXS,
				ColumnNames.OrderedVFDForInternationalMarket);
		}

		public void TestMerchandiseOrderedButNotFinalised_ToOtherUSFTZs()
		{
			AssertMerchandiseOrderedButNotFinalised(WhsBondedWarehouseAttributeOutwardType.Codes.TOF,
				ColumnNames.OrderedVFDForOtherUSFTZs);
		}

		void AssertMerchandiseOrderedButNotFinalised(string outwardType, string columnName)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;

			var order = Helper.CreateWhsOrder(data.Org1, whs1, "O3");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;

			CreateCustomsOrderLine(order, partA, 2, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(order, partB, 4, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
			CreateCustomsOrderLine(order, partA, 2, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(order, partB, 4, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.EXS);
			CreateCustomsOrderLine(order, partA, 2, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);
			CreateCustomsOrderLine(order, partB, 4, 2, 2,
				outwardType: WhsBondedWarehouseAttributeOutwardType.Codes.TOF);

			Factory.Save();
			var permitService = CreatePermitService(order);
			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Precondition - Order must not be finalised.", false, order.IsFinalised);
				AssertEquals("Precondition - Pick must not be finalised.", false, pick.IsFinalised);
				Factory.Save();
			}

			AssertFTZAnnualReportValue(whs1, 2016, columnName, 0m);
		}

		#endregion

		#endregion

		#region TestFTZReport_DocketLineWithNoBondedQty

		public void TestFTZReport_DocketLineWithNoBondedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 1, data.Whs1.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 0;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 0;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			AssertEquals("Precondition - adjustment in line has no bonded qty.", 0m,
				adjustmentInLine.CustomsData.WB_BondedWhsQty);

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentOut.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentOutLine =
				Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -1, data.Whs1.DefaultLocationInBondedArea);
			adjustmentOutLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentOutLine.CustomsData.WB_EntryKey = "A";
			adjustmentOutLine.CustomsData.WB_ValueForDuty = 0;
			adjustmentOutLine.CustomsData.WB_BondedWhsQty = 0;
			adjustmentOutLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentOutLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentOutLine.WE_BondedEntryKey = "A";
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOut);
			Factory.Save();

			AssertEquals("Precondition - adjustment out line has no bonded qty.", 0m,
				adjustmentOutLine.CustomsData.WB_BondedWhsQty);

			var from = new ZDate(ZDate.Today.Year, 01, 01);
			var to = new ZDate(ZDate.Today.Year, 12, 31);
			AssertNoExceptionThrown("There should be no exception thrown in the generation of the report.",
				() => Load_Report_FTZAnnualReport(data.Whs1.PK, from, to));

			var results = Load_Report_FTZAnnualReport(data.Whs1.PK, from, to);
			AssertEquals("VFD should be 0.", 0m, results[0]["ForeignMerchandiseReceived"]);
		}

		#endregion

		#region TestFTZReport_TransferredStockOrdered

		public void TestFTZReport_TransferredStockOrdered_Domestic()
		{
			TestFTZReport_TransferredStockOrdered_Core(
				ZoneStatusList.Codes.Domestic,
				ColumnNames.BeginningValueForDomesticMerchandise,
				ColumnNames.EndingValueForDomesticMerchandise,
				ColumnNames.OrderedVFDForDomesticMarket);
		}

		public void TestFTZReport_TransferredStockOrdered_Foreign()
		{
			TestFTZReport_TransferredStockOrdered_Core(
				ZoneStatusList.Codes.NonPrivilegedForeign,
				ColumnNames.BeginningValueForForeignMerchandise,
				ColumnNames.EndingValueForForeignMerchandise,
				ColumnNames.OrderedVFDForInternationalMarket);
		}

		void TestFTZReport_TransferredStockOrdered_Core(string zoneStatus, string beginColumnName, string endColumnName,
			string orderedVFDColumnName)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			var whs1 = data.Whs1;
			SetupFTZWarehouse(data.Whs1);
			var productA = data.Part1;

			var bondedArea = Helper.CreateArea(whs1, "BONDED", Environment.CodeLists.AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			location2.WLV_WA_PickingArea = bondedArea.PK;

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, productA, 50m, 100m, zoneStatus: zoneStatus, location: location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 04, 04));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "T1");
			CreateCustomsTransferLine(transfer, productA, 50m, 0m, zoneStatus, location1, location2);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 05, 05));
			Factory.Save();

			AssertEquals("Precondition: Receive has no stock", 0m, receive.Lines[0].WE_StockOnHand);
			AssertEquals("Precondition: Transfer has stock", 50m, transfer.Lines[0].WE_StockOnHand);

			var orderInWhs1 = CreateCustomOrder(data.Org1, whs1, "O1");
			var orderLine = CreateCustomsOrderLine(orderInWhs1, productA, 50m, 100m, 100m);
			orderLine.CustomsData.WB_OutwardType = zoneStatus != ZoneStatusList.Codes.Domestic
				? WhsBondedWarehouseAttributeOutwardType.Codes.EXS
				: WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			Factory.Save();
			CreatePickAndFinalise(orderInWhs1, new ZDate(2015, 12, 30));

			var result = Load_Report_FTZAnnualReport(whs1.PK, new ZDate(2015, 01, 01), new ZDate(2015, 12, 31));
			CombineAssertions(() =>
			{
				AssertEquals("Should always return 1 result.", 1, result.Count);
				AssertEquals("Begin Value should be correct.", 0m, result[0][beginColumnName]);
				AssertEquals("Ending Value should be correct.", 0m, result[0][endColumnName]);
				AssertEquals("Ordered VFD Value should be correct.", 100m, result[0][orderedVFDColumnName]);
			});
		}

		public void TestFTZReport_TransferredStockOrdered_Ratio()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 2);
			var whs1 = data.Whs1;
			SetupFTZWarehouse(data.Whs1);
			var productA = data.Part1;

			var bondedArea = Helper.CreateArea(whs1, "BONDED", Environment.CodeLists.AreaTypes.Codes.Bonded);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			location2.WLV_WA_PickingArea = bondedArea.PK;

			var receive = CreateCustomReceive(data.Org1, whs1, "R1");
			CreateCustomsReceiveLine(receive, productA, 50m, 70m, zoneStatus: ZoneStatusList.Codes.Domestic,
				location: location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 04, 04));
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "T1");
			CreateCustomsTransferLine(transfer, productA, 40m, 0m, ZoneStatusList.Codes.Domestic, location1, location2);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 05, 05));
			Factory.Save();

			AssertEquals("Precondition: Receive has correct stock", 10m, receive.Lines[0].WE_StockOnHand);
			AssertEquals("Precondition: Transfer has correct stock", 40m, transfer.Lines[0].WE_StockOnHand);

			var order = CreateCustomOrder(data.Org1, whs1, "O1");
			var orderLine = CreateCustomsOrderLine(order, productA, 20m, 70m, 0m);
			Factory.Save();

			var permitService = CreatePermitService(order);
			WhsPick pick = null;
			using (ObjectFactory.Substitute(permitService.Object))
			{
				pick = Helper.CreatePickNew(order);
				Factory.Save();
			}

			var pickLine1 = orderLine.PickLines[0];
			var pickLine2 = orderLine.PickLines.Last();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, new ZDateTimeOffset(2015, 05, 05));
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, new ZDateTimeOffset(2015, 05, 05));
			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();
			Assert(transferLine1.IsFinalised);
			Assert(transferLine2.IsFinalised);

			using (ObjectFactory.Substitute(permitService.Object))
			{
				pick.FinaliseOrder(order);
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);
				order.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 30));
				Factory.Save();
			}

			var result = Load_Report_FTZAnnualReport(whs1.PK, new ZDate(2015, 01, 01), new ZDate(2015, 12, 31));
			CombineAssertions(() =>
			{
				AssertEquals("Should always return 1 result.", 1, result.Count);
				AssertEquals("Begin Value should be correct.", 0m,
					result[0][ColumnNames.BeginningValueForDomesticMerchandise]);
				AssertEquals("Ending Value should be correct.", 42m,
					result[0][ColumnNames.EndingValueForDomesticMerchandise]);
				AssertEquals("Ordered VFD Value should be correct.", 28m,
					result[0][ColumnNames.OrderedVFDForDomesticMarket]);
			});
		}

		#endregion

		#region TestTop5ReceivingCommodities

		public void TestReceivingCommoditiesOnly5TopCompaniesAreReturned()
		{
			AssertReceivingCommoditiesOnly5TopCompaniesAreReturned(new ZDate(2016, 06, 01));
		}

		void AssertReceivingCommoditiesOnly5TopCompaniesAreReturned(ZDate finalisedDate)
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partC = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");
			var partE = Helper.CreateProduct(data.Org1, "E");
			var partF = Helper.CreateProduct(data.Org1, "F");
			var partG = Helper.CreateProduct(data.Org1, "G");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			var commodityD = Helper.CreateCommodityCode("D");
			var commodityE = Helper.CreateCommodityCode("E");
			var commodityF = Helper.CreateCommodityCode("F");
			var commodityG = Helper.CreateCommodityCode("G");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partC, commodityC.RH_Code);
			SetCommodity(partD, commodityD.RH_Code);
			SetCommodity(partE, commodityE.RH_Code);
			SetCommodity(partF, commodityF.RH_Code);
			SetCommodity(partG, commodityG.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 1, 10, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 2, 20, countryOfOrigin: "CH");
			CreateCustomsReceiveLine(receive, partC, 3, 30, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partD, 4, 40, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partE, 5, 50, countryOfOrigin: "KW");
			CreateCustomsReceiveLine(receive, partF, 6, 60, countryOfOrigin: "NZ");
			CreateCustomsReceiveLine(receive, partG, 7, 70, countryOfOrigin: "AU");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDate(finalisedDate));
			Factory.Save();

			var resultsFor2016 =
				Load_Report_ReceivedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 5 records for Received Merchandise.", 5, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "G", 70, "AU");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "F", 60, "NZ");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "E", 50, "KW");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[3], "D", 40, "IN");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[4], "C", 30, "LK");
		}

		public void TestTop5ReceivingCommodities_MerchandiseReceivedAccrossMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("W2");
			SetupFTZWarehouse(whs1);
			SetupFTZWarehouse(whs2);

			var partA = data.Part1;
			var partB = data.Part2;
			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 2, 10, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partB, 4, 20, countryOfOrigin: "AU");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 06, 01));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs2, data.Org1, "R2", partA, 2, new ZDateTimeOffset(2015, 12, 31), 10);

			var resultsFor2015 =
				Load_Report_ReceivedMerchandise(data.Whs1.PK, new ZDate(2015, 01, 01), new ZDate(2015, 12, 31));
			AssertEquals(0, resultsFor2015.Count);

			var resultsFor2016 =
				Load_Report_ReceivedMerchandise(data.Whs1.PK, new ZDate(2015, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals(2, resultsFor2016.Count);

			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "B", 20, "AU");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "A", 10, "LK");
		}

		public void TestTop5ReceivingCommodities_ReceivedAtTheBeginningOfTheYear()
		{
			AssertReceivingCommoditiesOnly5TopCompaniesAreReturned(new ZDate(2016, 01, 01));
		}

		public void TestTop5ReceivingCommodities_ReceivedAtTheEndOfTheYear()
		{
			AssertReceivingCommoditiesOnly5TopCompaniesAreReturned(new ZDate(2016, 12, 31));
		}

		public void TestTop5ReceivingCommodities_SameCommodityInMultipleProducts()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partC = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partC, commodityA.RH_Code);
			SetCommodity(partD, commodityC.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 1, 10, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 2, 20, countryOfOrigin: "CH");
			CreateCustomsReceiveLine(receive, partC, 3, 35, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partB, 1, 5, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partD, 4, 40, countryOfOrigin: "IN");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 06, 01));
			Factory.Save();

			var resultsFor2016 =
				Load_Report_ReceivedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 3 records for Received Merchandise.", 3, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "A", 45, "LK");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "C", 40, "IN");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "B", 25, "CH");
		}

		public void TestReceivingCountryOfOrigin()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partC = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");
			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partC, commodityA.RH_Code);
			SetCommodity(partD, commodityC.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 1, 10, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partB, 2, 20, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partC, 3, 35, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 1, 5, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partD, 4, 40, countryOfOrigin: "CA");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 06, 01));
			Factory.Save();

			var resultsFor2016 =
				Load_Report_ReceivedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 3 records for Received Merchandise.", 3, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "A", 45, "AU");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "C", 40, "CA");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "B", 25, "LK");
		}

		public void TestTop5ReceivingCommodities_NoBondedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var date = new ZDateTimeOffset(2022, 08, 07);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 1, data.Whs1.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 0;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 0;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			AssertEquals("Precondition - adjustment in line has no bonded qty.", 0m,
				adjustmentInLine.CustomsData.WB_BondedWhsQty);

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentOut.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentOutLine =
				Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -1, data.Whs1.DefaultLocationInBondedArea);
			adjustmentOutLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentOutLine.CustomsData.WB_EntryKey = "A";
			adjustmentOutLine.CustomsData.WB_ValueForDuty = 0;
			adjustmentOutLine.CustomsData.WB_BondedWhsQty = 0;
			adjustmentOutLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentOutLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentOutLine.WE_BondedEntryKey = "A";
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(adjustmentOut);
			Factory.Save();

			AssertEquals("Precondition - adjustment out line has no bonded qty.", 0m,
				adjustmentOutLine.CustomsData.WB_BondedWhsQty);

			var currentYear = date.Year;
			var from = new ZDate(currentYear - 1, 12, 31);
			var to = new ZDate(currentYear, 12, 31);
			AssertNoExceptionThrown("There should be no exception thrown in the generation of the report.",
				() => Load_Report_ReceivedMerchandise(data.Whs1.PK, from, to));

			var results = Load_Report_ReceivedMerchandise(data.Whs1.PK, from, to);
			AssertEquals("VFD should be 0.", 0m, results[0][ColumnNames.Value]);
		}

		public void TestTop5ReceivingCommodities_VFD_RoundedTo2DecimalPlaces()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var date = new ZDateTimeOffset(2022, 08, 07);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 6m, data.Whs1.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 10;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 6;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A2");
			adjustmentOut.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentOutLine =
				Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -2m, data.Whs1.DefaultLocationInBondedArea);
			adjustmentOutLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentOutLine.CustomsData.WB_EntryKey = "A";
			adjustmentOutLine.CustomsData.WB_ValueForDuty = 10;
			adjustmentOutLine.CustomsData.WB_BondedWhsQty = 6;
			adjustmentOutLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentOutLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentOutLine.WE_BondedEntryKey = "A";
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentOut);
			adjustmentOut.WD_FinalisedDate = date;
			Factory.Save();

			var currentYear = date.Year;
			var from = new ZDate(currentYear - 1, 12, 31);
			var to = new ZDate(currentYear, 12, 31);
			var results = Load_Report_ReceivedMerchandise(data.Whs1.PK, from, to);
			AssertEquals("VFD should be correct.", 6.67m, results[0][ColumnNames.Value]);
		}

		#endregion

		#region TestTop5ForwardedCommodities

		public void TestForwardedCommoditiesOnly5TopCompaniesAreReturned()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partC = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");
			var partE = Helper.CreateProduct(data.Org1, "E");
			var partF = Helper.CreateProduct(data.Org1, "F");
			var partG = Helper.CreateProduct(data.Org1, "G");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			var commodityD = Helper.CreateCommodityCode("D");
			var commodityE = Helper.CreateCommodityCode("E");
			var commodityF = Helper.CreateCommodityCode("F");
			var commodityG = Helper.CreateCommodityCode("G");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partC, commodityC.RH_Code);
			SetCommodity(partD, commodityD.RH_Code);
			SetCommodity(partE, commodityE.RH_Code);
			SetCommodity(partF, commodityF.RH_Code);
			SetCommodity(partG, commodityG.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 10, 100, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "CH");
			CreateCustomsReceiveLine(receive, partC, 10, 100, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partD, 10, 100, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partE, 10, 100, countryOfOrigin: "KW");
			CreateCustomsReceiveLine(receive, partF, 10, 100, countryOfOrigin: "NZ");
			CreateCustomsReceiveLine(receive, partG, 10, 100, countryOfOrigin: "AU");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 06, 01));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "R1");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			CreateCustomsOrderLine(order, partA, 1, 100, 10m, countryOfOrigin: "AU");
			CreateCustomsOrderLine(order, partB, 2, 100, 10m, countryOfOrigin: "CH");
			CreateCustomsOrderLine(order, partC, 3, 100, 10m, countryOfOrigin: "LK");
			CreateCustomsOrderLine(order, partD, 4, 100, 10m, countryOfOrigin: "IN");
			CreateCustomsOrderLine(order, partE, 5, 100, 10m, countryOfOrigin: "KW");
			CreateCustomsOrderLine(order, partF, 6, 100, 10m, countryOfOrigin: "NZ");
			CreateCustomsOrderLine(order, partG, 7, 100, 10m, countryOfOrigin: "AU");
			Factory.Save();
			CreatePickAndFinalise(order, new ZDate(2016, 06, 02));

			var resultsFor2016 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 5 records for Ordered Merchandise.", 5, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "G", 70, "AU");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "F", 60, "NZ");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "E", 50, "KW");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[3], "D", 40, "IN");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[4], "C", 30, "LK");
		}

		public void TestTop5ForwardedCommodities_MerchandiseOrderedAccrossMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("W2");
			SetupFTZWarehouse(whs1);
			SetupFTZWarehouse(whs2);

			var partA = data.Part1;
			var partB = data.Part2;
			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 100, 100, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 100, 200, countryOfOrigin: "CH");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			CreateCustomsReceiveWithInventory(whs2, data.Org1, "R2", partA, 2, new ZDateTimeOffset(2015, 12, 31), 10);

			var order1 = CreateCustomOrder(data.Org1, data.Whs1, "01");
			CreateCustomsOrderLine(order1, partA, 1, 100, 100, countryOfOrigin: "AU");
			CreateCustomsOrderLine(order1, partB, 2, 200, 100, countryOfOrigin: "CH");
			Factory.Save();
			CreatePickAndFinalise(order1, new ZDate(2016, 01, 01));

			var order2 = CreateCustomOrder(data.Org1, whs2, "02");
			CreateCustomsOrderLine(order2, partA, 1, 2, 2);
			Factory.Save();
			CreatePickAndFinalise(order2, new ZDate(2016, 01, 01));

			var resultsFor2015 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2015, 01, 01), new ZDate(2015, 12, 31));
			AssertEquals("There must be no results for 2015", 0, resultsFor2015.Count);

			var resultsFor2016 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 2 records for Ordered Merchandise.", 2, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "B", 4, "CH");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "A", 1, "AU");
		}

		public void TestTop5ForwardedCommodities_OrderedAtTheBeginningOfTheYear()
		{
			AssertTop5ForwardedCommodities_Ordered(new ZDate(2016, 01, 01));
		}

		public void TestTop5ForwardedCommodities_OrderedAtTheEndOfTheYear()
		{
			AssertTop5ForwardedCommodities_Ordered(new ZDate(2016, 12, 31));
		}

		void AssertTop5ForwardedCommodities_Ordered(ZDate orderFinalisedDate)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			SetupFTZWarehouse(whs1);

			var partA = data.Part1;
			var partB = data.Part2;
			var partC = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");
			var partE = Helper.CreateProduct(data.Org1, "E");
			var partF = Helper.CreateProduct(data.Org1, "F");
			var partG = Helper.CreateProduct(data.Org1, "G");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			var commodityD = Helper.CreateCommodityCode("D");
			var commodityE = Helper.CreateCommodityCode("E");
			var commodityF = Helper.CreateCommodityCode("F");
			var commodityG = Helper.CreateCommodityCode("G");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partC, commodityC.RH_Code);
			SetCommodity(partD, commodityD.RH_Code);
			SetCommodity(partE, commodityE.RH_Code);
			SetCommodity(partF, commodityF.RH_Code);
			SetCommodity(partG, commodityG.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 10, 100, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "CH");
			CreateCustomsReceiveLine(receive, partC, 10, 100, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partD, 10, 100, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partE, 10, 100, countryOfOrigin: "KW");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2015, 12, 31));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			CreateCustomsOrderLine(order, partA, 1, 100, 10, countryOfOrigin: "AU");
			CreateCustomsOrderLine(order, partB, 2, 100, 10, countryOfOrigin: "CH");
			CreateCustomsOrderLine(order, partC, 3, 100, 10, countryOfOrigin: "LK");
			CreateCustomsOrderLine(order, partD, 4, 100, 10, countryOfOrigin: "IN");
			CreateCustomsOrderLine(order, partE, 5, 100, 10, countryOfOrigin: "KW");
			Factory.Save();

			CreatePickAndFinalise(order, orderFinalisedDate);

			var resultsFor2016 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 5 records for Ordered Merchandise.", 5, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "E", 50, "KW");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "D", 40, "IN");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "C", 30, "LK");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[3], "B", 20, "CH");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[4], "A", 10, "AU");
		}

		public void TestTop5ForwardedCommodities_SameCommodityInMultipleProducts()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partA2 = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partA2, commodityA.RH_Code);
			SetCommodity(partD, commodityC.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 10, 100, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "CH");
			CreateCustomsReceiveLine(receive, partA2, 10, 100, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partD, 10, 100, countryOfOrigin: "KW");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 01, 01));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			CreateCustomsOrderLine(order, partA, 1, 100, 10, countryOfOrigin: "AU");
			CreateCustomsOrderLine(order, partB, 2, 100, 10, countryOfOrigin: "CH");
			CreateCustomsOrderLine(order, partA2, 3, 100, 10, countryOfOrigin: "LK");
			CreateCustomsOrderLine(order, partB, 1, 100, 10, countryOfOrigin: "IN");
			CreateCustomsOrderLine(order, partD, 6, 100, 10, countryOfOrigin: "KW");
			Factory.Save();
			CreatePickAndFinalise(order, new ZDate(2016, 06, 01));

			var resultsFor2016 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 3 records for Ordered Merchandise.", 3, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "C", 60, "KW");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "A", 40, "LK");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "B", 30, "CH");
		}

		public void TestOrderedCountryOfOrigin()
		{
			var data = GetDataSimpleEnviornmentWithFTZWarehouse();
			var whs1 = data.Whs1;

			var partA = data.Part1;
			var partB = data.Part2;
			var partA2 = Helper.CreateProduct(data.Org1, "C");
			var partD = Helper.CreateProduct(data.Org1, "D");

			var commodityA = Helper.CreateCommodityCode("A");
			var commodityB = Helper.CreateCommodityCode("B");
			var commodityC = Helper.CreateCommodityCode("C");
			SetCommodity(partA, commodityA.RH_Code);
			SetCommodity(partB, commodityB.RH_Code);
			SetCommodity(partA2, commodityA.RH_Code);
			SetCommodity(partD, commodityC.RH_Code);

			var receive = CreateCustomReceive(data.Org1, data.Whs1, "R1");
			CreateCustomsReceiveLine(receive, partA, 10, 100, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "LK");
			CreateCustomsReceiveLine(receive, partA2, 10, 100, countryOfOrigin: "AU");
			CreateCustomsReceiveLine(receive, partB, 10, 100, countryOfOrigin: "IN");
			CreateCustomsReceiveLine(receive, partD, 10, 100, countryOfOrigin: "CA");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDate(2016, 06, 01));
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			// Order VFD and Country of Origin are calculated from Inventory
			CreateCustomsOrderLine(order, partA, 1, 100, 10, countryOfOrigin: "LK"); //		VFD = 1/100 = 10	CoO = LK
			CreateCustomsOrderLine(order, partB, 2, 100, 10, countryOfOrigin: "LK"); //		VFD = 2/100 = 20	CoO = LK
			CreateCustomsOrderLine(order, partA2, 4, 100, 10, countryOfOrigin: "AU"); //	VFD = 4/100 = 40	CoO = AU
			CreateCustomsOrderLine(order, partB, 1, 100, 10, countryOfOrigin: "IN"); //		VFD = 1/100 = 10	CoO = IN
			CreateCustomsOrderLine(order, partD, 4, 100, 10, countryOfOrigin: "CA"); //		VFD = 4/100 = 40	CoO = CA
			Factory.Save();
			CreatePickAndFinalise(order, new ZDate(2016, 06, 01));

			var resultsFor2016 =
				Load_Report_OrderedMerchandise(data.Whs1.PK, new ZDate(2016, 01, 01), new ZDate(2016, 12, 31));
			AssertEquals("There must be only 3 records for Ordered Merchandise.", 3, resultsFor2016.Count);
			AssertReceivedAndOrderedMerchandise(resultsFor2016[0], "A", 50, "AU");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[1], "C", 40, "CA");
			AssertReceivedAndOrderedMerchandise(resultsFor2016[2], "B", 30, "LK");
		}

		public void TestTop5ForwardedCommodities_NoBondedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 1, data.Whs1.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 0;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 0;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			AssertEquals("Precondition - adjustment in line has no bonded qty.", 0m,
				adjustmentInLine.CustomsData.WB_BondedWhsQty);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = "A";
			orderLine.CustomsData.WB_ValueForDuty = 0;
			orderLine.CustomsData.WB_BondedWhsQty = 0;
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			orderLine.WE_BondedEntryKey = "A";
			Factory.Save();
			CreatePickAndFinalise(order, ZDate.Today);

			var from = new ZDate(ZDate.Today.Year, 01, 01);
			var to = new ZDate(ZDate.Today.Year, 12, 31);
			AssertNoExceptionThrown("There should be no exception thrown in the generation of the report.",
				() => Load_Report_OrderedMerchandise(data.Whs1.PK, from, to));

			var results = Load_Report_OrderedMerchandise(data.Whs1.PK, from, to);
			AssertEquals("VFD should be 0.", 0m, results[0][ColumnNames.Value]);
		}

		public void TestTop5ForwardedCommodities_VFD_RoundedTo2DecimalPlaces()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupFTZWarehouse(data.Whs1);
			Factory.Save();

			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			adjustmentIn.WD_DocketSubType = OrderType.Codes.Customs;
			var adjustmentInLine =
				Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 6m, data.Whs1.DefaultLocationInBondedArea);
			adjustmentInLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentInLine.CustomsData.WB_EntryKey = "A";
			adjustmentInLine.CustomsData.WB_ValueForDuty = 10;
			adjustmentInLine.CustomsData.WB_BondedWhsQty = 6;
			adjustmentInLine.CustomsData.WB_IsFromAnotherFTZWhs = false;
			adjustmentInLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			adjustmentInLine.WE_BondedEntryKey = "A";
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustmentIn);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = "A";
			orderLine.CustomsData.WB_ValueForDuty = 10;
			orderLine.CustomsData.WB_BondedWhsQty = 6;
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			orderLine.WE_BondedEntryKey = "A";
			Factory.Save();
			CreatePickAndFinalise(order, ZDate.Today);

			var from = new ZDate(ZDate.Today.Year, 01, 01);
			var to = new ZDate(ZDate.Today.Year, 12, 31);
			var results = Load_Report_OrderedMerchandise(data.Whs1.PK, from, to);
			AssertEquals("VFD should be correct.", 6.67m, results[0][ColumnNames.Value]);
		}

		#endregion

		#region Implementation

		void AssertBeginningAndEndingMerchandiseValues(int year, WhsWarehouse warehouse, decimal domBeginVFD,
			decimal domEndVFD, decimal fgnBeginVFD, decimal fgnEndVFD)
		{
			var resultsFor2016 =
				Load_Report_FTZAnnualReport(warehouse.PK, new ZDate(year, 01, 01), new ZDate(year, 12, 31));
			AssertEquals("There must be a record for 2016.", 1, resultsFor2016.Count);
			AssertEquals($"Beginning value for domestic merchandise is {domBeginVFD}.", domBeginVFD,
				resultsFor2016[0][ColumnNames.BeginningValueForDomesticMerchandise]);
			AssertEquals($"End value for domestic merchandise is {domEndVFD}.", domEndVFD,
				resultsFor2016[0][ColumnNames.EndingValueForDomesticMerchandise]);
			AssertEquals($"Beginning value for foreign merchandise is {fgnBeginVFD}.", fgnBeginVFD,
				resultsFor2016[0][ColumnNames.BeginningValueForForeignMerchandise]);
			AssertEquals($"End value for foreign merchandise is {fgnEndVFD}.", fgnEndVFD,
				resultsFor2016[0][ColumnNames.EndingValueForForeignMerchandise]);
		}

		void FinaliseDocketAndSave(WhsReceive receive, ZDate finalisedDate)
		{
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = receive.Warehouse.GetWarehouseBranchDateTimeOffset(finalisedDate);
			Factory.Save();
		}

		WhsReceive CreateCustomReceive(OrgHeader org, WhsWarehouse warehouse, string docketNumber)
		{
			var receive = Helper.CreateWhsReceive(org, warehouse, docketNumber);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			return receive;
		}

		WhsOrder CreateCustomOrder(OrgHeader org, WhsWarehouse warehouse, string docketNumber)
		{
			var order = Helper.CreateWhsOrder(org, warehouse, docketNumber);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			return order;
		}

		void CreatePickAndFinalise(WhsOrder order, ZDate orderFinalisedDate)
		{
			var permitService = CreatePermitService(order);
			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseOrder(order);
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);
				order.WD_FinalisedDate = order.Warehouse.GetWarehouseBranchDateTimeOffset(orderFinalisedDate);
				Factory.Save();
			}
		}

		static Mock<IPermitService> CreatePermitService(WhsOrder order)
		{
			var permitService = new Mock<IPermitService>();
			var responsesForIsAvailable = new List<WhsPermitWithdrawRequestResponseForTest>();
			var responsesForTryGetPermits = new List<WhsPermitWithdrawRequestResponseForTest>();

			foreach (WhsOrderLine line in order.Lines)
			{
				var responseIsAvailable = new WhsPermitWithdrawRequestResponseForTest(line, SuccessOrFailure.Success,
					line.WE_TransactionQuantity, "A");
				var responseTryGetPermits = new WhsPermitWithdrawRequestResponseForTest(line, SuccessOrFailure.Success,
					line.WE_TransactionQuantity, "A");
				responsesForIsAvailable.Add(responseIsAvailable);
				responsesForTryGetPermits.Add(responseTryGetPermits);
			}

			permitService
				.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns(
					new WhsPermitWithdrawRequestResponseResultForTest
					{
						Responses = responsesForIsAvailable.ToArray()
					});

			permitService
				.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns(
					new WhsPermitWithdrawRequestResponseResultForTest
					{
						Responses = responsesForTryGetPermits.ToArray()
					});

			permitService
				.Setup(m => m.ConfirmPermitTransactions(It.IsAny<IEnumerable<IPermitWithdrawalRequestDetail>>()))
				.Returns(true);

			return permitService;
		}

		WhsReceive CreateCustomsReceiveWithInventory(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, ZDateTimeOffset finalisedDate, decimal valueOfDuty, string entryKey = "A",
			string countryOfOrigin = "AU", string zoneStatus = "", bool isFromOtherUSFTZs = false,
			string outwardType = "")
		{
			var receive = CreateCustomReceive(org, warehouse, docketNumber);
			CreateCustomsReceiveLine(receive, part, units, valueOfDuty, entryKey, countryOfOrigin, zoneStatus,
				warehouse.DefaultLocationInBondedArea, isFromOtherUSFTZs, outwardType);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			return receive;
		}

		void CreateCustomsReceiveLine(WhsReceive receive, OrgSupplierPart part, decimal units, decimal valueOfDuty,
			string entryKey = "A", string countryOfOrigin = "AU", string zoneStatus = "", WhsLocation location = null,
			bool isFromOtherUSFTZs = false, string outwardType = "")
		{
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, part, units,
				location ?? receive.Warehouse.DefaultLocationInBondedArea);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = entryKey;
			receiveLine.CustomsData.WB_ValueForDuty = valueOfDuty;
			receiveLine.CustomsData.WB_RN_NKCountryOfOrigin = countryOfOrigin;
			receiveLine.CustomsData.WB_ZoneStatus = zoneStatus;
			receiveLine.CustomsData.WB_IsFromAnotherFTZWhs = isFromOtherUSFTZs;
			receiveLine.CustomsData.WB_OutwardType = outwardType;
			receiveLine.CustomsData.WB_BondedWhsQty = units;
			receiveLine.InDocketLine.WE_BondedEntryKey = entryKey;
		}

		void CreateCustomsTransferLine(WhsTransfer transfer, OrgSupplierPart part, decimal units, decimal valueOfDuty,
			string zoneStatus, WhsLocation sourceLocation, WhsLocation destinationLocation)
		{
			var transferLine = Helper.CreateWhsTransferLine(transfer, part, units, sourceLocation, destinationLocation);
			transferLine.CustomsData.WB_EntryLineNo = 1;
			transferLine.CustomsData.WB_EntryKey = "A";
			transferLine.CustomsData.WB_ValueForDuty = valueOfDuty;
			transferLine.CustomsData.WB_ZoneStatus = zoneStatus;
			transferLine.WE_BondedEntryKey = "A";
		}

		void CreateCustomsAdjustmentsWithInventory(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, ZDate finalisedDate, decimal units, decimal valueOfDuty, string entryKey = "A",
			string countryOfOrigin = "AU", string zoneStatus = "", bool isFromOtherUSFTZs = false)
		{
			var adjustment = Helper.CreateWhsAdjustment(org, warehouse, docketNumber);
			adjustment.WD_DocketSubType = OrderType.Codes.Customs;
			CreateCustomsAdjustmentLine(adjustment, part, units, valueOfDuty, zoneStatus, warehouse.DefaultLocation,
				entryKey, countryOfOrigin, isFromOtherUSFTZs);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			adjustment.WD_FinalisedDate = warehouse.GetWarehouseBranchDateTimeOffset(finalisedDate);
			Factory.Save();
		}

		void CreateCustomsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, decimal units,
			decimal valueOfDuty, string zoneStatus, WhsLocation location, string entryKey = "A",
			string countryOfOrigin = "AU", bool isFromOtherUSFTZs = false)
		{
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, part, units, location);
			adjustmentLine.CustomsData.WB_EntryLineNo = 1;
			adjustmentLine.CustomsData.WB_EntryKey = entryKey;
			adjustmentLine.CustomsData.WB_ValueForDuty = valueOfDuty;
			adjustmentLine.CustomsData.WB_ZoneStatus = zoneStatus;
			adjustmentLine.CustomsData.WB_BondedWhsQty = units;
			adjustmentLine.CustomsData.WB_IsFromAnotherFTZWhs = isFromOtherUSFTZs;
			adjustmentLine.CustomsData.WB_RN_NKCountryOfOrigin = countryOfOrigin;
			adjustmentLine.WE_BondedEntryKey = entryKey;
		}

		void CreateCustomsOrderWithOrderLine(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, decimal valueForDuty, ZDate finalisedDate, decimal bondedQty,
			string entryKey = "A", string outwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN)
		{
			var order = Helper.CreateWhsOrder(org, warehouse, docketNumber);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			CreateCustomsOrderLine(order, part, units, valueForDuty, bondedQty, entryKey, outwardType);
			Factory.Save();
			CreatePickAndFinalise(order, finalisedDate);
		}

		WhsOrderLine CreateCustomsOrderLine(WhsOrder order, OrgSupplierPart part, decimal units, decimal valueForDuty,
			decimal bondedQty, string entryKey = "A",
			string outwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN, string countryOfOrigin = "AU")
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = entryKey;
			orderLine.CustomsData.WB_ValueForDuty = valueForDuty;
			orderLine.CustomsData.WB_OutwardType = outwardType;
			orderLine.CustomsData.WB_BondedWhsQty = bondedQty;
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = countryOfOrigin;
			orderLine.WE_BondedEntryKey = entryKey; // used to match to inventory entry key
			return orderLine;
		}

		static void AssertReceivedAndOrderedMerchandise(DynamicBusinessObject result, string expectedCategory,
			decimal value, string countryOfOrigin)
		{
			AssertEquals(expectedCategory, result[ColumnNames.Category]);
			AssertEquals(value, result[ColumnNames.Value]);
			AssertEquals(countryOfOrigin, result[ColumnNames.CountryOfOrigin]);
		}

		void AssertFTZAnnualReportValue(WhsWarehouse whs, ZInt year, ZString column, int expectedValue)
		{
			AssertFTZAnnualReportValueCore(whs, year, column, (ZInt)expectedValue);
		}

		void AssertFTZAnnualReportValue(WhsWarehouse whs, ZInt year, ZString column, ZDecimal expectedValue)
		{
			AssertFTZAnnualReportValueCore(whs, year, column, expectedValue);
		}

		void AssertFTZAnnualReportValueCore(WhsWarehouse whs, ZInt year, ZString column, IZType expectedValue)
		{
			var from = new ZDate(year, 01, 01);
			var to = new ZDate(year, 12, 31);
			var result = Load_Report_FTZAnnualReport(whs.PK, from, to);
			AssertEquals("Should always return 1 result.", 1, result.Count);
			AssertEquals($"Expected {expectedValue} to be returned for {column} in {whs.WW_WarehouseName} for {year}.",
				expectedValue, result[0][column]);
		}

		static void SetCommodity(OrgSupplierPart part, string commodityCode)
		{
			part.OP_RH_NKCommodityCode = commodityCode;
		}

		void SetupFTZWarehouse(WhsWarehouse warehouse)
		{
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
		}

		DynamicBusinessObjectCollection Load_Report_FTZAnnualReport(ZGuid warehousePK, ZDate fromDate, ZDate toDate)
		{
			var sql =
				$@"select * from FTZReport('{warehousePK}', '{fromDate.ToString("yyyy - MM - dd")}', '{toDate.ToString("yyyy-MM-dd")}')";

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		DynamicBusinessObjectCollection Load_Report_ReceivedMerchandise(ZGuid warehousePK, ZDate fromDate, ZDate toDate)
		{
			var sql =
				$@"select * from Top5FTZMerchandiseReport('{warehousePK}', '{fromDate.ToString("yyyy - MM - dd")}', '{toDate.ToString("yyyy-MM-dd")}', 'I')";

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		DynamicBusinessObjectCollection Load_Report_OrderedMerchandise(ZGuid warehousePK, ZDate fromDate, ZDate toDate)
		{
			var sql =
				$@"select * from Top5FTZMerchandiseReport('{warehousePK}', '{fromDate.ToString("yyyy - MM - dd")}', '{toDate.ToString("yyyy-MM-dd")}', 'O')";

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		#endregion
	}
}
