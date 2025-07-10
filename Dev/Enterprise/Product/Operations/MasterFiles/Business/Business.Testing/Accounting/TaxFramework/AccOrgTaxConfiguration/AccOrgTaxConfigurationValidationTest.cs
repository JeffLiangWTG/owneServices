using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccOrgTaxConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsThresholdUsed()
		{
			var taxConfigNOT = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			var taxConfigT10 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			taxConfigT10.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			taxConfigT10.ETC_ThresholdAmount = 10.0000M;

			Factory.Save();

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var orgTaxConfig = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigNOT, companyData, true);

			orgTaxConfig.OTC_IsThresholdUsed = true;
			const string expectedError = "Threshold will not be activated for Tax Configurations with threshold method 'NOT'.";
			AssertHasWarning(orgTaxConfig.OTC_IsThresholdUsedInfo, expectedError);

			orgTaxConfig.OTC_ETC = taxConfigT10.PK;
			AssertNoWarnings(orgTaxConfig.OTC_IsThresholdUsedInfo);

			orgTaxConfig.OTC_ETC = taxConfigNOT.PK;
			AssertHasWarning(orgTaxConfig.OTC_IsThresholdUsedInfo, expectedError);

			using (orgTaxConfig.GetValidationSuspender())
			{
				orgTaxConfig.OTC_IsActive = false;
			}
			orgTaxConfig.RunPreSaveValidation();
			AssertNoWarnings(orgTaxConfig.OTC_IsThresholdUsedInfo);
		}

		public void TestOTC_RecoverTax()
		{
			var taxConfigNOR = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			var taxConfigREC = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			taxConfigREC.ETC_RecoveryMethod = TaxRecoveryMethods.RecoverTaxExpense.Code;

			Factory.Save();

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var orgTaxConfig = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigNOR, companyData, true);

			orgTaxConfig.OTC_RecoverTax = true;
			const string expectedError = "Tax Recovery will not be activated for Tax Configurations with recovery method 'NOR'.";
			AssertHasWarning(orgTaxConfig.OTC_RecoverTaxInfo, expectedError);

			orgTaxConfig.OTC_ETC = taxConfigREC.PK;
			AssertNoWarnings(orgTaxConfig.OTC_RecoverTaxInfo);

			orgTaxConfig.OTC_ETC = taxConfigNOR.PK;
			AssertHasWarning(orgTaxConfig.OTC_RecoverTaxInfo, expectedError);

			orgTaxConfig.OTC_IsActive = false;
			AssertNoWarnings(orgTaxConfig.OTC_RecoverTaxInfo);
		}

		public void TestValidationWillBeExecutedIfIsInDatabaseAndOTC_ETCHasChanges()
		{
			var taxConfig = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			var taxConfig2 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			Factory.Save();

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var orgTaxConfig = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData, true);

			Assert("Precondition: IsInDatabase", !orgTaxConfig.IsInDatabase);
			Factory.Save();

			Assert("Precondition: OTC_ETC HasChanges", !orgTaxConfig.OTC_ETCInfo.HasChanges);
			orgTaxConfig.OTC_ETC = taxConfig2.PK;

			AssertHasError(orgTaxConfig.OTC_ETCInfo, DefaultDbErrorExpectedMessage);
		}

		public void TestValidationOTC_ETCDisplayChangedErrorDetailButNotSavedInDb()
		{
			var taxConfig = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			var taxConfig2 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany);
			Factory.Save();

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var orgTaxConfig = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData, true);
			AssertNoErrors(orgTaxConfig.OTC_ETCInfo);

			AssertNoNotifications(DefaultDbErrorExpectedMessage, orgTaxConfig.OTC_ETCInfo);

			var orgTaxRate = orgTaxConfig.TaxRates.AddNew();
			orgTaxRate.OTR_Source = RateSourceMethods.Monthly.Code;

			orgTaxConfig.OTC_ETC = taxConfig2.PK;
			AssertHasError(orgTaxConfig.OTC_ETCInfo, string.Format("Value '{0}' can't be changed when Tax Rates are entered.", taxConfig.ETC_Code));

			orgTaxConfig.OTC_ETC = taxConfig.PK;
			Factory.Save();
			AssertNoErrors(orgTaxConfig.OTC_ETCInfo);

			orgTaxConfig.OTC_ETC = taxConfig2.PK;
			AssertHasError(orgTaxConfig.OTC_ETCInfo, DefaultDbErrorExpectedMessage);
		}

		public void TestETC_OTCNotChanged()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();

			CombineAssertions(() =>
			{
				AssertTestETC_OTCNotChanged(TaxConfigurationLedgers.AccountsReceivable.Code);
				AssertTestETC_OTCNotChanged(TaxConfigurationLedgers.AccountsPayable.Code);
			});

			void AssertTestETC_OTCNotChanged(ZString ledger)
			{
				var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);
				var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);

				Factory.Save();

				var accOrgTaxConfiguration = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData, true);
				AssertNoErrors(accOrgTaxConfiguration.OTC_ETCInfo);

				var orgTaxRate = accOrgTaxConfiguration.TaxRates.AddNew();
				orgTaxRate.OTR_Source = RateSourceMethods.Quarterly.Code;

				accOrgTaxConfiguration.OTC_ETC = taxConfiguration2.PK;
				AssertHasError(accOrgTaxConfiguration.OTC_ETCInfo, string.Format("Value '{0}' can't be changed when Tax Rates are entered.", taxConfiguration.ETC_Code));
				accOrgTaxConfiguration.OTC_ETC = taxConfiguration.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var accOrgTaxConfiguration_reload = newFactory.Load<AccOrgTaxConfiguration>(accOrgTaxConfiguration.PK);
				AssertNoErrors(accOrgTaxConfiguration_reload.OTC_ETCInfo);

				accOrgTaxConfiguration_reload.OTC_ETC = taxConfiguration2.PK;
				AssertHasError(accOrgTaxConfiguration_reload.OTC_ETCInfo, string.Format("Value '{0}' can't be changed when Tax Rates are entered.", taxConfiguration.ETC_Code));
			}
		}

		string DefaultDbErrorExpectedMessage => "This value cannot be changed. Changing a Tax Code already saved against an organization is not permitted.";

		protected AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
