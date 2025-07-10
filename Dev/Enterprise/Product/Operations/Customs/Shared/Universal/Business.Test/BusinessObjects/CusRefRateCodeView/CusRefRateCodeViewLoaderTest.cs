using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateCodeView.Loader))]
	public class CusRefRateCodeViewLoaderTest : LoaderTestCase
	{
		public void TestLoadByDataSet()
		{
			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "T01", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "T02", true);
			var rateCodes = CusRefRateCodeView.Loader.LoadByDataSet(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.DataSetTypes.OWNData, false);
			AssertCollectionContains("Should load T01", rateCode1, rateCodes);
			AssertCollectionNotContains("Should not load T02", rateCode2, rateCodes);
			rateCodes = CusRefRateCodeView.Loader.LoadByDataSet(Factory, Core.Constants.CountryCodes.Vanuatu, Core.Constants.Customs.Universal.DataSetTypes.WTGData, false);
			AssertCollectionContains("Should load T02", rateCode2, rateCodes);
			AssertCollectionNotContains("Should not load T01", rateCode1, rateCodes);
			rateCodes = CusRefRateCodeView.Loader.LoadByDataSet(Factory, Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.DataSetTypes.OWNData, false);
			AssertCollectionNotContains("Should not load T01", rateCode1, rateCodes);
			AssertCollectionNotContains("Should not load T02", rateCode2, rateCodes);
			rateCodes = CusRefRateCodeView.Loader.LoadByDataSet(Factory, Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.DataSetTypes.WTGData, false);
			AssertCollectionNotContains("Should not load T01", rateCode1, rateCodes);
			AssertCollectionNotContains("Should not load T02", rateCode2, rateCodes);
		}

		public void TestLoad_IncludeMultipleRateTypes()
		{
			var rateCodes = SetupLoadRateCodes();
			var dtyAndCvdRateTypeArray = new ZString[] { Constants.RateTypes.Duty, Constants.RateTypes.Countervailing };
			var includeDtyAndCvdTypesRateCodeCriteria = new RateCodeLoadCriteria()
			{ RateTypesToInclude = dtyAndCvdRateTypeArray };
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateTypesToInclude=DTY,CVD", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Tonga, includeDtyAndCvdTypesRateCodeCriteria), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaCvdSysDefInternal }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaAddSysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoad_ExcludeMultipleRateTypes()
		{
			var rateCodes = SetupLoadRateCodes();
			var dtyAndCvdRateTypeArray = new ZString[] { Constants.RateTypes.Duty, Constants.RateTypes.Countervailing };
			var excludeDtyAndCvdTypesRateCodeCriteria = new RateCodeLoadCriteria()
			{ RateTypesToExclude = dtyAndCvdRateTypeArray };
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateTypesToExclude=DTY,CVD", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Tonga, excludeDtyAndCvdTypesRateCodeCriteria), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaAddSysDef }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaCvdSysDefInternal, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoad_InternalUseRate()
		{
			var rateCodes = SetupLoadRateCodes();
			var dtyAndCvdRateTypeArray = new ZString[] { Constants.RateTypes.Duty, Constants.RateTypes.Countervailing };
			var internalUseDtyAndCvdTypesRateCodeCriteria = new RateCodeLoadCriteria()
			{ InternalUseRate = false, RateTypesToInclude = dtyAndCvdRateTypeArray };
			AssertRateCodeCollection("Filter: DataGrouping=TO, InternalUseRate=false, RateTypesToInclude=DTY,CVD", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Tonga, internalUseDtyAndCvdTypesRateCodeCriteria), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaAddSysDef, rateCodes.TongaCvdSysDefInternal, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoad_DataGroupingNoRates()
		{
			var rateCodes = SetupLoadRateCodes();
			var includeDutyTypeRateCodeCriteria = new RateCodeLoadCriteria()
			{ RateTypesToInclude = new ZString[] { Constants.RateTypes.Duty } };
			AssertRateCodeCollection("Filter: DataGrouping=SB, RateTypesToInclude=DTY", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.SolomonIslands, includeDutyTypeRateCodeCriteria), expectedElements: Array.Empty<CusRefRateCodeView>(), nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaAddSysDef, rateCodes.TongaCvdSysDefInternal, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoad_DataGroupingEmpty()
		{
			var rateCodes = SetupLoadRateCodes();
			var includeDutyTypeRateCodeCriteria = new RateCodeLoadCriteria()
			{ RateTypesToInclude = new ZString[] { Constants.RateTypes.Duty } };
			var initialCount = Factory.GetTableHitCount(CusRefRateCodeViewSchema.Constants.TableName);
			AssertRateCodeCollection("Filter: DataGrouping=<empty>, RateTypesToInclude=DTY", CusRefRateCodeView.Loader.Load(Factory, dataGrouping: ZString.Empty, includeDutyTypeRateCodeCriteria), expectedElements: Array.Empty<CusRefRateCodeView>(), nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaAddSysDef, rateCodes.TongaCvdSysDefInternal, rateCodes.VanuatoDtyUserDef });
			AssertEquals("No result query no table hit", initialCount, Factory.GetTableHitCount(CusRefRateCodeViewSchema.Constants.TableName));
		}

		public void TestLoadByRateCodeExcludeParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);

			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Constants.RateTypes.Duty, "T01", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.Duty, "T02", true);
			var rateCodes = CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.France, new RateCodeLoadCriteria(), false);

			AssertRateCodeCollection("Should load T01, not load T02. ", rateCodes, new CusRefRateCodeView[] { rateCode1 }, new CusRefRateCodeView[] { rateCode2 });
		}

		public void TestLoad_CriteriaEmpty()
		{
			var rateCodes = SetupLoadRateCodes();
			AssertRateCodeCollection("Filter: DataGrouping=TO (default additional criteria)", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Tonga, new RateCodeLoadCriteria()), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaAddSysDef, rateCodes.TongaCvdSysDefInternal }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoad_CriteriaNull()
		{
			var rateCodes = SetupLoadRateCodes();
			AssertRateCodeCollection("Filter: DataGrouping=TO (additional criteria null)", CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Tonga), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaAddSysDef, rateCodes.TongaCvdSysDefInternal }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateType_Duty()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateType=DTY", CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaAddSysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateType_AntiDumping()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateType=ADD", CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.AntiDumping), expectedElements: new CusRefRateCodeView[] { rateCodes.TongaAddSysDef }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateType_Invalid()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateType=~XX", CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.Tonga, "~XX"), expectedElements: Array.Empty<CusRefRateCodeView>(), nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef, rateCodes.TongaAddSysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateType_DataGrouping()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=VU, RateType=DTY", CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty), expectedElements: new CusRefRateCodeView[] { rateCodes.VanuatoDtyUserDef }, nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef, rateCodes.TongaAddSysDef });
		}

		public void TestLoadByRateType_DataGroupingNoRates()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=SB, RateType=DTY", CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.SolomonIslands, Constants.RateTypes.Duty), expectedElements: Array.Empty<CusRefRateCodeView>(), nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef, rateCodes.TongaAddSysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateType_DataGroupingEmpty()
		{
			var rateCodes = SetupLoadByRateTypeCodes();
			AssertRateCodeCollection("Filter: DataGrouping=<empty>, RateType=DTY", CusRefRateCodeView.Loader.LoadByRateType(Factory, ZString.Empty, Constants.RateTypes.Duty), expectedElements: Array.Empty<CusRefRateCodeView>(), nonExpectedElements: new CusRefRateCodeView[] { rateCodes.TongaDtyUserDef, rateCodes.TongaDtySysDef, rateCodes.TongaAddSysDef, rateCodes.VanuatoDtyUserDef });
		}

		public void TestLoadByRateCode_Code()
		{
			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R01", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R02", true);
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateCode=R01", CusRefRateCodeView.Loader.LoadByRateCode(Factory, Core.Constants.CountryCodes.Tonga, "R01"), expectedElements: new CusRefRateCodeView[] { rateCode1 }, nonExpectedElements: new CusRefRateCodeView[] { rateCode2 });
		}

		public void TestLoadByRateCode_CodeWithDifferentTypes()
		{
			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R01", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Countervailing, "R01", true);
			var rateCode3 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "R01", false);
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateCode=R01", CusRefRateCodeView.Loader.LoadByRateCode(Factory, Core.Constants.CountryCodes.Tonga, "R01"), expectedElements: new CusRefRateCodeView[] { rateCode1, rateCode2 }, nonExpectedElements: new CusRefRateCodeView[] { rateCode3 });
		}

		public void TestLoadByRateCode_DataSet()
		{
			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R01", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R01", true);
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateCode=R01, DataSet=O", CusRefRateCodeView.Loader.LoadByRateCode(Factory, Core.Constants.CountryCodes.Tonga, "R01", Core.Constants.Customs.Universal.DataSetTypes.OWNData), expectedElements: new CusRefRateCodeView[] { rateCode1 }, nonExpectedElements: new CusRefRateCodeView[] { rateCode2 });
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateCode=R01, DataSet=Z", CusRefRateCodeView.Loader.LoadByRateCode(Factory, Core.Constants.CountryCodes.Tonga, "R01", Core.Constants.Customs.Universal.DataSetTypes.WTGData), expectedElements: new CusRefRateCodeView[] { rateCode2 }, nonExpectedElements: new CusRefRateCodeView[] { rateCode1 });
			AssertRateCodeCollection("Filter: DataGrouping=TO, RateCode=R01, DataSet=''", CusRefRateCodeView.Loader.LoadByRateCode(Factory, Core.Constants.CountryCodes.Tonga, "R01", ""), expectedElements: new CusRefRateCodeView[] { rateCode1, rateCode2 }, nonExpectedElements: Array.Empty<CusRefRateCodeView>());
		}

		public void TestLoadByRateCode_DataGroupingEmpty()
		{
			var rateCode1 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R02", false);
			var rateCode2 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "R01", true);
			var rateCode3 = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "R02", false);
			AssertRateCodeCollection("Filter: DataGrouping=<empty>, RateCode=R02", CusRefRateCodeView.Loader.LoadByRateCode(Factory, dataGrouping: ZString.Empty, "R02"), expectedElements: new CusRefRateCodeView[] { rateCode1, rateCode3 }, nonExpectedElements: new CusRefRateCodeView[] { rateCode2 });
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusRefRateCodeView.Loader(Factory);

		void AssertRateCodeCollection(string assertionCaption, CusRefRateCodeView[] actualRateCodes, CusRefRateCodeView[] expectedElements, CusRefRateCodeView[] nonExpectedElements)
		{
			CombineAssertions(assertionCaption, () =>
			{
				AssertEquals("Number of Rate Code elements", expectedElements.Length, actualRateCodes.Length);
				foreach (var expectedRateCode in expectedElements)
				{
					AssertCollectionContains($"[{expectedRateCode.ZY1_RateCode}] loaded.", expectedRateCode, actualRateCodes);
				}

				foreach (var nonExpectedRateCode in nonExpectedElements)
				{
					AssertCollectionNotContains($"Was [{nonExpectedRateCode.ZY1_RateCode}] loaded?", nonExpectedRateCode, actualRateCodes);
				}
			}

			);
		}

		(CusRefRateCodeView TongaDtyUserDef, CusRefRateCodeView TongaAddSysDef, CusRefRateCodeView TongaCvdSysDefInternal, CusRefRateCodeView VanuatoDtyUserDef) SetupLoadRateCodes()
		{
			var rcTongaDtyUserDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "RC1", isSystem: false);
			var rcTongaAddSysDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.AntiDumping, "RC2", isSystem: true);
			var rcTongaCvdSysDefInternal = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Countervailing, "RC3", isSystem: true, internalUse: true);
			var rcVanuatoDtyUserDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "RC4", isSystem: false);
			return (rcTongaDtyUserDef, rcTongaAddSysDef, rcTongaCvdSysDefInternal, rcVanuatoDtyUserDef);
		}

		(CusRefRateCodeView TongaDtyUserDef, CusRefRateCodeView TongaDtySysDef, CusRefRateCodeView TongaAddSysDef, CusRefRateCodeView VanuatoDtyUserDef) SetupLoadByRateTypeCodes()
		{
			var rcTongaDtyUserDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "RC1", isSystem: false);
			var rcTongaDtySysDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.Duty, "RC2", isSystem: true);
			var rcTongaAddSysDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Tonga, Constants.RateTypes.AntiDumping, "RC3", isSystem: true);
			var rcVanuatoDtyUserDef = CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Vanuatu, Constants.RateTypes.Duty, "RC4", isSystem: false);
			return (rcTongaDtyUserDef, rcTongaDtySysDef, rcTongaAddSysDef, rcVanuatoDtyUserDef);
		}
	}
}
