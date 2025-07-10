using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTaxOrFee.Loader))]
	public class RefCusTaxOrFeeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefCusTaxOrFee.Loader(Factory);

		public void TestIsExistedTaxOrFee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
			Factory.Save();
			helper.CreateTaxOrFee("DV1", 0.5m, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, 0m, 1m, "VAT", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			helper.CreateTaxOrFee("DV2", 0.6m, Core.Constants.CountryCodes.Germany, 0m, 1m, "VAT", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			helper.CreateTaxOrFee("DV3", 0.7m, Core.Constants.CountryCodes.China, 0m, 1m, "VAT", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(2));
			Factory.Save();
			CombineAssertions(() =>
			{
				var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
				Assert("datagrouping is empty", !loader.IsExistedTaxOrFee("", "DV3", "VAT"));
				Assert("code is empty", !loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "", "VAT"));
				Assert("type is empty", !loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "DV3", ""));
				Assert("DE-VAT-DV1 existed", loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "DV1", "VAT"));
				Assert("DE-VAT-DV2 existed", loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "DV2", "VAT"));
				Assert("DE-VAT-DV3 not existed", !loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "DV3", "VAT"));
				Assert("CN-VAT-DV3 existed", loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.China, "DV3", "VAT"));
				Assert("DE-OTH-DV1 not existed", !loader.IsExistedTaxOrFee(Core.Constants.CountryCodes.Germany, "DV1", "OTH"));
			}

			);
		}

		public void TestLoadMostRecentEffectiveTaxOrFeeFromCodeDateWithParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: euDataGrouping);
			Factory.Save();
			var taxOrFee1 = helper.CreateTaxOrFee("DV1", 20000.0m, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			var taxOrFee2 = helper.CreateTaxOrFee("DV1", 20000.1m, Core.Constants.CountryCodes.Germany, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			var taxOrFee3 = helper.CreateTaxOrFee("DV1", 20000.2m, Core.Constants.CountryCodes.Germany, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(2));
			var taxOrFee4 = helper.CreateTaxOrFee("DV1", 20000.3m, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today.AddDays(-2), ZDate.Today);
			Factory.Save();
			CombineAssertions(() =>
			{
				var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
				AssertEquals("DE-Specified DV1", taxOrFee3, loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, "DV1", ZDateTime.Today));
				AssertNull("No Effective DE-Specified DV1", loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, "DV1", ZDateTime.Today.AddDays(3)));
				AssertEquals("DE-Specified DV1 when fallBackToParentDataGrouping", taxOrFee3, loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, "DV1", ZDateTime.Today));
				AssertNull("No Effective DE-Specified DV1 when fallBackToParentDataGrouping", loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, "DV1", ZDateTime.Today.AddDays(3)));
				AssertEquals("IT can use EUN DV1", taxOrFee1, loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Italy, "DV1", ZDateTime.Today));
			}

			);
		}

		public void TestLoadMostRecentEffectiveTaxOrFeeFromCodeValuationDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var testTaxOrFee1 = helper.CreateTaxOrFee("GB1", 0.1, "GB", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 5, 1), "GBDESC1");
			var testTaxOrFee2 = helper.CreateTaxOrFee("GB1", 0.2, "GB", new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1), "GBDESC1another");
			var testTaxOrFee3 = helper.CreateTaxOrFee("GB1", 0.3, "IT", new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1), "IT-GBDESC");
			Factory.Save();
			var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
			var testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("GB", "GB0", new ZDateTime(2011, 02, 01));
			CombineAssertions(() =>
			{
				AssertNull(testTaxOrFee);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("GB", "GB1", new ZDateTime(2011, 02, 01));
				AssertEquals(testTaxOrFee1.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("GB", "GB1", new ZDateTime(2011, 03, 01));
				AssertEquals(testTaxOrFee2.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("GB", "GB1", new ZDateTime(2011, 04, 05));
				AssertEquals(testTaxOrFee1.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("IT", "GB1", new ZDateTime(2011, 03, 15));
				AssertEquals(testTaxOrFee3.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate("ZA", "GB1", new ZDateTime(2011, 03, 15));
				AssertNull(testTaxOrFee);
			}

			);
		}

		public void TestLoadMostRecentEffectiveTaxOrFeeFromCodeStartAndEndDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var testTaxOrFee1 = helper.CreateTaxOrFee("TX1", 0.1, Core.Constants.CountryCodes.UnitedKingdom, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 5, 1));
			var testTaxOrFee2 = helper.CreateTaxOrFee("TX2", 0.2, Core.Constants.CountryCodes.UnitedKingdom, new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1));
			var testTaxOrFee3 = helper.CreateTaxOrFee("TX2", 0.3, Core.Constants.CountryCodes.Italy, new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1));
			Factory.Save();
			var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
			CombineAssertions(() =>
			{
				var testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2010, 12, 1), new ZDateTime(2011, 4, 1));
				AssertNull("Test where start date is too early", testTaxOrFee);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2011, 1, 2), new ZDateTime(2011, 6, 1));
				AssertNull("Test where end date is too late", testTaxOrFee);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2010, 12, 1), new ZDateTime(2011, 6, 1));
				AssertNull("Both start and end date are out of range", testTaxOrFee);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2011, 1, 2), new ZDateTime(2011, 4, 30));
				AssertEquals("Start and end date are valid for a Tax/Fee", testTaxOrFee1.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX3", new ZDateTime(2010, 12, 1), new ZDateTime(2011, 4, 1));
				AssertNull("Invalid Code", testTaxOrFee);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX2", new ZDateTime(2011, 3, 2), new ZDateTime(2011, 3, 31));
				AssertEquals("Start and end date are valid for a Tax/Fee in UK", testTaxOrFee2.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Italy, "TX2", new ZDateTime(2011, 3, 2), new ZDateTime(2011, 3, 31));
				AssertEquals("Start and end date are valid for a Tax/Fee in IT", testTaxOrFee3.PK, testTaxOrFee.PK);
				testTaxOrFee = loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Germany, "TX2", new ZDateTime(2011, 3, 2), new ZDateTime(2011, 3, 31));
				AssertNull("Invalid Data Grouping", testTaxOrFee);
			}

			);
		}

		public void TestGetList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("GB1", 0.1, "GB", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 3, 1), "GBDESC1");
			helper.CreateTaxOrFee("GB2", 0.2, "GB", new ZDateTime(2011, 2, 1), new ZDateTime(2011, 4, 1), "GBDESC2");
			helper.CreateTaxOrFee("IT", 0.3, "IT", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 4, 1), "ITDESC");
			Factory.Save();
			var testList = RefCusTaxOrFee.Loader.GetList(Factory, "GB", new ZDateTime(2011, 2, 14));
			CombineAssertions(() =>
			{
				AssertEquals(2, testList.Count);
				AssertEquals(true, testList.ContainsCode("GB1"));
				AssertEquals(true, testList.ContainsCode("GB2"));
				testList = RefCusTaxOrFee.Loader.GetList(Factory, "GB", new ZDateTime(2011, 3, 14));
				AssertEquals(1, testList.Count);
				AssertEquals(true, testList.ContainsCode("GB2"));
				testList = RefCusTaxOrFee.Loader.GetList(Factory, "GB", new ZDateTime(2011, 4, 14));
				AssertEquals(0, testList.Count);
				testList = RefCusTaxOrFee.Loader.GetList(Factory, "IT", new ZDateTime(2011, 3, 14));
				AssertEquals(1, testList.Count);
				AssertEquals(true, testList.ContainsCode("IT"));
			}

			);
		}

		public void TestGetRefCusTaxOrFeeQueryUsesNoResult()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var testTaxOrFee1 = helper.CreateTaxOrFee("TX1", 0.1, Core.Constants.CountryCodes.UnitedKingdom, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 5, 1));
			var testTaxOrFee2 = helper.CreateTaxOrFee("TX2", 0.2, Core.Constants.CountryCodes.UnitedKingdom, new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1));
			var testTaxOrFee3 = helper.CreateTaxOrFee("TX2", 0.3, Core.Constants.CountryCodes.Italy, new ZDateTime(2011, 3, 1), new ZDateTime(2011, 4, 1));
			Factory.Save();
			var loader = new RefCusTaxOrFee.Loader(Factory);
			CombineAssertions(() =>
			{
				var invalidDate = ZDateTime.Empty;
				var dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2011, 1, 2), new ZDateTime(2011, 4, 1));
				var newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("All parameters are valid query is built", dbhits + 1, newDbhits);
				dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(ZString.Empty, "TX1", new ZDateTime(2010, 12, 1), new ZDateTime(2011, 4, 1));
				newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("Test where no dataGrouping is empty", dbhits, newDbhits);
				dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", invalidDate, new ZDateTime(2011, 6, 1));
				newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("Test where start date is empty", dbhits, newDbhits);
				dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", new ZDateTime(2010, 12, 1), invalidDate);
				newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("Test where end date is empty", dbhits, newDbhits);
				dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, "TX1", ZDateTime.Invalid, ZDateTime.Invalid);
				newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("Test where start and end date is invalid", dbhits, newDbhits);
				dbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.UnitedKingdom, ZString.Empty, ZDateTime.Invalid, ZDateTime.Invalid);
				newDbhits = Factory.GetTableHitCount(RefCusTaxOrFeeSchema.Constants.TableName);
				AssertEquals("Code is empty", dbhits, newDbhits);
			}

			);
		}

		public void TestLoadMostRecentEffectiveDeminimusOfCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Constants.RefCusTaxOrFeeTypes.Deminimus, 800.0m, "US", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Deminimus");
			helper.CreateTaxOrFee(Constants.RefCusTaxOrFeeTypes.Deminimus, 1000.0m, "AU", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Deminimus");
			helper.CreateTaxOrFee(Constants.RefCusTaxOrFeeTypes.Deminimus, 400.0m, "NZ", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Deminimus");
			Factory.Save();
			CombineAssertions(() =>
			{
				var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
				AssertEquals(800m, loader.LoadMostRecentEffectiveDeminimusOfCountry("US"));
				AssertEquals(1000m, loader.LoadMostRecentEffectiveDeminimusOfCountry("AU"));
				AssertEquals(400m, loader.LoadMostRecentEffectiveDeminimusOfCountry("NZ"));
				AssertEquals(ZDecimal.Zero, loader.LoadMostRecentEffectiveDeminimusOfCountry("FR"));
			}

			);
		}

		public void TestLoadExportDeminimusOfCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Constants.RefCusTaxOrFeeTypes.ExportDeminimus, 1000m, "SG", 0m, 0m, "", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), "Export Deminimus");
			Factory.Save();
			var loader = (RefCusTaxOrFee.Loader)GetNewLoaderToTest();
			AssertEquals(1000m, loader.LoadExportDeminimusOfCountry("SG"));
			AssertEquals(ZDecimal.Zero, loader.LoadExportDeminimusOfCountry("FR"));
		}

		public void TestLoadTaxOrFeeFromTypeDate()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var testTaxOrFee1 = helper.CreateTaxOrFee("ENT1", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2019, 1, 1), new ZDateTime(2021, 12, 31));
			var testTaxOrFee2 = helper.CreateTaxOrFee("ENT2", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2019, 1, 1), new ZDateTime(2021, 12, 31));
			var testTaxOrFee3 = helper.CreateTaxOrFee("ENT3", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2022, 1, 1), new ZDateTime(2078, 6, 6));
			var testTaxOrFee4 = helper.CreateTaxOrFee("ENT4", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2022, 1, 1), new ZDateTime(2078, 6, 6));
			var testTaxOrFee5 = helper.CreateTaxOrFee("TXT1", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, "TFT", new ZDateTime(2019, 1, 1), new ZDateTime(2078, 6, 6));
			var testTaxOrFee6 = helper.CreateTaxOrFee("TXT2", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, "TFT", new ZDateTime(2019, 1, 1), new ZDateTime(2078, 6, 6));
			var testTaxOrFee7 = helper.CreateTaxOrFee("TXT3", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, "TFT", new ZDateTime(2019, 1, 1), new ZDateTime(2078, 6, 6));
			var testTaxOrFee8 = helper.CreateTaxOrFee("TXT4", 38.56m, Core.Constants.CountryCodes.Brazil, 115.67m, 192.79m, "TFT", new ZDateTime(2019, 1, 1), new ZDateTime(2078, 6, 6));
			factory.Save();

			CombineAssertions(() =>
			{
				var testTaxOrFee = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2011, 1, 2));
				AssertEquals("Test where start date is too early", 0, testTaxOrFee.Length);

				testTaxOrFee = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2080, 1, 2));
				AssertEquals("Test where end date is too late", 0, testTaxOrFee.Length);

				testTaxOrFee = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2011, 1, 2));
				AssertEquals("Country without tax or fee code", 0, testTaxOrFee.Length);

				testTaxOrFee = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, new ZDateTime(2020, 1, 2));
				var taxOrFeeCodes = testTaxOrFee.Select(x => x.ZZF_Code).ToArray();
				AssertContainsExactElementsInExactOrder("Collection must contain", new string[] { "ENT1", "ENT2" }, taxOrFeeCodes);

				testTaxOrFee = RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, Core.Constants.CountryCodes.Brazil, "TFT", new ZDateTime(2020, 1, 2));
				taxOrFeeCodes = testTaxOrFee.Select(x => x.ZZF_Code).ToArray();
				AssertContainsExactElementsInExactOrder("Collection must contain", new string[] { "TXT1", "TXT2", "TXT3", "TXT4" }, taxOrFeeCodes);
			});
		}
	}
}
