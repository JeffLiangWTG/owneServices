using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ProductValuationReport : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "WarehouseCountry", "Warehouse", "ImporterName", "ImporterCode", "ProductCode", "CountryOfOrigin",
					"CustomsTariffCode", "CustomsUnitOfQty", "CustomsSecondUnitQty", "CustomsThirdUnitQty", "DocketID", "WhsCustomsCode" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "ArrivalDate" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTimeOffset), dateCol));
				}

				foreach (var decimalCol in new string[] { "QuantityBalance", "OriginalQuantity", "OriginalCustomsValue", "CustomsValueBalance", "CustomsQty",
					"CustomsSecondQuantity", "CustomsThirdQuantity", "Ratio", "AllDuties", "VAT", "CurrentLiability" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				foreach (var shortCol in new string[] { "LineNumber", "SubLineNumber" })
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
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsHelper.Part.PK), 	// @ProductPK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsHelper.Importer.PK), // @ClientPK
					string.Format(CultureInfo.InvariantCulture, "'{0}'", whsWarehouse.PK) // @WarehousePK
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

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CustomsRegNo = "XXBOS 05901";
			cusCode.OK_OH = whsHelper.Importer.PK;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			whsHelper.Importer.CustomsCodes.Add(cusCode);
			Factory.Save();

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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttributeOld);
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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttribute1);

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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttribute2, "9" + dutyHelper.TariffCode); // Unknown tariff

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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttribute3);

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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttribute4);

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
			SetDefaultBondedWarehouseAttributeProperties(whsBondedWarehouseAttribute5);

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
			Factory.Save();
		}

		void SetDefaultBondedWarehouseAttributeProperties(IWhsBondedWarehouseAttribute warehouseAttribute, string tariffCode = null)
		{
			warehouseAttribute.WB_PrimaryPreference = "200";
			warehouseAttribute.WB_Tariff = tariffCode ?? dutyHelper.TariffCode;
			warehouseAttribute.WB_CustomsSecondQuantity = 2000m;
			warehouseAttribute.WB_CustomsSecondUnitQty = "KG";
			warehouseAttribute.WB_CustomsThirdQuantity = 3000m;
			warehouseAttribute.WB_CustomsThirdUnitQty = "NO";
			warehouseAttribute.WB_AllDutiesAmount = 300m;
			warehouseAttribute.WB_VATAmount = 200m;
		}

		protected override void PrepareTestData()
		{
			dutyHelper.SetupTestData(Factory);
			Factory.Save();
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "ProductValuationReport"; }
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, true);
				var row2 = FormatRowsValues(results.Rows[1], results, true);
				var row3 = FormatRowsValues(results.Rows[2], results, true);
				var row4 = FormatRowsValues(results.Rows[3], results, true);

				AssertMultilineASCIIEquals("Row1", $"[Warehouse]='WZA'; [ImporterName]='TestImp'; [WarehouseCountry]='US'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(-2))}'; [QuantityBalance]='600.000'; [OriginalCustomsValue]='1000.0049'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [CustomsQty]='1000.00000'; [CustomsValueBalance]='600.002940'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000001'; [LineNumber]='1'; [CustomsTariffCode]='1111111'; [SubLineNumber]='0'; [WhsCustomsCode]='WZA - XXBOS 05901'; [AllDuties]='180.000000'; [VAT]='120.000000'; [CurrentLiability]='300.000000'", row1);
				AssertMultilineASCIIEquals("Row2", $"[Warehouse]='WZA'; [ImporterName]='TestImp'; [WarehouseCountry]='US'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='600.000'; [OriginalCustomsValue]='1000.0049'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [CustomsQty]='1000.00000'; [CustomsValueBalance]='600.002940'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000002'; [LineNumber]='1'; [CustomsTariffCode]='1111111'; [SubLineNumber]='4'; [WhsCustomsCode]='WZA - XXBOS 05901'; [AllDuties]='180.000000'; [VAT]='120.000000'; [CurrentLiability]='300.000000'", row2);
				AssertMultilineASCIIEquals("Row3", $"[Warehouse]='WZA'; [ImporterName]='TestImp'; [WarehouseCountry]='US'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='400.000'; [OriginalCustomsValue]='1000.0049'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [CustomsQty]='1000.00000'; [CustomsValueBalance]='400.001960'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.40000000000000000000'; [DocketID]='W00000003'; [LineNumber]='1'; [CustomsTariffCode]='1111111'; [SubLineNumber]='1'; [WhsCustomsCode]='WZA - XXBOS 05901'; [AllDuties]='120.000000'; [VAT]='80.000000'; [CurrentLiability]='200.000000'", row3);
				AssertMultilineASCIIEquals("Row4", $"[Warehouse]='WZA'; [ImporterName]='TestImp'; [WarehouseCountry]='US'; [ImporterCode]='IMP'; [ProductCode]='~~1'; [ArrivalDate]='{FormatDateTime(dutyHelper.StartDate.AddDays(30))}'; [QuantityBalance]='600.000'; [OriginalCustomsValue]='1000.0049'; [OriginalQuantity]='1000.000'; [CountryOfOrigin]='CN'; [CustomsQty]='1000.00000'; [CustomsValueBalance]='600.002940'; [CustomsUnitOfQty]='LI'; [CustomsSecondQuantity]='2000.00000'; [CustomsSecondUnitQty]='KG'; [CustomsThirdQuantity]='3000.00000'; [CustomsThirdUnitQty]='NO'; [Ratio]='0.60000000000000000000'; [DocketID]='W00000002'; [LineNumber]='2'; [CustomsTariffCode]='91111111'; [SubLineNumber]='1'; [WhsCustomsCode]='WZA - XXBOS 05901'; [AllDuties]='180.000000'; [VAT]='120.000000'; [CurrentLiability]='300.000000'", row4);
			});
		}

		string FormatDateTime(ZDateTime dateTime) => dateTime.ToOffset().ToString("d/MM/yyyy hh:mm:ss tt zzz");
	}
}
