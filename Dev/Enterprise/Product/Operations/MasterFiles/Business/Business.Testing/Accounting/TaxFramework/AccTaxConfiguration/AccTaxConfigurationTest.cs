using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxConfiguration))]
	sealed class AccTaxConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTaxAmountRounding()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertEquals(TaxAmountRoundingMethods.Standard.Code, taxConfiguration.ETC_TaxAmountRounding);

			taxConfiguration.ETC_TaxAmountRounding = TaxAmountRoundingMethods.RoundDownToMinorUnit.Code;
			Factory.Save();
			AssertEquals(TaxAmountRoundingMethods.RoundDownToMinorUnit.Code, taxConfiguration.ETC_TaxAmountRounding);
		}

		public void TestCanDelete_And_ReasonForNotAbleToDelete_WhenTaxConfigUsedInOrganisation()
		{
			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			var taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();

			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORGABC";
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = orgHeader.PK;
			var orgTaxConfiguration = Factory.NewWithValidTestData<AccOrgTaxConfiguration>();
			orgTaxConfiguration.OTC_OB = orgCompanyData.PK;
			orgTaxConfiguration.OTC_ETC = taxConfiguration1.PK;

			Factory.Save();

			AssertEquals(false, taxConfiguration1.CanDelete);
			AssertEquals("The Tax Configuration cannot be deleted because it is used in Tax Configurations for Organizations: 'ORGABC'.", taxConfiguration1.ReasonForNotAbleToDelete);
			AssertEquals(true, taxConfiguration2.CanDelete);
			AssertEquals(ZString.Empty, taxConfiguration2.ReasonForNotAbleToDelete);
		}

		public void TestCanDelete_WhenTaxConfigUsedInTaxOverrideGroup()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var taxConfigurationWithOverride = TestObjectCreator.CreateTaxConfiguration(company);
			var taxConfigurationWithoutOverride = TestObjectCreator.CreateTaxConfiguration(company);

			Factory.Save();

			var overrideGroup = TestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationWithOverride);

			Factory.Save();

			AssertEquals(false, taxConfigurationWithOverride.CanDelete);
			AssertEquals(true, taxConfigurationWithoutOverride.CanDelete);
		}

		public void TestReasonForNotAbleToDelete_WhenTaxConfigUsedInTaxOverrideGroup()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var taxConfigurationWithOverride = TestObjectCreator.CreateTaxConfiguration(company);
			var taxConfigurationWithoutOverride = TestObjectCreator.CreateTaxConfiguration(company);

			Factory.Save();

			var overrideGroup = TestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationWithOverride);
			overrideGroup.AX_Code = "TX3";
			var overrideGroup2 = TestObjectCreator.CreateTaxOverrideGroup(company, taxConfigurationWithOverride);
			overrideGroup2.AX_Code = "TX4";

			Factory.Save();

			AssertEquals(ZString.Empty, taxConfigurationWithoutOverride.ReasonForNotAbleToDelete);
			AssertEquals("The Tax Configuration cannot be deleted because it is configured for use against one or more Tax Configuration Override Groups in this record's login company. Please review and update the Tax Configuration Override Group records within that login company.", taxConfigurationWithOverride.ReasonForNotAbleToDelete);
		}

		public void TestGetTaxSystem()
		{
			var testHelper = new AccountingTestObjectCreator(Factory);
			var collection = new TaxSystemsConfigurationCollection();
			var taxConfiguration1 = testHelper.CreateTaxSystem("ABC");
			var taxConfiguration2 = testHelper.CreateTaxSystem("DEF");
			collection.AddRange(taxConfiguration1, taxConfiguration2);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			collection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_TaxSystemCode = taxConfiguration1.Code;
			AssertEquals(taxConfiguration1.Code, taxConfig.TaxSystem.Code);

			taxConfig.ETC_TaxSystemCode = taxConfiguration2.Code;
			AssertEquals(taxConfiguration2.Code, taxConfig.TaxSystem.Code);

			taxConfig.ETC_TaxSystemCode = "PQR";
			AssertNull(taxConfig.TaxSystem);
		}

		public void TestReadOnlyProperties_WhenSavedInDB()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TestObjectCreator.CreateTaxSystem("TS");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_RN_NKCountry = taxSystem.Country;
			taxConfig.ETC_TaxSystemCode = taxSystem.Code;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			Assert(!taxConfig.ETC_LedgerInfo.ReadOnly);
			Assert(!taxConfig.ETC_TaxSystemCodeInfo.ReadOnly);
			Assert(!taxConfig.ETC_TaxAuthorityCodeInfo.ReadOnly);
			Factory.Save();

			Assert(taxConfig.ETC_LedgerInfo.ReadOnly);
			Assert(taxConfig.ETC_TaxSystemCodeInfo.ReadOnly);
			Assert(taxConfig.ETC_TaxAuthorityCodeInfo.ReadOnly);
		}

		public void TestCancellationPolicyAndCreationTriggerDerfaultValues()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			AssertEquals("ETC_TaxRecordCreationTrigger", TaxRecordCreationTrigger.PostDate.Code, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_CancellationPolitcy", CancellationPolicyMethods.NoRestriction.Code, taxConfig.ETC_CancellationPolicy);
		}

		public void TestCreationTriggerAndRealisationMethodSettingBehaviour_ForSPRAP()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystemSPR = TestObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxSystemNonSPR = TestObjectCreator.CreateTaxSystem("NSPR", taxSuperType: TaxSuperTypeList.Perceptions.Code);
			taxSystemsConfigCollection.AddRange(new[] { taxSystemSPR, taxSystemNonSPR });
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_RN_NKCountry = CountryCodes.Australia;
			taxConfig.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			var glAccountsProviderMock = new Mock<IAccTaxConfigGLAccountsProvider>();
			taxConfig.GLAccountsProvider_ReplacementForTestOnly = glAccountsProviderMock.Object;

			taxConfig.ETC_TaxSystemCode = taxSystemSPR.Code;
			AssertNoErrors("Postcondition: ETC_TaxSystemCode", taxConfig.ETC_TaxSystemCodeInfo);
			AssertEquals("ETC_TaxRecordCreationTrigger", TaxRecordCreationTrigger.PostDate.Code, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_TaxRealisationMethod", ZString.Empty, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
			glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));

			glAccountsProviderMock.Invocations.Clear();
			taxConfig.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			AssertNoErrors("Postcondition: ETC_Ledger", taxConfig.ETC_LedgerInfo);
			var expectedCreationTriggerValue = TaxRecordCreationTrigger.PostDate.Code;
			var expectedRealisationMethodValue = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			AssertEquals("ETC_TaxRecordCreationTrigger", expectedCreationTriggerValue, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_TaxRealisationMethod", expectedRealisationMethodValue, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
			glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));

			glAccountsProviderMock.Invocations.Clear();
			taxConfig.ETC_TaxSystemCode = taxSystemNonSPR.Code;
			AssertNoErrors("Postcondition: ETC_TaxSystemCode", taxConfig.ETC_TaxSystemCodeInfo);
			AssertEquals("ETC_TaxRecordCreationTrigger", expectedCreationTriggerValue, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_TaxRealisationMethod", expectedRealisationMethodValue, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
			glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));

			taxConfig.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertNotEquals("Precondition: ETC_TaxRecordCreationTrigger", expectedCreationTriggerValue, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertNotEquals("Precondition: ETC_TaxRealisationMethod", expectedRealisationMethodValue, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Invocations.Clear();
			taxConfig.ETC_TaxSystemCode = taxSystemSPR.Code;
			AssertNoErrors("Postcondition: ETC_TaxSystemCode", taxConfig.ETC_TaxSystemCodeInfo);
			AssertEquals("ETC_TaxRecordCreationTrigger", expectedCreationTriggerValue, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_TaxRealisationMethod", expectedRealisationMethodValue, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
			glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));

			glAccountsProviderMock.Invocations.Clear();
			taxConfig.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			AssertNoErrors("Postcondition: ETC_Ledger", taxConfig.ETC_LedgerInfo);
			AssertEquals("ETC_TaxRecordCreationTrigger", expectedCreationTriggerValue, taxConfig.ETC_TaxRecordCreationTrigger);
			AssertEquals("ETC_TaxRealisationMethod", expectedRealisationMethodValue, taxConfig.ETC_TaxRealisationMethod);
			glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
			glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));
		}

		public void TestPropertyValueChangesRecalculateGLAccounts()
		{
			AssertPropertyValueChangesRecalculateGLAccounts(t => t.ETC_TaxSystemCode = "TT", t => t.ETC_TaxSystemCode = "TS");
			AssertPropertyValueChangesRecalculateGLAccounts(t => t.ETC_Ledger = LedgerTypes.CashBook, t => t.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code);
			AssertPropertyValueChangesRecalculateGLAccounts(t => t.ETC_TaxRealisationMethod = "RM", t => t.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code);

			void AssertPropertyValueChangesRecalculateGLAccounts(Action<AccTaxConfiguration> actionFailingValidation, Action<AccTaxConfiguration> actionPassingValidation)
			{
				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				var taxSystem = TestObjectCreator.CreateTaxSystem("TS");
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

				Guid regValue1 = Guid.NewGuid();
				Guid regValue2 = Guid.NewGuid();
				Guid regValue3 = Guid.NewGuid();
				Guid regValue4 = Guid.NewGuid();
				var taxConfig = Factory.New<AccTaxConfiguration>();
				var glAccountsProviderMock = new Mock<IAccTaxConfigGLAccountsProvider>();
				AccTaxConfiguration taxConfigParam = null;
				glAccountsProviderMock.Setup(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()))
					.Returns((GLAccountRegistryType.TaxTransactionExpenseAccount, GLAccountRegistryType.TaxTransactionNegativeRevenueAccount,
					GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount)).Callback<AccTaxConfiguration>(t => taxConfigParam = t);
				glAccountsProviderMock.Setup(o => o.GetRegistryValue(GLAccountRegistryType.TaxTransactionExpenseAccount)).Returns(regValue1);
				glAccountsProviderMock.Setup(o => o.GetRegistryValue(GLAccountRegistryType.TaxTransactionNegativeRevenueAccount)).Returns(regValue2);
				glAccountsProviderMock.Setup(o => o.GetRegistryValue(GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount)).Returns(regValue3);
				glAccountsProviderMock.Setup(o => o.GetRegistryValue(GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount)).Returns(regValue4);
				taxConfig.GLAccountsProvider_ReplacementForTestOnly = glAccountsProviderMock.Object;

				taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
				taxConfig.ETC_TaxSystemCode = taxSystem.Code;
				taxConfig.ETC_RN_NKCountry = taxSystem.Country;
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_LedgerControlAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxControlAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxExpenseAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxPendingControlAccount);

				glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Never);
				glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Never);
				actionFailingValidation(taxConfig);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_LedgerControlAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxControlAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxExpenseAccount);
				AssertEquals(ZGuid.Empty, taxConfig.ETC_AG_TaxPendingControlAccount);
				glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Never);
				glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Never);

				actionPassingValidation(taxConfig);
				AssertEquals(regValue1, taxConfig.ETC_AG_LedgerControlAccount);
				AssertEquals(regValue2, taxConfig.ETC_AG_TaxControlAccount);
				AssertEquals(regValue3, taxConfig.ETC_AG_TaxExpenseAccount);
				AssertEquals(regValue4, taxConfig.ETC_AG_TaxPendingControlAccount);
				glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
				glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));
				AssertEquals(taxConfig, taxConfigParam);

				actionPassingValidation(taxConfig);
				AssertEquals(regValue1, taxConfig.ETC_AG_LedgerControlAccount);
				AssertEquals(regValue2, taxConfig.ETC_AG_TaxControlAccount);
				AssertEquals(regValue3, taxConfig.ETC_AG_TaxExpenseAccount);
				AssertEquals(regValue4, taxConfig.ETC_AG_TaxPendingControlAccount);
				glAccountsProviderMock.Verify(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>()), Times.Exactly(1));
				glAccountsProviderMock.Verify(o => o.GetRegistryValue(It.IsAny<GLAccountRegistryType>()), Times.Exactly(4));
			}
		}

		public void TestReadOnlyProperties_BasedOnGLAccountRegistryType()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			var glAccountsProviderMock = new Mock<IAccTaxConfigGLAccountsProvider>();
			taxConfig.GLAccountsProvider_ReplacementForTestOnly = glAccountsProviderMock.Object;
			var otherTypeThanErrorOrNotApplicable = GLAccountRegistryType.APControlAccount;

			AssertReadOnly_BasedOnGLAccountRegistryType(null, taxConfig.ETC_AG_TaxControlAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType(null, taxConfig.ETC_AG_TaxExpenseAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType(null, taxConfig.ETC_AG_TaxPendingControlAccountInfo, true);

			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxControlAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxControlAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxControlAccountInfo, false);

			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxExpenseAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxExpenseAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxExpenseAccountInfo, false);

			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error),
				taxConfig.ETC_AG_TaxPendingControlAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable),
				taxConfig.ETC_AG_TaxPendingControlAccountInfo, true);
			AssertReadOnly_BasedOnGLAccountRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable),
				taxConfig.ETC_AG_TaxPendingControlAccountInfo, false);

			void AssertReadOnly_BasedOnGLAccountRegistryType((GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? glAccRegTypes, ZPropertyInfo info, bool expected)
			{
				if (glAccRegTypes.HasValue)
				{
					glAccountsProviderMock.Setup(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>())).Returns(glAccRegTypes.Value);
					taxConfig.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
					taxConfig.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
				}
				AssertEquals(expected, info.ReadOnly);
			}
		}

		public void TestReadOnlynessIsPreserved_ForLoaded()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = TestObjectCreator.CreateTaxSystem("TS");
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_RN_NKCountry = taxSystem.Country;
			taxConfig.ETC_TaxSystemCode = taxSystem.Code;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			Assert(taxConfig.ETC_AG_LedgerControlAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxControlAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxExpenseAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxPendingControlAccountInfo.ReadOnly);
			Factory.Save();

			var newFactory = NewFactory();
			taxConfig = newFactory.Load<AccTaxConfiguration>(taxConfig.PK);
			Assert(taxConfig.ETC_AG_LedgerControlAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxControlAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxExpenseAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxPendingControlAccountInfo.ReadOnly);

			taxConfig = newFactory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_RN_NKCountry = taxSystem.Country;
			taxConfig.ETC_TaxSystemCode = taxSystem.Code;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			taxConfig.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			Assert(taxConfig.ETC_AG_LedgerControlAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxControlAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxExpenseAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxPendingControlAccountInfo.ReadOnly);
			newFactory.Save();

			newFactory = NewFactory();
			taxConfig = newFactory.Load<AccTaxConfiguration>(taxConfig.PK);
			Assert(taxConfig.ETC_AG_LedgerControlAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxControlAccountInfo.ReadOnly);
			Assert(!taxConfig.ETC_AG_TaxExpenseAccountInfo.ReadOnly);
			Assert(taxConfig.ETC_AG_TaxPendingControlAccountInfo.ReadOnly);
		}

		public void TestETC_AG_LedgerControlAccountReadOnly()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			Assert(taxConfig.ETC_AG_LedgerControlAccountInfo.ReadOnly);
		}

		public void TestETC_CodeReadOnly()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			Assert(taxConfig.ETC_CodeInfo.ReadOnly);
		}

		public void TestETC_RN_NKCountryReadOnly()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			Assert(taxConfig.ETC_RN_NKCountryInfo.ReadOnly);
		}

		public void TestTaxConfigurationETCCodeAutogeneratedForCompany()
		{
			AssertETCCodeAutogenerated(Enterprise.Core.Constants.CountryCodes.Australia, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code, "AUT", "TSC");
			AssertETCCodeAutogenerated(Enterprise.Core.Constants.CountryCodes.Brazil, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code, "IMRJ", "ISS");
			AssertETCCodeAutogenerated(Enterprise.Core.Constants.CountryCodes.Argentina, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code, "CABA", "PIB");

			void AssertETCCodeAutogenerated(string countryCode, string ledgerCode, string authorityCode, string systemCode)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.TemporarilySetCountry(countryCode);

				AccTaxConfigurationCollection collection = new AccTaxConfigurationCollectionForCompanyOrBranch(company);

				AccTaxConfiguration taxConfiguration = collection.AddNew();
				AssertEquals($"ETC_Code should be equal to {countryCode} only.", $"{countryCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_Ledger = ledgerCode;
				AssertEquals($"ETC_Code should be equal to {countryCode}-{ledgerCode}", $"{countryCode}-{ledgerCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_TaxAuthorityCode = authorityCode;
				AssertEquals($"ETC_Code should be equal to {countryCode}-{authorityCode}-{ledgerCode}", $"{countryCode}-{authorityCode}-{ledgerCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_TaxSystemCode = systemCode;
				AssertEquals($"ETC_Code should be equal to {countryCode}-{authorityCode}-{systemCode}-{ledgerCode}", $"{countryCode}-{authorityCode}-{systemCode}-{ledgerCode}", taxConfiguration.ETC_Code);
			}
		}

		public void TestTaxConfigurationETCCodeAutogeneratedForBranch()
		{
			AssertETCCodeAutogeneratedForBranch(Enterprise.Core.Constants.CountryCodes.Australia, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code, "AUT", "TSC");
			AssertETCCodeAutogeneratedForBranch(Enterprise.Core.Constants.CountryCodes.Brazil, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code, "IMRJ", "ISS");
			AssertETCCodeAutogeneratedForBranch(Enterprise.Core.Constants.CountryCodes.Argentina, AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code, "CABA", "PIB");

			void AssertETCCodeAutogeneratedForBranch(string countryCode, string ledgerCode, string authorityCode, string systemCode)
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();

				branch.GB_RN_NKCountryCode = countryCode;
				string branchCode = branch.GB_Code;

				AccTaxConfigurationCollection collection = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);

				AccTaxConfiguration taxConfiguration = collection.AddNew();

				AssertEquals($"ETC_Code should be equal to AU-{branchCode}.", $"AU-{branchCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_Ledger = ledgerCode;
				AssertEquals($"ETC_Code should be equal to AU-{branchCode}-{ledgerCode}", $"AU-{branchCode}-{ledgerCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_TaxAuthorityCode = authorityCode;
				AssertEquals($"ETC_Code should be equal to AU-{branchCode}-{authorityCode}-{ledgerCode}", $"AU-{branchCode}-{authorityCode}-{ledgerCode}", taxConfiguration.ETC_Code);

				taxConfiguration.ETC_TaxSystemCode = systemCode;
				AssertEquals($"ETC_Code should be equal to AU-{branchCode}-{authorityCode}-{systemCode}-{ledgerCode}", $"AU-{branchCode}-{authorityCode}-{systemCode}-{ledgerCode}", taxConfiguration.ETC_Code);
			}
		}

		public void TestTaxConfigurationETCCodeAutogenerated_ByChangingBranchOrCompany()
		{
			AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			taxConfiguration.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfiguration.ETC_TaxAuthorityCode = "CABA";
			taxConfiguration.ETC_TaxSystemCode = "PIB";

			taxConfiguration.ETC_ParentId = ZGuid.Empty;
			taxConfiguration.ETC_ParentTableCode = ZString.Empty;
			AssertEquals("ETC_Code should be equal to AR-CABA-PIB-AR.", "AR-CABA-PIB-AR", taxConfiguration.ETC_Code);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			taxConfiguration.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			taxConfiguration.ETC_ParentId = branch1.PK;
			AssertEquals($"ETC_Code should be equal to AR-{branch1.GB_Code}-CABA-PIB-AR.", $"AR-{branch1.GB_Code}-CABA-PIB-AR", taxConfiguration.ETC_Code);

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			taxConfiguration.ETC_ParentId = branch2.PK;
			AssertEquals($"ETC_Code should be equal to AR-{branch2.GB_Code}-CABA-PIB-AR.", $"AR-{branch2.GB_Code}-CABA-PIB-AR", taxConfiguration.ETC_Code);

			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			AssertEquals("ETC_Code should be equal to AR-CABA-PIB-AR.", "AR-CABA-PIB-AR", taxConfiguration.ETC_Code);
		}

		public void TestTaxConfigurationETCCodeAutogenerated_ByChangingBranchCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Test Branch";
			branch.GB_Code = "TBR";

			AccTaxConfigurationCollection collection = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);
			AccTaxConfiguration taxConfiguration1 = collection.AddNew();
			taxConfiguration1.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfiguration1.ETC_TaxAuthorityCode = "AUT";
			taxConfiguration1.ETC_TaxSystemCode = "TSC";
			AssertEquals("Precondition: ETC_Code should be equal to AU-TBR-AUT-TSC-AR", "AU-TBR-AUT-TSC-AR", taxConfiguration1.ETC_Code);

			AccTaxConfiguration taxConfiguration2 = collection.AddNew();
			taxConfiguration2.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfiguration2.ETC_TaxAuthorityCode = "IMRJ";
			taxConfiguration2.ETC_TaxSystemCode = "PIB";
			AssertEquals("Precondition: ETC_Code should be equal to AU-TBR-IMRJ-PIB-AP", "AU-TBR-IMRJ-PIB-AP", taxConfiguration2.ETC_Code);

			branch.GB_Code = "RRR";

			AssertEquals("ETC_Code should be equal to AU-RRR-AUT-TSC-AR", "AU-RRR-AUT-TSC-AR", taxConfiguration1.ETC_Code);
			AssertEquals("ETC_Code should be equal to AU-RRR-IMRJ-PIB-AP", "AU-RRR-IMRJ-PIB-AP", taxConfiguration2.ETC_Code);
		}

		public void TestTaxConfigurationCollection_SetDefaultsForNewElement()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company1.Branches.AddNew();
			branch.GB_GC = company1.PK;
			AccTaxConfigurationCollection collection1 = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			AccTaxConfiguration taxConfiguration1 = collection1.AddNew();
			AssertEquals("ParentTableCode", GlbBranchSchema.Constants.Prefix, taxConfiguration1.ETC_ParentTableCode);
			AssertEquals("Country", Core.Constants.CountryCodes.Argentina, taxConfiguration1.ETC_RN_NKCountry);
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			AccTaxConfiguration taxConfiguration2 = collection1.AddNew();
			AssertEquals("ParentTableCode", GlbBranchSchema.Constants.Prefix, taxConfiguration2.ETC_ParentTableCode);
			AssertEquals("Country", Core.Constants.CountryCodes.Brazil, taxConfiguration2.ETC_RN_NKCountry);

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			AccTaxConfigurationCollection collection2 = new AccTaxConfigurationCollectionForCompanyOrBranch(company2);
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			AccTaxConfiguration taxConfiguration3 = collection2.AddNew();
			AssertEquals("ParentTableCode", GlbCompanySchema.Constants.Prefix, taxConfiguration3.ETC_ParentTableCode);
			AssertEquals("Country", Core.Constants.CountryCodes.India, taxConfiguration3.ETC_RN_NKCountry);
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;
			AccTaxConfiguration taxConfiguration4 = collection2.AddNew();
			AssertEquals("ParentTableCode", GlbCompanySchema.Constants.Prefix, taxConfiguration4.ETC_ParentTableCode);
			AssertEquals("Country", Core.Constants.CountryCodes.Uruguay, taxConfiguration4.ETC_RN_NKCountry);
		}

		public void TestConfigurationCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			var config1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			config1.ETC_ParentId = company.PK;
			AssertEquals("Company level configuration", company, config1.Company);

			var config2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config2.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			config2.ETC_ParentId = branch.PK;
			AssertEquals("Branch level configuration", company, config2.Company);

			var config3 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config3.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			AssertNull("Company level configuration but company not specified", config3.Company);

			var config4 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config4.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertNull("Branch level configuration but branch not specified", config4.Company);
		}

		public void TestETC_ThresholdAmount_OnlyPositiveValuesCanBeSaved()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var thresholdMethodCodes = new ETC_ThresholdMethods().GetAllCodes();
			thresholdMethodCodes.ForEach(item => SetThresholdValues(Factory.NewWithValidTestData<AccTaxConfiguration>(), item == ETC_ThresholdMethods.NoThreshold.Code ? 0.0 : 800.0, item));

			AssertNoExceptionThrown(() => Factory.Save());

			thresholdMethodCodes.ForEach(item => AssertUnableToSaveThresholdAmountAndMethod(taxConfig, -1800.0, item));

			void SetThresholdValues(AccTaxConfiguration taxConfig, ZDecimal amount, string thresholdMethod)
			{
				taxConfig.ETC_ThresholdAmount = amount;
				taxConfig.ETC_ThresholdMethod = thresholdMethod;
			}
		}

		public void TestETC_ThresholdAmountErrorWhenSaveWithZeroAmount()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_ThresholdMethod = ETC_ThresholdMethods.NoThreshold.Code;
			taxConfig.ETC_ThresholdAmount = 0.0;
			AssertNoExceptionThrown(() => Factory.Save());

			var thresholdMethodCodes = new ETC_ThresholdMethods().GetAllCodes();
			thresholdMethodCodes.Except(ETC_ThresholdMethods.NoThreshold.Code).ForEach(item => AssertUnableToSaveThresholdAmountAndMethod(taxConfig, 0, item));
		}

		public void TestETC_ThresholdAmountErrorWhenSaveInvalidThresholdMethod()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			AssertUnableToSaveThresholdAmountAndMethod(taxConfig, 1500.0, "DDD");
		}

		public void TestETC_ThresholdAmountDecimalPlacesUsesCompanyLocalCurrency()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			AccTaxConfigurationCollection collection = new AccTaxConfigurationCollectionForCompanyOrBranch(company);
			AccTaxConfiguration taxConfig = collection.AddNew();

			AssertNotEquals("Precondition: Log in Company is different from Tax-Configuration's Company", GlbCompany.CurrentCompany, taxConfig.Company);

			var decimalPlacesTester = new DecimalPlacesAttributeTester(taxConfig, company);
			decimalPlacesTester.CheckLocalCurrency(new List<string>() { nameof(taxConfig.ETC_ThresholdAmount) }, nameof(taxConfig.LocalCurrencyDecimalPlaces));
		}

		public void TestETC_ThresholdAmountDecimalPlacesUsesBranchLocalCurrency()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			AccTaxConfigurationCollection collection = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);
			AccTaxConfiguration taxConfig = collection.AddNew();

			AssertEquals("Precondition: ParentCompany will be null for a Branch's Tax-Configuration", null, taxConfig.ParentCompany);
			AssertNotEquals("Precondition: Log in Company is different from Branch's Company", GlbCompany.CurrentCompany, taxConfig.Company);

			var decimalPlacesTester = new DecimalPlacesAttributeTester(taxConfig, company);
			decimalPlacesTester.CheckLocalCurrency(new List<string>() { nameof(taxConfig.ETC_ThresholdAmount) }, nameof(taxConfig.LocalCurrencyDecimalPlaces));
		}

		public void TestETC_ThresholdAmountResetsToZeroWhenThresholdSetToNOT()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			var thresholdMethods = new ETC_ThresholdMethods().GetAllCodes();

			foreach (var thresholdMethod in thresholdMethods)
			{
				taxConfig.ETC_ThresholdMethod = thresholdMethod;
				taxConfig.ETC_ThresholdAmount = 800.00;

				AssertNotEquals("Precondition: ThresholdAmount is not zero", ZDecimal.Zero, taxConfig.ETC_ThresholdAmount);

				taxConfig.ETC_ThresholdMethod = ETC_ThresholdMethods.NoThreshold.Code;
				AssertEquals(ZDecimal.Zero, taxConfig.ETC_ThresholdAmount);
			}
		}

		public void TestETC_ThresholdAmountIsReadOnlyWhenThresholdIsNOT()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();

			Assert(taxConfig.ETC_ThresholdAmount_ReadOnly);

			var thresholdMethods = new ETC_ThresholdMethods().GetAllCodes();
			foreach (var thresholdMethod in thresholdMethods.Except(ETC_ThresholdMethods.NoThreshold.Code))
			{
				AssertETC_ThresholdAmountReadOnly(taxConfig, thresholdMethod, false);
			}
			AssertETC_ThresholdAmountReadOnly(taxConfig, ETC_ThresholdMethods.NoThreshold.Code, true);

			void AssertETC_ThresholdAmountReadOnly(AccTaxConfiguration taxConfig, string thresholdMethod, bool isReadOnly)
			{
				taxConfig.ETC_ThresholdMethod = thresholdMethod;
				AssertEquals(isReadOnly, taxConfig.ETC_ThresholdAmount_ReadOnly);
			}
		}

		void AssertUnableToSaveThresholdAmountAndMethod(AccTaxConfiguration taxConfig, ZDecimal amount, string thresholdMethod)
		{
			taxConfig.ETC_ThresholdMethod = thresholdMethod;
			taxConfig.ETC_ThresholdAmount = amount;
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
