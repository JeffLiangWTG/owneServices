using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccOrgTaxRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOverlappingOnChangeSource()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration1 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData, true);

			var orgTaxRate1 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code);
			var orgTaxRate2 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate2.OTR_StartDateInfo);

			orgTaxRate2.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code;
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
		}

		public void TestOverlappingOnChangeOrgTaxConfiguration()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration1 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData, true);
			var accOrgTaxConfiguration2 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData, true);

			var orgTaxRate1 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			var orgTaxRate2 = AddOrgTaxRateItem(accOrgTaxConfiguration2, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate2.OTR_StartDateInfo);

			orgTaxRate2.OTR_OTC = accOrgTaxConfiguration1.PK;
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
		}

		public void TestOverlappingDatesAndSource()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration1 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData, true);

			var orgTaxRate1 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code);
			var orgTaxRate2 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Monthly.Code);
			AssertNoErrors("No overlapping dates with same source because use different tax rate option at the same tax configuration.", orgTaxRate2.OTR_StartDateInfo);

			var orgTaxRate3 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Monthly.Code);
			AssertHasError(orgTaxRate3.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");

			var accOrgTaxConfiguration2 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData, true);
			var orgTaxRate4 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			var orgTaxRate5 = AddOrgTaxRateItem(accOrgTaxConfiguration2, ZDate.Today, ZDate.Today, AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors("No overlapping dates with same source and tax rate values because use a other tax configuration.", orgTaxRate5.OTR_StartDateInfo);
		}

		public void TestCheck_OverlapDatesAndSourceForTaxCode()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration1 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData, true);

			var orgTaxRate1 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today, ZDate.Today.AddDays(2), AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			var orgTaxRate2 = AddOrgTaxRateItem(accOrgTaxConfiguration1, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(-1), AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate2.OTR_StartDateInfo);

			SetDate(orgTaxRate2, ZDate.Today.AddDays(-1), ZDate.Today);
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(2));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(3));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");

			SetDate(orgTaxRate2, ZDate.Today, ZDate.Today);
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today, ZDate.Today.AddDays(1));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today, ZDate.Today.AddDays(2));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today, ZDate.Today.AddDays(3));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");

			SetDate(orgTaxRate2, ZDate.Today.AddDays(1), ZDate.Today.AddDays(+1));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(1), ZDate.Today.AddDays(+2));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(1), ZDate.Today.AddDays(+3));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");

			SetDate(orgTaxRate2, ZDate.Today.AddDays(2), ZDate.Today.AddDays(2));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");
			SetDate(orgTaxRate2, ZDate.Today.AddDays(2), ZDate.Today.AddDays(3));
			AssertHasError(orgTaxRate2.OTR_StartDateInfo, "Date Ranges overlap for same Tax Code & Source.");

			SetDate(orgTaxRate2, ZDate.Today.AddDays(3), ZDate.Today.AddDays(3));
			AssertNoErrors(OrgTaxRate.OTR_StartDateInfo);

			void SetDate(AccOrgTaxRate item, ZDate startDate, ZDate endDate)
			{
				item.OTR_StartDate = startDate;
				item.OTR_EndDate = endDate;
			}
		}

		public void TestOTR_OTCIsNotNullOrEmpty()
		{
			OrgTaxRate.OTR_OTC = ZGuid.Empty;
			OrgTaxRate.RunPreSaveValidation();
			AssertHasError(OrgTaxRate.OTR_OTCInfo, "Please enter a value.");
		}

		public void TestOTR_OTC()
		{
			var accOrgTaxRate = Factory.New<AccOrgTaxRate>();
			accOrgTaxRate.Validation.ValidateOTR_OTC();
			AssertHasError(accOrgTaxRate.OTR_OTCInfo, "Please enter a value.");
		}

		public void TestOTR_Source()
		{
			OrgTaxRate.OTR_Source = ZString.Empty;
			OrgTaxRate.RunPreSaveValidation();
			AssertHasError(OrgTaxRate.OTR_SourceInfo, "Please enter a Source.");

			OrgTaxRate.OTR_Source = "AB";
			OrgTaxRate.RunPreSaveValidation();
			AssertHasError(OrgTaxRate.OTR_SourceInfo, "Enter a valid Source.");

			OrgTaxRate.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code;
			OrgTaxRate.RunPreSaveValidation();
			AssertNoErrors(OrgTaxRate.OTR_SourceInfo);

			OrgTaxRate.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Monthly.Code;
			OrgTaxRate.RunPreSaveValidation();
			AssertNoErrors(OrgTaxRate.OTR_SourceInfo);

			OrgTaxRate.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code;
			OrgTaxRate.RunPreSaveValidation();
			AssertNoErrors(OrgTaxRate.OTR_SourceInfo);
		}

		public void TestValidationInOTR_SourceSetter()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData, true);

			var orgTaxRate = AddOrgTaxRateItem(accOrgTaxConfiguration, ZDate.Today, ZDate.Today.AddDays(1), AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate.OTR_StartDateInfo);

			Factory.SuspendValidation();
			orgTaxRate.OTR_StartDate = ZDate.Today.AddDays(2);
			Factory.ResumeValidation();

			orgTaxRate.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code;
			AssertHasError(orgTaxRate.OTR_StartDateInfo, "Start Date cannot be a later date than the End Date.");
		}

		public void TestOTR_StartDate()
		{
			OrgTaxRate.OTR_StartDate = new ZDate(1900, 1, 1);
			AssertHasError(OrgTaxRate.OTR_StartDateInfo, "The date '01-Jan-1900' is more than 10 years old and thus is not valid.");

			OrgTaxRate.OTR_StartDate = ZDate.Today;
			AssertNoErrors("Should not have any error", OrgTaxRate.OTR_StartDateInfo);

			OrgTaxRate.OTR_EndDate = ZDate.Today;
			AssertNoErrors("Start Date and End Date can be equals.", OrgTaxRate.OTR_StartDateInfo);

			OrgTaxRate.OTR_EndDate = ZDate.Today.AddDays(-1);
			AssertHasError(OrgTaxRate.OTR_StartDateInfo, "Start Date cannot be a later date than the End Date.");
		}

		public void TestValidationInOTR_StartDateSetter()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData, true);

			var orgTaxRate = AddOrgTaxRateItem(accOrgTaxConfiguration, ZDate.Today, ZDate.Today.AddDays(1), AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate.OTR_EndDateInfo);

			orgTaxRate.OTR_StartDate = ZDate.Today.AddDays(2);
			AssertHasError(orgTaxRate.OTR_EndDateInfo, "End Date cannot be an earlier date than the start date.");
		}

		public void TestOTR_StartDateIsMandatory()
		{
			OrgTaxRate.OTR_StartDate = ZDate.Empty;
			AssertHasError(OrgTaxRate.OTR_StartDateInfo, "Please enter a Start Date.");
		}

		public void TestOTR_EndDate()
		{
			OrgTaxRate.OTR_EndDate = new ZDate(9999, 12, 31);
			AssertHasError(OrgTaxRate.OTR_EndDateInfo, "The date '31-Dec-9999' is more than 5 years from now and thus is not valid.");

			OrgTaxRate.OTR_EndDate = ZDate.Today;
			AssertNoErrors("Should not have any error.", OrgTaxRate.OTR_EndDateInfo);

			OrgTaxRate.OTR_StartDate = ZDate.Today.AddDays(-1);
			OrgTaxRate.OTR_EndDate = ZDate.Today;
			AssertNoErrors("Should not have any error", OrgTaxRate.OTR_EndDateInfo);

			OrgTaxRate.OTR_StartDate = ZDate.Today;
			OrgTaxRate.OTR_EndDate = ZDate.Today;
			AssertNoErrors("Start Date and End Date can be equals.", OrgTaxRate.OTR_EndDateInfo);

			OrgTaxRate.OTR_StartDate = ZDate.Today;
			OrgTaxRate.OTR_EndDate = ZDate.Today.AddDays(-1);
			AssertHasError(OrgTaxRate.OTR_EndDateInfo, "End Date cannot be an earlier date than the start date.");
		}

		public void TestValidationInOTR_EndDateSetter()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			Factory.Save();

			var accOrgTaxConfiguration = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData, true);

			var orgTaxRate = AddOrgTaxRateItem(accOrgTaxConfiguration, ZDate.Today, ZDate.Today.AddDays(1), AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code);
			AssertNoErrors(orgTaxRate.OTR_StartDateInfo);

			orgTaxRate.OTR_EndDate = ZDate.Today.AddDays(-2);
			AssertHasError(orgTaxRate.OTR_StartDateInfo, "Start Date cannot be a later date than the End Date.");
		}

		public void TestOTR_OTR_EndDateIsMandatory()
		{
			OrgTaxRate.OTR_EndDate = ZDate.Empty;
			AssertHasError(OrgTaxRate.OTR_EndDateInfo, "Please enter an End Date.");
		}

		public void TestOTR_StartDateCannotBeGreaterThanEndDate()
		{
			OrgTaxRate.OTR_EndDate = ZDate.Today.AddDays(10);
			OrgTaxRate.OTR_StartDate = ZDate.Today.AddDays(12);
			AssertHasError(OrgTaxRate.OTR_StartDateInfo, "Start Date cannot be a later date than the End Date.");
		}

		public void TestOTR_EndDateCannotBeEarlierThanStartDate()
		{
			OrgTaxRate.OTR_StartDate = ZDate.Today;
			OrgTaxRate.OTR_EndDate = ZDate.Today.AddDays(-1);
			AssertHasError(OrgTaxRate.OTR_EndDateInfo, "End Date cannot be an earlier date than the start date.");
		}

		public void TestOTR_RateNumeratorIsValid()
		{
			OrgTaxRate.OTR_RateNumerator = -1;
			AssertHasError(OrgTaxRate.OTR_RateNumeratorInfo, "Numerator cannot be a negative number, only zero value is allowed.");

			OrgTaxRate.OTR_RateNumerator = (ZInt)0;
			AssertNoErrors(OrgTaxRate.OTR_RateNumeratorInfo);

			OrgTaxRate.OTR_RateNumerator = (ZInt)999999999;
			AssertNoErrors(OrgTaxRate.OTR_RateNumeratorInfo);
		}

		public void TestOTR_RateDenominatorIsValid()
		{
			OrgTaxRate.OTR_RateDenominator = 0;
			AssertHasError(OrgTaxRate.OTR_RateDenominatorInfo, "Denominator cannot be equal or less than zero.");

			OrgTaxRate.OTR_RateDenominator = -1;
			AssertHasError(OrgTaxRate.OTR_RateDenominatorInfo, "Denominator cannot be equal or less than zero.");

			OrgTaxRate.OTR_RateDenominator = (ZInt)999999999;
			AssertNoErrors(OrgTaxRate.OTR_RateDenominatorInfo);
		}

		public void TestTaxRateValidationOnRateNumeratorSetter()
		{
			var expectedError = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";
			OrgTaxRate.OTR_RateNumerator = 0;
			AssertEquals("Precondition", 0m, OrgTaxRate.RateInfo.Value);

			OrgTaxRate.OTR_RateDenominator = 12;
			OrgTaxRate.OTR_RateNumerator = 100;
			AssertNoErrors(OrgTaxRate.RateInfo);

			OrgTaxRate.OTR_RateNumerator = 1200;
			AssertNoErrors(OrgTaxRate.RateInfo);

			OrgTaxRate.OTR_RateNumerator = 2000;
			AssertHasError(OrgTaxRate.RateInfo, expectedError);
		}

		public void TestTaxRateValidationOnRateDenominatorSetter()
		{
			var expectedError = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";
			OrgTaxRate.OTR_RateNumerator = 0;
			AssertEquals("Precondition", 0m, OrgTaxRate.RateInfo.Value);
			orgTaxRate.OTR_RateNumerator = 150;

			orgTaxRate.OTR_RateDenominator = 3;
			AssertNoErrors(orgTaxRate.RateInfo);

			orgTaxRate.OTR_RateDenominator = 2;
			AssertNoErrors(orgTaxRate.RateInfo);

			orgTaxRate.OTR_RateDenominator = 1;
			AssertHasError(orgTaxRate.RateInfo, expectedError);
		}

		public void TestCheckTaxRateIsValidOnCallingValidateAll()
		{
			var expectedErrorGreaterThan100 = "Rate must be less than 100%. Please correct the Numerator and Denominator entered.";
			var orgTaxRate = Factory.NewWithValidTestData<AccOrgTaxRate>();

			using (orgTaxRate.GetValidationSuspender())
			{
				orgTaxRate.OTR_RateNumerator = 2550;
				orgTaxRate.OTR_RateDenominator = 3;
			}
			AssertNoErrors("Precondition", orgTaxRate.RateInfo);
			orgTaxRate.RunPreSaveValidation();

			AssertHasErrors(expectedErrorGreaterThan100, orgTaxRate.RateInfo);
		}

		AccOrgTaxRate AddOrgTaxRateItem(AccOrgTaxConfiguration config, ZDate startDate, ZDate endDate, string source)
		{
			var item = config.TaxRates.AddNew();
			item.OTR_StartDate = startDate;
			item.OTR_EndDate = endDate;
			item.OTR_Source = source;

			return item;
		}

		AccOrgTaxRate OrgTaxRate => orgTaxRate ?? (orgTaxRate = Factory.New<AccOrgTaxRate>());
		AccOrgTaxRate orgTaxRate;

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
