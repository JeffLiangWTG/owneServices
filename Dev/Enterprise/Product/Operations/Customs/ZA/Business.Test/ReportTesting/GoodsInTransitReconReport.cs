using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class GoodsInTransitReconReport : ReportFunctionalTestCase
	{
		public void TestReportWithBlankInwardsEntryNo()
		{
			PrepareTestData();
			inwardsEntryNo = ZString.Empty;

			var data = FireReportAndReturnDataTable();
			AssertUnfilteredPartTestResults(data);
		}

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "InwardsEntryNumber", "ProductCode", "Client", "ClientCode", "ProductDesc", "WarehouseCode", "WarehouseName", "WOT_CustomsEntryNumber" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var decimalCol in new string[] { "BondTotal", "WHSOperatorTotal", "WIT" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				return allCols;
			}
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>()
				{
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsWarehouse.WW_OA_WarehouseAddress), // @WarehouseAddressPK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsWarehouse.PK), // @WarehousePK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsHelper.Importer.PK), // @ClientPK
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL"), 	// @ProductPK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", inwardsEntryNo) // @InwardsEntryNo
				};
			}
		}

		ZAWhsDataTestHelper whsHelper;
		ZAWhsInventoryDutyAndTaxCalculatorTestHelper dutyHelper;
		IWhsWarehouse whsWarehouse;
		ZString inwardsEntryNo = "EN00123";

		protected override void SetUp()
		{
			base.SetUp();
			dutyHelper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			whsHelper = new ZAWhsDataTestHelper(Factory);
			whsWarehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, true, "WZA");
			var whsReceiveArrivalDateOlderThanStartDate = whsHelper.GetNewWhsReceive(whsWarehouse.PK, whsHelper.Importer.PK, "OLDJOB", dutyHelper.StartDate.ToOffset());
			var whsReceiveLineOld = whsHelper.GetNewWhsReceiveLine(
				whsReceiveArrivalDateOlderThanStartDate.PK,
				whsHelper.Part.PK,
				"PACKAGEOLD",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00124-1");
			var whsBondedWarehouseAttributeOld = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLineOld.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 600m, "NO", "", "EN00124", 1);
			whsBondedWarehouseAttributeOld.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttributeOld.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttributeOld.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttributeOld.WB_CustomsThirdUnitQty = "NO";
			whsBondedWarehouseAttributeOld.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttributeOld.WB_Tariff = dutyHelper.TariffCode;
			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceiveArrivalDateOlderThanStartDate.PK);
			whsReceiveArrivalDateOlderThanStartDate.FinaliseDocketWithoutUserConfirmation();

			var arrivalDate = dutyHelper.StartDate.AddDays(30);

			var whsReceive1 = whsHelper.GetNewWhsReceive(whsWarehouse.PK, whsHelper.Importer.PK, "WA0000182", arrivalDate.ToOffset());
			var whsReceiveLine1 = whsHelper.GetNewWhsReceiveLine(
				whsReceive1.PK,
				whsHelper.Part.PK,
				"PACKAGE1",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00123-1");
			whsReceiveLine1.WE_LineNo = 2;
			whsReceiveLine1.WE_SubLineNo = 2;
			var whsBondedWarehouseAttribute1 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 600m, "NO", "", "EN00123", 1);
			whsBondedWarehouseAttribute1.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute1.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute1.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute1.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute1.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute1.WB_CustomsThirdUnitQty = "NO";

			var whsReceiveLine2 = whsHelper.GetNewWhsReceiveLine(
				whsReceive1.PK,
				whsHelper.Part.PK,
				"PACKAGE2",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00123-2");
			whsReceiveLine2.WE_LineNo = 2;
			whsReceiveLine2.WE_SubLineNo = 1;
			var whsBondedWarehouseAttribute2 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 600m, "NO", "", "EN00123", 2);
			whsBondedWarehouseAttribute2.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute2.WB_Tariff = "9" + dutyHelper.TariffCode; // Unknown tariff
			whsBondedWarehouseAttribute2.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute2.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute2.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute2.WB_CustomsThirdUnitQty = "NO";

			var whsReceiveLine3 = whsHelper.GetNewWhsReceiveLine(
				whsReceive1.PK,
				whsHelper.Part2.PK,
				"PACKAGE3",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00123-3");
			whsReceiveLine3.WE_LineNo = 3;
			whsReceiveLine3.WE_SubLineNo = 1;
			var whsBondedWarehouseAttribute3 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 0m, "NO", "", "EN00123", 3);
			whsBondedWarehouseAttribute3.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute3.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute3.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute3.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute3.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute3.WB_CustomsThirdUnitQty = "NO";

			var whsReceiveLine4 = whsHelper.GetNewWhsReceiveLine(
				whsReceive1.PK,
				whsHelper.Part2.PK,
				"PACKAGE4",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00123-4");
			whsReceiveLine1.WE_LineNo = 1;
			whsReceiveLine1.WE_SubLineNo = 4;
			var whsBondedWarehouseAttribute4 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 500m, "NO", "", "EN00123", 4);
			whsBondedWarehouseAttribute4.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute4.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute4.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute4.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute4.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute4.WB_CustomsThirdUnitQty = "NO";

			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();

			var whsReceive2 = whsHelper.GetNewWhsReceive(whsWarehouse.PK, whsHelper.Importer.PK, "WA0000181", ZDateTimeOffset.Today.AddDays(-1));
			var whsReceiveLine5 = whsHelper.GetNewWhsReceiveLine(
				whsReceive2.PK,
				whsHelper.Part.PK,
				"PACKAGE5",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00125-1");
			whsReceiveLine5.WE_LineNo = 1;
			whsReceiveLine5.WE_SubLineNo = 1;
			var whsBondedWarehouseAttribute5 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 400m, "NO", "", "EN00125", 1);
			whsBondedWarehouseAttribute5.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute5.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute5.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute5.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute5.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute5.WB_CustomsThirdUnitQty = "NO";

			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			whsReceiveLineOld.WE_AdjustmentArrivalDate = dutyHelper.StartDate.ToOffset().AddDays(-2);
			whsReceiveLine5.WE_AdjustmentArrivalDate = arrivalDate.ToOffset();
			Factory.Save();

			var adjustment = whsHelper.WhsHelper.CreateWhsAdjustment(whsHelper.Importer.PK, whsWarehouse.PK, "AD1", null);
			adjustment[WhsDocketSchema.WD_DocketSubType] = "CUS";
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part.PK, -400m, "RR1", "", "", "", "EN00124-1", 1m, "PACKAGEOLD");
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part.PK, -400m, "RR1", "", "", "", "EN00123-1", 1m, "PACKAGE1");
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part.PK, -400m, "RR1", "", "", "", "EN00123-2", 1m, "PACKAGE2");
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part2.PK, -1000m, "RR1", "", "", "", "EN00123-3", 1m, "PACKAGE3");
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part2.PK, -500m, "RR1", "", "", "", "EN00123-4", 1m, "PACKAGE4");
			whsHelper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, whsHelper.Part.PK, -600m, "RR1", "", "", "", "EN00125-1", 1m, "PACKAGE5");
			whsHelper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);

			CreateCusWhsData();

			Factory.Save();
		}

		void CreateCusWhsData()
		{
			var arrivalDate = dutyHelper.StartDate.AddDays(30);

			var batch1 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch1.WOB_Batch = "Batch1";
			batch1.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch1.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order1 = Factory.New<CusWHSOperatorTransaction>();
			order1.WOT_BatchLineNo = 1;
			order1.WOT_ExportType = WarehouseOperatorTransactionExportTypeList.Codes.EXP;
			order1.WOT_IsCustomsControlled = false;
			order1.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			order1.WOT_OP_Product = whsHelper.Part.PK;
			order1.WOT_OwnerReference = "OwnerReference";
			order1.WOT_Quantity = 100m;
			order1.WOT_RN_NKOrigin = ZString.Empty;
			order1.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order1.WOT_TotalValue = 1000m;
			order1.WOT_TransactionDate = arrivalDate.Date;
			order1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;

			var order2 = Factory.New<CusWHSOperatorTransaction>();
			order2.WOT_BatchLineNo = 2;
			order2.WOT_ExportType = ZString.Empty;
			order2.WOT_IsCustomsControlled = false;
			order2.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			order2.WOT_OP_Product = whsHelper.Part.PK;
			order2.WOT_OwnerReference = "OwnerReference";
			order2.WOT_Quantity = 100m;
			order2.WOT_RN_NKOrigin = ZString.Empty;
			order2.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order2.WOT_TotalValue = 1000m;
			order2.WOT_TransactionDate = arrivalDate.Date;
			order2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order2.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			order2.WOT_CustomsEntryNumber = "EN00123";

			var receipt1 = Factory.New<CusWHSOperatorTransaction>();
			receipt1.WOT_BatchLineNo = 3;
			receipt1.WOT_ExportType = ZString.Empty;
			receipt1.WOT_IsCustomsControlled = true;
			receipt1.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt1.WOT_OP_Product = whsHelper.Part.PK;
			receipt1.WOT_OwnerReference = "OwnerReference";
			receipt1.WOT_Quantity = 500m;
			receipt1.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt1.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt1.WOT_TotalValue = 5000m;
			receipt1.WOT_TransactionDate = arrivalDate.Date;
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			receipt1.WOT_CustomsEntryNumber = "EN00123";

			var batch2 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch2.WOB_Batch = "Batch2";
			batch2.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch2.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			// Local Stock
			var receipt2 = Factory.New<CusWHSOperatorTransaction>();
			receipt2.WOT_BatchLineNo = 4;
			receipt2.WOT_ExportType = ZString.Empty;
			receipt2.WOT_IsCustomsControlled = false;
			receipt2.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt2.WOT_OP_Product = whsHelper.Part.PK;
			receipt2.WOT_OwnerReference = "OwnerReference";
			receipt2.WOT_Quantity = 111m;
			receipt2.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt2.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt2.WOT_TotalValue = 1111m;
			receipt2.WOT_TransactionDate = arrivalDate.Date;
			receipt2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt2.WOT_WOB_CusWHSTransactionBatch = batch2.PK;
			receipt2.WOT_CustomsEntryNumber = "EN00123";

			var receipt4 = Factory.New<CusWHSOperatorTransaction>();
			receipt4.WOT_BatchLineNo = 5;
			receipt4.WOT_ExportType = ZString.Empty;
			receipt4.WOT_IsCustomsControlled = false;
			receipt4.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt4.WOT_OP_Product = whsHelper.Part2.PK;
			receipt4.WOT_OwnerReference = "OwnerReference";
			receipt4.WOT_Quantity = 500m;
			receipt4.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt4.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt4.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt4.WOT_TotalValue = 5000m;
			receipt4.WOT_TransactionDate = arrivalDate.Date;
			receipt4.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt4.WOT_WOB_CusWHSTransactionBatch = batch2.PK;
			receipt4.WOT_CustomsEntryNumber = "EN00123";

			// Duty Paid Stock
			var receipt3 = Factory.New<CusWHSOperatorTransaction>();
			receipt3.WOT_BatchLineNo = 6;
			receipt3.WOT_ExportType = ZString.Empty;
			receipt3.WOT_IsCustomsControlled = false;
			receipt3.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt3.WOT_OP_Product = whsHelper.Part.PK;
			receipt3.WOT_OwnerReference = "OwnerReference";
			receipt3.WOT_Quantity = 222m;
			receipt3.WOT_RN_NKOrigin = Core.Constants.CountryCodes.China;
			receipt3.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt3.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt3.WOT_TotalValue = 2222m;
			receipt3.WOT_TransactionDate = arrivalDate.Date;
			receipt3.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt3.WOT_WOB_CusWHSTransactionBatch = batch2.PK;

			var orderTransactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine1.WOL_Quantity = 100m;
			orderTransactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order1.PK;
			orderTransactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var orderTransactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine2.WOL_Quantity = 50m;
			orderTransactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			orderTransactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var entry = whsHelper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", whsHelper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", whsHelper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 25m);
			entry.InvoiceLines.First().JI_ParentID = orderTransactionLine2.PK;

			var orderTransactionLine3 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine3.WOL_Quantity = 25m;
			orderTransactionLine3.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			orderTransactionLine3.WOL_WOT_WHSOperatorTransactionReceipt = receipt2.PK;
			var entry2 = whsHelper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001234", whsHelper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT1435", whsHelper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 10m);
			entry2.InvoiceLines.First().JI_ParentID = orderTransactionLine3.PK;

			Factory.Save();
		}

		protected override void PrepareTestData()
		{
			dutyHelper.SetupTestData(Factory);
			Factory.Save();
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "WhsGoodsInTransitReconReport"; }
		}

		void AssertUnfilteredPartTestResults(System.Data.DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, results.Rows.Count);

				var row1 = FormatRowsValues(results.Rows[0], results, true);
				AssertMultilineASCIIEquals("Row1", $"[WarehouseCode]='WZA'; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [Client]='TestImp'; [InwardsEntryNumber]='EN00123'; [BondTotal]='1200.000'; [WOT_CustomsEntryNumber]='EN00123'; [ProductCode]='~~1'; [ProductDesc]='~~1 DESC'; [WHSOperatorTotal]='611.00000'; [WIT]='589.000'", row1);

				var row2 = FormatRowsValues(results.Rows[1], results, true);
				AssertMultilineASCIIEquals("Row2", $"[WarehouseCode]='WZA'; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [Client]='TestImp'; [InwardsEntryNumber]='EN00124'; [BondTotal]='600.000'; [WOT_CustomsEntryNumber]=''; [ProductCode]='~~1'; [ProductDesc]='~~1 DESC'; [WHSOperatorTotal]='0.00000'; [WIT]='600.000'", row2);

				var row3 = FormatRowsValues(results.Rows[2], results, true);
				AssertMultilineASCIIEquals("Row3", $"[WarehouseCode]='WZA'; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [Client]='TestImp'; [InwardsEntryNumber]='EN00125'; [BondTotal]='400.000'; [WOT_CustomsEntryNumber]=''; [ProductCode]='~~1'; [ProductDesc]='~~1 DESC'; [WHSOperatorTotal]='0.00000'; [WIT]='400.000'", row3);
			});
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Single row, no WIT=0", 1, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, true);

				AssertMultilineASCIIEquals("Row1", $"[WarehouseCode]='WZA'; [WarehouseName]='WZA NAME'; [ClientCode]='IMP'; [Client]='TestImp'; [InwardsEntryNumber]='EN00123'; [BondTotal]='1200.000'; [WOT_CustomsEntryNumber]='EN00123'; [ProductCode]='~~1'; [ProductDesc]='~~1 DESC'; [WHSOperatorTotal]='611.00000'; [WIT]='589.000'", row1);
			});
		}
	}
}
