using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "CTN", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCIU", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var lookups = header.Bills.AddNew().PackedItems.AddNew().Lookups;

			var list = lookups.CustomsUQList;
			CombineAssertions(() =>
			{
				AssertEquals(4, list.Count);
				AssertEquals("CTN, DOZ, PCS, YDS", list.CodesAsString);
			});
		}

		public void TestPreferenceList()
		{
			SetupTariffAndRate();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.DeclarationDate = new ZDateTime(2023, 9, 25);
			var packItem = bill.PackedItems.AddNew();
			var lookups = packItem.Lookups;

			packItem.API_Tariff = ZString.Empty;
			packItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but tariff is empty", new string[] { "STD", "PR1", "PR2" }, lookups.PreferenceList.GetAllCodes());

			packItem.API_Tariff = "21039090200";
			packItem.API_RN_NKGoodsOrigin = ZString.Empty;
			AssertContainsExactElementsInAnyOrder("UseUniversalTariff but CountryOfOrigin  is empty", new string[] { "STD", "PR1", "PR2" }, lookups.PreferenceList.GetAllCodes());

			packItem.API_Tariff = "21039090200";
			packItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Botswana;
			AssertContainsExactElementsInAnyOrder("match country using univiersal tariff", new string[] { "STD", "PR1" }, lookups.PreferenceList.GetAllCodes());

			packItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.AlandIslands;
			AssertContainsExactElementsInAnyOrder("no match country using univiersal tariff", System.Array.Empty<string>(), lookups.PreferenceList.GetAllCodes());
		}

		void SetupTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = "TW";
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupStandard = refDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			refDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
			Factory.Save();

			var hsnTariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();
			var dutyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCodeDTA = refDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", dutyRateType.PK);
			var rateCodeDTS = refDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", dutyRateType.PK);
			Factory.Save();

			var preferenceSTD = refDataHelper.CreatePreferenceForCountry("STD", "STD", "TW");
			var preferencePR1 = refDataHelper.CreatePreferenceForCountry("PR1", "PR1", "TW");
			refDataHelper.CreatePreferenceForCountry("PR2", "PR2", "TW");
			Factory.Save();

			var cusTariff = refDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "21039090200", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = refDataHelper.CreateRate(cusTariff, rateCodeDTA.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			refDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
			refDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");

			var testRate2 = refDataHelper.CreateRate(cusTariff, rateCodeDTA.PK, date1, date4, "0", preferencePk: preferencePR1.PK);
			refDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
			refDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add22", "ord22");

			var testRate3 = refDataHelper.CreateRate(cusTariff, rateCodeDTS.PK, date1, date4, "0", preferencePk: preferencePR1.PK);
			refDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, "add31", "ord31");
			Factory.Save();
		}
	}
}
