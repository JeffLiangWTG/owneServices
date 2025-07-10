using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class BondedWarehouseExposureReport : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "WarehouseCountry", "Warehouse", "ImporterName", "ImporterCode", "ProductCode", "CountryOfOrigin", "DeclarationReference", "EntryKey",
															"CustomsTariffCode", "CustomsUnitOfQty", "CustomsSecondUnitQty", "CustomsThirdUnitQty", "DocketID" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "ArrivalDate" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTimeOffset), dateCol));
				}

				foreach (var decimalCol in new string[] { "QuantityBalance", "OriginalQuantity", "OriginalCustomsValue", "CustomsValueBalance", "CustomsQty", "CustomsSecondQuantity", "CustomsThirdQuantity", "Ratio" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				foreach (var shortCol in new string[] { "LineNumber", "SubLineNumber", "EntryLineNo" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(short), shortCol));
				}
				foreach (var guidCol in new string[] { "PK", "WarehousePK", "ImporterPK", "ProductPK" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(Guid), guidCol));
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
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsHelper.Importer.PK), // @ClientPK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsWarehouse.PK), 	// @WarehousePK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", ZDate.Today.AddDays(1)) 	// @EntriesUpToDate
				};
			}
		}

		ZAWhsDataTestHelper whsHelper;
		ZAWhsInventoryDutyAndTaxCalculatorTestHelper dutyHelper;
		IWhsWarehouse whsWarehouse;
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

			var whsReceive3 = whsHelper.GetNewWhsReceive(whsWarehouse.PK, whsHelper.Importer.PK, "WA0000183", arrivalDate.ToOffset());
			var whsReceiveLine6 = whsHelper.GetNewWhsReceiveLine(
				whsReceive3.PK,
				whsHelper.Part.PK,
				"PACKAGE6",
				1m,
				1000m,
				1000m,
				bondedEntryKey: "EN00126-1");
			whsReceiveLine6.WE_LineNo = 1;
			whsReceiveLine6.WE_SubLineNo = 1;
			var whsBondedWarehouseAttribute6 = whsHelper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 1000.0049m, 1000m, "LI", Core.Constants.CountryCodes.China, 600m, "NO", "", "EN00123", 1);
			whsBondedWarehouseAttribute6.WB_PrimaryPreference = "200";
			whsBondedWarehouseAttribute6.WB_Tariff = dutyHelper.TariffCode;
			whsBondedWarehouseAttribute6.WB_CustomsSecondQuantity = 2000m;
			whsBondedWarehouseAttribute6.WB_CustomsSecondUnitQty = "KG";
			whsBondedWarehouseAttribute6.WB_CustomsThirdQuantity = 3000m;
			whsBondedWarehouseAttribute6.WB_CustomsThirdUnitQty = "NO";
			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive3.PK);
			whsReceive3.FinaliseDocketWithoutUserConfirmation();
			whsReceive3.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(10);

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
			get { return "BondedWarehouseExposureReport"; }
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(5, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, true);
				var row2 = FormatRowsValues(results.Rows[1], results, true);
				var row3 = FormatRowsValues(results.Rows[2], results, true);
				var row4 = FormatRowsValues(results.Rows[3], results, true);
				var row5 = FormatRowsValues(results.Rows[4], results, true);

				AssertMultilineASCIIEquals("Row1", $"[WarehouseCountry]='ZA'; [Warehouse]='WZA'; [ImporterName]='TestImp'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='600.000'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [DeclarationReference]=''; [EntryKey]='EN00123'; [EntryLineNo]='1'; [CustomsTariffCode]='1111111'; [OriginalCustomsValue]='1000.0049'; [CustomsValueBalance]='600.002940'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000002'; [LineNumber]='1'; [SubLineNumber]='4'", row1);
				AssertMultilineASCIIEquals("Row2", $"[WarehouseCountry]='ZA'; [Warehouse]='WZA'; [ImporterName]='TestImp'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='600.000'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [DeclarationReference]=''; [EntryKey]='EN00123'; [EntryLineNo]='2'; [CustomsTariffCode]='91111111'; [OriginalCustomsValue]='1000.0049'; [CustomsValueBalance]='600.002940'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000002'; [LineNumber]='2'; [SubLineNumber]='1'", row2);
				AssertMultilineASCIIEquals("Row3", $"[WarehouseCountry]='ZA'; [Warehouse]='WZA'; [ImporterName]='TestImp'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(-2))}'; [QuantityBalance]='600.000'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [DeclarationReference]=''; [EntryKey]='EN00124'; [EntryLineNo]='1'; [CustomsTariffCode]='1111111'; [OriginalCustomsValue]='1000.0049'; [CustomsValueBalance]='600.002940'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000001'; [LineNumber]='1'; [SubLineNumber]='0'", row3);
				AssertMultilineASCIIEquals("Row4", $"[WarehouseCountry]='ZA'; [Warehouse]='WZA'; [ImporterName]='TestImp'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='400.000'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [DeclarationReference]=''; [EntryKey]='EN00125'; [EntryLineNo]='1'; [CustomsTariffCode]='1111111'; [OriginalCustomsValue]='1000.0049'; [CustomsValueBalance]='400.001960'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.40000000000000000000'; [DocketID]='W00000003'; [LineNumber]='1'; [SubLineNumber]='1'", row4);
				AssertMultilineASCIIEquals("Row5", $"[WarehouseCountry]='ZA'; [Warehouse]='WZA'; [ImporterName]='TestImp'; [ImporterCode]='IMP'; [ProductCode]='~~2'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='500.000'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [DeclarationReference]=''; [EntryKey]='EN00123'; [EntryLineNo]='4'; [CustomsTariffCode]='1111111'; [OriginalCustomsValue]='1000.0049'; [CustomsValueBalance]='500.002450'; [CustomsQty]='1000.00000'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.50000000000000000000'; [DocketID]='W00000002'; [LineNumber]='4'; [SubLineNumber]='0'", row5);
			});
		}

		string FormatDateTime(ZDateTime dateTime) => dateTime.ToOffset().ToString("d/MM/yyyy hh:mm:ss tt zzz");
	}
}
