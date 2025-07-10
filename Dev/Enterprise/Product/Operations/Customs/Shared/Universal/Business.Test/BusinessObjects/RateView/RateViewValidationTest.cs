using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	internal class RateViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAtLeastOneApplicabilityExists()
		{
			var rateView = Factory.New<RateView>();
			rateView.ZZ2_IsSystem = true;
			var error = "At least one applicability is needed for this rate.";

			CombineAssertions(() =>
			{
				rateView.Validation.ValidateAll();
				AssertNoRowError("No error when IsSystem", rateView, error);

				rateView.ZZ2_IsSystem = false;
				rateView.Validation.ValidateAll();
				AssertNoRowError("No error when CusTariff is null", rateView, error);

				var tariff = Factory.New<TariffView>();
				tariff.ZZ1_IsSystem = false;
				tariff.ZZ1_StartDate = ZDateTime.Now;
				tariff.ZZ1_EndDate = ZDateTime.Now.AddDays(1);
				rateView.ZZ2_ZZ1_ParentTariffOrNationalCode = tariff.PK;
				rateView.ZZ2_StartDate = tariff.ZZ1_StartDate;
				rateView.ZZ2_EndDate = tariff.ZZ1_EndDate;

				rateView.Validation.ValidateAll();
				AssertHasRowError("Has error when no applicability", rateView, error);

				var applicability = rateView.FilteredRateApplicabilities.AddNew();
				rateView.Validation.ValidateAll();
				AssertNoRowError("No error", rateView, error);
			});
		}

		public void TestValidateZZ2_ZZS_PreferenceCodeEmpty_ZZRateView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			const string countryCode = Core.Constants.CountryCodes.SouthAfrica;
			helper.CreateNewOrGetExistingDataGrouping(countryCode);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, "1P1");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(countryCode, Constants.RateTypes.Duty, "Duty");
			var exciseRateType = helper.CreateNewOrGetExistingRateType(countryCode, Constants.RateTypes.Excise, "Excise");

			var dutyRateCode = helper.CreateCusRateCode(Factory, "DJC", dutyRateType.PK, cusRateType: "DTY", countryCode: countryCode);
			var exciseRateCode = helper.CreateCusRateCode(Factory, "EJC", exciseRateType.PK, cusRateType: "EXC", countryCode: countryCode);

			var cusTariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "ZZTariff", startDate, endDate, "dummy Description 0");
			var preferenceView = helper.CreatePreferenceForCountry("PR", "Test Preference", countryCode);
			var dutyRateView = helper.CreateRate(cusTariff, dutyRateCode.PK, startDate, endDate, "0");
			var dutyRateView2 = helper.CreateRate(cusTariff, dutyRateCode.PK, startDate, endDate, "0", preferencePk: preferenceView.PK);
			var exciseRateView = helper.CreateRate(cusTariff, exciseRateCode.PK, startDate, endDate, "0");
			var exciseRateView2 = helper.CreateRate(cusTariff, exciseRateCode.PK, startDate, endDate, "0", preferencePk: preferenceView.PK);
			Factory.Save();

			const string message = "The preference should not be empty when Rate Type is Duty(DTY).";
			CombineAssertions(() =>
			{
				dutyRateView.Validation.ValidateZZ2_ZZS_Preference();
				AssertEquals("RateType is Duty(DTY) if ZZ2_ZZS_Preference is empty", false, dutyRateView.ZZ2_ZZS_PreferenceInfo.HasError(message));

				dutyRateView2.Validation.ValidateZZ2_ZZS_Preference();
				AssertEquals("RateType is Duty(DTY) if ZZ2_ZZS_Preference is not empty", false, dutyRateView2.ZZ2_ZZS_PreferenceInfo.HasError(message));

				exciseRateView.Validation.ValidateZZ2_ZZS_Preference();
				AssertEquals("RateType is not Duty(DTY) if ZZ2_ZZS_Preference is empty", false, exciseRateView.ZZ2_ZZS_PreferenceInfo.HasError(message));

				exciseRateView2.Validation.ValidateZZ2_ZZS_Preference();
				AssertEquals("RateType is not Duty(DTY) if ZZ2_ZZS_Preference is not empty", false, exciseRateView2.ZZ2_ZZS_PreferenceInfo.HasError(message));
			});
		}

		public void TestValidateZZ2_ZZS_PreferenceCodeEmpty_ManualRateView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			const string countryCode = Core.Constants.CountryCodes.Congo;
			helper.CreateNewOrGetExistingDataGrouping(countryCode);

			var tariffType = helper.CreateTariffType(countryCode, "1P1", ensureDataGroupingExists: false);

			var dutyRateCode = helper.CreateCusRateCode(Factory, "DJC", ZGuid.Empty, isSystem: false, cusRateType: "DTY", countryCode: countryCode);
			var exciseRateCode = helper.CreateCusRateCode(Factory, "EJC", ZGuid.Empty, isSystem: false, cusRateType: "EXC", countryCode: countryCode);

			var cusTariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "ManualTariff", startDate, endDate, "dummy Description 0", isSystem: false);
			var preferenceView = helper.CreatePreferenceForCountry("PR", "Test Preference", countryCode, isSystem: false);
			var dutyRateView = helper.CreateRate(cusTariff, dutyRateCode.PK, startDate, endDate, "0", isSystem: false);
			var dutyRateView2 = helper.CreateRate(cusTariff, dutyRateCode.PK, startDate, endDate, "0", isSystem: false, preferencePk: preferenceView.PK);
			var exciseRateView = helper.CreateRate(cusTariff, exciseRateCode.PK, startDate, endDate, "0", isSystem: false);
			var exciseRateView2 = helper.CreateRate(cusTariff, exciseRateCode.PK, startDate, endDate, "0", isSystem: false, preferencePk: preferenceView.PK);

			Factory.Save();

			const string message = "The preference should not be empty when Rate Type is Duty(DTY).";
			CombineAssertions(() =>
			{
				dutyRateView.Validation.ValidateZZ2_ZZS_Preference();
				AssertHasError("RateType is Duty(DTY) if ZZ2_ZZS_Preference is empty", dutyRateView.ZZ2_ZZS_PreferenceInfo, message);

				dutyRateView2.Validation.ValidateZZ2_ZZS_Preference();
				AssertNoError("RateType is Duty(DTY) if ZZ2_ZZS_Preference is not empty", dutyRateView2.ZZ2_ZZS_PreferenceInfo, message);

				exciseRateView.Validation.ValidateZZ2_ZZS_Preference();
				AssertNoError("RateType is not Duty(DTY) if ZZ2_ZZS_Preference is empty", exciseRateView.ZZ2_ZZS_PreferenceInfo, message);

				exciseRateView2.Validation.ValidateZZ2_ZZS_Preference();
				AssertNoError("RateType is not Duty(DTY) if ZZ2_ZZS_Preference is not empty", exciseRateView2.ZZ2_ZZS_PreferenceInfo, message);
			});
		}

		public void TestCheckZZ2_RateFormula_NoUnitList()
		{
			var rateView = CreateRateView();
			rateView.ZZ2_IsSystem = false;
			AssertNoErrors("no error when ZZ2_RateFormula is empty", rateView.ZZ2_RateFormulaInfo);

			AssertFormulaParserError(rateView);
			AssertFormulaCountrySpecificValueError(rateView);

			var error = "The Formula is not valid due to the error(s): Unit of Measure code:";
			AssertEquals("The units of the tariff's UOMS is empty", "", rateView.CusTariff.UnitList.CodesAsString);

			rateView.ZZ2_RateFormula = "MAX(5*VFD,[B])";
			AssertHasErrorContaining("UnitOfMeasure Value error 1", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "10000000003 * [KG]";
			AssertHasErrorContaining("UnitOfMeasure Value error 2", rateView.ZZ2_RateFormulaInfo, error);
		}

		public void TestCheckZZ2_RateFormula_HasUnitList()
		{
			var rateView = CreateRateView(true);
			rateView.ZZ2_IsSystem = false;

			AssertFormulaParserError(rateView);
			AssertFormulaCountrySpecificValueError(rateView);

			var error = "The Formula is not valid due to the error(s): Unit of Measure code:";
			AssertEquals("The units of the tariff's UOMS", "NO", rateView.CusTariff.UnitList.CodesAsString);

			rateView.ZZ2_RateFormula = "MAX(100,[KG])";
			AssertHasErrorContaining("Kg is not in the valid unit list", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "MAX(100,[NO])";
			AssertNoErrors("valid unit Value NO", rateView.ZZ2_RateFormulaInfo);
		}

		void AssertFormulaParserError(RateView rateView)
		{
			var error = "The Formula is not valid due to the error(s): Syntax error at";
			rateView.ZZ2_RateFormula = "99 / (1 && 3)";
			AssertHasErrorContaining("Parse error 1", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "{}";
			AssertHasErrorContaining("Parse error 2", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "3 * [KG";
			AssertHasErrorContaining("Parse error 3", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "15 * 0.5 + 10";
			AssertNoErrors(rateView.ZZ2_RateFormulaInfo);
		}

		void AssertFormulaCountrySpecificValueError(RateView rateView)
		{
			var error = "The Formula is not valid due to the error(s): Country Specific Value";
			rateView.ZZ2_RateFormula = "A*B";
			AssertHasErrorContaining("Country Specific Value error 1", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "MAX(A,B)";
			AssertHasErrorContaining("Country Specific Value error 2", rateView.ZZ2_RateFormulaInfo, error);

			rateView.ZZ2_RateFormula = "MAX(100,5*VFD)";
			AssertNoErrors("valid reserved word VFD", rateView.ZZ2_RateFormulaInfo);
		}

		public void TestCheckZZ2_StartDate()
		{
			var message = "Start Date cannot be later than End Date.";
			var message2 = "Start Date cannot be earlier than Tariff's Effective From date.";
			var rateView = CreateRateView();
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_EndDate = new ZDateTime(2021, 03, 01);
			rateView.ZZ2_StartDate = new ZDateTime(2021, 03, 02);

			CombineAssertions(() =>
			{
				AssertHasError("Start Date is later than End Date. Has error message", rateView.ZZ2_StartDateInfo, message);

				rateView.ZZ2_StartDate = new ZDateTime(2020, 01, 21);
				AssertHasError("Start Date is earlier than Tariff's Effective From date. Has error message2", rateView.ZZ2_StartDateInfo, message2);

				rateView.ZZ2_StartDate = new ZDateTime(2021, 03, 01);
				AssertNoError("Start Date is Equal to End Date. No error message", rateView.ZZ2_StartDateInfo, message);

				rateView.ZZ2_StartDate = new ZDateTime(2021, 02, 22);
				AssertNoError("Start Date is earlier than End Date. No error message", rateView.ZZ2_StartDateInfo, message);
				AssertNoError("Start Date is later than Tariff's Effective From date. No error message2", rateView.ZZ2_StartDateInfo, message2);
			});
		}

		public void TestCheckZZ2_StartDateIsValidZDateTimeRange()
		{
			var rateView = CreateRateView();
			rateView.CusTariff.ZZ1_StartDate = ZDateTime.MinSmallDateTimeValue;
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_StartDate = new ZDateTime(1999, 01, 01);

			AssertNoErrors(rateView.ZZ2_StartDateInfo);
		}

		public void TestCheckZZ2_EndDate()
		{
			var message = "End Date must be later than Start Date.";
			var message2 = "End Date cannot be later than Tariff's Effective To date.";
			var rateView = CreateRateView();
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_StartDate = new ZDateTime(2021, 02, 22);
			rateView.ZZ2_EndDate = new ZDateTime(2021, 02, 20);

			CombineAssertions(() =>
			{
				AssertHasError("End Date is earlier than Start Date. Has error message", rateView.ZZ2_EndDateInfo, message);

				rateView.ZZ2_EndDate = new ZDateTime(2022, 01, 21);
				AssertHasError("End Date is later than Tariff's Effective To date. Has error message2", rateView.ZZ2_EndDateInfo, message2);

				rateView.ZZ2_EndDate = new ZDateTime(2021, 02, 22);
				AssertNoError("End Date is Equal to Start Date. No error message2", rateView.ZZ2_EndDateInfo, message2);

				rateView.ZZ2_EndDate = new ZDateTime(2021, 02, 24);
				AssertNoError("End Date is latter than Start Date. No error message", rateView.ZZ2_EndDateInfo, message);
				AssertNoError("End Date is earlier than Tariff's Effective To date. No error message2", rateView.ZZ2_EndDateInfo, message2);
			});
		}

		[TestDate(2021, 02, 23)]
		public void TestCheckZZT_EndDateIsValidZDateTimeRange()
		{
			var rateView = CreateRateView();
			rateView.CusTariff.ZZ1_EndDate = ZDateTime.MaxSmallDateTime;
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_EndDate = new ZDateTime(2027, 02, 23);

			AssertNoErrors(rateView.ZZ2_EndDateInfo);
		}

		public void TestCheckZZ2_ZY1_RateCode()
		{
			var rateView = CreateRateView();

			CombineAssertions(() =>
			{
				rateView.Validation.ValidateZZ2_ZY1_RateCode();
				AssertHasErrorContaining("Has error", rateView.ZZ2_ZY1_RateCodeInfo, MandatoryValidation.MustBeEntered);

				rateView.ZZ2_ZY1_RateCode = Factory.New<CusRefRateCode>().PK;
				AssertNoErrorContaining("No error", rateView.ZZ2_ZY1_RateCodeInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckZZ2_ZY1_RateCode_RateCodeAndPreferenceAndStartDateIsUnique()
		{
			var message = "The Rate with same Start Date, Preference and Rate Code already exists.";
			var rateView = CreateRateView();
			var cusRefPreference = Factory.New<CusRefPreference>();
			var cusRefRateCode = Factory.New<CusRefRateCode>();

			rateView.ZZ2_StartDate = new ZDateTime(2021, 01, 01);
			rateView.ZZ2_ZZS_Preference = cusRefPreference.PK;
			rateView.ZZ2_ZY1_RateCode = cusRefRateCode.PK;

			var rateView2 = rateView.CusTariff.FilteredRates.AddNew();
			rateView2.ZZ2_StartDate = rateView.ZZ2_StartDate;
			rateView2.ZZ2_ZZS_Preference = rateView.ZZ2_ZZS_Preference;
			rateView2.ZZ2_ZY1_RateCode = rateView.ZZ2_ZY1_RateCode;

			CombineAssertions(() =>
			{
				AssertHasError("Same Start Date, Preference and Rate Code", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_ZZS_Preference = ZGuid.Empty;
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertNoError("Same Start Date and Rate Code, different preference", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_ZZS_Preference = rateView.ZZ2_ZZS_Preference;
				rateView2.ZZ2_StartDate = new ZDateTime(2021, 02, 02);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertNoError("Same Preference and Rate Code, different Start Date", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_StartDate = rateView.ZZ2_StartDate;
				rateView2.ZZ2_ZY1_RateCode = Factory.New<CusRefRateCode>().PK;
				AssertNoError("Same Start Date and preference, different Rate Code", rateView2.ZZ2_ZY1_RateCodeInfo, message);
			});
		}

		public void TestCheckZZ2_ZY1_RateCode_RateCodeAndPreferenceIsSameAndNotOverlap()
		{
			var message = "The date range of this Rate overlaps with another Rate with same Preference and Rate Code.";
			var rateView = CreateRateView();
			var cusRefPreference = Factory.New<CusRefPreference>();
			var cusRefRateCode = Factory.New<CusRefRateCode>();

			rateView.ZZ2_StartDate = new ZDateTime(2021, 02, 01);
			rateView.ZZ2_EndDate = new ZDateTime(2021, 03, 01);
			rateView.ZZ2_ZZS_Preference = cusRefPreference.PK;
			rateView.ZZ2_ZY1_RateCode = cusRefRateCode.PK;

			var rateView2 = rateView.CusTariff.FilteredRates.AddNew();
			rateView2.ZZ2_ZZS_Preference = rateView.ZZ2_ZZS_Preference;
			rateView2.ZZ2_ZY1_RateCode = rateView.ZZ2_ZY1_RateCode;
			CombineAssertions(() =>
			{
				rateView2.ZZ2_StartDate = new ZDateTime(2021, 01, 01);
				rateView2.ZZ2_EndDate = new ZDateTime(2021, 01, 10);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertNoError("ZZ2_StartDate < ZZ2_EndDate < Existing StartDate", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_StartDate = new ZDateTime(2021, 01, 01);
				rateView2.ZZ2_EndDate = new ZDateTime(2021, 02, 01);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertHasError("ZZ2_StartDate < Existing StartDate <= ZZ2_EndDate", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_StartDate = new ZDateTime(2021, 02, 10);
				rateView2.ZZ2_EndDate = new ZDateTime(2021, 03, 01);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertHasError("ZZ2_StartDate < Existing EndDate <= ZZ2_EndDate", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_StartDate = new ZDateTime(2021, 03, 01);
				rateView2.ZZ2_EndDate = new ZDateTime(2021, 03, 10);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertHasError("ZZ2_StartDate <= Existing EndDate < ZZ2_EndDate", rateView2.ZZ2_ZY1_RateCodeInfo, message);

				rateView2.ZZ2_StartDate = new ZDateTime(2021, 03, 10);
				rateView2.ZZ2_EndDate = new ZDateTime(2021, 03, 31);
				rateView2.Validation.ValidateZZ2_ZY1_RateCode();
				AssertNoError("Existing EndDate < ZZ2_StartDate < ZZ2_EndDate", rateView2.ZZ2_ZY1_RateCodeInfo, message);
			});
		}

		public void TestCheckZZ2_ZZZ_NKDataGroupingIsNotEmpty()
		{
			var rateView = CreateRateView();
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_ZZZ_NKDataGrouping = ZString.Empty;

			AssertNoErrors(rateView.ZZ2_ZZZ_NKDataGroupingInfo);
		}

		public void TestCheckZZ2_RX_NKCurrencyOverrideIsNotEmpty()
		{
			var rateView = CreateRateView();
			rateView.ZZ2_IsSystem = false;
			rateView.ZZ2_RX_NKCurrencyOverride = ZString.Empty;

			AssertNoErrors(rateView.ZZ2_RX_NKCurrencyOverrideInfo);
		}

		RateView CreateRateView(bool createTariffUOM = false)
		{
			var country = Core.Constants.CountryCodes.Eritrea;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(country);
			var tariffType = helper.CreateTariffType(country, "1P1", ensureDataGroupingExists: false);
			if (createTariffUOM)
			{
				helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ");
				helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilogram", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
				helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "NO", "Number", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			}
			Factory.Save();

			var tariffView = helper.CreateTariff(country, tariffType.PK, "DUMMYTRF", new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01), "dummy Description 0");
			if (createTariffUOM)
			{
				var uom1 = tariffView.UnitsOfMeasure.AddNew();
				uom1.ZZ8_Type = "CU1";
				uom1.ZZ8_UOM = "NO";
				var uom2 = tariffView.UnitsOfMeasure.AddNew();
				uom2.ZZ8_Type = "CU2";
				uom2.ZZ8_UOM = "11";// Invalid uom
			}
			var rateView = tariffView.FilteredRates.AddNew();
			return rateView;
		}
	}
}
