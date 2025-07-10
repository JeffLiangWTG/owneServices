using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfiguration))]
	class AccOrgTaxConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTaxConfiguratioMustBeInDatabase()
		{
			AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			AssertExceptionThrown(typeof(InvalidOperationException), "TaxConfiguration must be saved.", () => orgTaxConfiguration.OTC_ETC = taxConfiguration.PK);
		}

		public void TestTaxConfigurationLedgerNotChanged()
		{
			AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			Factory.Save();

			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();

			taxConfiguration.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			AssertExceptionThrown(typeof(InvalidOperationException), "TaxConfiguration Ledger is not correct.", () => orgTaxConfiguration.OTC_ETC = taxConfiguration.PK);
		}

		public void TestTaxConfigurationChangeLedger()
		{
			AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();

			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			AssertNoExceptionThrown("Ledger empty , setting empty ledger again - no excemption", () => orgTaxConfiguration.Ledger = string.Empty);
			AssertNoExceptionThrown("Ledger empty , setting not empty ledger - no excemption", () => orgTaxConfiguration.Ledger = taxConfiguration.ETC_Ledger);
			AssertExceptionThrown("Setted ledger, back to empty - excemption", typeof(InvalidOperationException), "Ledger must not be changed.", () => orgTaxConfiguration.Ledger = string.Empty);
			AssertExceptionThrown("Setted ledger, change ledger - excemption", typeof(InvalidOperationException), "Ledger must not be changed.", () => orgTaxConfiguration.Ledger = "XX");
			orgTaxConfiguration.OTC_ETC = taxConfiguration.PK;
			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgTaxConfiguration.OTC_OB = companyData.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var orgConfig = newFactory.Load<AccOrgTaxConfiguration>(orgTaxConfiguration.PK);
			AssertExceptionThrown("Setted ledger, change ledger - excemption", typeof(InvalidOperationException), "Ledger must not be changed.", () => orgConfig.Ledger = "XX");
		}

		public void TestCorrectLedgerInitializationAfterLoad()
		{
			AccTaxConfiguration taxConfigurationAR = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAR.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			AccTaxConfiguration taxConfigurationAP = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAP.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			Factory.Save();

			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			AccOrgTaxConfiguration orgTaxConfigurationAR = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfigurationAR.Ledger = taxConfigurationAR.ETC_Ledger;
			orgTaxConfigurationAR.OTC_ETC = taxConfigurationAR.PK;
			orgTaxConfigurationAR.OTC_OB = companyData.PK;
			AccOrgTaxConfiguration orgTaxConfigurationAP = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfigurationAP.Ledger = taxConfigurationAP.ETC_Ledger;
			orgTaxConfigurationAP.OTC_ETC = taxConfigurationAP.PK;
			orgTaxConfigurationAP.OTC_OB = companyData.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var loadOrgConfigAR = newFactory.Load<AccOrgTaxConfiguration>(orgTaxConfigurationAR.PK);
			var loadOrgConfigAP = newFactory.Load<AccOrgTaxConfiguration>(orgTaxConfigurationAP.PK);

			AssertEquals(loadOrgConfigAR.Ledger, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code);
			AssertEquals(loadOrgConfigAP.Ledger, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
		}

		public void TestShowTaxConfigurationDescription()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Description = "Tax Description";
			Factory.Save();

			var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = taxConfiguration.ETC_Ledger;

			orgTaxConfiguration.OTC_ETC = taxConfiguration.PK;
			AssertEquals(taxConfiguration.ETC_Description, orgTaxConfiguration.OTC_ETC_Description);

			orgTaxConfiguration.OTC_ETC = ZGuid.Empty;
			AssertEquals(ZString.Empty, orgTaxConfiguration.OTC_ETC_Description);
		}

		public void TestTaxRatesReadOnly_TaxRateSource()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			CombineAssertions(() =>
			{
				ZString ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
				AssertTestTaxRatesReadOnly_TaxRateSource("CW1", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW2", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW3", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxID.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW4", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxGroupOnly.Code, true);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW5", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxIDOnly.Code, true);

				ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
				AssertTestTaxRatesReadOnly_TaxRateSource("CW6", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW7", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW8", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxID.Code, false);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW9", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxGroupOnly.Code, true);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW10", ledger, AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxIDOnly.Code, true);
			});

			void AssertTestTaxRatesReadOnly_TaxRateSource(ZString code, ZString ledger, ZString taxRateSourceCode, ZBool readOnly)
			{
				var taxSystemConfig = new TaxSystemsConfiguration();
				taxSystemConfig.Code = code;
				taxSystemConfig.Name = "TAX TEST";
				taxSystemConfig.Country = Core.Constants.CountryCodes.Australia;
				taxSystemConfig.TaxRateSource = taxRateSourceCode;
				taxSystemConfig.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
				taxSystemConfig.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
				taxSystemConfig.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
				taxSystemConfig.IncludeInInvoceTotal = true;
				taxSystemConfig.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
				taxSystemConfig.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
				taxSystemConfig.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
				taxSystemConfig.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;

				mockITaxFrameworkConfigurationHelper.Reset();
				mockITaxFrameworkConfigurationHelper.WithGetTaxSystem(Factory, taxSystemConfig).WithGetTaxSystems(taxSystemConfig);

				var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig.ETC_Ledger = ledger;
				taxConfig.ETC_IsActive = true;
				taxConfig.ETC_RN_NKCountry = taxSystemConfig.Country;
				taxConfig.ETC_TaxSystemCode = taxSystemConfig.Code;

				Factory.Save();

				var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
				orgTaxConfiguration.Ledger = taxConfig.ETC_Ledger;
				orgTaxConfiguration.OTC_OB = companyData.PK;
				orgTaxConfiguration.OTC_ETC = taxConfig.PK;
				AssertEquals("AccOrgTaxConfiguration.TaxRates.ReadOnly for set OTC_ETC", readOnly, orgTaxConfiguration.TaxRates.ReadOnly);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				mockITaxFrameworkConfigurationHelper.WithGetTaxSystem(newFactory, taxSystemConfig).WithGetTaxSystems(taxSystemConfig);
				var orgTaxConfiguration_reload = newFactory.Load<AccOrgTaxConfiguration>(orgTaxConfiguration.PK);
				AssertEquals("AccOrgTaxConfiguration.TaxRates.ReadOnly for OnLoaded", readOnly, orgTaxConfiguration_reload.TaxRates.ReadOnly);
			}
		}

		public void TestTaxRatesReadOnly_OrgTaxConfig_ETC_OTC_IsEmptyOrHasError()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			CombineAssertions(() =>
			{
				AssertTestTaxRatesReadOnly_TaxRateSource("CW1", AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code);
				AssertTestTaxRatesReadOnly_TaxRateSource("CW2", AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code);
			});

			void AssertTestTaxRatesReadOnly_TaxRateSource(ZString code, ZString ledger)
			{
				var taxSystemConfig = new TaxSystemsConfiguration();
				taxSystemConfig.Code = code;
				taxSystemConfig.Name = "TAX TEST";
				taxSystemConfig.Country = Core.Constants.CountryCodes.Australia;
				taxSystemConfig.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;
				taxSystemConfig.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
				taxSystemConfig.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
				taxSystemConfig.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
				taxSystemConfig.IncludeInInvoceTotal = true;
				taxSystemConfig.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
				taxSystemConfig.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
				taxSystemConfig.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
				taxSystemConfig.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;

				mockITaxFrameworkConfigurationHelper.Reset();
				mockITaxFrameworkConfigurationHelper.WithGetTaxSystem(Factory, taxSystemConfig).WithGetTaxSystems(taxSystemConfig);

				var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig.ETC_Ledger = ledger;
				taxConfig.ETC_IsActive = true;
				taxConfig.ETC_RN_NKCountry = taxSystemConfig.Country;
				taxConfig.ETC_TaxSystemCode = taxSystemConfig.Code;

				Factory.Save();

				var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
				orgTaxConfiguration.Ledger = taxConfig.ETC_Ledger;
				orgTaxConfiguration.OTC_OB = companyData.PK;

				orgTaxConfiguration.OTC_ETC = ZGuid.Empty;
				AssertEquals("AccOrgTaxConfiguration.TaxRates must be read only when AccOrgTaxConfiguration.OTC_ETC is empty", true, orgTaxConfiguration.TaxRates.ReadOnly);

				orgTaxConfiguration.OTC_ETC = ZGuid.Invalid;
				AssertEquals("AccOrgTaxConfiguration.TaxRates must be read only when AccOrgTaxConfiguration.OTC_ETC has errors", true, orgTaxConfiguration.TaxRates.ReadOnly);

				orgTaxConfiguration.OTC_ETC = taxConfig.PK;
				AssertEquals("AccOrgTaxConfiguration.TaxRates must not be read only", false, orgTaxConfiguration.TaxRates.ReadOnly);
			}
		}

		public void TestTaxRatesReadOnly_SecurityCheckpoint()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var arSecurity = Env.Security.OrgReceivablesModifyTaxConfigurationRates;
			var apSecurity = Env.Security.OrgPayablesModifyTaxConfigurationRates;
			var arLedger = TaxConfigurationLedgers.AccountsReceivable.Code;
			var apLedger = TaxConfigurationLedgers.AccountsPayable.Code;
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			CombineAssertions(() =>
			{
				AssertTestTaxRatesReadOnly_Checkpoint("CW1", arLedger, arSecurity, false);
				AssertTestTaxRatesReadOnly_Checkpoint("CW2", arLedger, arSecurity, true);
				AssertTestTaxRatesReadOnly_Checkpoint("CW3", apLedger, apSecurity, false);
				AssertTestTaxRatesReadOnly_Checkpoint("CW4", apLedger, apSecurity, true);
			});

			void AssertTestTaxRatesReadOnly_Checkpoint(ZString code, ZString ledger, SecurityCheckpoint checkpoint, ZBool allowed)
			{
				var taxSystem = new AccountingTestObjectCreator(Factory).CreateTaxSystem(code);
				mockITaxFrameworkConfigurationHelper.Reset();
				mockITaxFrameworkConfigurationHelper.WithGetTaxSystem(Factory, taxSystem).WithGetTaxSystems(taxSystem);

				var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig.ETC_Ledger = ledger;
				taxConfig.ETC_IsActive = true;
				taxConfig.ETC_RN_NKCountry = taxSystem.Country;
				taxConfig.ETC_TaxSystemCode = taxSystem.Code;

				Factory.Save();

				var origIsAllowed = checkpoint.IsAllowed;
				// set checkpoint here so it is visible in OTC_ETC setter below
				checkpoint.IsAllowed = allowed;

				var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
				orgTaxConfiguration.Ledger = taxConfig.ETC_Ledger;
				orgTaxConfiguration.OTC_OB = companyData.PK;
				orgTaxConfiguration.OTC_ETC = taxConfig.PK; // setter calls SetTaxRateReadOnly

				try
				{
					AssertEquals("AccOrgTaxConfiguration.TaxRates.ReadOnly with SecurityCheckpoint", allowed, !orgTaxConfiguration.TaxRates.ReadOnly);
				}
				finally
				{
					checkpoint.IsAllowed = origIsAllowed;
				}
			}
		}

		public virtual void TestGetParentCompany()
		{
			var taxSystem = TestObjectCreator.CreateTaxSystem("TTS");
			var taxAuthority = TestObjectCreator.CreateTaxAuthority("TTA");
			var taxConfig = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, LedgerTypes.AccountsPayable);
			Factory.Save();

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = TestObjectCreator.NonCurrentCompany.PK;
			var orgTaxConfig = TestObjectCreator.CreateOrgTaxConfiguration(taxConfig, companyData);

			AssertEquals("Parent company should be specified by tax config company data", TestObjectCreator.NonCurrentCompany.PK, orgTaxConfig.GetParentCompany().PK);

			orgTaxConfig.OTC_OB = ZGuid.Empty;
			AssertEquals(null, orgTaxConfig.CompanyData);
			AssertEquals("Parent company is null because there is no company data", null, orgTaxConfig.GetParentCompany());
		}

		public void TestCopyEditableColumns()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			Factory.Save();

			var sourceOrgTaxConfig = TestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration);
			sourceOrgTaxConfig.OTC_IsActive = false;
			sourceOrgTaxConfig.OTC_IsThresholdUsed = true;
			sourceOrgTaxConfig.OTC_RecoverTax = true;

			AccOrgTaxConfiguration targetOrgTaxConfig = TestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration);
			targetOrgTaxConfig.OTC_IsActive = true;
			targetOrgTaxConfig.OTC_IsThresholdUsed = false;
			targetOrgTaxConfig.OTC_RecoverTax = false;

			targetOrgTaxConfig.CopyEditableColumns(sourceOrgTaxConfig);

			AssertEquals("Value copied", false, targetOrgTaxConfig.OTC_IsActive);
			AssertEquals(true, targetOrgTaxConfig.OTC_IsThresholdUsed);
			AssertEquals(true, targetOrgTaxConfig.OTC_RecoverTax);

			AssertEquals("Source data unchange", false, sourceOrgTaxConfig.OTC_IsActive);
			AssertEquals(true, sourceOrgTaxConfig.OTC_IsThresholdUsed);
			AssertEquals(true, sourceOrgTaxConfig.OTC_RecoverTax);
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
