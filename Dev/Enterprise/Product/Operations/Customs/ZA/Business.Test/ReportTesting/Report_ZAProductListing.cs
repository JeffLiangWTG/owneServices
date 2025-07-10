using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class Report_ZAProductListing : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_ZAProductListing";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "ProductCode", "Description", "Unit", "Tariff", "DutyRate", "Origin", "CustomsOrganisation", "Type", "NewUsed", "EngineCapacity", "VehicleFormat", "VehicleType", "VehicleColour", "RelatedOrganisation", "RelationshipType" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}
				return allCols;
			}
		}

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override void AssertTestResults(DataTable results)
		{
			AssertEquals("Expect 2 CusClassPivot rows joined by pivot and 2 rows directly joined by CI_OH all with CI_RN_NKCountry = ZA", 4, results.Rows.Count);

			var row1 = FormatRowsValues(results.Rows[0], results, true);
			var row2 = FormatRowsValues(results.Rows[1], results, true);
			var row3 = FormatRowsValues(results.Rows[2], results, true);
			var row4 = FormatRowsValues(results.Rows[3], results, true);
			AssertMultilineASCIIEquals("Row1 - linked by pivot, multiple duty rates, should select the latest rate", "[ProductCode]='PART1'; [Description]='PRODUCT1'; [Unit]='1'; [Tariff]='61046290'; [DutyRate]='45%'; [Origin]='ZA'; [CustomsOrganisation]=''; [Type]='HTB'; [NewUsed]='1'; [EngineCapacity]='1'; [VehicleFormat]='F1'; [VehicleType]='T1'; [VehicleColour]='C1'; [RelatedOrganisation]='ZAIMPORTER1'; [RelationshipType]='OWN'", row1);
			AssertMultilineASCIIEquals("Row2 - linked by pivot", "[ProductCode]='PART2'; [Description]='PRODUCT2'; [Unit]='2'; [Tariff]='222333444'; [DutyRate]='45%'; [Origin]='ZA'; [CustomsOrganisation]=''; [Type]='HTB'; [NewUsed]='2'; [EngineCapacity]='2'; [VehicleFormat]='F2'; [VehicleType]='T2'; [VehicleColour]='C2'; [RelatedOrganisation]='ZAIMPORTER1'; [RelationshipType]='OWN'", row2);
			AssertMultilineASCIIEquals("Row3 - linked by CI_OH", "[ProductCode]='PART3'; [Description]='PRODUCT3'; [Unit]='3'; [Tariff]='333444555'; [DutyRate]='45%'; [Origin]='ZA'; [CustomsOrganisation]='ZAIMPORTER1'; [Type]='HTB'; [NewUsed]='3'; [EngineCapacity]='3'; [VehicleFormat]='F3'; [VehicleType]='T3'; [VehicleColour]='C3'; [RelatedOrganisation]=''; [RelationshipType]=''", row3);
			AssertMultilineASCIIEquals("Row4 - linked by CI_OH", "[ProductCode]='PART4'; [Description]='PRODUCT4'; [Unit]='4'; [Tariff]='444555666'; [DutyRate]='45%'; [Origin]='ZA'; [CustomsOrganisation]='ZAIMPORTER1'; [Type]='HTB'; [NewUsed]='4'; [EngineCapacity]='4'; [VehicleFormat]='F4'; [VehicleType]='T4'; [VehicleColour]='C4'; [RelatedOrganisation]=''; [RelationshipType]=''", row4);
		}

		protected override void PrepareTestData()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);

			var rateTypeDty = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY", "Duty");
			rateTypeDty.ZZR_IsPayable = true;
			var rateCodeDty = helper.LoadOrCreateNewCusRateCode(Factory, "1P1", rateTypeDty.PK);
			var preference100 = helper.CreatePreferenceForCountryAndGrouping("100", "None", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var preference200 = helper.CreatePreferenceForCountryAndGrouping("200", "None", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			helper.CreateTaxOrFee("VAT", 0.14, "ZA", Convert.ToDateTime("1900-01-01"), Convert.ToDateTime("2018-03-31"), "VAT Normal");
			helper.CreateTaxOrFee("VAT", 0.15, "ZA", Convert.ToDateTime("2018-04-01"), Convert.ToDateTime("2079-06-06"), "VAT Normal");

			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);

			CreateTariffAndRates(helper, "61046290", tariffType1P1, "OTHER", "2012-01-01", "2015-12-31", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.2 * VFD", "FREE", "20%", "45%", "20%");
			CreateTariffAndRates(helper, "61046290", tariffType1P1, "OTHER", "2016-01-01", "2016-10-09", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.2 * VFD", "FREE", "20%", "45%", "20%");
			CreateTariffAndRates(helper, "61046290", tariffType1P1, "OTHER", "2016-10-10", "2079-06-06", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.27 * VFD", "FREE", "20%", "45%", "27%");

			CreateTariffAndRates(helper, "222333444", tariffType1P1, "OTHER", "2016-10-10", "2079-06-06", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.27 * VFD", "FREE", "20%", "45%", "27%");
			CreateTariffAndRates(helper, "333444555", tariffType1P1, "OTHER", "2016-10-10", "2079-06-06", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.27 * VFD", "FREE", "20%", "45%", "27%");
			CreateTariffAndRates(helper, "444555666", tariffType1P1, "OTHER", "2016-10-10", "2079-06-06", "VAT", "ZA", rateCodeDty, preference100, preference200, "0", "0.2 * VFD", "0.45 * VFD", "0.27 * VFD", "FREE", "20%", "45%", "27%");

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ZAIMPORTER1";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ZASUPPLIER1";
			var classification1 = Factory.New<CusClassification>();
			classification1.FillWithValidTestData();

			CreateProduct("ZA", importer, classification1, "1", "61046290");
			CreateProduct("ZA", importer, classification1, "2", "222333444");
			CreateProduct("ZA", importer, classification1, "3", "333444555", true);
			CreateProduct("ZA", importer, classification1, "4", "444555666", true);

			CreateProduct("ZA", supplier, classification1, "5", "555666777");
			CreateProduct("ZA", supplier, classification1, "6", "666777888");
			CreateProduct("GB", importer, classification1, "7", "777777777");
			CreateProduct("GB", importer, classification1, "8", "888888888");

			organisationParam = importer.PK.ToString();
		}

		void CreateTariffAndRates(ZAUniversalReferenceTestDataHelper helper, string tariffCode, RefCusTariffType tariffType, string tariffDescription, string startDate, string endDate, string taxOrFeeCode, string dataGrouping, CusRefRateCodeView rateCode, CusRefPreferenceView preference100, CusRefPreferenceView preference200,
				string rateFormula1, string rateFormula2, string rateFormula3, string rateFormula4,
				string rateFormulaDerivedFrom1, string rateFormulaDerivedFrom2, string rateFormulaDerivedFrom3, string rateFormulaDerivedFrom4)
		{
			var startDate1 = Convert.ToDateTime(startDate);
			var endDate1 = Convert.ToDateTime(startDate);
			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, tariffCode, startDate1, endDate1, tariffDescription, taxOrFeeCode: taxOrFeeCode);
			helper.CreateRate(tariff, rateCode.PK, Convert.ToDateTime("2012-01-01"), Convert.ToDateTime("2015-12-31"), rateFormula1, preference200.PK, rateFormulaDerivedFrom1, "ZA");
			helper.CreateRate(tariff, rateCode.PK, Convert.ToDateTime("2012-01-01"), Convert.ToDateTime("2015-12-31"), rateFormula2, preference200.PK, rateFormulaDerivedFrom2, "ZA");
			helper.CreateRate(tariff, rateCode.PK, Convert.ToDateTime("2012-01-01"), Convert.ToDateTime("2015-12-31"), rateFormula3, preference100.PK, rateFormulaDerivedFrom3, "ZA");
			helper.CreateRate(tariff, rateCode.PK, Convert.ToDateTime("2016-10-10"), Convert.ToDateTime("2079-06-06"), rateFormula4, preference200.PK, rateFormulaDerivedFrom4, "ZA");
		}

		void CreateProduct(string dataGrouping, OrgHeader relatedOrg, CusClassification classification, string index, string tariffNum, bool directLinkToOrg = false)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = $"PART{index}";
			product.OP_Desc = $"PRODUCT{index}";
			product.OP_StockKeepingUnit = $"{index}";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_RN_NKCountry = dataGrouping;
			pivot.CI_RN_NKCountryOfOrigin = dataGrouping;
			pivot.CI_TariffNum = tariffNum;
			pivot.CI_NewUsed = $"{index}";
			pivot.CI_EngineCapacity = ZInt.ParseSafe(index, ZInt.Zero);
			pivot.CI_VehicleFormat = $"F{index}";
			pivot.CI_VehicleType = $"T{index}";
			pivot.CI_Colour = $"C{index}";

			if (directLinkToOrg)
			{
				pivot.CI_OH = relatedOrg.PK;
			}
			else
			{
				var productRelatedOrg = product.RelatedOrganisations.AddNew();
				productRelatedOrg.OU_Relationship = "OWN";
				productRelatedOrg.OU_OH = relatedOrg.PK;
			}
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>()
				{
					string.Format(CultureInfo.InvariantCulture, $"'{organisationParam}'"),
					string.Format(CultureInfo.InvariantCulture, $"'{productCodeParam}'"),
					string.Format(CultureInfo.InvariantCulture, $"'{descriptionParam}'"),
					string.Format(CultureInfo.InvariantCulture, $"'{tariffCodeParam}'")
				};
			}
		}

		string organisationParam = string.Empty;
		readonly string productCodeParam = string.Empty;
		readonly string descriptionParam = string.Empty;
		readonly string tariffCodeParam = string.Empty;
	}
}
