using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusClassPartPivotLookupsTest : TestCaseWithFactory
	{
		public void TestROOTypeOrPreferenceList()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateAdditionalInformationCusCodeEntry("AGO");
			testHelper.CreateAdditionalInformationCusCodeEntry("ETA");
			testHelper.CreateAdditionalInformationCusCodeEntry("EUR");
			testHelper.CreateAdditionalInformationCusCodeEntry("GSP");
			testHelper.CreateAdditionalInformationCusCodeEntry("SAD");
			testHelper.CreateAdditionalInformationCusCodeEntry("MER");
			Factory.Save();
			var preference = testHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", startDate, endDate);
			var rateType_ZA_REB = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var tradeGroup1 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, description: "Standard Agreement");
			testHelper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Factory.Save();
			var tariff1Rate = testHelper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			Factory.Save();
			testHelper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			InitialisePivot();
			pivot.CI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			pivot.CI_TariffNum = "1020304050";
			var testLookups = pivot.Lookups;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			CombineAssertions("Test TradeAgreementsList", () =>
			{
				Assert("Test 1", !testLookups.ROOTypeOrPreferenceList.ContainsCode("AGO"));
				Assert("Test 2", testLookups.ROOTypeOrPreferenceList.ContainsCode("100"));
			});
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			CombineAssertions("Test ROOTypesList", () =>
			{
				AssertEquals("Test Count", 6, testLookups.ROOTypeOrPreferenceList.Count);
				Assert("Test 1", testLookups.ROOTypeOrPreferenceList.ContainsCode("AGO"));
				Assert("Test 2", testLookups.ROOTypeOrPreferenceList.ContainsCode("ETA"));
				Assert("Test 3", testLookups.ROOTypeOrPreferenceList.ContainsCode("EUR"));
				Assert("Test 4", testLookups.ROOTypeOrPreferenceList.ContainsCode("GSP"));
				Assert("Test 5", testLookups.ROOTypeOrPreferenceList.ContainsCode("SAD"));
				Assert("Test 6", testLookups.ROOTypeOrPreferenceList.ContainsCode("MER"));
			});
		}

		public void TestTariffs()
		{
			InitialisePivot();
			AssertType<TariffViewCollection>(pivot.Lookups.Tariffs);
		}

		public void TestVehicleFormats()
		{
			InitialisePivot();
			AssertSame(Factory.GetCachedValue<VehicleFormatList>(), pivot.Lookups.VehicleFormats);
		}

		public void TestVehicleTypes()
		{
			InitialisePivot();
			AssertSame(Factory.GetCachedValue<VehicleTypeList>(), pivot.Lookups.VehicleTypes);
		}

		public void TestGoodsTypeList()
		{
			InitialisePivot();
			AssertEquals(typeof(GoodsTypeList), pivot.Lookups.GoodsTypeList.GetType());
			Assert(pivot.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.N));
			Assert(pivot.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.S));
			Assert(pivot.Lookups.GoodsTypeList.ContainsCode(GoodsTypeList.Codes.U));
		}

		void InitialisePivot()
		{
			part = Factory.New<OrgSupplierPart>();
			pivot = part.PivotsForBinding.AddNew();
		}

		OrgSupplierPart part;
		CusClassPartPivot pivot;
	}
}
