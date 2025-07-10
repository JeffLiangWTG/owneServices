using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	class TariffViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZ1_Description()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_IsSystem = false;

			CombineAssertions(() =>
			{
				tariff.Validation.ValidateZZ1_Description();
				AssertHasErrorContaining("Description must be mandatory", tariff.ZZ1_DescriptionInfo, MandatoryValidation.MustBeEntered);

				tariff.ZZ1_Description = "1";
				AssertNoErrors("Description has value no matter the length", tariff.ZZ1_DescriptionInfo);
			});
		}

		public void TestTariffCodeIsNumeric()
		{
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "FDASD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2));
			tariff.Validation.ValidateZZ1_TariffCode();
			AssertHasError(tariff.ZZ1_TariffCodeInfo, "Tariff Codes must be numeric.");
		}

		public void TestDateRangeValidation()
		{
			const string fromDateLaterThanToDate = "Effective From date cannot be later than Effective To date.";
			const string toDateEarlierThanFromDate = "Effective To date cannot be earlier than the Effective From date.";

			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2020, 11, 24), new ZDateTime(2020, 11, 25));
			tariff.ZZ1_EndDate = new ZDateTime(2020, 11, 23);
			tariff.Validation.ValidateZZ1_StartDate();
			tariff.Validation.ValidateZZ1_EndDate();

			CombineAssertions(() =>
			{
				AssertHasError(tariff.ZZ1_StartDateInfo, fromDateLaterThanToDate);
				AssertHasError(tariff.ZZ1_EndDateInfo, toDateEarlierThanFromDate);
			});
		}

		public void TestCheckNKDataGroupingIsInList()
		{
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2020, 11, 25), new ZDateTime(2020, 11, 26));
			tariff.Validation.ValidateZZ1_ZZZ_NKDataGrouping();
			AssertHasError(tariff.ZZ1_ZZZ_NKDataGroupingInfo, "Enter a valid Country/Region or Grouping.");
		}

		public void TestCheckDuplicateTariffs_StartDate()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2020, 12, 7), new ZDateTime(2020, 12, 8), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2020, 12, 9), new ZDateTime(2020, 12, 19), tariffVersion: "VER1");
			tariff.ZZ1_StartDate = new ZDateTime(2020, 12, 7);
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestCheckDuplicateTariffs_EndDate()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "20001", new ZDateTime(2020, 12, 7), new ZDateTime(2020, 12, 9), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "20001", new ZDateTime(2020, 12, 18), new ZDateTime(2020, 12, 19), tariffVersion: "VER1");
			tariff.ZZ1_EndDate = new ZDateTime(2020, 12, 9);
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestCheckDuplicateTariffs_Version()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER2");
			tariff.ZZ1_CRT_NKTariffVersion = "VER1";
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestCheckDuplicateTariffs_TariffCode()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40002", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			tariff.ZZ1_TariffCode = "40001";
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestCheckDuplicateTariffs_DataGrouping()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			UniversalReferenceHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Congo);
			var tariff = UniversalReferenceHelper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			tariff.ZZ1_ZZZ_NKDataGrouping = currentCounty;
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestCheckDuplicateTariffs_TariffType()
		{
			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "OTH", "40001", new ZDateTime(2020, 12, 14), new ZDateTime(2020, 12, 16), tariffVersion: "VER1");
			tariff.ZZ1_ZZI_NKTariffType = "HSN";
			tariff.Validation.ValidateAll();
			AssertHasRowError(tariff, DuplicateTariffMessage);
		}

		public void TestOverlappingDateRanges()
		{
			var baseDate = new ZDateTime(2020, 12, 10);
			const string overlappingDateError = "The date range of this tariff overlaps with another tariff.";

			UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", baseDate.AddDays(100), baseDate.AddDays(200), tariffVersion: "VER1");
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", baseDate.AddDays(201), baseDate.AddDays(299), tariffVersion: "VER1");

			CombineAssertions(() =>
			{
				tariff.Validation.ValidateAll();
				AssertNoRowErrors(tariff);

				tariff.ZZ1_EndDate = baseDate.AddDays(199);
				tariff.Validation.ValidateAll();
				AssertHasRowError("End Date overlap one existed tariff's end date", tariff, overlappingDateError);

				tariff.ZZ1_EndDate = baseDate.AddDays(299);
				tariff.ZZ1_StartDate = baseDate.AddDays(199);
				tariff.Validation.ValidateAll();
				AssertHasRowError("Start Date overlap one existed tariff's end date", tariff, overlappingDateError);
			});
		}

		public void TestDateRangeIsValidValidation()
		{
			const string tooEarlyError = "Effective From date must not be before 01/01/2000.";
			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2020, 12, 7), new ZDateTime(2020, 12, 8));
			tariff.ZZ1_StartDate = new ZDateTime(1999, 12, 31);
			tariff.ZZ1_IsSystem = false;

			CombineAssertions(() =>
			{
				tariff.Validation.ValidateAll();
				AssertHasError(tariff.ZZ1_StartDateInfo, tooEarlyError);

				tariff.ZZ1_EndDate = new ZDateTime(9999, 12, 31);
				tariff.Validation.ValidateAll();
				AssertNoErrors(tariff.ZZ1_EndDateInfo);
			});
		}

		public void TestCheckZZ1_ZZF_NKTaxOrFeeCode_ListValidation()
		{
			UniversalReferenceHelper.CreateNewOrGetExistingRateType(currentCounty, Constants.RateTypes.Duty, "Duty");
			UniversalReferenceHelper.CreateTaxOrFee("AU1", 0.1, currentCounty, 0, 10, "VAT", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 3, 1), "ERDESC1");
			Factory.Save();

			var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2011, 2, 14), new ZDateTime(2011, 2, 22));
			CombineAssertions(() =>
			{
				tariff.ZZ1_ZZF_NKTaxOrFeeCode = "XX";
				AssertHasErrorContaining("Isn't system and invalid code", tariff.ZZ1_ZZF_NKTaxOrFeeCodeInfo, ListValidation.InvalidCodeError);
				tariff.ZZ1_ZZF_NKTaxOrFeeCode = "AU1";
				AssertNoErrorContaining("Isn't system and valid code", tariff.ZZ1_ZZF_NKTaxOrFeeCodeInfo, ListValidation.InvalidCodeError);
				tariff.ZZ1_IsSystem = true;
				tariff.ZZ1_ZZF_NKTaxOrFeeCode = "XX";
				AssertNoErrorContaining("Is system and invalid code", tariff.ZZ1_ZZF_NKTaxOrFeeCodeInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckZZ1_ZZF_NKTaxOrFeeCode_AllowEmpty()
		{
			CombineAssertions(() =>
			{
				var tariff = Factory.New<TariffView>();
				tariff.Validation.ValidateZZ1_ZZF_NKTaxOrFeeCode();
				AssertNoNotifications("Not system", tariff.ZZ1_ZZF_NKTaxOrFeeCodeInfo);
				tariff.ZZ1_IsSystem = true;
				tariff.Validation.ValidateZZ1_ZZF_NKTaxOrFeeCode();
				AssertNoNotifications("System", tariff.ZZ1_ZZF_NKTaxOrFeeCodeInfo);
			});
		}

		public void TestCheckZZ1_CRT_NKTariffVersion()
		{
			const string error = "Enter a valid Tariff Version.";

			CombineAssertions(() =>
			{
				var tariff = UniversalReferenceHelper.CreateManualTariff(currentCounty, "HSN", "10001", new ZDateTime(2011, 2, 14), new ZDateTime(2011, 2, 22), tariffVersion: "");
				tariff.Validation.ValidateZZ1_CRT_NKTariffVersion();
				AssertHasError("Tariff Version must has value when is not system", tariff.ZZ1_CRT_NKTariffVersionInfo, "Please enter a Tariff Version.");

				tariff.ZZ1_IsSystem = true;
				tariff.Validation.ValidateZZ1_CRT_NKTariffVersion();
				AssertNoError("No Tariff Version validation when is system", tariff.ZZ1_CRT_NKTariffVersionInfo, error);

				tariff.ZZ1_IsSystem = false;
				tariff.ZZ1_CRT_NKTariffVersion = "ZZ";
				AssertHasError("Tariff Version validation when is not system", tariff.ZZ1_CRT_NKTariffVersionInfo, error);

				UniversalReferenceHelper.CreateTariffVersion("V1", "V1 Desc.", ZDate.Today.AddDays(-1));
				tariff.ZZ1_CRT_NKTariffVersion = "V1";
				AssertNoError("Valid Tariff Version", tariff.ZZ1_CRT_NKTariffVersionInfo, error);
			});
		}

		protected override void SetUp()
		{
			currentCounty = GlbCompany.CurrentCompany.Country.Code;
			UniversalReferenceHelper.CreateNewOrGetExistingDataGrouping(currentCounty);
		}

		UniversalReferenceTestDataHelper UniversalReferenceHelper => universalReferenceHelper ?? (universalReferenceHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper universalReferenceHelper;

		string currentCounty;
		const string DuplicateTariffMessage = "Save would result in a duplicate for this Tariff.";
	}
}
