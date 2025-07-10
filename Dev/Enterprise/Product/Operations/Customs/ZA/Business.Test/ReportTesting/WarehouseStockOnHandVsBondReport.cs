using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	abstract class WarehouseStockOnHandVsBondReportTest : ReportFunctionalTestCase
	{
		protected override List<string> ParametersValuesList => new List<string>()
		{
			QuoteParameter(GlbCompany.CurrentCompany.PK), // @CompanyPK
			QuoteParameter(whsWarehouse.PK), // @WarehousePK
			QuoteParameter(whsWarehouse.WW_OA_WarehouseAddress), // @WarehouseAddressPK
			QuoteParameter(whsHelper.Importer.PK) // @ClientPK
		};

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		ZAWhsDataTestHelper whsHelper;
		protected ZAWhsInventoryDutyAndTaxCalculatorTestHelper dutyHelper;
		IWhsWarehouse whsWarehouse;

		protected override void SetUp()
		{
			dutyHelper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			whsHelper = new ZAWhsDataTestHelper(Factory);
			whsHelper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			whsWarehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "WZA");

			var whsReceiveArrivalDateOlderThanStartDate = whsHelper.GetNewWhsReceive(whsWarehouse.PK, whsHelper.Importer.PK, "OLDJOB", dutyHelper.StartDate.ToOffset());
			var whsReceiveLineOld = whsHelper.GetNewWhsReceiveLine(
				whsReceiveArrivalDateOlderThanStartDate.PK,
				whsHelper.Part.PK,
				"PACKAGEOLD",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00124-1");
			var whsBondedWarehouseAttributeOld = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLineOld.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China,
				600m, "NO", "", "EN00124", 1);
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
				300m,
				300m,
				bondedEntryKey: "EN00123-1");
			var whsBondedWarehouseAttribute1 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000.0049m, 300m, "LI", Core.Constants.CountryCodes.China,
				200m, "NO", "", "EN00123", 1);
			whsBondedWarehouseAttribute1.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute1.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute1.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute1.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute1.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute1.WB_CustomsThirdUnitQty = "NO";
			whsBondedWarehouseAttribute1.WB_MatchingKey = "OwnerReference10001";

			var whsReceiveLine2 = whsHelper.GetNewWhsReceiveLine(
				whsReceive1.PK,
				whsHelper.Part2.PK,
				"PACKAGE2",
				1m,
				400m,
				400m,
				bondedEntryKey: "EN00123-2");
			var whsBondedWarehouseAttribute2 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1000.0049m, 400m, "LI", Core.Constants.CountryCodes.China,
				200m, "NO", "", "EN00123", 4);
			whsBondedWarehouseAttribute2.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute2.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute2.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute2.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute2.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute2.WB_CustomsThirdUnitQty = "NO";

			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();

			var batch1 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch1.WOB_Batch = "Batch1";
			batch1.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch1.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order1 = WarehouseStockOnHandVsBondHelper.CreateOrder(Factory, whsHelper, batch1, arrivalDate, 1, WarehouseOperatorTransactionExportTypeList.Codes.EXP, 100m, 1000m);
			var order2 = WarehouseStockOnHandVsBondHelper.CreateOrder(Factory, whsHelper, batch1, arrivalDate, 2, string.Empty, 100m, 1000m);
			var receipt1 = WarehouseStockOnHandVsBondHelper.CreateReceipt(Factory, whsHelper, batch1, arrivalDate, 3, 300m, 3000m, Core.Constants.CountryCodes.SouthAfrica, isCustomsControlled: true);
			var receipt2 = WarehouseStockOnHandVsBondHelper.CreateReceipt(Factory, whsHelper, batch1, arrivalDate.AddDays(1), 4, 200m, 2000m, Core.Constants.CountryCodes.SouthAfrica, isCustomsControlled: true);
			WarehouseStockOnHandVsBondHelper.CreateTransactionLine(Factory, order1, receipt1, 100m);
			WarehouseStockOnHandVsBondHelper.CreateTransactionLine(Factory, order2, receipt1, 50m);

			// Local and Duty Paid Stock
			var batchWithoutCustomsControl = Factory.New<CusWHSOperatorTransactionBatch>();
			batchWithoutCustomsControl.WOB_Batch = "BatchWithoutCustomsControl";
			batchWithoutCustomsControl.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batchWithoutCustomsControl.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var receiptLocalStock = WarehouseStockOnHandVsBondHelper.CreateReceipt(Factory, whsHelper, batchWithoutCustomsControl, arrivalDate, 1, 111m, 1111m, Core.Constants.CountryCodes.SouthAfrica, isCustomsControlled: false);
			var receiptDutyPaidStock = WarehouseStockOnHandVsBondHelper.CreateReceipt(Factory, whsHelper, batchWithoutCustomsControl, arrivalDate, 2, 222m, 2222m, Core.Constants.CountryCodes.China, isCustomsControlled: false);

			// Used Stock
			var batchUsed = Factory.New<CusWHSOperatorTransactionBatch>();
			batchUsed.WOB_Batch = "BatchUsed";
			batchUsed.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batchUsed.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var orderUsed = WarehouseStockOnHandVsBondHelper.CreateOrder(Factory, whsHelper, batchUsed, arrivalDate, 1, string.Empty, 30m, 300m);
			WarehouseStockOnHandVsBondHelper.CreateTransactionLine(Factory, orderUsed, receiptLocalStock, 10m);
			WarehouseStockOnHandVsBondHelper.CreateTransactionLine(Factory, orderUsed, receiptDutyPaidStock, 20m);

			Factory.Save();
		}

		protected override void PrepareTestData()
		{
			dutyHelper.SetupTestData(Factory);
			Factory.Save();
		}
	}

	sealed class WarehouseStockOnHandVsBondReportWithDutyVAT : WarehouseStockOnHandVsBondReportTest
	{
		protected override ZString ObjectName => "WarehouseStockOnHandVsBondReportWithDutyVAT";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn>()
		{
			new ReportSchemaColumn(typeof(string), "ProductCode"),
			new ReportSchemaColumn(typeof(Guid), "ProductPK"),
			new ReportSchemaColumn(typeof(decimal), "BondedStock"),
			new ReportSchemaColumn(typeof(string), "WarehouseCountry"),
			new ReportSchemaColumn(typeof(Guid), "PK"),
			new ReportSchemaColumn(typeof(string), "CustomsTariffCode"),
			new ReportSchemaColumn(typeof(string), "CountryOfOrigin"),
			new ReportSchemaColumn(typeof(decimal), "OriginalCustomsValue"),
			new ReportSchemaColumn(typeof(decimal), "CustomsQty"),
			new ReportSchemaColumn(typeof(string), "CustomsUnitOfQty"),
			new ReportSchemaColumn(typeof(decimal), "CustomsSecondQuantity"),
			new ReportSchemaColumn(typeof(string), "CustomsSecondUnitQty"),
			new ReportSchemaColumn(typeof(decimal), "CustomsThirdQuantity"),
			new ReportSchemaColumn(typeof(string), "CustomsThirdUnitQty"),
			new ReportSchemaColumn(typeof(decimal), "Ratio"),
			new ReportSchemaColumn(typeof(DateTimeOffset), "ArrivalDate"),
			new ReportSchemaColumn(typeof(long), "RowRank"),
			new ReportSchemaColumn(typeof(decimal), "StockOnHand"),
			new ReportSchemaColumn(typeof(decimal), "WorkInProgressQuantity"),
			new ReportSchemaColumn(typeof(decimal), "DutyPaidStock"),
			new ReportSchemaColumn(typeof(decimal), "LocalStock"),
			new ReportSchemaColumn(typeof(decimal), "InTransitStock")
		};

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, ignoreGuid: true);
				var row2 = FormatRowsValues(results.Rows[1], results, ignoreGuid: true);
				var row3 = FormatRowsValues(results.Rows[2], results, ignoreGuid: true);

				AssertMultilineASCIIEquals("Row1", $"[ProductCode]='~~1'; [BondedStock]='300.000'; [WarehouseCountry]='ZA'; [CustomsTariffCode]='1111111'; [CountryOfOrigin]='CN'; [OriginalCustomsValue]='1000.0049'; [CustomsQty]='300.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='1.00000000000000000000'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [RowRank]='1'; [StockOnHand]='603.00000'; [WorkInProgressQuantity]='50.00000'; [DutyPaidStock]='202.00000'; [LocalStock]='101.00000'; [InTransitStock]='467.000'", row1);
				AssertMultilineASCIIEquals("Row2", $"[ProductCode]='~~1'; [BondedStock]='1000.000'; [WarehouseCountry]='ZA'; [CustomsTariffCode]='1111111'; [CountryOfOrigin]='CN'; [OriginalCustomsValue]='1000.0049'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='1.00000000000000000000'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate)}'; [RowRank]='2'; [StockOnHand]=''; [WorkInProgressQuantity]=''; [DutyPaidStock]=''; [LocalStock]=''; [InTransitStock]=''", row2);
				AssertMultilineASCIIEquals("Row3", $"[ProductCode]='~~2'; [BondedStock]='400.000'; [WarehouseCountry]='ZA'; [CustomsTariffCode]='1111111'; [CountryOfOrigin]='CN'; [OriginalCustomsValue]='1000.0049'; [CustomsQty]='400.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='1.00000000000000000000'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [RowRank]='1'; [StockOnHand]=''; [WorkInProgressQuantity]=''; [DutyPaidStock]=''; [LocalStock]=''; [InTransitStock]='400.000'", row3);
			});
		}

		string FormatDateTime(ZDateTime dateTime) => dateTime.ToOffset().ToString("d/MM/yyyy hh:mm:ss tt zzz");
	}

	sealed class WarehouseStockOnHandVsBondReportWithoutDutyVAT : WarehouseStockOnHandVsBondReportTest
	{
		protected override ZString ObjectName => "WarehouseStockOnHandVsBondReportWithoutDutyVAT";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn>()
		{
			new ReportSchemaColumn(typeof(string), "ProductCode"),
			new ReportSchemaColumn(typeof(Guid), "ProductPK"),
			new ReportSchemaColumn(typeof(decimal), "BondedStock"),
			new ReportSchemaColumn(typeof(decimal), "StockOnHand"),
			new ReportSchemaColumn(typeof(decimal), "WorkInProgressQuantity"),
			new ReportSchemaColumn(typeof(decimal), "DutyPaidStock"),
			new ReportSchemaColumn(typeof(decimal), "LocalStock"),
			new ReportSchemaColumn(typeof(decimal), "InTransitStock")
		};

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(2, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, ignoreGuid: true);
				var row2 = FormatRowsValues(results.Rows[1], results, ignoreGuid: true);

				AssertMultilineASCIIEquals("Row1", "[ProductCode]='~~1'; [BondedStock]='1300.000'; [StockOnHand]='603.00000'; [WorkInProgressQuantity]='50.00000'; [DutyPaidStock]='202.00000'; [LocalStock]='101.00000'; [InTransitStock]='467.000'", row1);
				AssertMultilineASCIIEquals("Row2", "[ProductCode]='~~2'; [BondedStock]='400.000'; [StockOnHand]='0.00000'; [WorkInProgressQuantity]='0.00000'; [DutyPaidStock]='0.00000'; [LocalStock]='0.00000'; [InTransitStock]='400.000'", row2);
			});
		}
	}
}
