using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using RateTypes = Enterprise.Customs.Universal.Constants.RateTypes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class UniversalReferenceDataHelperTest : TestCaseWithFactory
	{
		public void TestGetTradeGroupByPreference()
		{
			var startDate = new ZDate(2010, 01, 01);
			var endDate1 = new ZDate(2019, 11, 30);
			var endDate2 = new ZDate(2079, 06, 06);
			var tradeGroup1 = BaseHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "tradeGroup1", startDate, endDate2, "tradeGroup1 DEC");
			BaseHelper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Australia, startDate, endDate2);
			var tradeGroup2 = BaseHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "tradeGroup2", startDate, endDate2);
			BaseHelper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Australia, startDate, endDate2);
			Factory.Save();
			var hsnTariffType = BaseHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "HSN");
			Factory.Save();
			var dutyRateType = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = BaseHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = BaseHelper.CreatePreferenceForCountry("STD", "Standard", Core.Constants.CountryCodes.SouthAfrica);
			var preferenceRED = BaseHelper.CreatePreferenceForCountry("RED", "Reduced", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var cusTariff = BaseHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, hsnTariffType.PK, "DUMMYTRF", startDate, endDate2, "dummy Description 0");
			Factory.Save();
			var testRate1 = BaseHelper.CreateRate(cusTariff, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceSTD.PK);
			BaseHelper.CreateCusApplicability(testRate1, tradeGroup1, startDate, endDate2, "add11", "ord11");
			var testRate2 = BaseHelper.CreateRate(cusTariff, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceRED.PK);
			BaseHelper.CreateCusApplicability(testRate2, tradeGroup1, startDate, endDate1, "add21", "ord21");
			var testRate3 = BaseHelper.CreateRate(cusTariff, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceSTD.PK);
			BaseHelper.CreateCusApplicability(testRate3, tradeGroup2, startDate, endDate2, additionalCode: "add31", "ord31");
			Factory.Save();
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = BaseHelper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.SouthAfrica, "", "", emptyAdditionalCodeSet, date, "", "");
			var tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(cusTariff, criteria);
			AssertEquals("preference are empty", "", tradeGroup.Key);
			tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(null, criteria);
			AssertEquals("tariff is null", "", tradeGroup.Key);
			criteria = BaseHelper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.SouthAfrica, "RED", "", emptyAdditionalCodeSet, endDate1.AddDays(+1), "", "");
			tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(cusTariff, criteria);
			AssertEquals("no match date", "", tradeGroup.Key);
			criteria = BaseHelper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.SouthAfrica, "STD", "", new HashSet<ZString>()
			{ "add11" }, date, "", "");
			tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(cusTariff, criteria);
			AssertEquals("no match countryoforigin", "", tradeGroup.Key);
			criteria = BaseHelper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.SouthAfrica, "RED", "", emptyAdditionalCodeSet, date, "", "");
			tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(cusTariff, criteria);
			AssertEquals("match specific preference", "tradeGroup1", tradeGroup.Key);
			AssertEquals("match specific preference", "tradeGroup1 DEC", tradeGroup.Value);
			criteria = BaseHelper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.SouthAfrica, "STD", "ord11", new HashSet<ZString>()
			{ "add11" }, date, "", "");
			tradeGroup = UniversalReferenceDataHelper.GetTradeGroupByPreference(cusTariff, criteria);
			AssertEquals("match specific addionalcode and preference and ordernumber", "tradeGroup1", tradeGroup.Key);
			AssertEquals("match specific addionalcode and preference and ordernumber", "tradeGroup1 DEC", tradeGroup.Value);
		}

		public void TestIsRefund()
		{
			var tariffType = BaseHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "XXX");
			Factory.Save();
			var cacheKey = $"ZA_IsRefund_{tariffType.PK}";
			Assert(!tariffType.IsRefund());
			var rateType_US_REF = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Constants.RateTypes.Refund, "Refd");
			var rateCode_US_REF_D = BaseHelper.LoadOrCreateNewCusRateCode(Factory, "XXX", rateType_US_REF.PK);
			Factory.Save();
			Factory.ClearQueryCache();
			Factory.ClearCachedValue<bool>(cacheKey);
			Assert(!tariffType.IsRefund());
			var rateType_ZA_DTY = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "DTY");
			var rateCode_ZA_DTY_D = BaseHelper.LoadOrCreateNewCusRateCode(Factory, "XXX", rateType_ZA_DTY.PK);
			Factory.Save();
			Factory.ClearQueryCache();
			Factory.ClearCachedValue<bool>(cacheKey);
			Assert(!tariffType.IsRefund());
			var rateType_ZA_REF = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Refund, "Refd");
			var rateCode_ZA_REF_D = BaseHelper.LoadOrCreateNewCusRateCode(Factory, "XXX", rateType_ZA_REF.PK);
			Factory.Save();
			Factory.ClearQueryCache();
			Factory.ClearCachedValue<bool>(cacheKey);
			Assert(tariffType.IsRefund());
		}

		public void TestGetCusTariffIncludingCheckDigit()
		{
			var tariffType3P1 = BaseHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			Factory.Save();
			var tariff1 = BaseHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "123456789", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute1 = Factory.New<TariffAttributeView>();
			attribute1.ZZ3_ZZ1_ParentTariffOrNationalCode = tariff1.PK;
			attribute1.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.CheckDigit;
			attribute1.ZZ3_Value = "AB";
			BaseHelper.CreateTariffRelationship(tariff1.PK, tariffType3P1.PK, "5407");

			var tariff2 = BaseHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "123456789", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			var attribute2 = Factory.New<TariffAttributeView>();
			attribute2.ZZ3_ZZ1_ParentTariffOrNationalCode = tariff2.PK;
			attribute2.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.CheckDigit;
			attribute2.ZZ3_Value = "CD";
			BaseHelper.CreateTariffRelationship(tariff2.PK, tariffType3P1.PK, "");

			var tariffView = Factory.GetCusTariffIncludingCheckDigit("3P1", "123456789", ZDateTime.Today, "CD", "540742");
			AssertEquals("Should be Tariff with check digit CD", tariff2.PK, tariffView.PK);
		}

		public void TestGetTariffCodeWithCheckDigit()
		{
			TariffView tariff = null;
			AssertEquals(ZString.Empty, tariff.GetTariffCodeWithCheckDigit());
			var tariffType1P1 = BaseHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariffType12B = BaseHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			var tariffDTY = BaseHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "10203040", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariffNonDTY = BaseHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020304", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			CombineAssertions("DTY", () =>
			{
				AssertEquals("10203040", tariffDTY.GetTariffCodeWithCheckDigit());
				var attribute = Factory.New<TariffAttributeView>();
				attribute.ZZ3_ZZ1_ParentTariffOrNationalCode = tariffDTY.PK;
				AssertEquals("10203040", tariffDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Value = "3";
				AssertEquals("10203040", tariffDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.CheckDigit;
				AssertEquals("102030403", tariffDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Name = "BOB";
				AssertEquals("10203040", tariffDTY.GetTariffCodeWithCheckDigit());
			});
			CombineAssertions("Non DTY", () =>
			{
				AssertEquals("1020304", tariffNonDTY.GetTariffCodeWithCheckDigit());
				var attribute = Factory.New<TariffAttributeView>();
				attribute.ZZ3_ZZ1_ParentTariffOrNationalCode = tariffNonDTY.PK;
				AssertEquals("1020304", tariffNonDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Value = "3";
				AssertEquals("1020304", tariffNonDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.CheckDigit;
				AssertEquals("10203043", tariffNonDTY.GetTariffCodeWithCheckDigit());
				attribute.ZZ3_Name = "BOB";
				AssertEquals("1020304", tariffNonDTY.GetTariffCodeWithCheckDigit());
			});
		}

		public void TestIsDutiableCustomsProcedure()
		{
			RefCusProcedure procedure = null;
			AssertEquals(false, procedure.IsDutiableCustomsProcedure());
			procedure = BaseHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, true, false);
			AssertEquals(true, procedure.IsDutiableCustomsProcedure());
			procedure.ZZ6_CalculateDuty = false;
			AssertEquals(false, procedure.IsDutiableCustomsProcedure());
			procedure.ZZ6_LandedCost = true;
			AssertEquals(true, procedure.IsDutiableCustomsProcedure());
		}

		public void TestIsApplicableForDutyCalculation()
		{
			RefCusProcedure procedure = null;
			TariffView tariffView = null;
			AssertEquals(false, tariffView.IsApplicableForDutyCalculation(procedure));
			AssertEquals(false, tariffView.IsApplicableForDutyCalculation(false, new List<ZString>()));
			procedure = BaseHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			AssertEquals(false, tariffView.IsApplicableForDutyCalculation(procedure));
			AssertEquals(false, tariffView.IsApplicableForDutyCalculation(false, new List<ZString>()));
			tariffView = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "511", "RT1", "1010102030", Universal.Constants.RateTypes.Duty);
			tariffView.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			Assert("IsPayableDuty tariff is applicable whenever procedure should CalculateDuty ", tariffView.IsApplicableForDutyCalculation(procedure));
			procedure.ZZ6_CalculateDuty = false;
			Assert("IsPayableDuty tariff is not applicable whenever procedure should not CalculateDuty ", !tariffView.IsApplicableForDutyCalculation(procedure));
			CombineAssertions("IsPayableDuty tariff is applicable only if, whenever concession has a specific tariff and procedure should calculate duty, then tariff is in concession.", () =>
			{
				procedure.ZZ6_CalculateDuty = true;
				procedure.ZZ6_Concession = "12A";
				Assert("Tariff not in concession specific type list.", !tariffView.IsApplicableForDutyCalculation(procedure));
				procedure.ZZ6_Concession = "511";
				Assert("Tariff is in concession specific type list. 511", tariffView.IsApplicableForDutyCalculation(procedure));
				procedure.ZZ6_Concession = "511,512";
				Assert("Tariff is in concession specific type list. 511, 512", tariffView.IsApplicableForDutyCalculation(procedure));
				var rateType2 = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty);
				var rateCode2 = BaseHelper.LoadOrCreateNewCusRateCode(Factory, "RT2", rateType2.PK);
				Factory.Save();
				var tariffView2 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "511", "RT1", "1010102031", Universal.Constants.RateTypes.Duty);
				BaseHelper.CreateRate(tariffView2, rateCode2.PK, new ZDateTime(1990, 1, 1), new ZDateTime(1991, 1, 1));
				Assert("Tariff is in concession specific type list. 512", tariffView2.IsApplicableForDutyCalculation(procedure));
			});
			CombineAssertions("!IsPayableDuty tariff is applicable only if, whenever concession has a non-specific tariff, then tariff starts with concession.", () =>
			{
				tariffView = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "311", "RT3", "1010102032", Universal.Constants.RateTypes.Rebate);
				procedure.ZZ6_Concession = "3";
				Assert("Tariff Type starts with concession.", tariffView.IsApplicableForDutyCalculation(procedure));
				procedure.ZZ6_Concession = "4";
				Assert("Tariff Type does not starts with concession.", !tariffView.IsApplicableForDutyCalculation(procedure));
			});
		}

		public void TestIsPayableDuty()
		{
			TariffView tariff = null;
			AssertEquals(false, tariff.IsPayableDuty());
			tariff = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "511", "DTY", "1010102030", Universal.Constants.RateTypes.AntiDumping);
			var rateType2 = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.AntiDumping);
			AssertEquals(rateType2.ZZR_IsPayable, tariff.IsPayableDuty());
			rateType2.ZZR_IsPayable = false;
			var cacheKey = $"ZA_IsPayableDuty_{tariff.PK}";
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(false, tariff.IsPayableDuty());
			rateType2.ZZR_IsPayable = true;
			Factory.ClearCachedValue<bool>(cacheKey);
			AssertEquals(true, tariff.IsPayableDuty());
		}

		public void TestIsPayableDutyIsCachedInFactory()
		{
			var tariff = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "511", "DTY", "1010102030", Universal.Constants.RateTypes.AntiDumping);
			var rateType2 = BaseHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.AntiDumping);
			rateType2.ZZR_IsPayable = true;
			AssertEquals(true, tariff.IsPayableDuty());
			Factory.TryGetValueFromCacheOnly<bool>(FormattableString.Invariant($"ZA_IsPayableDuty_{tariff.PK}"), out var isPayableDutyCached);
			AssertEquals(true, isPayableDutyCached);
		}

		[TestDate(1990, 6, 1)]
		public void TestGetValidRefCusTariffRelationshipSortedDictionary()
		{
			var procedure1 = BaseHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "3", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var tariff1P1 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "1010101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff12A = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "2020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff2P2 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P2", "DTY", "2020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff3P1 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "3P1", "DTY", "3020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff4P1 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "4P1", "DTY", "4020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff5P1 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "5P1", "DTY", "5020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff6P1 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "6P1", "DTY", "6020101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff12A2 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "1030101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff2P22 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P2", "DTY", "2030101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff3P12 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "3P1", "DTY", "3030101010", Universal.Constants.RateTypes.AntiDumping);
			var tariff4P12 = ZaHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "4P1", "DTY", "4030101010", Universal.Constants.RateTypes.AntiDumping);
			var tariffType1PK = tariff1P1.ZZ1_ZZI_TariffType;
			var relationship1 = BaseHelper.CreateTariffRelationship(tariff12A.PK, tariffType1PK, "101010");
			var relationship2 = BaseHelper.CreateTariffRelationship(tariff2P2.PK, tariffType1PK, "101010");
			var relationship3 = BaseHelper.CreateTariffRelationship(tariff3P1.PK, tariffType1PK, "101010");
			var relationship4 = BaseHelper.CreateTariffRelationship(tariff4P1.PK, tariffType1PK, "101010");
			var relationship5 = BaseHelper.CreateTariffRelationship(tariff5P1.PK, tariffType1PK, "101010");
			var relationship6 = BaseHelper.CreateTariffRelationship(tariff6P1.PK, tariffType1PK, "101010");
			var relationship7 = BaseHelper.CreateTariffRelationship(tariff3P12.PK, tariffType1PK, "101010");
			var relationship8 = BaseHelper.CreateTariffRelationship(tariff4P12.PK, tariff12A.ZZ1_ZZI_TariffType, "102010");
			Factory.Save();
			RefCusProcedure procedure = null;
			var dictionary = procedure.GetValidRefCusTariffSortedDictionary("1P1", "1010101010", ZDateTime.Today);
			AssertEquals(0, dictionary.Count);
			procedure = procedure1;
			dictionary = procedure.GetValidRefCusTariffSortedDictionary("1P1", ZString.Empty, ZDateTime.Today);
			AssertEquals(0, dictionary.Count);
			dictionary = procedure.GetValidRefCusTariffSortedDictionary("1P1", "1010101010", ZDateTime.Empty);
			AssertEquals(0, dictionary.Count);
			dictionary = procedure.GetValidRefCusTariffSortedDictionary("1P1", "1010101010", ZDateTime.Today);
			AssertEquals(6, dictionary.Count);
			List<TariffView> list;
			AssertEquals(true, dictionary.TryGetValue(tariff12A.CusTariffType, out list));
			AssertEquals(1, list.Count);
			AssertCollectionContains(relationship1.RelatedTariffFrom, list);
			AssertEquals(true, dictionary.TryGetValue(tariff2P2.CusTariffType, out list));
			AssertEquals(1, list.Count);
			AssertCollectionContains(relationship2.RelatedTariffFrom, list);
			AssertEquals(true, dictionary.TryGetValue(tariff3P1.CusTariffType, out list));
			AssertEquals(2, list.Count);
			AssertCollectionContains(relationship3.RelatedTariffFrom, list);
			AssertCollectionContains(relationship7.RelatedTariffFrom, list);
			procedure.ZZ6_Concession = "4";
			dictionary = procedure.GetValidRefCusTariffSortedDictionary("1P1", "1010101010", ZDateTime.Today);
			AssertEquals(6, dictionary.Count);
			AssertEquals(true, dictionary.TryGetValue(tariff12A.CusTariffType, out list));
			AssertEquals(1, list.Count);
			AssertCollectionContains(relationship1.RelatedTariffFrom, list);
			AssertEquals(true, dictionary.TryGetValue(tariff2P2.CusTariffType, out list));
			AssertEquals(1, list.Count);
			AssertCollectionContains(relationship2.RelatedTariffFrom, list);
			AssertEquals(true, dictionary.TryGetValue(tariff4P1.CusTariffType, out list));
			AssertEquals(1, list.Count);
			AssertCollectionContains(relationship4.RelatedTariffFrom, list);
		}

		public void TestGetDA63PartList()
		{
			var factory = new BusinessObjectFactory();
			var tester = factory.GetDA63PartList();
			AssertEquals("12A, 12B, 13A, 13B, 13C, 13D, 15A, 15B, 2P1, 2P2, 2P3, FOR, PEN, PPA, PPC, PPE, PPG, PPR, PPT", tester.CodesAsString);
		}

		public void TestGetRateCodesWithEX1()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "12B" }, Factory.GetRateCodesWithEX1().Select(i => i.ZY1_RateCode));
		}

		public void TestGetRateCodesWithRateType()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "12B" }, Factory.GetRateCodesWithRateType(Constants.RateTypes.AdValoremExcise).Select(i => i.ZY1_RateCode));
			AssertContainsExactElementsInAnyOrder(new string[] { "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6" }, Factory.GetRateCodesWithRateType(Constants.RateTypes.Rebate).Select(i => i.ZY1_RateCode));
		}

		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure where ZZ6_Description like 'bob%'");
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory, ZaHelper);
		}

		UniversalReferenceTestDataHelper baseHelper;
		UniversalReferenceTestDataHelper BaseHelper => baseHelper ?? (baseHelper = new UniversalReferenceTestDataHelper(Factory));

		ZAUniversalReferenceTestDataHelper zaHelper;
		ZAUniversalReferenceTestDataHelper ZaHelper => zaHelper ?? (zaHelper = new ZAUniversalReferenceTestDataHelper(Factory));
	}
}
