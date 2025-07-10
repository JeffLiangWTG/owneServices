using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class UniversalReferenceDataHelperTest : TestCaseWithFactory
	{
		public void TestGetRateTypeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingRateType("EUN", Constants.RateTypes.Duty, "Duty");
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			Factory.Save();

			AssertEquals("Has ZA DTY ratetype", "Duty", UniversalReferenceDataHelper.GetRateTypeDescription(Factory, "ZA", "DTY"));
			AssertEquals("Has EUN ratetype", "Duty", UniversalReferenceDataHelper.GetRateTypeDescription(Factory, "IT", "DTY"));
			AssertEquals("No AU ratetype", "", UniversalReferenceDataHelper.GetRateTypeDescription(Factory, "AU", "DTY"));
			AssertEquals("No ZA ADD ratetype", "", UniversalReferenceDataHelper.GetRateTypeDescription(Factory, "ZA", "ADD"));
		}

		public void TestGetConditionTypeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTP", "Condition Type Desc FR");
			helper.CreateOrGetExistingRefCusConditionType("EUN", Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "COT", "Condition Type Desc EUN");
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			Factory.Save();

			AssertEquals("FR Has CTP Condition Type", "Condition Type Desc FR", UniversalReferenceDataHelper.GetConditionTypeDescription(Factory, "FR", "CTP"));
			AssertEquals("EUN Has COT Condition Type", "Condition Type Desc EUN", UniversalReferenceDataHelper.GetConditionTypeDescription(Factory, "IT", "COT"));
			AssertEquals("AU No Condition Type", "", UniversalReferenceDataHelper.GetConditionTypeDescription(Factory, "AU", "CTP"));
			AssertEquals("No Condition Type CCC", "", UniversalReferenceDataHelper.GetConditionTypeDescription(Factory, "FR", "CCC"));
		}

		public void TestGetTariffAdditionalCodeCategoryDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingTariffAdditionalCodeCategory(Core.Constants.CountryCodes.France, "IMP", "Statistical additional codes for import");
			helper.CreateNewOrGetExistingTariffAdditionalCodeCategory("EUN", "EXP", "Statistical additional codes for export");
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentGrouping);
			Factory.Save();
			AssertEquals("FR has IMP category", "Statistical additional codes for import", UniversalReferenceDataHelper.GetTariffAdditionalCodeCategoryDescription(Factory, Core.Constants.CountryCodes.France, "IMP"));
			AssertEquals("EUN Has EXP Category", "Statistical additional codes for export", UniversalReferenceDataHelper.GetTariffAdditionalCodeCategoryDescription(Factory, Core.Constants.CountryCodes.Italy, "EXP"));
			AssertEquals("AU has no category", ZString.Empty, UniversalReferenceDataHelper.GetTariffAdditionalCodeCategoryDescription(Factory, Core.Constants.CountryCodes.Australia, "IMP"));
			AssertEquals("FR has no EXP category", ZString.Empty, UniversalReferenceDataHelper.GetTariffAdditionalCodeCategoryDescription(Factory, Core.Constants.CountryCodes.France, "EXP"));
		}

		public void TestPrimaryPreferenceList()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroup = testHelper.CreateTradeGroup(Datagrouping, "TradeGroupTest", date1, date2);
			testHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Botswana, date1, date2);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(Datagrouping, "HSN");
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(Datagrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", Datagrouping);
			var preferenceRED = testHelper.CreatePreferenceForCountry("RED", "Reduced", Datagrouping);
			var preferenceMFN = testHelper.CreatePreferenceForCountry("MFN", "MFN", Datagrouping);
			testHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", Datagrouping);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(Datagrouping, hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			Factory.Save();
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroup, date1, date2, "add11", "ord11");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceRED.PK);
			testHelper.CreateCusApplicability(testRate2, tradeGroup, date1, date2, "add21", "ord21", secondTradeGroup: tradeGroup);
			Factory.Save();

			var date = date2.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			CombineAssertions("TestPrimaryPreferenceList", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "", "");
				var preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, false, "", "BW", null, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("Not UseUniversalTariff and no specfic prefernce Lsit", new string[] { "MFN", "STD", "RED", "AD1", "AD2", "DTY" }, preferenceList.GetAllCodes());

				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "", "BW", null, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("UseUniversalTariff but Tariff is empty", new string[] { "MFN", "STD", "RED", "AD1", "AD2", "DTY" }, preferenceList.GetAllCodes());
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "", null, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("UseUniversalTariff but country of origin is empty", new string[] { "MFN", "STD", "RED", "AD1", "AD2", "DTY" }, preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", null, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("UseUniversalTariff but No univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "", "", new HashSet<ZString> { "TradeGroupTest" });
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("match country using univiersal tariff", new string[] { "STD", "RED" }, preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.AlandIslands, Datagrouping, "", "", emptyAdditionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("no match country using univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "ord11", emptyAdditionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("match ordernumber using univiersal tariff", new string[] { "STD" }, preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "ord52", emptyAdditionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("no match ordernumber using univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				var additionalCodeSet = new HashSet<ZString>() { "add11" };
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", additionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("match additionalCode using univiersal tariff", new string[] { "STD" }, preferenceList.GetAllCodes());

				additionalCodeSet = new HashSet<ZString>() { "addXX" };
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", additionalCodeSet, date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("no match additionalCode using univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "RC1", new HashSet<ZString> { "TradeGroupTest" });
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("match rateType and rateCode using univiersal tariff", new string[] { "STD", "RED" }, preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "ADD", "");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("no match rateType using univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "TTT");
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("no match rateCode using univiersal tariff", Array.Empty<string>(), preferenceList.GetAllCodes());

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Botswana, Datagrouping, "RED", "", emptyAdditionalCodeSet, date, "", "", new HashSet<ZString> { "TradeGroupTest" });
				preferenceList = UniversalReferenceDataHelper.GetPreferenceList(Factory, true, "123456789", "BW", cusTariff, criteria, Datagrouping);
				AssertContainsExactElementsInAnyOrder("get preference lookup list regardless selected preference value", new string[] { "STD", "RED" }, preferenceList.GetAllCodes());
			});
		}

		public void TestGetCountryPreferenceList()
		{
			CombineAssertions("preference and ordernumber and additionalcode are empty", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetPreferenceListByCountry(Factory, Datagrouping);
				AssertEquals(5, preferenceList.Count);
				AssertEquals("AD1, AD2, DTY, RED, STD", preferenceList.CodesAsString);
			});

			var preferenceList2 = UniversalReferenceDataHelper.GetPreferenceListByCountry(Factory, Core.Constants.CountryCodes.Italy);
			AssertEquals("unmatch specific countryoforigin", 0, preferenceList2.Count);
		}

		public void TestGetDynamicPreferenceList_EmptyPreferenceAndOrdernumberAndAdditionalcode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "", "");

			CombineAssertions("preference and ordernumber and additionalcode are empty", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals(2, preferenceList.Count);
				Assert(preferenceList.ContainsCode("STD"));
				Assert(preferenceList.ContainsCode("RED"));
			});
		}

		public void TestGetDynamicPreferenceList_WithSpecificOrdernumber()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "ord11", emptyAdditionalCodeSet, date, "", "");
			CombineAssertions("Get Preference with specific quota(orderNumber)", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals(1, preferenceList.Count);
				Assert(preferenceList.ContainsCode("STD"));
			});
		}

		public void TestGetDynamicPreferenceList_WithSpecificAdditionalcode()
		{
			var date = endDate1.AddDays(-1);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", new HashSet<ZString> { "add11" }, date, "", "");
			CombineAssertions("Get Preference with specific additionalcode", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals(1, preferenceList.Count);
				Assert(preferenceList.ContainsCode("STD"));
			});
		}

		public void TestGetDynamicPreferenceList_WithSpecificCountryOfOrigin()
		{
			var date = endDate1.AddDays(-1);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, Datagrouping, "", "", new HashSet<ZString> { "add11" }, date, "", "");
			var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
			AssertEquals("unmatch specific countryoforigin", 0, preferenceList.Count);
		}

		public void TestGetDynamicPreferenceList_EmptyPreferenceAndZZT_Ordernumber()
		{
			var date = endDate1.AddDays(-1);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "Ord", new HashSet<ZString> { "add11" }, date, "", "");
			var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff2, criteria);
			AssertEquals("Get Preference with ZZS_Preference.IsEmpty && ZZT_OrderNumber.IsEmpty", 0, preferenceList.Count);
		}

		public void TestGetDynamicPreferenceList_MultiAdditionalcode()
		{
			var date = endDate1.AddDays(-1);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", new HashSet<ZString> { "add51", "add71" }, date, "", "");
			CombineAssertions("Get Preference with ADD rate that has additional code and DTY rate that doesn't require additional code", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff2, criteria);
				AssertEquals(3, preferenceList.Count);
				Assert(preferenceList.ContainsCode("AD1"));
				Assert(preferenceList.ContainsCode("AD2"));
				Assert(preferenceList.ContainsCode("DTY"));
			});
		}

		public void TestGetDynamicPreferenceList_SecondTradeGroup()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "ord81", emptyAdditionalCodeSet, date, "", "", new HashSet<ZString> { "XX" });
			CombineAssertions("Get Preference with SecondTradeGroup", () =>
			{
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals("No matched SecondTradeGroup with XX", 0, preferenceList.Count);

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "ord81", emptyAdditionalCodeSet, date, "", "", new HashSet<ZString> { "XX", "STANDARD" });
				preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals("testRate8 matched", "RED", preferenceList.CodesAsString);
			});
		}

		public void TestGetDynamicPreferenceList_TranslatedPreferenceDescription()
		{
			var date = endDate1.AddDays(-1);

			var staffIT = Factory.NewWithValidTestData<GlbStaff>();
			staffIT.GS_WorkingLanguage = Core.SharedConstants.Languages.Italian;
			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffIT.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "ord11", new HashSet<ZString>(), date, "", "");
				var preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals("ElementsAsString (when translated description is available)", "STD - Preferenza di base", preferenceList.ElementsAsString);

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "ord81", new HashSet<ZString>(), date, "", "");
				preferenceList = UniversalReferenceDataHelper.GetDynamicPreferenceList(cusTariff, criteria);
				AssertEquals("ElementsAsString (when translated description is not available)", "RED - Reduced", preferenceList.ElementsAsString);
			}
		}

		public void TestGetRateSelectionCriteriaInfo_EmptyTariff()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(null, criteria);
			AssertEquals("Empty tariff", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_EmptyCriteria()
		{
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, Array.Empty<IZZRateSelectionCriteria>());
			AssertEquals("Empty criteria", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_EmptyTradeGroupCountry()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria("", Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("Empty TradeGroupCountry", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_EmptyDate()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, ZDateTime.Empty, "DTY", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("Empty date", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_EmptyRateTypeAndRateCode()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("Empty rateType and rateCode means match any type and code", 4, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_NoTradeGroupCountryMatched()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.NewZealand, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("No country match", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_NoDateMatched()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, endDate2.AddDays(+1), "", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("No date match", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_NoRateTypeMatched()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "MFN", "");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("No rateType match", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo_NoRateCodeMatched()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "XXX");
			var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria);
			AssertEquals("No rateCode match", 0, result.Count());
		}

		public void TestGetRateSelectionCriteriaInfo()
		{
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1");
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "ADD", "RC2");
			var criterias = new IZZRateSelectionCriteria[] { criteria, criteria1 };
			var result1 = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match tradeGroupStandard and date and rateType and rateCode", () =>
			{
				AssertEquals("count", 4, result1.Length);

				Helper.AssertRateSelectionCriteriaInfoResult(result1[0], "AU", testDate, "DTY", "RC1", "ord11", "add11", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[1], "AU", testDate, "DTY", "RC1", "ord21", "add21", "RED", "Reduced", "STANDARD", "STANDARD DEC");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[2], "AU", testDate, "ADD", "RC2", "ord31", "add31", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[3], "AU", testDate, "ADD", "RC2", "ord81", "add81", "RED", "Reduced", "STANDARD", "STANDARD DEC", "STANDARD");
			});
		}

		public void TestGetRateSelectionCriteriaInfo_SecondTradeGroup()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			CombineAssertions("Get RateSelectionCriteriaInfo with SecondTradeGroup", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "ADD", "RC2", new HashSet<ZString> { "XX", "STANDARD" });
				var result = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(cusTariff, criteria).OrderBy(x => x.ZZT_OrderNumber).ToArray();
				AssertEquals("The criteria of the rate with matched ZZT_ZZA_TradeGroup selected as well", 3, result.Length);
				Helper.AssertRateSelectionCriteriaInfoResult(result[0], "AU", date, "ADD", "RC2", "ord31", "add31", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertRateSelectionCriteriaInfoResult(result[1], "AU", date, "ADD", "RC2", "ord32", "add32", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertRateSelectionCriteriaInfoResult(result[2], "AU", date, "ADD", "RC2", "ord81", "add81", "RED", "Reduced", "STANDARD", "STANDARD DEC", "STANDARD");
			});
		}

		public void TestGetDynamicOrderNumberList_UnmatchCountryoforigin()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
			AssertEquals("unmatch specific countryoforigin", 0, orderNumbersList.Count);
		}

		public void TestGetDynamicOrderNumberList_UnmatchRateType()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "XXX", "RC1");
			var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
			AssertEquals("unmatch specific rateType", 0, orderNumbersList.Count);
		}

		public void TestGetDynamicOrderNumberList_UnmatchRateCode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "DTY", "XXX");
			var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
			AssertEquals("unmatch specific rateCode", 0, orderNumbersList.Count);
		}

		public void TestGetDynamicOrderNumberList_MatchAllPreference()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			CombineAssertions("empty preference means match any perference value", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals(4, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord12"));
				Assert(orderNumbersList.ContainsCode("ord21"));
				Assert(orderNumbersList.ContainsCode("ord22"));
			});
		}

		public void TestGetDynamicOrderNumberList_MatchPreferenceAndAllRateTypeAndRateCode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "", "");
			CombineAssertions("match specific preference and all rateType and rateCode", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals(4, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord12"));
				Assert(orderNumbersList.ContainsCode("ord31"));
				Assert(orderNumbersList.ContainsCode("ord32"));
			});
		}

		public void TestGetDynamicOrderNumberList_MatchPreferenceAndTradeCountryAndRateTypeAndRateCode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			CombineAssertions("match specific preference and trade country and rateType and rateCode", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals(2, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord12"));
			});
		}

		public void TestGetDynamicOrderNumberList_MatchPreferenceAndAdditionalCode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", new HashSet<ZString>() { "add11" }, date, "DTY", "RC1");
			CombineAssertions("match specific preference and addionalcode", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals(1, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
			});
		}

		public void TestGetDynamicOrderNumberList_MultiCriterias()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, endDate1.AddDays(+1), "DTY", "RC1");
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, endDate1.AddDays(+1), "ADD", "RC2");
			var criterias = new IZZRateSelectionCriteria[] { criteria, criteria1 };
			CombineAssertions("match effective date and multi ratetype", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criterias);
				AssertEquals(2, orderNumbersList.Count);
				Assert(orderNumbersList.ContainsCode("ord11"));
				Assert(orderNumbersList.ContainsCode("ord31"));
			});
		}

		public void TestGetDynamicOrderNumberList_SecondTradeGroup()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "RED", "", emptyAdditionalCodeSet, date, "ADD", "RC2", new HashSet<ZString> { "XX" });
			CombineAssertions("Get DynamicOrderNumberList with SecondTradeGroup", () =>
			{
				var orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals("No matched SecondTradeGroup with XX", 0, orderNumbersList.Count);

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "RED", "", emptyAdditionalCodeSet, date, "ADD", "RC2", new HashSet<ZString> { "XX", "STANDARD" });
				orderNumbersList = UniversalReferenceDataHelper.GetDynamicOrderNumberList(cusTariff, criteria);
				AssertEquals("Matched SecondTradeGroup with STANDARD", "ord81", orderNumbersList.CodesAsString);
			});
		}

		public void TestGetDynamicRateApplicabilityCodeList_UnmatchCountryOfOrigin()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
			AssertEquals("unmatch specific countryoforigin", 0, additionalCodesList.Count);
		}

		public void TestGetDynamicRateApplicabilityCodeList_UnmatchRateType()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "XXX", "RC1");
			var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
			AssertEquals("unmatch specific rateType", 0, additionalCodesList.Count);
		}

		public void TestGetDynamicRateApplicabilityCodeList_UnmatchRateCode()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "XXX");
			var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
			AssertEquals("unmatch specific rateCode", 0, additionalCodesList.Count);
		}

		public void TestGetDynamicRateApplicabilityCodeList_PreferenceOrdernumberAdditionalcodeEmpty()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			CombineAssertions("preference and ordernumber and additionalcode are empty", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12", "add21", "add22" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 12", additionalCodesList.GetDescriptionFromCode("add12"));
				AssertEquals("desciption 21", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("desciption 22", additionalCodesList.GetDescriptionFromCode("add22"));
			});
		}

		public void TestGetDynamicRateApplicabilityCodeList_MatchPerference()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "", emptyAdditionalCodeSet, date, "DTY", "RC1");
			CombineAssertions("match perference", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 12", additionalCodesList.GetDescriptionFromCode("add12"));
			});
		}

		public void TestGetDynamicRateApplicabilityCodeList_MultiCriterias()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, endDate1.AddDays(+1), "DTY", "RC1");
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, endDate1.AddDays(+1), "ADD", "RC2");
			var criterias = new IZZRateSelectionCriteria[] { criteria, criteria1 };
			CombineAssertions("match effectivedate for multi criterias", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criterias);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add21", "add31", "add81" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 21", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("desciption 31", additionalCodesList.GetDescriptionFromCode("add31"));
			});
		}

		public void TestGetDynamicRateApplicabilityCodeList_MatchOrdernumberPreference()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "STD", "ord11", emptyAdditionalCodeSet, date, "DTY", "RC1");
			CombineAssertions("match ordernumber and preference", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
			});
		}

		public void TestGetDynamicRateApplicabilityCodeList_SecondTradeGroup()
		{
			var date = endDate1.AddDays(-1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "RED", "", emptyAdditionalCodeSet, date, "ADD", "RC2", new HashSet<ZString> { "XX" });
			CombineAssertions("Get DynamicRateApplicabilityCodeList with SecondTradeGroup", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
				AssertEquals("No matched SecondTradeGroup with XX", 0, additionalCodesList.Count);

				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "RED", "", emptyAdditionalCodeSet, date, "ADD", "RC2", new HashSet<ZString> { "XX", "STANDARD" });
				additionalCodesList = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(cusTariff, additionalCodeDescriptions, criteria);
				AssertEquals("Matched SecondTradeGroup with STANDARD", "add81", additionalCodesList.CodesAsString);
			});
		}

		public void TestGetTariffAdditionalCodeApplicabilities()
		{
			var date = endDate1.AddDays(-1);
			var criteria = Helper.CreateTariffAdditionalCodeSelectionCriteria("IMP", date, Core.Constants.CountryCodes.Australia, Datagrouping);
			var additionalCodeList = UniversalReferenceDataHelper.GetTariffAdditionalCodeApplicabilities(cusTariff, criteria).Select(x => x.ZY2_AdditionalCode);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "add99" }, additionalCodeList);
		}

		public void TestGetConditionApplicabilitiesByCriteria_EmptyTariff()
		{
			SetupCondition();
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var result = UniversalReferenceDataHelper.GetConditionApplicabilitiesByCriteria(null, new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("Empty tariff", 0, result.Count());
		}

		public void TestGetConditionApplicabilitiesByCriteria_EmptyCriteria()
		{
			SetupCondition();

			var result = UniversalReferenceDataHelper.GetConditionApplicabilitiesByCriteria(cusTariff, Array.Empty<IZZConditionSelectionCriteria>());
			AssertEquals("Empty criteria", 0, result.Count());
		}

		public void TestGetConditionApplicabilitiesByCriteria_MultiCriterias()
		{
			SetupCondition();
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria1 = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "CTRL", "TY1");
			var criteria2 = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "RATE", "TY2");
			var criterias = new IZZConditionSelectionCriteria[] { criteria1, criteria2 };
			var result = UniversalReferenceDataHelper.GetConditionApplicabilitiesByCriteria(cusTariff, criterias);
			var result1 = result.OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match tradeGroupStandard and date and conditionClass and conditionType", () =>
			{
				AssertSame("Result is cached", result, UniversalReferenceDataHelper.GetConditionApplicabilitiesByCriteria(cusTariff, criterias));
				AssertEquals("count", 4, result1.Length);

				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[0], "AU", testDate, "RATE", "TY2", "", "", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[1], "AU", testDate, "CTRL", "TY1", "orn11", "cdd11", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[2], "AU", testDate, "CTRL", "TY1", "orn12", "cdd12", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[3], "AU", testDate, "RATE", "TY2", "orn31", "cdd31", "STD", "Standard", "STANDARD", "STANDARD DEC");
			});
		}

		public void TestGetConditionApplicabilitiesByCriteria_SecondTradeGroup()
		{
			SetupCondition();
			var conditionType1 = Helper.CreateOrGetExistingRefCusConditionType(Datagrouping, "CTRL", "TY1");
			var preferenceRED = Helper.CreatePreferenceForCountry("RED", "Reduced", Datagrouping);
			var conditionCode7 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType1.PK, cusTariff2.PK, "", false, true, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode7, tradeGroupStandard, startDate, endDate2, "cdd71", "orn71", tradeGroupStandard);
			Factory.Save();

			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = endDate1.AddDays(+1);

			var criteria1 = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Export, "CTRL", "TY1", new HashSet<ZString> { "STANDARD" });
			var criterias = new IZZConditionSelectionCriteria[] { criteria1 };
			var result = UniversalReferenceDataHelper.GetConditionApplicabilitiesByCriteria(cusTariff2, criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match SecondTradeGroup", () =>
			{
				AssertEquals("count", 2, result.Length);
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result[0], "AU", testDate, "CTRL", "TY1", "orn51", "cdd51", "RED", "Reduced", "STANDARD", "STANDARD DEC", "");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result[1], "AU", testDate, "CTRL", "TY1", "orn71", "cdd71", "RED", "Reduced", "STANDARD", "STANDARD DEC", "STANDARD");
			});
		}

		public void TestGetDynamicAdditionalCodeList_RateAdditionalCodeListIsNull()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			IZZRateSelectionCriteria rateCriteria = null;
			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var vatCriteria = Helper.CreateVATSelectionCriteria(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testDate, "tax1", emptyAdditionalCodeSet, new HashSet<ZString>());
			AssertNoExceptionThrown(() =>
			{
				UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, new IZZRateSelectionCriteria[] { rateCriteria }, new IZZConditionSelectionCriteria[] { conditionCriteria }, Array.Empty<ZString>(), vatCriteria, null);
			});
		}

		public void TestGetDynamicAdditionalCodeList_RateAdditionalCodeListIsEmpty()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var rateCriteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.NewZealand, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "", "");
			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var vatCriteria = Helper.CreateVATSelectionCriteria(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testDate, "tax1", emptyAdditionalCodeSet, new HashSet<ZString>());
			CombineAssertions("Rate additional code list is empty", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, new IZZRateSelectionCriteria[] { rateCriteria }, new IZZConditionSelectionCriteria[] { conditionCriteria }, Array.Empty<ZString>(), vatCriteria, null);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "cdd11", "cdd12", "cdd31", "add91" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11 condition", additionalCodesList.GetDescriptionFromCode("cdd11"));
				AssertEquals("desciption 12 condition", additionalCodesList.GetDescriptionFromCode("cdd12"));
				AssertEquals("desciption 31 condition", additionalCodesList.GetDescriptionFromCode("cdd31"));
				AssertEquals("desciption 91", additionalCodesList.GetDescriptionFromCode("add91"));
			});
		}

		public void TestGetDynamicAdditionalCodeList_ConditionAdditionalCodeListIsEmpty()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var rateCriteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "", "");
			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.NewZealand, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var vatCriteria = Helper.CreateVATSelectionCriteria(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testDate, "tax1", emptyAdditionalCodeSet, new HashSet<ZString>());
			CombineAssertions("Condition additional code list is empty", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, new IZZRateSelectionCriteria[] { rateCriteria }, new IZZConditionSelectionCriteria[] { conditionCriteria }, Array.Empty<ZString>(), vatCriteria, null);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add21", "add31", "add81", "add91" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 21", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("desciption 31", additionalCodesList.GetDescriptionFromCode("add31"));
				AssertEquals("desciption 81", additionalCodesList.GetDescriptionFromCode("add81"));
				AssertEquals("desciption 91", additionalCodesList.GetDescriptionFromCode("add91"));
			});
		}

		public void TestGetDynamicAdditionalCodeList_ConditionTypesToExclude()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var conditionTypesToExclude = new ZString[] { "TY1" };
			CombineAssertions("Condition additional code list does not contain codes with excluded Condition Types.", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, Array.Empty<IZZRateSelectionCriteria>(), new IZZConditionSelectionCriteria[] { conditionCriteria }, conditionTypesToExclude, null, null);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "cdd31" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 31 condition", additionalCodesList.GetDescriptionFromCode("cdd31"));
			});
		}

		public void TestGetDynamicAdditionalCodeList_VATAdditionalCodeListIsEmpty()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var rateCriteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, "", "", emptyAdditionalCodeSet, testDate, "", "");
			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", Datagrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var vatCriteria = Helper.CreateVATSelectionCriteria(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testDate, "tax2", emptyAdditionalCodeSet, new HashSet<ZString>());
			CombineAssertions("Condition additional code list is empty", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, new IZZRateSelectionCriteria[] { rateCriteria }, new IZZConditionSelectionCriteria[] { conditionCriteria }, Array.Empty<ZString>(), vatCriteria, null);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "cdd11", "cdd12", "cdd31", "add11", "add21", "add31", "add81" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11 condition", additionalCodesList.GetDescriptionFromCode("cdd11"));
				AssertEquals("desciption 12 condition", additionalCodesList.GetDescriptionFromCode("cdd12"));
				AssertEquals("desciption 31 condition", additionalCodesList.GetDescriptionFromCode("cdd31"));
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 21", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("desciption 31", additionalCodesList.GetDescriptionFromCode("add31"));
				AssertEquals("desciption 81", additionalCodesList.GetDescriptionFromCode("add81"));
			});
		}

		public void TestGetDynamicAdditionalCodeList()
		{
			SetupCondition();
			var testDate = endDate1.AddDays(1);
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var additionalCodeDescriptions = GetTestAdditionalCodeDescriptions();

			var rateCriteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, Datagrouping, ZString.Empty, ZString.Empty, emptyAdditionalCodeSet, testDate, ZString.Empty, ZString.Empty);
			var conditionCriteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, ZString.Empty, emptyAdditionalCodeSet, ZString.Empty, Datagrouping, ConditionChecker.ConditionDirection.Import, ZString.Empty, ZString.Empty);
			var vatCriteria = Helper.CreateVATSelectionCriteria(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testDate, "tax1", emptyAdditionalCodeSet, new HashSet<ZString>());
			var tariffAdditionalCodeCriteria = Helper.CreateTariffAdditionalCodeSelectionCriteria("IMP", testDate, Core.Constants.CountryCodes.Australia, Datagrouping);
			CombineAssertions("Include rate, condition, vat and tariff additional codes", () =>
			{
				var additionalCodesList = UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(cusTariff, additionalCodeDescriptions, new IZZRateSelectionCriteria[] { rateCriteria }, new IZZConditionSelectionCriteria[] { conditionCriteria }, Array.Empty<ZString>(), vatCriteria, tariffAdditionalCodeCriteria);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add21", "add31", "cdd11", "cdd12", "cdd31", "add81", "add91", "add99" }, additionalCodesList.GetAllCodesZString());
				AssertEquals("desciption 11", additionalCodesList.GetDescriptionFromCode("add11"));
				AssertEquals("desciption 21", additionalCodesList.GetDescriptionFromCode("add21"));
				AssertEquals("desciption 31", additionalCodesList.GetDescriptionFromCode("add31"));
				AssertEquals("desciption 11 condition", additionalCodesList.GetDescriptionFromCode("cdd11"));
				AssertEquals("desciption 12 condition", additionalCodesList.GetDescriptionFromCode("cdd12"));
				AssertEquals("desciption 31 condition", additionalCodesList.GetDescriptionFromCode("cdd31"));
				AssertEquals("desciption 81", additionalCodesList.GetDescriptionFromCode("add81"));
				AssertEquals("desciption 91", additionalCodesList.GetDescriptionFromCode("add91"));
				AssertEquals("desciption 99", additionalCodesList.GetDescriptionFromCode("add99"));
			});
		}

		void SetupCondition()
		{
			var preferenceSTD = Helper.CreatePreferenceForCountry("STD", "Standard", Datagrouping);
			var preferenceRED = Helper.CreatePreferenceForCountry("RED", "Reduced", Datagrouping);

			var conditionType1 = Helper.CreateOrGetExistingRefCusConditionType(Datagrouping, "CTRL", "TY1");
			var conditionType2 = Helper.CreateOrGetExistingRefCusConditionType(Datagrouping, "RATE", "TY2");

			var conditionCode1 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType1.PK, cusTariff.PK, "", true, false, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate2, "cdd11", "orn11");
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate2, "cdd12", "orn12");
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate1, "cdd13", "orn13");

			var conditionCode2 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType1.PK, cusTariff.PK, "", false, true, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode2, tradeGroupStandard, startDate, endDate2, "cdd21", "orn21");

			var conditionCode3 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType2.PK, cusTariff.PK, "", true, false, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode3, tradeGroupStandard, startDate, endDate2, "cdd31", "orn31");
			Helper.CreateCusApplicability(conditionCode3, tradeGroupStandard, startDate, endDate2);

			var conditionCode4 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType1.PK, cusTariff2.PK, "", true, false, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate2, "cdd41", "orn41");
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate2, "cdd42", "orn42");
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate1, "cdd43", "orn43");

			var conditionCode5 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType1.PK, cusTariff2.PK, "", false, true, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode5, tradeGroupStandard, startDate, endDate2, "cdd51", "orn51");

			var conditionCode6 = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, conditionType2.PK, cusTariff2.PK, "", true, false, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode6, tradeGroupStandard, startDate, endDate2, "cdd61", "orn61");
			Helper.CreateCusApplicability(conditionCode6, tradeGroupStandard, startDate, endDate2);

			Factory.Save();
		}

		static CodeDescriptionPairList GetTestAdditionalCodeDescriptions()
		{
			var additionalCodeDescriptions = new CodeDescriptionPairList();
			additionalCodeDescriptions.AddPair("add11", "desciption 11");
			additionalCodeDescriptions.AddPair("add12", "desciption 12");
			additionalCodeDescriptions.AddPair("add21", "desciption 21");
			additionalCodeDescriptions.AddPair("add22", "desciption 22");
			additionalCodeDescriptions.AddPair("add31", "desciption 31");
			additionalCodeDescriptions.AddPair("add32", "desciption 32");
			additionalCodeDescriptions.AddPair("add81", "desciption 81");
			additionalCodeDescriptions.AddPair("add91", "desciption 91");
			additionalCodeDescriptions.AddPair("add99", "desciption 99");
			additionalCodeDescriptions.AddPair("Other", "desciption Other");
			additionalCodeDescriptions.AddPair("cdd11", "desciption 11 condition");
			additionalCodeDescriptions.AddPair("cdd12", "desciption 12 condition");
			additionalCodeDescriptions.AddPair("cdd13", "desciption 13 condition");
			additionalCodeDescriptions.AddPair("cdd21", "desciption 21 condition");
			additionalCodeDescriptions.AddPair("cdd31", "desciption 31 condition");
			additionalCodeDescriptions.AddPair("cdd41", "desciption 41 condition");
			additionalCodeDescriptions.AddPair("cdd42", "desciption 42 condition");
			additionalCodeDescriptions.AddPair("cdd43", "desciption 43 condition");
			additionalCodeDescriptions.AddPair("cdd51", "desciption 51 condition");
			additionalCodeDescriptions.AddPair("cdd61", "desciption 61 condition");
			return additionalCodeDescriptions;
		}

		protected override void SetUp()
		{
			startDate = new ZDate(2010, 01, 01);
			endDate1 = new ZDate(2019, 11, 30);
			endDate2 = new ZDate(2029, 06, 06);

			Helper.CreateOrGetLanguage("IT", "Italian");

			tradeGroupStandard = Helper.CreateTradeGroup(Datagrouping, "STANDARD", startDate, endDate2, "STANDARD DEC");
			Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate2);
			Factory.Save();

			var hsnTariffType = Helper.CreateNewOrGetExistingTariffType(Datagrouping, "HSN");
			Factory.Save();
			var dutyRateType = Helper.CreateNewOrGetExistingRateType(Datagrouping, Constants.RateTypes.Duty, "Duty");
			var rateCode1 = Helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var addRateType = Helper.CreateNewOrGetExistingRateType(Datagrouping, Constants.RateTypes.AntiDumping, "ADD");
			var rateCode2 = Helper.LoadOrCreateNewCusRateCode(Factory, "RC2", addRateType.PK);
			Factory.Save();
			var preferenceSTD = Helper.CreatePreferenceForCountry("STD", "Standard", Datagrouping);
			Helper.CreatePreferenceLanguage(preferenceSTD, "IT", "Preferenza di base");
			var preferenceRED = Helper.CreatePreferenceForCountry("RED", "Reduced", Datagrouping);
			var preferenceDTY = Helper.CreatePreferenceForCountry("DTY", "preferenceDTY", Datagrouping);
			var preferenceADD1 = Helper.CreatePreferenceForCountry("AD1", "preferenceADD1", Datagrouping);
			var preferenceADD2 = Helper.CreatePreferenceForCountry("AD2", "preferenceADD2", Datagrouping);
			Factory.Save();

			cusTariff = Helper.CreateTariff(Datagrouping, hsnTariffType.PK, "DUMMYTRF", startDate, endDate2, "dummy Description 0");
			cusTariff2 = Helper.CreateTariff(Datagrouping, hsnTariffType.PK, "TARIFF2", startDate, endDate2, "dummy Description 0");
			Factory.Save();

			Helper.CreateVATApplicabilityView(cusTariff, Datagrouping, "tax1", startDate, endDate2, additionalCode: "add91", tradeGroup: tradeGroupStandard.PK);

			var tariffAdditionalCode = Helper.CreateTariffAdditionalCodeView(cusTariff, "IMP", "add99", Datagrouping);
			Helper.CreateCusApplicability(tariffAdditionalCode, tradeGroupStandard, startDate, endDate2);

			var testRate1 = Helper.CreateRate(cusTariff, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceSTD.PK);
			Helper.CreateCusApplicability(testRate1, tradeGroupStandard, startDate, endDate2, "add11", "ord11");
			Helper.CreateCusApplicability(testRate1, tradeGroupStandard, startDate, endDate1, "add12", "ord12");

			var testRate2 = Helper.CreateRate(cusTariff, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceRED.PK);
			Helper.CreateCusApplicability(testRate2, tradeGroupStandard, startDate, endDate2, "add21", "ord21");
			Helper.CreateCusApplicability(testRate2, tradeGroupStandard, startDate, endDate1, "add22", "ord22");

			var testRate3 = Helper.CreateRate(cusTariff, rateCode2.PK, startDate, endDate2, "0", preferencePk: preferenceSTD.PK);
			Helper.CreateCusApplicability(testRate3, tradeGroupStandard, startDate, endDate2, "add31", "ord31");
			Helper.CreateCusApplicability(testRate3, tradeGroupStandard, startDate, endDate1, "add32", "ord32");

			var testRate4 = Helper.CreateRate(cusTariff2, rateCode1.PK, startDate, endDate2, "0", preferencePk: preferenceDTY.PK);
			Helper.CreateCusApplicability(testRate4, tradeGroupStandard, startDate, endDate2, "", "");

			var testRate5 = Helper.CreateRate(cusTariff2, rateCode2.PK, startDate, endDate2, "0", preferencePk: preferenceADD1.PK);
			Helper.CreateCusApplicability(testRate5, tradeGroupStandard, startDate, endDate2, "add51", "");

			var testRate6 = Helper.CreateRate(cusTariff2, rateCode2.PK, startDate, endDate2, "0");
			Helper.CreateCusApplicability(testRate6, tradeGroupStandard, startDate, endDate2, "", "");

			var testRate7 = Helper.CreateRate(cusTariff2, rateCode2.PK, startDate, endDate2, "0.5", preferencePk: preferenceADD2.PK);
			Helper.CreateCusApplicability(testRate7, tradeGroupStandard, startDate, endDate2, "add71", "");

			var testRate8 = Helper.CreateRate(cusTariff, rateCode2.PK, startDate, endDate2, "0", preferencePk: preferenceRED.PK);
			Helper.CreateCusApplicability(testRate8, tradeGroupStandard, startDate, endDate2, "add81", "ord81", secondTradeGroup: tradeGroupStandard);

			Factory.Save();
		}

		static ZString Datagrouping => GlbCompany.CurrentCompany.Country.Code;

		TariffView cusTariff, cusTariff2;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
		ZDate startDate;
		ZDate endDate1;
		ZDate endDate2;
		CusRefTradeGroupView tradeGroupStandard;
	}
}
