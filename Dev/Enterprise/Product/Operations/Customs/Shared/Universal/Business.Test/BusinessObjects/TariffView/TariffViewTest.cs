using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffView))]
	sealed class TariffViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetHighestVATRate()
		{
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentDataGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTaxOrFee("RID", 0.05m, Core.Constants.CountryCodes.Italy);
			helper.CreateTaxOrFee("ORD", 0.1m, Core.Constants.CountryCodes.Italy);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, "ORD");
			Factory.Save();
			AssertEquals(0.1m, tariff.GetHighestVATRateByTaxOrFeeCode("IT", "", ZDate.Today));
		}

		public void TestGetHighestVATRateByTaxOrFeeCode()
		{
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentDataGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTaxOrFee("RID", 0.05m, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateTaxOrFee("RID", 0.04m, Core.Constants.CountryCodes.Italy);
			helper.CreateTaxOrFee("ORD", 0.1m, Core.Constants.CountryCodes.Italy);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, "ORD");
			Factory.Save();
			AssertEquals(0.05m, tariff.GetHighestVATRateByTaxOrFeeCode("IT", "RID", ZDate.Today));
		}

		public void TestGetHighestVATRateByTaxOrFeeCodeWhenVATisnotEntered()
		{
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentDataGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			AssertEquals(0m, tariff.GetHighestVATRateByTaxOrFeeCode("IT", ZString.Empty, ZDate.Today));
		}

		public void TestGetHighestVATRateByTaxOrFeeCodeWhenNoVATApplicability()
		{
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
			Factory.Save();
			var tariffWithApplicability = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffWithoutApplicability = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTaxOrFee("RID", 0.04m, Core.Constants.CountryCodes.Italy);
			helper.CreateTaxOrFee("RID", 0.04m, Core.Constants.CountryCodes.Latvia);
			helper.CreateTaxOrFee("ORD", 0.1m, Core.Constants.CountryCodes.Italy);

			AssertEquals(0.1m, tariffWithoutApplicability.GetHighestVATRateByTaxOrFeeCode("IT", "ORD", ZDate.Today));

			helper.CreateNewOrGetExistingVATApplicability(tariffWithApplicability, Core.Constants.CountryCodes.Italy, "RID");

			AssertEquals(0.04m, tariffWithApplicability.GetHighestVATRateByTaxOrFeeCode("IT", "RID", ZDate.Today));
			AssertEquals(0.04m, tariffWithApplicability.GetHighestVATRateByTaxOrFeeCode("LV", "XXX", ZDate.Today));
		}

		public void TestZZ1_TariffTypeDescription()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_IsSystem = false;
			tariff.ZZ1_ZZI_NKTariffType = Constants.TariffTypes.HarmonizedSystem;

			AssertEquals("HSN type description", Constants.TariffTypes.HarmonizedSystemDescription, tariff.ZZ1_TariffTypeDescription);
		}

		public void TestUnitList_HasUoms()
		{
			var tariff = CreateTariff(true);
			AssertEquals("The valid UOM code", "NO", tariff.UnitList.CodesAsString);
		}

		public void TestUnitList_NoUoms()
		{
			var tariff = CreateTariff(false);
			AssertEquals(0, tariff.UnitList.Count);
		}

		TariffView CreateTariff(bool createUOM)
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ");
			helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilogram", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "NO", "Number", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffType = helper.CreateNewOrGetExistingTariffType(country, "DTY");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(country, tariffType.PK, "99999999", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "My Description", isSystem: false);
			if (createUOM)
			{
				helper.CreateTariffUOM(tariff, "CU1", "NO");
				helper.CreateTariffUOM(tariff, "CU2", "ZZ"); // Invalid uom which not in Unit list of the tariff "KG, NO"
			}

			return tariff;
		}

		public void TestRateSelectionCriteriaInfo()
		{
			var date = new ZDate(2020, 01, 15);
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "AU", "Pre1", "Ord1", new HashSet<ZString>()
			{ "Add1", "Add3" }, date, "DTY", "RC1");
			var criteria2 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "AU", "Pre2", "Ord2", new HashSet<ZString>()
			{ "Add2", "Add3" }, date, "ADD", "RC2");
			var rateSelectionCriteriaInfo = new RateSelectionCriteriaInfo { EffectiveDate = date, TradeGroupCountry = "AU", RateType = "DTY", RateCode = "RC1", ZZT_OrderNumber = "Ord1", ZZT_AdditionalCode = "Add1", ZZA_TradeGroup = "TT", ZZA_Description = "TT DEC", ZZS_Preference = "Pre1", ZZS_Description = "Pre1 Dec", };
			Assert(rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria1));
			Assert(rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria1));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria2));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria2));
		}

		public void TestNullValuesForRateSelectionCriteriaInfo()
		{
			var date1 = ZDate.Today.AddDays(-1);
			var date2 = ZDate.Today.AddDays(1);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(DataGrouping, "HSN");
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(DataGrouping, Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(DataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date2, "dummy Description 0");
			Factory.Save();
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2);
			testHelper.CreateCusApplicabilityInternal(testRate1, null, date1, date2);
			Factory.Save();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", new HashSet<ZString>(), ZDate.Today, Constants.RateTypes.Duty, "RC1");
			var criterias = new IZZRateSelectionCriteria[] { criteria };
			CombineAssertions(() =>
			{
				var result = cusTariff.GetRateSelectionCriteriaInfo(criterias);
				AssertEquals("Prereq: There is 1 rate selection", 1, result.Count());
				var rateSelection = result.First();
				AssertEquals("AU", rateSelection.TradeGroupCountry);
				AssertEquals("DTY", rateSelection.RateType);
				AssertEquals("RC1", rateSelection.RateCode);
				AssertEquals(ZString.Empty, rateSelection.ZZT_OrderNumber);
				AssertEquals(ZString.Empty, rateSelection.ZZT_AdditionalCode);
				AssertEquals(ZString.Empty, rateSelection.ZZA_TradeGroup);
				AssertEquals(ZString.Empty, rateSelection.ZZA_Description);
				AssertEquals(ZString.Empty, rateSelection.ZZS_Preference);
				AssertEquals(ZString.Empty, rateSelection.ZZS_Description);
			});
		}

		public void TestGetRateSelectionCriteriaInfo_SecondTradeGroup()
		{
			var testTariffs = SetupRefDataForTariff();
			var tariff = testTariffs.tariff;
			var testDate = new ZDate(2019, 12, 10);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "ord6", new HashSet<ZString> { "add6" }, testDate, "", "", new HashSet<ZString> { "tradeGroup2" });
			var criterias = new IZZRateSelectionCriteria[] { criteria };
			var result = tariff.GetRateSelectionCriteriaInfo(criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Matched SecondTradeGroup", 5, result.Length);
				Helper.AssertRateSelectionCriteriaInfoResult(result[0], "AU", testDate, "DTY", "RC1", "", "", "RED", "Reduced", "tradeGroup2", "tradeGroup2 dec", "");
				Helper.AssertRateSelectionCriteriaInfoResult(result[1], "AU", testDate, "DTY", "RC1", "ord11", "add11", "STD", "Standard", "tradeGroup1", "tradeGroup1 dec", "");
				Helper.AssertRateSelectionCriteriaInfoResult(result[2], "AU", testDate, "DTY", "RC1", "ord31", "add31", "", "", "tradeGroup2", "tradeGroup2 dec", "");
				Helper.AssertRateSelectionCriteriaInfoResult(result[3], "AU", testDate, "ADD", "RC2", "ord41", "add41", "STD", "Standard", "tradeGroup2", "tradeGroup2 dec", "");
				Helper.AssertRateSelectionCriteriaInfoResult(result[4], "AU", testDate, "ADD", "RC2", "ord6", "add6", "RED", "Reduced", "tradeGroup1", "tradeGroup1 dec", "tradeGroup2");
			});
		}

		public void TestGetRateSelectionCriteriaInfo_TranslatedPreferenceLanguage()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateOrGetLanguage("IT", "Italian");
			var tradeGroup = refDataHelper.CreateTradeGroup(DataGrouping, "TG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Trade Group description");
			refDataHelper.AddCountry(tradeGroup, "AU");
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType(DataGrouping, "HSN");
			var dutyRateType = refDataHelper.CreateNewOrGetExistingRateType(DataGrouping, "DTY", "Duty");
			var dutyRateCode = refDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var preferenceSTD = refDataHelper.CreatePreferenceForCountry("STD", "Standard", DataGrouping);
			refDataHelper.CreatePreferenceLanguage(preferenceSTD, "IT", "Preferenza di base");
			var tariff = refDataHelper.CreateTariff(DataGrouping, tariffType.PK, "TARIFF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff description");
			var rate = refDataHelper.CreateRate(tariff, dutyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preferencePk: preferenceSTD.PK);
			refDataHelper.CreateCusApplicabilityInternal(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var staffIT = Factory.NewWithValidTestData<GlbStaff>();
			staffIT.GS_WorkingLanguage = Core.SharedConstants.Languages.Italian;
			var staffDE = Factory.NewWithValidTestData<GlbStaff>();
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			Factory.Save();

			var today = ZDate.Today;
			var criteria = Helper.CreateRateSelectionCriteria("AU", DataGrouping, "", "", new HashSet<ZString>(), today, "", "", new HashSet<ZString>());

			using (Env.Instance.SetTemporaryUserContext(staffIT.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertTranslatedPreferenceDescription(expectedTranslatedPreferenceDescription: "Preferenza di base");
			}

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertTranslatedPreferenceDescription(expectedTranslatedPreferenceDescription: "");
			}

			void AssertTranslatedPreferenceDescription(string expectedTranslatedPreferenceDescription)
			{
				var rateSelectionCriteriaInfo = tariff.GetRateSelectionCriteriaInfo(new IZZRateSelectionCriteria[] { criteria }).ToArray();
				AssertEquals("RateSelectionCriteriaInfo Count", 1, rateSelectionCriteriaInfo.Length);
				AssertEquals("TranslatedPreferenceDescription", expectedTranslatedPreferenceDescription, rateSelectionCriteriaInfo[0].TranslatedPreferenceDescription);
			}
		}

		public void TestGetRateSelectionCriteriaInfo()
		{
			var testTariffs = SetupRefDataForTariff();
			var tariff = testTariffs.tariff;
			var nationalCodeTariff = testTariffs.nationalCodeTariff;
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = new ZDate(2019, 12, 10);
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.NewZealand, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "", "");
			var criterias = new IZZRateSelectionCriteria[] { criteria };
			var result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("No country match", 0, result.Count());
			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, new ZDate(2029, 12, 06), "", "");
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("No date match", 0, result.Count());
			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "MFN", "");
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("No rateType match", 0, result.Count());
			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "XXX");
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("No rateCode match", 0, result.Count());
			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZZ", "", "", emptyAdditionalCodeSet, testDate, "DTY", "XXX");
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("No datagrouping match", 0, result.Count());
			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1");
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "ADD", "RC2");
			criterias = new IZZRateSelectionCriteria[] { criteria, criteria1 };
			var result1 = tariff.GetRateSelectionCriteriaInfo(criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();
			var result2 = nationalCodeTariff.GetRateSelectionCriteriaInfo(criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match tradeGroupStandard and date and rateType and rateCode", () =>
			{
				AssertEquals("count", 5, result1.Length);
				Helper.AssertRateSelectionCriteriaInfoResult(result1[0], "AU", testDate, "DTY", "RC1", "", "", "RED", "Reduced", "tradeGroup2", "tradeGroup2 dec");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[1], "AU", testDate, "DTY", "RC1", "ord11", "add11", "STD", "Standard", "tradeGroup1", "tradeGroup1 dec");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[2], "AU", testDate, "DTY", "RC1", "ord31", "add31", "", "", "tradeGroup2", "tradeGroup2 dec");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[3], "AU", testDate, "ADD", "RC2", "ord41", "add41", "STD", "Standard", "tradeGroup2", "tradeGroup2 dec");
				Helper.AssertRateSelectionCriteriaInfoResult(result1[4], "AU", testDate, "ADD", "RC2", "ord6", "add6", "RED", "Reduced", "tradeGroup1", "tradeGroup1 dec", "tradeGroup2");

				AssertEquals("count", 6, result2.Length);
				Helper.AssertTwoRateSelectionCriteriaInfoSame(result2[0], result1[0]);
				Helper.AssertTwoRateSelectionCriteriaInfoSame(result2[1], result1[1]);
				Helper.AssertTwoRateSelectionCriteriaInfoSame(result2[2], result1[2]);
				Helper.AssertTwoRateSelectionCriteriaInfoSame(result2[3], result1[3]);
				Helper.AssertRateSelectionCriteriaInfoResult(result2[4], "AU", testDate, "ADD", "RC2", "ord51", "add51", "RED", "Reduced", "tradeGroup2", "tradeGroup2 dec");
				Helper.AssertRateSelectionCriteriaInfoResult(result2[5], "AU", testDate, "ADD", "RC2", "ord6", "add6", "RED", "Reduced", "tradeGroup1", "tradeGroup1 dec", "tradeGroup2");
			});
		}

		public void TestGetRateSelectionCriteriaInfo_ImportExport()
		{
			var testTariffs = SetupRefDataForTariff();
			var tariff = testTariffs.tariff;
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = new ZDate(2019, 12, 10);

			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1", direction: RateDirection.Export);
			var criterias = new IZZRateSelectionCriteria[] { criteria };
			var result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport not matching", 0, result.Count());

			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1", direction: RateDirection.Import);
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport matching", 3, result.Count());
			AssertEquals("Direction is Export", RateDirection.Import, result.First().Direction);

			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "ADD", "RC2", direction: RateDirection.Import);
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport not matching", 0, result.Count());

			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "ADD", "RC2", direction: RateDirection.Export);
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport matching", 2, result.Count());
			AssertEquals("Direction is Import", RateDirection.Export, result.First().Direction);

			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "DTY", "RC1", direction: RateDirection.Both);
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport ignored", 3, result.Count());

			criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, DataGrouping, "", "", emptyAdditionalCodeSet, testDate, "ADD", "RC2", direction: RateDirection.Both);
			criterias = new IZZRateSelectionCriteria[] { criteria };
			result = tariff.GetRateSelectionCriteriaInfo(criterias);
			AssertEquals("ZZR_IsExport ignored", 2, result.Count());
		}

		(TariffView tariff, TariffView nationalCodeTariff) SetupRefDataForTariff()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2029, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup1 = testHelper.CreateTradeGroup(DataGrouping, "tradeGroup1", date1, date4, "tradeGroup1 dec");
			testHelper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Australia, date1, date4);
			var tradeGroup2 = testHelper.CreateTradeGroup(DataGrouping, "tradeGroup2", date1, date4, "tradeGroup2 dec");
			testHelper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Australia, date1, date4);
			Factory.Save();
			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(DataGrouping, "HSN");
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(DataGrouping, Universal.Constants.RateTypes.Duty, "Duty", isExport: false);
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var addRateType = testHelper.CreateNewOrGetExistingRateType(DataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD", isExport: true);
			var rateCode2 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", addRateType.PK);
			Factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", DataGrouping);
			var preferenceRED = testHelper.CreatePreferenceForCountry("RED", "Reduced", DataGrouping);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(DataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			var nationalCodeTariff = helper.CreateTariffNationalCode(DataGrouping, cusTariff.PK, "11", date1, date4, date1);
			Factory.Save();
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicabilityInternal(testRate1, tradeGroup1, date1, date4, "add11", "ord11");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			testHelper.CreateCusApplicabilityInternal(testRate2, tradeGroup2, date1, date4, "", "");
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0");
			testHelper.CreateCusApplicabilityInternal(testRate3, tradeGroup2, date1, date4, "add31", "ord31");
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicabilityInternal(testRate4, tradeGroup2, date1, date4, "add41", "ord41");
			var testRate5 = testHelper.CreateRate(nationalCodeTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			testHelper.CreateCusApplicabilityInternal(testRate5, tradeGroup2, date1, date4, "add51", "ord51");
			var testRate6 = testHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0.6", preferencePk: preferenceRED.PK);
			testHelper.CreateCusApplicabilityInternal(testRate6, tradeGroup1, date1, date4, "add6", "ord6", tradeGroup2);
			Factory.Save();
			return (cusTariff, nationalCodeTariff);
		}

		public void TestRates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var de = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType = helper.CreateNewOrGetExistingTariffType("EUN", "1P1");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("EUN", Constants.RateTypes.Duty, "Duty");
			Factory.Save();
			var djcRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DJC", dutyRateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff("EUN", tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var rate1 = helper.CreateRate(tariff, djcRateCode.PK, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "0", dataGrouping: Core.Constants.CountryCodes.Germany);
			var rate2 = helper.CreateRate(tariff, djcRateCode.PK, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "0", dataGrouping: Core.Constants.CountryCodes.SouthAfrica);
			var rate3 = helper.CreateRate(tariff, djcRateCode.PK, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "0", dataGrouping: "EUN");
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredRates = tariffReloaded.FilteredRates;
			var rates = tariffReloaded.Rates;
			CombineAssertions(() =>
			{
				AssertEquals("filteredRates", 2, filteredRates.Count);
				AssertNotNull("1 filteredRates.rate1", filteredRates.FindByPK(rate1.PK));
				AssertNotNull("1 filteredRates.rate3", filteredRates.FindByPK(rate3.PK));
				AssertEquals("1 rates", 3, rates.Count);
				AssertNotNull("1 rates.rate1", rates.FindByPK(rate1.PK));
				AssertNotNull("1 rates.rate2", rates.FindByPK(rate2.PK));
				AssertNotNull("1 rates.rate3", rates.FindByPK(rate3.PK));
				wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Germany;
				AssertEquals("2 filteredRates", 1, filteredRates.Count);
				AssertNotNull("2 filteredRates.rate1", filteredRates.FindByPK(rate1.PK));
				AssertEquals("2 rates", 3, rates.Count);
				AssertNotNull("2 rates.rate1", rates.FindByPK(rate1.PK));
				AssertNotNull("2 rates.rate2", rates.FindByPK(rate2.PK));
				AssertNotNull("2 rates.rate3", rates.FindByPK(rate3.PK));
				wrapper.EffectiveDataGrouping = "EUN";
				AssertEquals("3 filteredRates", 1, filteredRates.Count);
				AssertNotNull("3 filteredRates.rate3", filteredRates.FindByPK(rate3.PK));
				AssertEquals("3 rates", 3, rates.Count);
				AssertNotNull("3 rates.rate1", rates.FindByPK(rate1.PK));
				AssertNotNull("3 rates.rate2", rates.FindByPK(rate2.PK));
				AssertNotNull("3 rates.rate3", rates.FindByPK(rate3.PK));
				wrapper.EffectiveDataGrouping = ZString.Empty;
				wrapper.EffectiveDate = new ZDate(2013, 1, 1);
				AssertEquals("4 filteredRates", 1, filteredRates.Count);
				AssertNotNull("4 filteredRates.rate3", filteredRates.FindByPK(rate3.PK));
				AssertEquals("4 rates", 3, rates.Count);
				AssertNotNull("4 rates.rate1", rates.FindByPK(rate1.PK));
				AssertNotNull("4 rates.rate2", rates.FindByPK(rate2.PK));
				AssertNotNull("4 rates.rate3", rates.FindByPK(rate3.PK));
				wrapper.EffectiveDate = new ZDate(2012, 12, 9);
				AssertEquals("5 filteredRates", 1, filteredRates.Count);
				AssertNotNull("5 filteredRates.rate1", filteredRates.FindByPK(rate1.PK));
				AssertEquals("5 rates", 3, rates.Count);
				AssertNotNull("5 rates.rate1", rates.FindByPK(rate1.PK));
				AssertNotNull("5 rates.rate2", rates.FindByPK(rate2.PK));
				AssertNotNull("5 rates.rate3", rates.FindByPK(rate3.PK));
			});
		}

		public void TestITariffMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shbType = helper.CreateTariffType(Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, shbType.PK, "99999999", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "My Description");
			helper.CreateTariffUOM(tariff, "CU1", "KG");
			helper.CreateTariffUOM(tariff, "CU2", "U");
			helper.CreateTariffUOM(tariff, "CU3", "ZZ");
			helper.CreateTariffUOM(tariff, "CU4", "CX4");
			helper.CreateTariffUOM(tariff, "CU5", "CX5");
			Factory.Save();
			ITariff iTariff = tariff;
			AssertEquals("iTariff.TariffCode", "99999999", iTariff.Code);
			AssertEquals("iTariff.Description", "My Description", iTariff.Description);
			AssertEquals("iTariff.UQ1", "KG", iTariff.UQ1);
			AssertEquals("iTariff.UQ2", "U", iTariff.UQ2);
			AssertEquals("iTariff.UQ3", "ZZ", iTariff.UQ3);
			AssertEquals("iTariff.UQ4", "CX4", iTariff.UQ4);
			AssertEquals("iTariff.UQ5", "CX5", iTariff.UQ5);
		}

		public void TestMatchDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("IT", parent: eun);
			helper.CreateNewOrGetExistingDataGrouping("DE", parent: eun);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, "EUN", "T1T", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff("EUN", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			AssertEquals(true, tariff.MatchDataGrouping(ZString.Empty));
			AssertEquals(true, tariff.MatchDataGrouping("EUN"));
			AssertEquals(true, tariff.MatchDataGrouping("DE"));
			AssertEquals(false, tariff.MatchDataGrouping("AU"));
			tariff = helper.CreateTariff("IT", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			AssertEquals(true, tariff.MatchDataGrouping(ZString.Empty));
			AssertEquals(false, tariff.MatchDataGrouping("EUN"));
			AssertEquals(false, tariff.MatchDataGrouping("DE"));
			AssertEquals(false, tariff.MatchDataGrouping("AU"));
			AssertEquals(true, tariff.MatchDataGrouping("IT"));
		}

		public void TestAdditionalCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT1");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.SouthAfrica, "CT2");
			var category3 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, "DG1", "CT2");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var additionalCode1 = helper.CreateTariffAdditionalCodeView(tariff, "CT1", "C11", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode2 = helper.CreateTariffAdditionalCodeView(tariff, "CT2", "C22", dataGrouping: Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode3 = helper.CreateTariffAdditionalCodeView(tariff, "CT2", "C33", dataGrouping: "DG1", ensureDataGroupingExists: false, ensureCategoryExists: false);
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = tariffReloaded.FilteredAdditionalCodes;
			var collection = tariffReloaded.AdditionalCodes;
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode1", filteredCollection.FindByPK(additionalCode1.PK));
			AssertNotNull("filteredCollection.additionalCode2", filteredCollection.FindByPK(additionalCode3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode1", filteredCollection.FindByPK(additionalCode1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode3", filteredCollection.FindByPK(additionalCode3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
		}

		public void TestFilteredVATApplicabilities()
		{
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			Factory.Save();
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var vatApplicability1 = helper.CreateVATApplicabilityView(tariff, Core.Constants.CountryCodes.Eritrea, "RT1", startDate: new ZDateTime(2011, 1, 1), endDate: new ZDateTime(2012, 12, 31));
			var vatApplicability2 = helper.CreateVATApplicabilityView(tariff, Core.Constants.CountryCodes.SouthAfrica, "RT2", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			var vatApplicability3 = helper.CreateVATApplicabilityView(tariff, "DG1", "RT3", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = tariffReloaded.FilteredVATApplicabilities;
			AssertEquals("filteredCollection", 3, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			AssertNotNull("filteredCollection.vatApplicability2", filteredCollection.FindByPK(vatApplicability2.PK));
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability2", filteredCollection.FindByPK(vatApplicability2.PK));
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
		}

		public void TestConditions()
		{
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var condType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.Eritrea, "CTRL", "724");
			condType.Factory.Save();
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.Eritrea, condType.PK, tariff.PK, "comment1", true, false, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31));
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.SouthAfrica, condType.PK, tariff.PK, "comment2", true, false, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06));
			var condition3 = helper.CreateOrGetExistingRefCusCondition("DG1", condType.PK, tariff.PK, "comment3", true, false, startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var collection = tariffReloaded.Conditions;
			AssertEquals("Collection", 3, collection.Count);
			var filteredCollection = tariffReloaded.FilteredConditions;
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition1", filteredCollection.FindByPK(condition1.PK));
			AssertNotNull("filteredCollection.condition3", filteredCollection.FindByPK(condition3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition1", filteredCollection.FindByPK(condition1.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition3", filteredCollection.FindByPK(condition3.PK));
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition1", filteredCollection.FindByPK(condition1.PK));
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition3", filteredCollection.FindByPK(condition3.PK));
		}

		public void TestGetDefaultTaxOrFeeCode_TariffHasNoVATApplicability_ExpectTaxOrFeeCodeFromTariff()
		{
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99", taxOrFeeCode: "ZA1");
			AssertEquals("ZA1", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestGetDefaultTaxOrFeeCode_TariffHasNoVATApplicabilityAndBlankTaxOrFeeCode_ExpectTaxOrFeeCodeWithMaxValueFromRefCusTaxOrFee()
		{
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			AssertEquals("ZA3", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestGetDefaultTaxOrFeeCode_VATApplicabilitiesOfTariffHasSingleDistinctTaxOrFeeCode_ExpectSingleDistinctTaxOrFeeCodeFromVATApplicabilitiesOfTariff()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var vatApplicability_ZA1 = Helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.SouthAfrica, "ZA1", startDate: startDate, endDate: endDate);
			AssertContainsExactElementsInAnyOrder(new[] { vatApplicability_ZA1 }, tariff.VATApplicabilities);
			AssertEquals("ZA1", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestGetDefaultTaxOrFeeCode_VATApplicabilitiesOfTariffHasDifferentDistinctTaxOrFeeCode_ExpectTaxOrFeeCodeWithMaxValueFromRefCusTaxOrFee()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var vatApplicability_ZA1 = Helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.SouthAfrica, "ZA1", startDate: startDate, endDate: endDate);
			var vatApplicability_ZA2 = Helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.SouthAfrica, "ZA2", startDate: startDate, endDate: endDate);
			AssertContainsExactElementsInAnyOrder(new[] { vatApplicability_ZA1, vatApplicability_ZA2 }, tariff.VATApplicabilities);
			AssertEquals("ZA3", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestGetDefaultTaxOrFeeCode_WithParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateRefCusTaxOrFeeType("OTH");
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: eun);
			Factory.Save();
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, "IMP");
			var tariffTypeSpain = helper.CreateTariffType(Core.Constants.CountryCodes.Spain, "IMP");
			var vatFee = helper.CreateTaxOrFee("DV1", 11, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			vatFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var vatFeeABC = helper.CreateTaxOrFee("ABCD", 11, Core.Constants.CountryCodes.France);
			vatFeeABC.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var vatFeeDV2 = helper.CreateTaxOrFee("DV2", 11, Core.Constants.CountryCodes.Spain);
			vatFeeDV2.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeSpain.PK, "1234567891", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			AssertEquals("ABCD", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.France));
			AssertEquals("DV1", tariff.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.Belgium));
			AssertEquals("DV1", tariff2.GetDefaultTaxOrFeeCode(ZDateTime.Today, Core.Constants.CountryCodes.Spain));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var declaration = (BusinessObject)Factory.New<FR.IJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
				var invoiceHeader = (BusinessObject)Factory.New<FR.IJobComInvoiceHeader>();
				invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
				invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
				var invoiceLine = (BusinessObject)Factory.New<FR.IJobComInvoiceLine>();
				invoiceLine[JobComInvoiceLineSchema.Constants.JI_JZ] = invoiceHeader.PK;
				invoiceLine[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;
				ZString tariffCode = "1234567890";
				invoiceLine[JobComInvoiceLineSchema.Constants.JI_Tariff] = tariffCode;
				AssertEquals("ABCD", invoiceLine[JobComInvoiceLineSchema.Constants.JI_ZZF_NKTaxType]);
			}
		}

		public void TestHasAttribute()
		{
			var attribute = Helper.CreateTariffAttribute("BOB", "B", CusTariff);
			AssertEquals(true, CusTariff.HasAttribute("BOB"));
			AssertEquals(false, CusTariff.HasAttribute("JACK"));
			AssertEquals(false, CusTariff.HasAttribute("BOB", "A"));
			AssertEquals(true, CusTariff.HasAttribute("BOB", "B"));
			AssertEquals(false, CusTariff.HasAttribute("JACK", "B"));
		}

		public void TestITariffDataMemebers()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			ITariffData tariffData = tariff;
			AssertEquals("tariffData.IsNomenclatureGroup", false, tariffData.IsNomenclatureGroup);
			AssertEquals("tariffData.CompositeKey", "99...99.99", tariffData.CompositeKey);
			AssertEquals("tariffData.TariffCode", "99999999", tariffData.TariffCode);
			AssertEquals("tariffData.GetDescription", "Alpha Bravo", tariffData.GetDescription(Env.CurrentUser.Language));
		}

		public void TestUnitQuantities_IsSystem()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			tariff.ZZ1_IsSystem = false;
			CombineAssertions(() =>
			{
				AssertEquals("tariff.ZZ1_ZZ8_UQ1", ZString.Empty, tariff.ZZ1_ZZ8_UQ1);
				AssertEquals("tariff.ZZ1_ZZ8_UQ2", ZString.Empty, tariff.ZZ1_ZZ8_UQ2);
				AssertEquals("tariff.ZZ1_ZZ8_UQ3", ZString.Empty, tariff.ZZ1_ZZ8_UQ3);
				AssertEquals("tariff.ZZ1_ZZ8_UQ4", ZString.Empty, tariff.ZZ1_ZZ8_UQ4);
				AssertEquals("tariff.ZZ1_ZZ8_UQ5", ZString.Empty, tariff.ZZ1_ZZ8_UQ5);
			}

			);
		}

		public void TestUnitQuantities_NoType()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var uq = tariff.UnitsOfMeasure.AddNew();
			uq.ZZ8_UOM = "NO";
			uq.ZZ8_Type = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("tariff.ZZ1_ZZ8_UQ1", ZString.Empty, tariff.ZZ1_ZZ8_UQ1);
				AssertEquals("tariff.ZZ1_ZZ8_UQ2", ZString.Empty, tariff.ZZ1_ZZ8_UQ2);
				AssertEquals("tariff.ZZ1_ZZ8_UQ3", ZString.Empty, tariff.ZZ1_ZZ8_UQ3);
				AssertEquals("tariff.ZZ1_ZZ8_UQ4", ZString.Empty, tariff.ZZ1_ZZ8_UQ4);
				AssertEquals("tariff.ZZ1_ZZ8_UQ5", ZString.Empty, tariff.ZZ1_ZZ8_UQ5);
			}

			);
		}

		public void TestUnitQuantities_TypeAndMeasure()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var uq = tariff.UnitsOfMeasure.AddNew();
			uq.ZZ8_UOM = "NO";
			uq.ZZ8_Type = Constants.UnitOfMeasureTypes.StatisticalUOMType;
			CombineAssertions(() =>
			{
				AssertEquals("tariff.ZZ1_ZZ8_UQ1", "NO", tariff.ZZ1_ZZ8_UQ1);
				AssertEquals("tariff.ZZ1_ZZ8_UQ2", ZString.Empty, tariff.ZZ1_ZZ8_UQ2);
				AssertEquals("tariff.ZZ1_ZZ8_UQ3", ZString.Empty, tariff.ZZ1_ZZ8_UQ3);
				AssertEquals("tariff.ZZ1_ZZ8_UQ4", ZString.Empty, tariff.ZZ1_ZZ8_UQ4);
			}

			);
		}

		public void TestUnitQuantities_MultipleTypeAndMeasure()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var uq1 = tariff.UnitsOfMeasure.AddNew();
			uq1.ZZ8_UOM = "NO";
			uq1.ZZ8_Type = Constants.UnitOfMeasureTypes.StatisticalUOMType;
			var uq2 = tariff.UnitsOfMeasure.AddNew();
			uq2.ZZ8_UOM = "PK";
			uq2.ZZ8_Type = Constants.UnitOfMeasureTypes.AdditionalUOMType;
			var uq3 = tariff.UnitsOfMeasure.AddNew();
			uq3.ZZ8_UOM = "KG";
			uq3.ZZ8_Type = Constants.UnitOfMeasureTypes.CustomsUOM3Type;
			var uq4 = tariff.UnitsOfMeasure.AddNew();
			uq4.ZZ8_UOM = "CX4";
			uq4.ZZ8_Type = Constants.UnitOfMeasureTypes.CustomsUOM4Type;
			var uq5 = tariff.UnitsOfMeasure.AddNew();
			uq5.ZZ8_UOM = "CX5";
			uq5.ZZ8_Type = Constants.UnitOfMeasureTypes.CustomsUOM5Type;
			CombineAssertions(() =>
			{
				AssertEquals("tariff.ZZ1_ZZ8_UQ1", "NO", tariff.ZZ1_ZZ8_UQ1);
				AssertEquals("tariff.ZZ1_ZZ8_UQ2", "PK", tariff.ZZ1_ZZ8_UQ2);
				AssertEquals("tariff.ZZ1_ZZ8_UQ3", "KG", tariff.ZZ1_ZZ8_UQ3);
				AssertEquals("tariff.ZZ1_ZZ8_UQ4", "CX4", tariff.ZZ1_ZZ8_UQ4);
				AssertEquals("tariff.ZZ1_ZZ8_UQ5", "CX5", tariff.ZZ1_ZZ8_UQ5);
			}

			);
		}

		public void TestUnitsOfMeasure_NationalCode()
		{
			var parentTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateTariffUOM(parentTariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateTariffUOM(parentTariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTR", Core.Constants.CountryCodes.SouthAfrica);
			var tariff = helper.LoadOrCreateNewTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, parentTariff.PK, "111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), ZDate.BrettsBirthday);
			AssertContainsExactElementsInAnyOrder(new[] { "KGM", "LTR" }, tariff.UnitsOfMeasure.Select(x => x.ZZ8_UOM));
		}

		public void TestRateType()
		{
			var rate = CusTariff.Factory.New<RateView>();
			rate.ZZ2_ZZ1_ParentTariffOrNationalCode = CusTariff.PK;
			rate.ZZ2_ZZZ_NKDataGrouping = CusTariff.ZZ1_ZZZ_NKDataGrouping;
			AssertEquals("ZZ2_ZZR_RateTypeCode", ZString.Empty, rate.ZZ2_ZZR_RateTypeCode);
			AssertEquals("ZZ2_ZZR_RateTypeDesc", ZString.Empty, rate.ZZ2_ZZR_RateTypeDesc);
			rate.ZZ2_ZY1_RateCode = djcRateCode.PK;
			AssertEquals("ZZ2_ZZR_RateTypeCode", "DTY", rate.ZZ2_ZZR_RateTypeCode);
			AssertEquals("ZZ2_ZZR_RateTypeDesc", "Duty", rate.ZZ2_ZZR_RateTypeDesc);
		}

		public void TestRateTypeCode()
		{
			var rate = CusTariff.Factory.New<RateView>();
			rate.ZZ2_ZZ1_ParentTariffOrNationalCode = CusTariff.PK;
			rate.ZZ2_ZZZ_NKDataGrouping = CusTariff.ZZ1_ZZZ_NKDataGrouping;
			AssertEquals("ZZ2_ZZR_RateTypeCode", ZString.Empty, rate.ZZ2_ZZR_RateTypeCode);
			rate.ZZ2_ZY1_RateCode = djcRateCode.PK;
			AssertEquals("ZZ2_ZZR_RateTypeCode", "DTY", rate.ZZ2_ZZR_RateTypeCode);
		}

		public void TestGetApplicableRate()
		{
			var preference1 = Helper.CreatePreferenceForCountryAndGroupingInternal("PRF", "Cus Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var preference2 = Helper.CreatePreferenceForCountryAndGroupingInternal("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var eutrade = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "EU Trade Agreement Jan 2000");
			Helper.AddCountry(eutrade, "ZA");
			var mercosurquota = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "MERCOSURQUOTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "MERCOSUR Quota trade agreement");
			Helper.AddCountry(mercosurquota, "PY");
			var sadc = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "SADC Trade Agreement 2000");
			Helper.AddCountry(sadc, "PY");
			var testRate1 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0", preference2.PK);
			var testRate2 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 29), new ZDateTime(2079, 06, 06), "0", preference2.PK);
			var testRate3 = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 29), new ZDateTime(2079, 06, 06), "0", preference1.PK);
			Factory.Save();
			var applic1 = Factory.New<Internal.RefCusApplicability>();
			applic1.ZZT_ZZ2_Rate = testRate1.PK;
			applic1.ZZT_StartDate = ZDateTime.MinSmallDateTimeValue;
			applic1.ZZT_EndDate = ZDateTime.MaxSmallDateTimeValue;
			applic1.ZZT_ZZA_TradeGroup = eutrade.PK;
			var applic2 = Factory.New<Internal.RefCusApplicability>();
			applic2.ZZT_ZZ2_Rate = testRate2.PK;
			applic2.ZZT_StartDate = ZDateTime.MinSmallDateTimeValue;
			applic2.ZZT_EndDate = ZDateTime.MaxSmallDateTimeValue;
			applic2.ZZT_ZZA_TradeGroup = mercosurquota.PK;
			var applic3 = Factory.New<Internal.RefCusApplicability>();
			applic3.ZZT_ZZ2_Rate = testRate3.PK;
			applic3.ZZT_StartDate = ZDateTime.MinSmallDateTimeValue;
			applic3.ZZT_EndDate = ZDateTime.MaxSmallDateTimeValue;
			applic3.ZZT_ZZA_TradeGroup = sadc.PK;
			Factory.Save();
			var date = new DateTime(2011, 12, 15);
			var criteria = Helper.CreateRateSelectionCriteria("ZA", "ZA", "100", "", null, date, "DTY", "DJC");
			AssertEquals(testRate1.PK, cusTariff.GetApplicableRate(criteria).PK);
			criteria = Helper.CreateRateSelectionCriteria("PY", "ZA", "100", "", null, date, "DTY", "DJC");
			AssertEquals(testRate2.PK, cusTariff.GetApplicableRate(criteria).PK);
			criteria = Helper.CreateRateSelectionCriteria("PY", "ZA", "PRF", "", null, date, "DTY", "DJC");
			AssertEquals(testRate3.PK, cusTariff.GetApplicableRate(criteria).PK);
			criteria = Helper.CreateRateSelectionCriteria("PY", "ZA", "PRF", "", null, date, "", "");
			AssertEquals(testRate3.PK, cusTariff.GetApplicableRate(criteria).PK);
			criteria = Helper.CreateRateSelectionCriteria("PY", "ZA", "PRF", "", null, date, "ADD", "DJC");
			AssertNull(cusTariff.GetApplicableRate(criteria));
		}

		public void TestGetApplicableRates()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDateTime(2018, 07, 01, 13, 00, 00);
			var date4 = new ZDate(2079, 06, 06);
			var dataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, parent: dataGroup);
			var addRateType = Helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.AntiDumping, "ADD", ensureDataGroupingExists: false);
			var addRateCode = Helper.CreateCusRateCode(Factory, "ARD", addRateType.PK);
			Factory.Save();
			var tradeGroupEFTA = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "EFTA", date1, date4);
			Helper.AddCountry(tradeGroupEFTA, Core.Constants.CountryCodes.Italy, date1, date4);
			var tradeGroupStandard = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", date1, date4);
			Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Italy, date1, date4);
			Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			var preferenceSTD = Helper.CreatePreferenceForCountry("STD", "Standard", Core.Constants.CountryCodes.SouthAfrica);
			var preferenceRED = Helper.CreatePreferenceForCountry("RED", "Reduced", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var testRate1 = Helper.CreateRate(CusTariff, addRateCode.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			var testRate2 = Helper.CreateRate(CusTariff, djcRateCode.PK, date1, date4, "0", preferencePk: preferenceRED.PK, dataGrouping: Core.Constants.CountryCodes.SouthAfrica);
			Helper.CreateCusApplicability(testRate2, tradeGroupEFTA, date1, date4);
			var testRate3 = Helper.CreateRate(CusTariff, djcRateCode.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: Core.Constants.CountryCodes.SouthAfrica);
			Helper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			var testRate4 = Helper.CreateRate(CusTariff, djcRateCode.PK, date1, date4, "0", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Helper.CreateCusApplicabilityInternal(testRate4, tradeGroupStandard, date1, date4, "1", "ORD1");
			Factory.Save();
			CombineAssertions("Test GetApplicableRates", () =>
			{
				AssertContainsExactElementsInAnyOrder("return empty array if criteria is NULL", Array.Empty<RateView>(), CusTariff.GetApplicableRates(null).Select(a => a));
				var additionalCodes = new HashSet<ZString> { "1" };
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "XXX", "ORD1", additionalCodes, date2, "", "");
				AssertContainsExactElementsInAnyOrder("ZZ2_ZZS_Preference IS NULL means match any preference", new ZGuid[] { testRate4.PK }, CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "STD", "", new HashSet<ZString>()
				{ ZString.Empty }, date2, "", "");
				AssertContainsExactElementsInAnyOrder("match all criteria", new ZGuid[] { testRate1.PK, testRate3.PK }, CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.China, "ZA", "STD", "", null, date2, "", "");
				AssertContainsExactElementsInAnyOrder("unmatch CountryOfOrigin", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "STD", "ORD1", null, date2, "", "");
				AssertContainsExactElementsInAnyOrder("unmatch ordernumber", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "STD", "", additionalCodes, date2, "", "");
				AssertContainsExactElementsInAnyOrder("unmatch additionalCodes and select rates with empty additionalCodes", new ZGuid[] { testRate1.PK, testRate3.PK }, CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "RED", "", null, date2, "", "");
				AssertContainsExactElementsInAnyOrder("unmatch preference", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "STD", "", null, date1.AddMinutes(-1), "", "");
				AssertContainsExactElementsInAnyOrder("unmatch date", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "IT", "STD", "", null, date2, "", "");
				AssertContainsExactElementsInAnyOrder("unmatch dataGrouping", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, "ZA", "RED", "", null, date2, "DTY", "DJC");
				AssertContainsExactElementsInAnyOrder("match ratetype and ratecode", new ZGuid[] { testRate2.PK }, CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Italy, "ZA", "RED", "", null, date2, "ADD", "");
				AssertContainsExactElementsInAnyOrder("Not match ratetype and ratecode", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
				criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "", "", null, date2, "", "");
				AssertContainsExactElementsInAnyOrder("Empty preference mean match Empty value not Any", Array.Empty<ZGuid>(), CusTariff.GetApplicableRates(criteria).Select(a => a.PK));
			}

			);
		}

		public void TestGetApplicableRatesWithEmptyDate()
		{
			var date = ZDate.Empty;
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.UnitedStates, "US", "", "", null, date, "", "");
			AssertNoExceptionThrown(() => CusTariff.GetApplicableRates(criteria));
		}

		public void TestGetApplicableRates_NationalCodeTariff()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDateTime(2018, 07, 01, 13, 00, 00);
			var date4 = new ZDate(2079, 06, 06);
			var dataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, parent: dataGroup);
			var addRateType = Helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.AntiDumping, "ADD", ensureDataGroupingExists: false);
			var addRateCode = Helper.CreateCusRateCode(Factory, "ARD", addRateType.PK);
			Factory.Save();
			var tradeGroupStandard = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", date1, date4);
			Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			var preferenceSTD = Helper.CreatePreferenceForCountry("STD", "Standard", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var nationalCodeTariff = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, CusTariff.PK, "11", date1, date4, date1);
			Factory.Save();
			var testRate1 = Helper.CreateRate(CusTariff, addRateCode.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			var testRate2 = Helper.CreateRate(nationalCodeTariff, addRateCode.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Helper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			Factory.Save();
			var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, "ZA", "STD", "", new HashSet<ZString>()
			{ ZString.Empty }, date2, "", "");
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { testRate1.PK, testRate2.PK }, nationalCodeTariff.GetApplicableRates(criteria).Select(a => a.PK));
		}

		public void TestGSTVATRate()
		{
			CombineAssertions(() =>
			{
				var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), taxOrFeeCode: "V##");
				AssertEquals("GSTVATRate", 0m, dummyTariff.GSTVATRate);
				Helper.CreateTaxOrFee("V##", 0.13m, Core.Constants.CountryCodes.SouthAfrica, new ZDateTime(2010, 12, 10), new ZDateTime(2019, 02, 18), "VAT Normal");
				Helper.CreateTaxOrFee("V##", 0.14m, Core.Constants.CountryCodes.SouthAfrica, new ZDateTime(2019, 02, 19), new ZDateTime(2079, 06, 06), "VAT Normal");
				Factory.Save();
				AssertEquals("GSTVATRate via effective date", 0.14m, dummyTariff.GSTVATRate);
				Helper.CreateTaxOrFee("V##", 0.15m, Core.Constants.CountryCodes.SouthAfrica, new ZDateTime(2019, 02, 20), new ZDateTime(2079, 06, 05), "VAT Normal");
				Factory.Save();
				AssertEquals("GSTVATRate via order by start date descending", 0.15m, dummyTariff.GSTVATRate);
			}

			);
		}

		public void TestGetAttribute()
		{
			var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Helper.CreateTariffAttribute("TEST1", "TEST1V", dummyTariff);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Has TEST1", true, dummyTariff.HasAttribute("TEST1"));
				AssertEquals("Value TEST1", "TEST1V", dummyTariff.GetAttribute("TEST1").ZZ3_Value);
				AssertEquals("Has TEST2", false, dummyTariff.HasAttribute("TEST2"));
				AssertEquals("Value TEST2", null, dummyTariff.GetAttribute("TEST2"));
			}

			);
		}

		public void TestGetAttributes()
		{
			var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Helper.CreateTariffAttribute("TEST1", "TEST1V1", dummyTariff);
			Helper.CreateTariffAttribute("TEST1", "TEST1V2", dummyTariff);
			Helper.CreateTariffAttribute("TEST1", "TEST1V3", dummyTariff);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Has TEST1", true, dummyTariff.HasAttribute("TEST1"));
				AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST1V1", "TEST1V2", "TEST1V3" }, dummyTariff.GetAttributes("TEST1").Select(x => x.ZZ3_Value));
				AssertEquals("Has TEST2", false, dummyTariff.HasAttribute("TEST2"));
				AssertEquals("Values TEST2 Count", 0, dummyTariff.GetAttributes("TEST2").Count());
			}

			);
		}

		public void TestFullTariffDescription()
		{
			const string description = "Test Description";
			const string description1 = "Test Description 01";
			const string compositeKey = "99.99.99.99.99";
			var agesAgo = new ZDateTime(1900, 01, 01);
			var expiryDate = new ZDateTime(2016, 09, 01);
			var beforeExpiryDate = new ZDateTime(2016, 08, 30);
			var afterExpiryDate = new ZDateTime(2016, 09, 15);
			var group1 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", agesAgo, expiryDate, "VWG", "99", Core.Constants.CountryCodes.SouthAfrica);
			var group2 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, ZString.Empty, agesAgo, expiryDate, "ChapterHeader", "99.99.99.99", Core.Constants.CountryCodes.SouthAfrica);
			var group3 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", agesAgo, expiryDate, description1, compositeKey, Core.Constants.CountryCodes.SouthAfrica);

			Factory.Save();
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), description, compositeKey: compositeKey);
			CombineAssertions("Variations of FullTarriffDescription Parameters", () =>
			{
				AssertEquals("Test Description 01 Test Description", tariff.FullTariffDescription(beforeExpiryDate, false, false));
				AssertEquals("ChapterHeader Test Description 01 Test Description", tariff.FullTariffDescription(beforeExpiryDate, false, true));
				AssertEquals("VWG Test Description 01 Test Description", tariff.FullTariffDescription(beforeExpiryDate, true, false));
				AssertEquals("VWG ChapterHeader Test Description 01 Test Description", tariff.FullTariffDescription(beforeExpiryDate, true, true));
				AssertEquals("Test Description", tariff.FullTariffDescription(afterExpiryDate, false, false));
				AssertEquals("Test Description", tariff.FullTariffDescription(afterExpiryDate, false, true));
				AssertEquals("Test Description", tariff.FullTariffDescription(afterExpiryDate, true, false));
				AssertEquals("Test Description", tariff.FullTariffDescription(afterExpiryDate, true, true));
			});

			var tariffSearchHelper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.ZZI_TariffType, null, null);
			var expectedFullDescriptionDEF = "VWG ChapterHeader Test Description 01 Test Description";
			var expectedFullDescriptionITA = "Testata Sezione Testata Capitolo Descrizione 01 Descrizione tariffa IT";
			Helper.CreateOrGetLanguage("IT", "Italian");
			Factory.Save();

			tariffSearchHelper.Language = "DEF";
			AssertEquals("Full description in Preferred Language DEF", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("No description Preferred Language IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("No description GS_WorkingLanguage IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			Helper.CreateNomenclatureGroupLanguage(group1.PK, "IT", "Testata Sezione");
			Helper.CreateNomenclatureGroupLanguage(group2.PK, "IT", "Testata Capitolo");
			Factory.Save();

			tariffSearchHelper.Language = "IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Partial description in Preferred Language IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Partial description GS_WorkingLanguage IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			Helper.CreateNomenclatureGroupLanguage(group3.PK, "IT", "Descrizione 01");
			Factory.Save();

			tariffSearchHelper.Language = "IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Missing tariff description in Preferred Language IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Missing tariff description GS_WorkingLanguage IT", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "IT", "Descrizione tariffa IT");
			Factory.Save();

			tariffSearchHelper.Language = "IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Full description in Preferred Language IT", expectedFullDescriptionITA, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-IT";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Full description GS_WorkingLanguage IT", expectedFullDescriptionITA, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "FR";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "";
			AssertEquals("No description Preferred Language FR", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR-FR";
			AssertEquals("No description GS_WorkingLanguage FR", expectedFullDescriptionDEF, tariff.FullTariffDescription(beforeExpiryDate, true, true, true));

			tariffSearchHelper.Language = "DEF";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "DEF";
			ClearFullTariffDescriptionCacheIT(tariff);
			AssertEquals("Full description GS_WorkingLanguage IT", expectedFullDescriptionITA, tariff.FullTariffDescription(beforeExpiryDate, true, true, true, "IT-IT"));
		}

		public void TestFullTariffDescription_NationalCode()
		{
			const string description = "Test Description";
			const string compositeKey = "99.99.99.99.99";

			const string nationalTariffDescription = "Tarifbezeichnung DE";

			var agesAgo = new ZDateTime(1900, 01, 01);
			var expiryDate = new ZDateTime(2016, 09, 01);
			var beforeExpiryDate = new ZDateTime(2016, 08, 30);
			var afterExpiryDate = new ZDateTime(2016, 09, 15);
			var group1 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", agesAgo, expiryDate, "SectionHeading", "99", Core.Constants.CountryCodes.SouthAfrica);
			var group2 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, ZString.Empty, agesAgo, expiryDate, "ChapterHeader", "99.99.99.99", Core.Constants.CountryCodes.SouthAfrica);
			var group3 = Helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", agesAgo, expiryDate, "Description1", "99.99.99.99.99DE", Core.Constants.CountryCodes.SouthAfrica);

			Factory.Save();

			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), description, compositeKey: compositeKey);
			var nationalCodeTariff = Helper.CreateTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, tariff.PK, "DE", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), new ZDate(2010, 12, 10), nationalTariffDescription);

			var tariffSearchHelper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.ZZI_TariffType, null, null);
			var expectedFullDescriptionDE = "Kopfzeile testen Bildunterschrift testen Beschreibung 01 Tarifbezeichnung DE";

			Helper.CreateOrGetLanguage("DE", "German");
			Factory.Save();

			Helper.CreateNomenclatureGroupLanguage(group1.PK, "DE", "Kopfzeile testen");
			Helper.CreateNomenclatureGroupLanguage(group2.PK, "DE", "Bildunterschrift testen");
			Helper.CreateNomenclatureGroupLanguage(group3.PK, "DE", "Beschreibung 01");
			Factory.Save();

			CombineAssertions("Variations of FullTarriffDescription Parameters", () =>
			{
				AssertEquals("Description1 Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(beforeExpiryDate, false, false));
				AssertEquals("ChapterHeader Description1 Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(beforeExpiryDate, false, true));
				AssertEquals("SectionHeading Description1 Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, false));
				AssertEquals("SectionHeading ChapterHeader Description1 Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true));
				AssertEquals("Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(afterExpiryDate, false, false));
				AssertEquals("Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(afterExpiryDate, false, true));
				AssertEquals("Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(afterExpiryDate, true, false));
				AssertEquals("Tarifbezeichnung DE", nationalCodeTariff.FullTariffDescription(afterExpiryDate, true, true));
			});

			tariffSearchHelper.Language = "";
			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE";
			ClearFullTariffDescriptionCacheDE(nationalCodeTariff);

			CombineAssertions(() =>
			{
				AssertEquals("Full description in Preferred Language DE", expectedFullDescriptionDE, nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true, true));

				tariffSearchHelper.Language = "DE";
				ClearFullTariffDescriptionCacheDE(nationalCodeTariff);
				AssertEquals("Full description in Preferred Language DE", expectedFullDescriptionDE, nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true, true));

				tariffSearchHelper.Language = "";
				GlbStaff.CurrentUser.GS_WorkingLanguage = "DE-DE";
				ClearFullTariffDescriptionCacheDE(nationalCodeTariff);
				AssertEquals("Full description in Preferred Language DE", expectedFullDescriptionDE, nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true, true));

				tariffSearchHelper.Language = "DEF";
				GlbStaff.CurrentUser.GS_WorkingLanguage = "DEF";
				ClearFullTariffDescriptionCacheDE(nationalCodeTariff);
				AssertEquals("Full description in Preferred Language DE", expectedFullDescriptionDE, nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true, true, "DE-DE"));

				tariffSearchHelper.Language = "DEF";
				GlbStaff.CurrentUser.GS_WorkingLanguage = "DEF";
				ClearFullTariffDescriptionCacheDE(nationalCodeTariff);
				AssertEquals("Full description in Preferred Language DE", expectedFullDescriptionDE, nationalCodeTariff.FullTariffDescription(beforeExpiryDate, true, true, true));
			});
		}

		public void TestChildTariffs()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var tariffType1P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1#");
			var tariffType12A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A#");
			Factory.Save();
			var tariff1P1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1P1##", startDate, endDate, description: "long desc");
			var tariff12A = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##", startDate, endDate, description: "longer desc");
			Helper.CreateTariffRelationship(tariff12A.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			var loaded1P1 = Factory.Load<TariffView>(tariff1P1.PK);
			AssertEquals("Count should be 1", 1, loaded1P1.ChildTariffs.Count);
			var testRelationShip = loaded1P1.ChildTariffs[0];
			AssertEquals(testRelationShip.ZZH_ZZI_TariffTypeCode, "1P1#");
		}

		public void TestFilteredChildTariffs()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var midDate = new ZDateTime(2022, 04, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var tariffType1P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1#");
			var tariffType12A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A#");
			Factory.Save();
			var tariff1P1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1P1##", startDate, endDate, description: "Parent");
			var tariff12A_1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##1", startDate, midDate, description: "Old Child");
			var tariff12A_2 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##2", midDate, endDate, description: "New Child");
			var rel1 = Helper.CreateTariffRelationship(tariff12A_1.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			var rel2 = Helper.CreateTariffRelationship(tariff12A_2.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			Factory.Save();
			var loaded1P1 = Factory.Load<TariffView>(tariff1P1.PK);
			AssertEquals("Count should be 2", 2, loaded1P1.ChildTariffs.Count);

			var wrapper = loaded1P1.Wrapper;
			wrapper.EffectiveDate = ZDate.Empty;

			var collection = loaded1P1.FilteredChildTariffs;

			CombineAssertions(() =>
			{
				AssertEquals("Filtered should be 2", 2, collection.Count);

				wrapper.EffectiveDate = new ZDate(2022, 01, 01);
				AssertEquals("filteredCollection-Old", 1, collection.Count);
				AssertNotNull("filteredCollection.12A_1", collection.FindByPK(rel1.PK));

				wrapper.EffectiveDate = new ZDate(2022, 04, 05);
				AssertEquals("filteredCollection-New", 1, collection.Count);
				AssertNotNull("filteredCollection.12A_2", collection.FindByPK(rel2.PK));
			});
		}

		public void TestDateTimeFormat()
		{
			var tariffView = Factory.New<TariffView>();
			AssertEquals("Format is short when no country set", ZDateTimePickerFormat.Short, tariffView.DateTimeFormat);
			var tariffViewZA = Factory.New<TariffView>();
			tariffViewZA.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("Format is long for ZA", ZDateTimePickerFormat.Long, tariffViewZA.DateTimeFormat);
		}

		public void TestHumanReadableName_Date()
		{
			var startDate = new ZDateTime(2010, 12, 10, 1, 35, 15);
			var endDate = new ZDateTime(2079, 06, 06);
			var sgTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Singapore, s1p1TariffType.PK, "DUMMYTRF1", startDate, endDate);

			CombineAssertions(() =>
			{
				AssertEndsWith("HumanReadableName: Format is short when not ZA", "Effective From: 10-Dec-10",
					sgTariff.HumanReadableName);
				AssertEndsWith("HumanReadableShortcutName: Format is short when not ZA", "- From: 10-Dec-10",
					sgTariff.HumanReadableShortcutName);

				var zaTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK,
					"DUMMYTRF2", startDate, endDate);
				AssertEndsWith("HumanReadableName: Format is long for ZA", "Effective From: 10-Dec-10 01:35",
					zaTariff.HumanReadableName);
				AssertEndsWith("HumanReadableShortcutName: Format is long for ZA", "- From: 10-Dec-10 01:35",
					zaTariff.HumanReadableShortcutName);
			});
		}

		public void TestHumanReadableName()
		{
			var startDate = new ZDateTime(2010, 12, 10, 1, 35, 15);
			var endDate = new ZDateTime(2079, 06, 06);
			var manualTariff = Helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, Constants.TariffTypes.HarmonizedSystem, "TT1", startDate, endDate);

			CombineAssertions(() =>
			{
				AssertEquals("CG manual tariff's HumanReadableName", "Tariff: CG/HSN/TT1 - Effective From: 10-Dec-10",
					manualTariff.HumanReadableName);
				AssertEquals("CG manual tariff's HumanReadableShortcutName", "CG/HSN/TT1 - From: 10-Dec-10",
					manualTariff.HumanReadableShortcutName);

				var sgTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Singapore, s1p1TariffType.PK, "TT2",
					startDate, endDate);
				AssertEquals("SG ref tariff's HumanReadableName", "Tariff: SG/1P1/TT2 - Effective From: 10-Dec-10",
					sgTariff.HumanReadableName);
				AssertEquals("SG ref tariff's HumanReadableShortcutName", "SG/1P1/TT2 - From: 10-Dec-10",
					sgTariff.HumanReadableShortcutName);
			});
		}

		public void TestFetchHint()
		{
			var tariff1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariff2 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF02", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();
			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff1.PK, "ENG", "Test Description 001");
			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff2.PK, "ENG", "Test Description 002");
			Factory.Save();
			var newFactory = NewFactory();
			AssertCollectionNotContains(CusRefTariffLanguageViewSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZI_TariffType, s1p1TariffType.PK);
			var tariffViews = newFactory.Load<TariffView>(query);
			AssertEquals(2, tariffViews.Length);
			AssertCollectionContains(CusRefTariffLanguageViewSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var descriptionsForLoad = tariffViews.Select(c => c.ZZ1_Description).ToArray();
			//CusRefTariffLanguageView: 1
			//RefDatabase_TariffView: 1
			//Hits: 2 / 0
			AssertMaxDbHits("Should reduce to 2 by the fetch hint.", 2, newFactory);
		}

		public void TestDefaultLanguageDescription()
		{
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffView = Factory.Load<TariffView>(tariff.PK);
			AssertEquals("Default Description", tariffView.ZZ1_Description);
			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "ENG", "Test Description");
			Factory.Save();
			var orginalLanguage = GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage];
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "EU";
			AssertEquals("Default Description", tariffView.ZZ1_Description);
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "EN";
			AssertEquals("Default Description", tariffView.ZZ1_Description);
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = orginalLanguage;
		}

		public void TestTranslationDescription()
		{
			var frBranch = CreateCompanyWithBranch(Core.Constants.CountryCodes.France);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, frBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
				var tariffView = Factory.Load<TariffView>(tariff.PK);
				AssertTranslationDescription(tariffView.PK, "EN", "Not Available");
				AssertTranslationDescription(tariffView.PK, "TH-TH", "Not Available");
				AssertTranslationDescription(tariffView.PK, "EU", "Not Available");
				var engLanguage = Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "ENG", "Test Description");
				Factory.Save();
				AssertTranslationDescription(tariffView.PK, "EN", engLanguage.ZX7_Description);
				AssertTranslationDescription(tariffView.PK, "FR-FR", "Not Available");
				AssertTranslationDescription(tariffView.PK, "EU", "Not Available");
				var frLanguage = Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariffView.PK, "FRN", @"für zivile Luftfahrzeuge");
				Factory.Save();
				AssertTranslationDescription(tariffView.PK, "EN", engLanguage.ZX7_Description);
				AssertTranslationDescription(tariffView.PK, "FR-FR", frLanguage.ZX7_Description);
				AssertTranslationDescription(tariffView.PK, "EU", frLanguage.ZX7_Description);
			}
		}

		void AssertTranslationDescription(ZGuid pk, string language, string expectedDescription)
		{
			var orginalLanguage = GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage];
			try
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = language;
				var newFactory = NewFactory();
				var tariffView = newFactory.Load<TariffView>(pk);
				AssertEquals(expectedDescription, tariffView.ZZ1_AlternateLanguageDescription);
			}
			finally
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = orginalLanguage;
			}
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		public void TestTariffLanguages()
		{
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Helper.CreateOrGetLanguage("ENG", "English");
			Helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			Factory.Save();
			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "ENG", "Test Description");
			Helper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff.PK, "ZHT", "測試");
			Factory.Save();
			var tariffView = Factory.Load<TariffView>(tariff.PK);
			var languages = tariffView.TariffLanguages;
			AssertEquals(2, languages.Count);
			Assert(languages.Any(x => x.ZX7_ZX6_NKLanguage == "ENG" && x.ZX7_Description == "Test Description"));
			Assert(languages.Any(x => x.ZX7_ZX6_NKLanguage == "ZHT" && x.ZX7_Description == "測試"));
		}

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = companyCountryCode;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			return branch;
		}

		[TestDate(2021, 10, 05)]
		public void TestGetEffectiveTariffFilter_TariffFromOData()
		{
			var tariff = "1234";
			Helper.CreateInternalManualTariff(Core.Constants.CountryCodes.Congo, Constants.TariffTypes.HarmonizedSystem, tariff, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Congo, Constants.TariffTypes.HarmonizedSystem, tariff, ZDateTime.Today);
			AssertNotNull("Expect OData tariff should load", universalTariff);
		}

		public void TestGetEffectiveTariffFilter_TariffVersion()
		{
			var currentCountry = Core.Constants.CountryCodes.Congo;
			var version1 = Helper.CreateTariffVersion("CG2021", "CG2021", new ZDate(2021, 1, 1));
			version1.CRT_RN_NKCountryCode = currentCountry;
			var version2 = Helper.CreateTariffVersion("CG2022", "CG2022", new ZDate(2022, 1, 1));
			version2.CRT_RN_NKCountryCode = currentCountry;
			var version3 = Helper.CreateTariffVersion("NA2022", "NA2022", new ZDate(2021, 1, 1));
			version3.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			Factory.Save();

			var tariff1 = Helper.CreateInternalManualTariff(currentCountry, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2021, 2, 2), new ZDateTime(2079, 6, 6), tariffVersion: "CG2021");
			var tariff2 = Helper.CreateInternalManualTariff(currentCountry, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2021, 2, 2), new ZDateTime(2079, 6, 6), tariffVersion: "CG2022");
			var tariff3 = Helper.CreateInternalManualTariff(Core.Constants.CountryCodes.Namibia, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2021, 2, 2), new ZDateTime(2079, 6, 6), tariffVersion: "NA2022");
			Factory.Save();

			CombineAssertions(() =>
			{
				var universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(currentCountry, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2021, 2, 3));
				AssertEquals("The tariff with version CG2021 as CG2022 not effective yet", tariff1.PK, universalTariff.PK);

				universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(currentCountry, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2022, 2, 3));
				AssertEquals("The tariff with version CG2022 as CG2021 is in-effective when CG2022 take into effect", tariff2.PK, universalTariff.PK);

				universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(currentCountry, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2020, 2, 3));
				AssertEquals("No version is effective on 2020-2-3", null, universalTariff);

				universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Namibia, Constants.TariffTypes.HarmonizedSystem, "101010", new ZDateTime(2022, 2, 3));
				AssertEquals("The tariff in NA", tariff3.PK, universalTariff.PK);
			});
		}

		[TestDate(2021, 07, 21)]
		public void TestVATApplicabilities()
		{
			var (tariff, expectedVatApplicabilities) = CreateVatApplicabilities(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertContainsExactElementsInAnyOrder(expectedVatApplicabilities, tariff.VATApplicabilities);
		}

		[TestDate(2021, 07, 21)]
		public void TestGetEffectiveVATApplicabilities_BeforeStartDate()
		{
			var (tariff, _) = CreateVatApplicabilities(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals(false, tariff.GetEffectiveVATApplicabilities(ZDateTime.Today.AddDays(-2)).Any());
		}

		[TestDate(2021, 07, 21)]
		public void TestGetEffectiveVATApplicabilities_OnStartDate()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var (tariff, expectedVatApplicabilities) = CreateVatApplicabilities(startDate, ZDateTime.Today.AddDays(1));
			AssertContainsExactElementsInAnyOrder(expectedVatApplicabilities, tariff.GetEffectiveVATApplicabilities(startDate).ToArray());
		}

		[TestDate(2021, 07, 21)]
		public void TestGetEffectiveVATApplicabilities_BetweenStartAndEndDate()
		{
			var (tariff, expectedVatApplicabilities) = CreateVatApplicabilities(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertContainsExactElementsInAnyOrder(expectedVatApplicabilities, tariff.GetEffectiveVATApplicabilities(ZDateTime.Today).ToArray());
		}

		[TestDate(2021, 07, 21)]
		public void TestGetEffectiveVATApplicabilities_OnEndDate()
		{
			var endDate = ZDateTime.Today.AddDays(1);
			var (tariff, expectedVatApplicabilities) = CreateVatApplicabilities(ZDateTime.Today.AddDays(-1), endDate);
			AssertContainsExactElementsInAnyOrder(expectedVatApplicabilities, tariff.GetEffectiveVATApplicabilities(endDate).ToArray());
		}

		[TestDate(2021, 07, 21)]
		public void TestGetEffectiveVATApplicabilities_AfterEndDate()
		{
			var (tariff, _) = CreateVatApplicabilities(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			AssertEquals(false, tariff.GetEffectiveVATApplicabilities(ZDateTime.Today.AddDays(2)).Any());
		}

		public void TestGetConditionApplicabilitiesByCriteriaInfo()
		{
			(var tariff1, var tariff2) = SetupTariffAndCondition();
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = new ZDate(2019, 12, 01);
			var endDate2 = new ZDate(2029, 06, 06);
			var criteria = new ZZConditionSelectionCriteria(testDate, "", "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("Empty TradeGroupCountry", 0, result.Count());
			criteria = new ZZConditionSelectionCriteria(ZDateTime.Empty, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "");
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("Empty date", 0, result.Count());
			criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "");
			var result1 = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria }).OrderBy(x => x.ZZT_AdditionalCode).ToArray();
			AssertEquals("Empty rateType and rateCode means match any type and code", 4, result1.Length);
			Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[0], "AU", testDate, "RATE", "TY2", "", "", "STD", "Standard", "STANDARD", "STANDARD DEC");
			Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[1], "AU", testDate, "CTRL", "TY1", "orn11", "cdd11", "STD", "Standard", "STANDARD", "STANDARD DEC");
			Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[2], "AU", testDate, "CTRL", "TY1", "orn12", "cdd12", "STD", "Standard", "STANDARD", "STANDARD DEC");
			Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[3], "AU", testDate, "RATE", "TY2", "orn31", "cdd31", "STD", "Standard", "STANDARD", "STANDARD DEC");
			criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.NewZealand, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "");
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("No country match", 0, result.Count());
			criteria = new ZZConditionSelectionCriteria(endDate2.AddDays(+1), Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "");
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("No date match", 0, result.Count());
			criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "CON3", "");
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("No conditionClass match", 0, result.Count());
			criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "", "TY3");
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(new IZZConditionSelectionCriteria[] { criteria });
			AssertEquals("No conditionType match", 0, result.Count());
			var criteria1 = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "CTRL", "TY1");
			var criteria2 = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Import, "RATE", "TY2");
			var criterias = new IZZConditionSelectionCriteria[] { criteria1, criteria2 };
			result = tariff1.GetConditionApplicabilitiesByCriteriaInfo(criterias);
			result1 = result.OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match tradeGroupStandard and date and conditionClass and conditionType", () =>
			{
				AssertEquals("count", 4, result1.Length);
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[0], "AU", testDate, "RATE", "TY2", "", "", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[1], "AU", testDate, "CTRL", "TY1", "orn11", "cdd11", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[2], "AU", testDate, "CTRL", "TY1", "orn12", "cdd12", "STD", "Standard", "STANDARD", "STANDARD DEC");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result1[3], "AU", testDate, "RATE", "TY2", "orn31", "cdd31", "STD", "Standard", "STANDARD", "STANDARD DEC");
			});
		}

		public void TestGetConditionApplicabilitiesByCriteriaInfo_SecondTradeGroup()
		{
			var (_, tariff2) = SetupTariffAndCondition();
			var emptyAdditionalCodeSet = new HashSet<ZString>();
			var testDate = new ZDate(2019, 12, 01);

			var criteria = new ZZConditionSelectionCriteria(testDate, Core.Constants.CountryCodes.Australia, "", emptyAdditionalCodeSet, "", DataGrouping, ConditionChecker.ConditionDirection.Export, "CTRL", "TY1", new HashSet<ZString> { "STANDARD", "XX" });
			var criterias = new IZZConditionSelectionCriteria[] { criteria };
			var result = tariff2.GetConditionApplicabilitiesByCriteriaInfo(criterias).OrderBy(x => x.ZZT_OrderNumber).ToArray();
			CombineAssertions("match SecondTradeGroup", () =>
			{
				AssertEquals("count", 2, result.Length);
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result[0], "AU", testDate, "CTRL", "TY1", "orn51", "cdd51", "RED", "Reduced", "STANDARD", "STANDARD DEC", "");
				Helper.AssertConditionApplicabilitiesByCriteriaResult(result[1], "AU", testDate, "CTRL", "TY1", "orn71", "cdd71", "RED", "Reduced", "STANDARD", "STANDARD DEC", "STANDARD");
			});
		}

		(TariffView tariff1, TariffView tariff2) SetupTariffAndCondition()
		{
			var startDate = new ZDate(2010, 01, 01);
			var endDate1 = new ZDate(2019, 11, 30);
			var endDate2 = new ZDate(2029, 06, 06);
			var tradeGroupStandard = Helper.CreateTradeGroup(DataGrouping, "STANDARD", startDate, endDate2, "STANDARD DEC");
			Helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate2);
			Factory.Save();
			var hsnTariffType = Helper.CreateNewOrGetExistingTariffType(DataGrouping, "HSN");
			Factory.Save();
			var cusTariff = Helper.CreateTariff(DataGrouping, hsnTariffType.PK, "DUMMYTRF", startDate, endDate2, "dummy Description 0");
			var cusTariff2 = Helper.CreateTariff(DataGrouping, hsnTariffType.PK, "TARIFF2", startDate, endDate2, "dummy Description 0");
			Factory.Save();
			var preferenceSTD = Helper.CreatePreferenceForCountry("STD", "Standard", DataGrouping);
			var preferenceRED = Helper.CreatePreferenceForCountry("RED", "Reduced", DataGrouping);
			var conditionType1 = Helper.CreateOrGetExistingRefCusConditionType(DataGrouping, "CTRL", "TY1");
			var conditionType2 = Helper.CreateOrGetExistingRefCusConditionType(DataGrouping, "RATE", "TY2");
			var conditionCode1 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType1.PK, cusTariff.PK, "", true, false, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate2, "cdd11", "orn11");
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate2, "cdd12", "orn12");
			Helper.CreateCusApplicability(conditionCode1, tradeGroupStandard, startDate, endDate1, "cdd13", "orn13");
			var conditionCode2 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType1.PK, cusTariff.PK, "", false, true, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode2, tradeGroupStandard, startDate, endDate2, "cdd21", "orn21");
			var conditionCode3 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType2.PK, cusTariff.PK, "", true, false, startDate, endDate2, preferencePK: preferenceSTD.PK);
			Helper.CreateCusApplicability(conditionCode3, tradeGroupStandard, startDate, endDate2, "cdd31", "orn31");
			Helper.CreateCusApplicability(conditionCode3, tradeGroupStandard, startDate, endDate2);
			var conditionCode4 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType1.PK, cusTariff2.PK, "", true, false, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate2, "cdd41", "orn41");
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate2, "cdd42", "orn42");
			Helper.CreateCusApplicability(conditionCode4, tradeGroupStandard, startDate, endDate1, "cdd43", "orn43");
			var conditionCode5 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType1.PK, cusTariff2.PK, "", false, true, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode5, tradeGroupStandard, startDate, endDate2, "cdd51", "orn51");
			var conditionCode6 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType2.PK, cusTariff2.PK, "", true, false, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode6, tradeGroupStandard, startDate, endDate2, "cdd61", "orn61");
			Helper.CreateCusApplicability(conditionCode6, tradeGroupStandard, startDate, endDate2);
			var conditionCode7 = Helper.CreateOrGetExistingRefCusCondition(DataGrouping, conditionType1.PK, cusTariff2.PK, "", false, true, startDate, endDate2, preferencePK: preferenceRED.PK);
			Helper.CreateCusApplicability(conditionCode7, tradeGroupStandard, startDate, endDate2, "cdd71", "orn71", tradeGroupStandard);
			Factory.Save();
			return (cusTariff, cusTariff2);
		}

		public void TestAdditionalCodesForAntiDumping()
		{
			SetupDataForGetAdditionalCodesForRateType(Constants.RateTypes.AntiDumping);
			var tariff = Factory.Load<TariffView>(CusTariff.PK);
			var additionalCodes = tariff.GetAdditionalCodesForAntiDumping(Core.Constants.CountryCodes.Yemen, ZDate.Today);
			AssertEquals("AntiDumping COO not match", "", string.Join(",", additionalCodes.OrderBy(c => c).ToArray()));
			additionalCodes = tariff.GetAdditionalCodesForAntiDumping(Core.Constants.CountryCodes.EuropeanUnion, ZDate.Today);
			AssertContainsExactElementsInAnyOrder("AntiDumping COO and Effective Date match", new string[] { "AC01", "AC02", "AC04" }, additionalCodes);
		}

		public void TestAdditionalCodesForCountervailing()
		{
			SetupDataForGetAdditionalCodesForRateType(Constants.RateTypes.Countervailing);
			var tariff = Factory.Load<TariffView>(CusTariff.PK);
			var additionalCodes = tariff.GetAdditionalCodesForCountervailing(Core.Constants.CountryCodes.Yemen, ZDate.Today);
			AssertEquals("Countervailing COO not match", "", string.Join(",", additionalCodes.OrderBy(c => c).ToArray()));
			additionalCodes = tariff.GetAdditionalCodesForCountervailing(Core.Constants.CountryCodes.EuropeanUnion, ZDate.Today);
			AssertContainsExactElementsInAnyOrder("Countervailing COO and Effective Date match", new string[] { "AC01", "AC02", "AC04" }, additionalCodes);
		}

		public void TestAdditionalCodesForDuty()
		{
			CombineAssertions(() =>
			{
				SetupDataForGetAdditionalCodesForRateType(Constants.RateTypes.Duty);
				var tariff = Factory.Load<TariffView>(CusTariff.PK);
				var additionalCodes = tariff.GetAdditionalCodesForDuty(Core.Constants.CountryCodes.Yemen, ZDate.Today, Core.Constants.CountryCodes.SouthAfrica, "100");
				AssertEquals("Duty A00 with Preference 100 not match", "", string.Join(",", additionalCodes.OrderBy(c => c).ToArray()));
				additionalCodes = tariff.GetAdditionalCodesForDuty(Core.Constants.CountryCodes.EuropeanUnion, ZDate.Today, Core.Constants.CountryCodes.SouthAfrica, "100");
				AssertContainsExactElementsInAnyOrder("Duty A00 with Preference 100 and Effective Date match", new string[] { "AC01", "AC02" }, additionalCodes);
				additionalCodes = tariff.GetAdditionalCodesForDuty(Core.Constants.CountryCodes.EuropeanUnion, ZDate.Today, Core.Constants.CountryCodes.SouthAfrica, "200");
				AssertContainsExactElementsInAnyOrder("Duty A00 with Preference 200 and Effective Date match", new string[] { "AC04" }, additionalCodes);
			});
		}

		public void TestSetupDefaultFilterDataIfNeeded_ValidDate()
		{
			var effectiveDate = ZDate.BrettsBirthday;
			var tariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Norway, tariffType.PK, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var mock = new Mock<ITariffViewFilterData>();
			mock.Setup(m => m.EffectiveDate).Returns(effectiveDate);
			tariff.SetupDefaultFilterDataIfNeeded(mock.Object);
			AssertEquals("Wrapper.EffectiveDate valid date", effectiveDate, tariff.Wrapper.EffectiveDate);
		}

		public void TestSetupDefaultFilterDataIfNeeded_InvalidDate()
		{
			var effectiveDate = ZDate.Invalid;
			var tariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Norway, tariffType.PK, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var mock = new Mock<ITariffViewFilterData>();
			mock.Setup(m => m.EffectiveDate).Returns(effectiveDate);
			tariff.SetupDefaultFilterDataIfNeeded(mock.Object);
			AssertEquals("Wrapper.EffectiveDate not affected by invalid date", ZDate.Today, tariff.Wrapper.EffectiveDate);
		}

		void SetupDataForGetAdditionalCodesForRateType(string rateTypeCode)
		{
			var tradeGroup = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "TG1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);

			var preference100 = helper.CreatePreferenceForCountry("100", "100", Core.Constants.CountryCodes.SouthAfrica);
			var preference200 = helper.CreatePreferenceForCountry("200", "200", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			Helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.EuropeanUnion);
			{
				var rateType = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, rateTypeCode);
				var rateCode = Helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
				var testRate = Helper.CreateRate(CusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePk: preference100.PK);
				Helper.CreateCusApplicabilityInternal(testRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
				Helper.CreateCusApplicabilityInternal(testRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");
				Helper.CreateCusApplicabilityInternal(testRate, tradeGroup, ZDateTime.Today.AddDays(10), ZDateTime.MaxSmallDateTime, "AC03");

				var testRate2 = Helper.CreateRate(CusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePk: preference200.PK);
				Helper.CreateCusApplicabilityInternal(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC04");
			}
			{
				var rateType = Helper.CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, "RTX", ensureDataGroupingExists: false);
				var rateCode = Helper.CreateCusRateCode(Factory, "RCX", rateType.PK);
				var testRate = Helper.CreateRate(CusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				Helper.CreateCusApplicabilityInternal(testRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC05");
			}
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject() => CusTariff;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CusTariff;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CusTariff;

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override void SetUp()
		{
			base.SetUp();
			Helper.CreateRefCusTaxOrFeeType("VAT");
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			s1p1TariffType = Helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			dutyRateType = Helper.CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			Factory.Save();
			djcRateCode = Helper.CreateCusRateCode(Factory, "DJC", dutyRateType.PK);
			var zA1Fee = Helper.CreateTaxOrFee("ZA1", 0.02m, Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false);
			zA1Fee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var zA2Fee = Helper.CreateTaxOrFee("ZA2", 0.01m, Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false);
			zA2Fee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var zA3Fee = Helper.CreateTaxOrFee("ZA3", 9999m, Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false);
			zA3Fee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			Helper.CreateOrGetLanguage("ENG", "English");
			Helper.CreateOrGetLanguage("FRN", "French");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		RefCusRateType dutyRateType;
		CusRefRateCodeView djcRateCode;

		(TariffView Tariff, VATApplicabilityView[] CreatedVatApplicabilities) CreateVatApplicabilities(ZDateTime startDate, ZDateTime endDate)
		{
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			var vatApplicability_ZA1 = Helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.SouthAfrica, "ZA1", startDate: startDate, endDate: endDate);
			var vatApplicability_ZA2 = Helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.SouthAfrica, "ZA2", startDate: startDate, endDate: endDate);
			return (tariff, new[] { vatApplicability_ZA1, vatApplicability_ZA2 });
		}

		void ClearFullTariffDescriptionCacheDE(TariffView tariff)
		{
			ClearFullTariffDescriptionCache(tariff, "DE", "DE");
			ClearFullTariffDescriptionCache(tariff, "DE-DE", "DE");
		}

		void ClearFullTariffDescriptionCacheIT(TariffView tariff) => ClearFullTariffDescriptionCache(tariff, "IT-IT", "IT");

		void ClearFullTariffDescriptionCache(TariffView tariff, ZString language, ZString languageCacheKey)
		{
			var fullDescriptionCacheKey = $"RefCusTariff_FullTariffDescription_{tariff.PK.ToStringKey()}_30-Aug-16_{language}_True_True_True";
			var tariffDescriptionCacheKey = ZString.Format("TariffView_{0}_ZX7_Description_{1}_False_Alternate", tariff.PK, languageCacheKey);
			var alternateLanguageCacheKey = ZString.Format("TariffViewAlternateLanguage_{0}_{1}_False", tariff.PK, languageCacheKey);

			tariff.Factory.ClearCachedValue<ZString>(fullDescriptionCacheKey);
			tariff.Factory.ClearCachedValue<ZString>(tariffDescriptionCacheKey);
			tariff.Factory.ClearCachedValue<BusinessObject>(alternateLanguageCacheKey);
		}

		TariffView cusTariff;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));

		UniversalReferenceTestDataHelper helper;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));

		static ZString DataGrouping => GlbCompany.CurrentCompany.Country.Code;
	}
}
