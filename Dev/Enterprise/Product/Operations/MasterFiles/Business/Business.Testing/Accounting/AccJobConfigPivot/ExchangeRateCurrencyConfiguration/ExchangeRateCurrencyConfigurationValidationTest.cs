using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExchangeRateCurrencyConfigurationValidationTest : AccJobConfigPivotValidationTest
	{
		public void TestValidateJCT_Code_CheckForEmpty()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();
			currencyConfig.AccExRateConfiguration.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			currencyConfig.JCT_Code = string.Empty;
			AssertHasError(currencyConfig.JCT_CodeInfo, "Please enter a Currency.");
			currencyConfig.JCT_Code = CurrencyCodes.Australia;
			AssertNoError(currencyConfig.JCT_CodeInfo, "Please enter a Currency.");
		}

		public void TestValidateJCT_Code_CheckForNotValidOnList()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_Code = "1.2";
			AssertHasError(currencyConfig.JCT_CodeInfo, "Enter a valid Currency.");
			currencyConfig.JCT_Code = CurrencyCodes.Australia;
			AssertNoError(currencyConfig.JCT_CodeInfo, "Enter a valid Currency.");
		}

		public void TestCheckDuplicate_ForTheSameAccExchangeRateConfiguration()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			AssertHasError(currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);

			currencyConfig2.JCT_Code = CurrencyCodes.UnitedStates;
			AssertNoError(currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);
		}

		public void TestCheckDuplicate_ForDifferentAccExchangeRateConfigurations()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			AssertHasError(currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);

			var accExRateConfig2 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			currencyConfig2.JCT_JCF_JobConfig = accExRateConfig2.PK;
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			AssertNoError(currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);
		}

		public void TestCheckDuplicate_TwoConfigInSameCollection()
		{
			var systemConfigs = new AccExchangeRateConfigurationCollection(Factory);
			var config1 = systemConfigs.AddNew();
			config1.JCE_JobType = "SHP";
			config1.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			var config2 = systemConfigs.AddNew();
			config2.JCE_JobType = "SHP";
			config2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			var currencyConfig1 = config1.CurrencyConfigurations.AddNew();
			var currencyConfig2 = config2.CurrencyConfigurations.AddNew();

			AssertEquals("PreCondition", true, config1.IsTheSameConfigWithCURCurrencyType(config2));
			CombineAssertions("We should compare currency settings between rate configs which's settings are same and JCE_Calc_CurrencyType is CUR.", () =>
			{
				currencyConfig1.JCT_Code = CurrencyCodes.Australia;
				currencyConfig2.JCT_Code = CurrencyCodes.Australia;
				AssertDuplicatedCurrencyError(currencyConfig1, expectedError: true);
				AssertDuplicatedCurrencyError(currencyConfig2, expectedError: true);

				currencyConfig1.JCT_Code = CurrencyCodes.Australia;
				currencyConfig2.JCT_Code = CurrencyCodes.UnitedStates;
				AssertDuplicatedCurrencyError(currencyConfig1, expectedError: false);
				AssertDuplicatedCurrencyError(currencyConfig2, expectedError: false);
			});

			config2.JCE_JobType = "ALL";
			AssertEquals("PreCondition", false, config1.IsTheSameConfigWithCURCurrencyType(config2));
			CombineAssertions("Would not combine testing two config's currencies with different settings.", () =>
			{
				currencyConfig1.JCT_Code = CurrencyCodes.Australia;
				currencyConfig2.JCT_Code = CurrencyCodes.Australia;
				AssertDuplicatedCurrencyError(currencyConfig1, expectedError: false);
				AssertDuplicatedCurrencyError(currencyConfig2, expectedError: false);
			});
		}

		void AssertDuplicatedCurrencyError(ExchangeRateCurrencyConfiguration currencyConfig, bool expectedError)
		{
			currencyConfig.Validation.ValidateJCT_Code();
			if (expectedError)
			{
				AssertHasError(currencyConfig.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);
			}
			else
			{
				AssertNoError(currencyConfig.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);
			}
		}

		public void TestCheckDuplicate_TwoCWInstances()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var systemConfigs_AnotherCW1 = new AccExchangeRateConfigurationCollection(newFactory);
			CreateUsdCurrencyExRateConfig(systemConfigs_AnotherCW1);

			var systemConfigs = new AccExchangeRateConfigurationCollection(Factory);
			CreateUsdCurrencyExRateConfig(systemConfigs);

			newFactory.Save();

			var ex = AssertExceptionThrown<ZSaveException>("Should fail the save because a duplicate config was created in another CW instance.", () => Factory.Save());
			AssertContains("Cannot insert duplicate key row in object 'dbo.vw_ExchangeRateCurrencyConfiguration' with unique index 'NR_UC__vw_ExchangeRateCurrencyConfiguration'.", ex.Message);

			void CreateUsdCurrencyExRateConfig(AccExchangeRateConfigurationCollection collection)
			{
				var config = collection.AddNew();
				config.JCE_JobType = "SHP";
				config.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
				var currency = config.CurrencyConfigurations.AddNew();
				currency.JCT_Code = CurrencyCodes.UnitedStates;
			}
		}

		public void TestJCT_ExRateType_HaveValueFromExRateTypeList()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();

			foreach (var rateType in currencyConfig.Lookups.ExRateTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				currencyConfig.JCT_ExRateType = rateType;
				currencyConfig.Validation.ValidateJCT_ExRateType();
				AssertNoError(currencyConfig.JCT_ExRateTypeInfo, "Enter a valid Ex Rate Type.");
			}

			currencyConfig.JCT_ExRateType = "ZZZ";
			currencyConfig.Validation.ValidateJCT_ExRateType();
			AssertHasError(currencyConfig.JCT_ExRateTypeInfo, "Enter a valid Ex Rate Type.");
		}

		public void TestJCT_ExRateType_IsNotEmpty()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();

			currencyConfig.JCT_ExRateType = ZString.Empty;

			AssertHasError(currencyConfig.JCT_ExRateTypeInfo, "Please enter an Ex Rate Type.");
		}

		public void TestExpiryDate_CheckEntered()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig.JCT_ExpiryDate = ZDate.Empty;

			AssertHasError(currencyConfig.JCT_ExpiryDateInfo, "Please enter an Expiry Date.");
		}

		public void TestStartDate_CheckEntered()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_ExpiryDate = new ZDate(2020, 1, 1);
			currencyConfig.JCT_StartDate = ZDate.Empty;

			AssertHasError(currencyConfig.JCT_StartDateInfo, "Please enter a Start Date.");
		}

		public void TestWhenStartDateGreaterThanExpiryDate()
		{
			var currencyConfig = Factory.NewWithValidTestData<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig.JCT_ExpiryDate = new ZDate(2019, 1, 1);

			AssertHasError(currencyConfig.JCT_ExpiryDateInfo, "Expiry Date must be after Start Date.");
			AssertHasError(currencyConfig.JCT_StartDateInfo, "Expiry Date must be after Start Date.");
		}

		public void TestDateRange_OverlappingDate()
		{
			var startDate = new ZDate(2020, 1, 1);
			var expiryDate = new ZDate(2020, 12, 31);

			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			currencyConfig1.JCT_StartDate = startDate;
			currencyConfig1.JCT_ExpiryDate = expiryDate;
			currencyConfig2.JCT_StartDate = expiryDate.AddDays(1);
			currencyConfig2.JCT_ExpiryDate = expiryDate.AddDays(5);

			AssertNoError(currencyConfig2.JCT_ExpiryDateInfo, OverlappingDateErrorMsg);
			AssertNoError(currencyConfig2.JCT_StartDateInfo, OverlappingDateErrorMsg);

			AssertOverlappingDateError("config2 date range within config1 date range", currencyConfig2, startDate.AddDays(1), expiryDate.AddDays(-1));

			AssertOverlappingDateError("config2 date range cover config1 date range", currencyConfig2, startDate.AddDays(-1), expiryDate.AddDays(1));

			AssertOverlappingDateError("config2 date range is contiguous with config1 date range", currencyConfig2, startDate.AddDays(-5), startDate);

			AssertOverlappingDateError("config2 date range is right after config1 date range", currencyConfig2, expiryDate, expiryDate.AddDays(5));

			AssertOverlappingDateError("config2 date range is the same with config1 date range", currencyConfig2, startDate, expiryDate);

			currencyConfig2.JCT_Code = CurrencyCodes.UnitedStates;
			AssertNoError(currencyConfig2.JCT_ExpiryDateInfo, OverlappingDateErrorMsg);
			AssertNoError(currencyConfig2.JCT_StartDateInfo, OverlappingDateErrorMsg);
		}

		void AssertOverlappingDateError(string comment, ExchangeRateCurrencyConfiguration currencyConfig, ZDate startDate, ZDate expiryDate)
		{
			currencyConfig.JCT_StartDate = startDate;
			currencyConfig.JCT_ExpiryDate = expiryDate;

			AssertHasError(comment, currencyConfig.JCT_ExpiryDateInfo, OverlappingDateErrorMsg);
			AssertHasError(comment, currencyConfig.JCT_StartDateInfo, OverlappingDateErrorMsg);
		}

		public void TestDuplicateCurrencyCode_WithDateRange()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			AssertHasError("Precondition",currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);

			currencyConfig2.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig2.JCT_ExpiryDate = new ZDate(2020, 12, 31);
			AssertNoError(currencyConfig2.JCT_CodeInfo, ExpectedDuplicateCurrencyCodeErrorMsg);
		}

		const string ExpectedDuplicateCurrencyCodeErrorMsg = @"The same Currency Code already exists in the Job Billing Exchange Rate configuration.
Please check values for each parameter, make sure the currency lists don't overlap when you save multiple Job Exchange Rate Configurations with the same attributes (job type, transport mode, ledger, direction, and currency type).";

		const string OverlappingDateErrorMsg = "This overlaps with an existing configuration with the same job parameters.";
	}
}
