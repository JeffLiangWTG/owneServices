using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void Test_ETC_TaxAmountRounding_BasicValidation()
		{
			AssertNoErrors(TaxConfiguration.ETC_TaxAmountRoundingInfo);

			TaxConfiguration.ETC_TaxAmountRounding = "";
			AssertHasError(TaxConfiguration.ETC_TaxAmountRoundingInfo, "Please enter a Tax Amount Rounding.");

			TaxConfiguration.ETC_TaxAmountRounding = "DDD";
			AssertHasError(TaxConfiguration.ETC_TaxAmountRoundingInfo, "Enter a valid Tax Amount Rounding.");

			foreach (CodeDescriptionPair taxAmountRounding in new TaxAmountRoundingMethods())
			{
				TaxConfiguration.ETC_TaxAmountRounding = taxAmountRounding.Code;
				AssertNoErrors(TaxConfiguration.ETC_TaxAmountRoundingInfo);
			}
		}

		public void TestETC_RecoveryMethod_BasicValidation()
		{
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);

			TaxConfiguration.ETC_RecoveryMethod = "";
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, "Please enter a Tax Recovery Method.");

			TaxConfiguration.ETC_RecoveryMethod = "DDD";
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, "Enter a valid Tax Recovery Method.");

			TaxConfiguration.ETC_RecoveryMethod = TaxRecoveryMethods.NoRecovery.Code;
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);
		}

		public void TestETC_RecoveryMethod()
		{
			const string expectedError = "REC is only permitted when Ledger = AR and Super type = TRX and Include in Invoice = NO";

			var registryTaxSystem = new TaxSystemsConfigurationCollection();
			var taxSystemValid = TestObjectCreator.CreateTaxSystem("TAX1", includeInInvoiceTotal: false, taxSuperType: TaxSuperTypeList.TurnoverTax.Code);
			var taxSystemInvalid1 = TestObjectCreator.CreateTaxSystem("TAX2", includeInInvoiceTotal: true, taxSuperType: TaxSuperTypeList.TurnoverTax.Code);
			var taxSystemInvalid2 = TestObjectCreator.CreateTaxSystem("TAX3", includeInInvoiceTotal: false, taxSuperType: TaxSuperTypeList.SalesTax.Code);
			var taxSystemInvalid3 = TestObjectCreator.CreateTaxSystem("TAX4", includeInInvoiceTotal: true, taxSuperType: TaxSuperTypeList.SalesTax.Code);
			registryTaxSystem.AddRange(taxSystemValid, taxSystemInvalid1, taxSystemInvalid2, taxSystemInvalid3);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryTaxSystem);
			registryTaxSystem.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemValid.Code;
			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			TaxConfiguration.ETC_RecoveryMethod = TaxRecoveryMethods.RecoverTaxExpense.Code;
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, expectedError);

			TaxConfiguration.ETC_RecoveryMethod = TaxRecoveryMethods.NoRecovery.Code;
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			TaxConfiguration.ETC_RecoveryMethod = TaxRecoveryMethods.RecoverTaxExpense.Code;
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);

			TaxConfiguration.ETC_TaxSystemCode = ZString.Empty;
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, expectedError);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemInvalid1.Code;
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, expectedError);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemInvalid2.Code;
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, expectedError);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemInvalid3.Code;
			AssertHasError(TaxConfiguration.ETC_RecoveryMethodInfo, expectedError);

			TaxConfiguration.ETC_RecoveryMethod = TaxRecoveryMethods.NoRecovery.Code;
			AssertNoErrors(TaxConfiguration.ETC_RecoveryMethodInfo);
		}

		public void TestValidateGLAccounts_BasedOnGLRegistryType()
		{
			var invalidGLAccountPK = ZGuid.NewZGuid();
			AccGLHeader validGLAccount;
			SetupCompanyBranchGLAccounts();
			TaxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			TaxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			var glAccountsProviderMock = new Mock<IAccTaxConfigGLAccountsProvider>();
			TaxConfiguration.GLAccountsProvider_ReplacementForTestOnly = glAccountsProviderMock.Object;
			var otherTypeThanErrorOrNotApplicable = GLAccountRegistryType.APControlAccount;
			string errorMessage = "GL Account cannot be defined for this configuration.";

			AssertValidateGLAccounts_BasedOnGLRegistryType((GLAccountRegistryType.Error, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_LedgerControlAccount(), TaxConfiguration.ETC_AG_LedgerControlAccountInfo, errorMessage);
			AssertValidateGLAccounts_BasedOnGLRegistryType((GLAccountRegistryType.NotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_LedgerControlAccount(), TaxConfiguration.ETC_AG_LedgerControlAccountInfo);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_LedgerControlAccount(), TaxConfiguration.ETC_AG_LedgerControlAccountInfo, "Please enter a Ledger Control Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_LedgerControlAccount = invalidGLAccountPK; v.ValidateETC_AG_LedgerControlAccount(); }, TaxConfiguration.ETC_AG_LedgerControlAccountInfo, "Enter a valid Ledger Control Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_LedgerControlAccount = validGLAccount.PK; v.ValidateETC_AG_LedgerControlAccount(); }, TaxConfiguration.ETC_AG_LedgerControlAccountInfo);

			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxControlAccount(), TaxConfiguration.ETC_AG_TaxControlAccountInfo, errorMessage);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxControlAccount(), TaxConfiguration.ETC_AG_TaxControlAccountInfo);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxControlAccount(), TaxConfiguration.ETC_AG_TaxControlAccountInfo, "Please enter a Tax Realization Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxControlAccount = invalidGLAccountPK; v.ValidateETC_AG_TaxControlAccount(); }, TaxConfiguration.ETC_AG_TaxControlAccountInfo, "Enter a valid Tax Realization Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxControlAccount = validGLAccount.PK; v.ValidateETC_AG_TaxControlAccount(); }, TaxConfiguration.ETC_AG_TaxControlAccountInfo);

			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxExpenseAccount(), TaxConfiguration.ETC_AG_TaxExpenseAccountInfo, errorMessage);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxExpenseAccount(), TaxConfiguration.ETC_AG_TaxExpenseAccountInfo);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxExpenseAccount(), TaxConfiguration.ETC_AG_TaxExpenseAccountInfo, "Please enter a Tax Expense Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxExpenseAccount = invalidGLAccountPK; v.ValidateETC_AG_TaxExpenseAccount(); }, TaxConfiguration.ETC_AG_TaxExpenseAccountInfo, "Enter a valid Tax Expense Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxExpenseAccount = validGLAccount.PK; v.ValidateETC_AG_TaxExpenseAccount(); }, TaxConfiguration.ETC_AG_TaxExpenseAccountInfo);

			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.Error), v => v.ValidateETC_AG_TaxPendingControlAccount(), TaxConfiguration.ETC_AG_TaxPendingControlAccountInfo, errorMessage);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, GLAccountRegistryType.NotApplicable), v => v.ValidateETC_AG_TaxPendingControlAccount(), TaxConfiguration.ETC_AG_TaxPendingControlAccountInfo);
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => v.ValidateETC_AG_TaxPendingControlAccount(), TaxConfiguration.ETC_AG_TaxPendingControlAccountInfo, "Please enter a Tax Pending Realization Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxPendingControlAccount = invalidGLAccountPK; v.ValidateETC_AG_TaxPendingControlAccount(); }, TaxConfiguration.ETC_AG_TaxPendingControlAccountInfo, "Enter a valid Tax Pending Realization Account.");
			AssertValidateGLAccounts_BasedOnGLRegistryType((otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable, otherTypeThanErrorOrNotApplicable), v => { TaxConfiguration.ETC_AG_TaxPendingControlAccount = validGLAccount.PK; v.ValidateETC_AG_TaxPendingControlAccount(); }, TaxConfiguration.ETC_AG_TaxPendingControlAccountInfo);

			void AssertValidateGLAccounts_BasedOnGLRegistryType((GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? glAccRegTypes, Action<AccTaxConfigurationValidation> validateAction, ZPropertyInfo info, string errorMessageToCheck = null)
			{
				glAccountsProviderMock.Setup(o => o.GetApplicableGLAccounts(It.IsAny<AccTaxConfiguration>())).Returns(glAccRegTypes.Value);
				TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
				TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
				validateAction(TaxConfiguration.Validation);

				if (errorMessageToCheck == null)
				{
					AssertNoErrors(info);
				}
				else
				{
					AssertHasError(info, errorMessageToCheck);
				}
			}

			void SetupCompanyBranchGLAccounts()
			{
				validGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
				validGLAccount.AG_AccountNum = "400";
				validGLAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				validGLAccount.AG_ControlAccount = false;
				validGLAccount.AG_IsGlobal = true;
				validGLAccount.AG_DisallowDirectPosting = true;

				Factory.Save();
			}
		}

		public void TestETC_Description()
		{
			TaxConfiguration.ETC_Description = ZString.Empty;
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_DescriptionInfo, "Please enter a Description.");

			TaxConfiguration.ETC_Description = "D.1";
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_DescriptionInfo);
		}

		public void TestETC_TaxAuthorityCode()
		{
			var code_BA = new CodeDescriptionPair("BA", "Buenos Aires");
			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper().WithGetTaxAuthorities(Core.Constants.CountryCodes.Argentina, null, new CodeDescriptionPairList { code_BA });

			var companyAR = Factory.NewWithValidTestData<GlbCompany>();
			companyAR.SetCountry(Core.Constants.CountryCodes.Argentina);
			var taxConfiguration = companyAR.AccTaxConfigurations.AddNew();

			taxConfiguration.ETC_TaxAuthorityCode = ZString.Empty;
			taxConfiguration.RunPreSaveValidation();
			AssertHasError(taxConfiguration.ETC_TaxAuthorityCodeInfo, "Please enter a Tax Authority.");

			taxConfiguration.ETC_TaxAuthorityCode = "BA";
			taxConfiguration.RunPreSaveValidation();
			AssertNoErrors(taxConfiguration.ETC_TaxAuthorityCodeInfo);

			taxConfiguration.ETC_TaxAuthorityCode = "TXAC.1";
			taxConfiguration.RunPreSaveValidation();
			AssertHasError(taxConfiguration.ETC_TaxAuthorityCodeInfo, "Enter a valid Tax Authority.");
		}

		public void TestETC_TaxSystemCode()
		{
			var registryTaxSystem = new TaxSystemsConfigurationCollection();
			registryTaxSystem.Add(NewTaxSystemsConfiguration("PIB", "IB Percepcione", Core.Constants.CountryCodes.Argentina, TaxSystemRegistrationLevels.Company.Code));
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, registryTaxSystem);
			registryTaxSystem.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var companyAR = Factory.NewWithValidTestData<GlbCompany>();
			companyAR.SetCountry(Core.Constants.CountryCodes.Argentina);
			var taxConfiguration = companyAR.AccTaxConfigurations.AddNew();

			taxConfiguration.ETC_TaxSystemCode = ZString.Empty;
			taxConfiguration.RunPreSaveValidation();
			AssertHasError(taxConfiguration.ETC_TaxSystemCodeInfo, "Please enter a Tax System.");

			taxConfiguration.ETC_TaxSystemCode = "PIB";
			taxConfiguration.RunPreSaveValidation();
			AssertNoErrors(taxConfiguration.ETC_TaxSystemCodeInfo);

			taxConfiguration.ETC_TaxSystemCode = "TXSC.1";
			taxConfiguration.RunPreSaveValidation();
			AssertHasError(taxConfiguration.ETC_TaxSystemCodeInfo, "Enter a valid Tax System.");

			TaxSystemsConfiguration NewTaxSystemsConfiguration(string code, string name, string country, string registrationLevel)
			{
				var item = new TaxSystemsConfiguration
				{
					Code = code,
					Name = name,
					Country = country,
					TaxAuthorityType = TaxAuthorityTypeList.State.Code,
					TaxSuperType = TaxSuperTypeList.Perceptions.Code,
					RegistrationLevel = registrationLevel,
					IncludeInInvoceTotal = true,
					AdjustmentSign = TaxCalculationAdjustmentSigns.Positive.Code,
					TaxBaseCalculationMethod = TaxBaseCalculationMethods.InvoiceLineAmount.Code,
					TaxAmountCalculationMethod = TaxAmountCalculationMethods.BaseTimesRate.Code,
					ThresholdRule = ThresholdRulesForTaxCalculation.NoThreshold.Code,
					TaxRateSource = TaxRateSources.OrganisationOnly.Code
				};

				return item;
			}
		}

		public void TestETC_Ledger()
		{
			TaxConfiguration.ETC_Ledger = ZString.Empty;
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_LedgerInfo, "Please enter a Ledger.");

			TaxConfiguration.ETC_Ledger = "AB";
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_LedgerInfo, "Enter a valid Ledger.");

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_LedgerInfo);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_LedgerInfo);
		}

		public void TestETC_Ledger_AR_WithNonSPRSuperType()
		{
			var taxSystemNonSPR = TestObjectCreator.CreateTaxSystem("NSPR", taxSuperType: TaxSuperTypeList.Perceptions.Code);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection() { taxSystemNonSPR };
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemNonSPR.Code;
			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_LedgerInfo);

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				TaxConfiguration.RunPreSaveValidation();
				AssertNoErrors(TaxConfiguration.ETC_LedgerInfo);
			}
		}

		public void TestETC_Ledger_AR_WithSPRSuperType()
		{
			var taxSystemSPR = TestObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection() { taxSystemSPR };
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemSPR.Code;
			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_LedgerInfo);

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				TaxConfiguration.RunPreSaveValidation();
				AssertHasError(TaxConfiguration.ETC_LedgerInfo, "CargoWise does not support Tax Configuration with Ledger AR for a SPR Tax Type. You can only create a Tax Configuration with Ledger AP for a SPR type tax.");
			}
		}

		#region ETC_RealisationMethod

		public void TestETC_RealisationMethod()
		{
			TaxConfiguration.ETC_TaxRealisationMethod = ZString.Empty;
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_TaxRealisationMethodInfo, "Please enter a Tax Realization.");

			TaxConfiguration.ETC_TaxRealisationMethod = "ABC";
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_TaxRealisationMethodInfo, "Enter a valid Tax Realization.");

			TaxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);

			TaxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = new AccountingTestObjectCreator(Factory).CreateTaxSystem("TS");
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.SalesTax.Code, TaxConfigurationLedgers.AccountsPayable.Code, false, false);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.TurnoverTax.Code, TaxConfigurationLedgers.AccountsReceivable.Code, false, false);

			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.SalesTax.Code, TaxConfigurationLedgers.AccountsPayable.Code, true);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.SalesTax.Code, TaxConfigurationLedgers.AccountsReceivable.Code, false);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.PostDate.Code, TaxSuperTypeList.SalesTax.Code, TaxConfigurationLedgers.AccountsPayable.Code, false);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.TurnoverTax.Code, TaxConfigurationLedgers.AccountsPayable.Code, false);

			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.TurnoverTax.Code, TaxConfigurationLedgers.AccountsReceivable.Code, true);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.TurnoverTax.Code, TaxConfigurationLedgers.AccountsPayable.Code, false);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.PostDate.Code, TaxSuperTypeList.TurnoverTax.Code, TaxConfigurationLedgers.AccountsReceivable.Code, false);
			AssertETC_TaxRealisationMethod(TaxRealisationMethods.MatchDate.Code, TaxSuperTypeList.SalesTax.Code, TaxConfigurationLedgers.AccountsReceivable.Code, false);

			void AssertETC_TaxRealisationMethod(ZString taxRealisationMethod, ZString superType, ZString ledger, bool isErrorExpected, bool taxSystemShouldBeFound = true)
			{
				taxSystem.TaxSuperType = superType;
				UpdateTaxSystemInRegistry();

				if (isErrorExpected)
				{
					TaxConfiguration.ETC_TaxRealisationMethod = taxRealisationMethod;

					TaxConfiguration.ETC_Ledger = ledger;
					TaxConfiguration.ETC_TaxSystemCode = ZString.Empty;
					AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);
					using (TaxConfiguration.GetValidationSuspender())
					{
						TaxConfiguration.ETC_TaxSystemCode = taxSystemShouldBeFound ? taxSystem.Code : ZString.Empty;
						AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);
					}
					TaxConfiguration.ETC_TaxSystemCode = taxSystemShouldBeFound ? taxSystem.Code : ZString.Empty;
					AssertMTDNotValidValueError();

					TaxConfiguration.ETC_Ledger = ZString.Empty;
					AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);
					using (TaxConfiguration.GetValidationSuspender())
					{
						TaxConfiguration.ETC_Ledger = ledger;
						AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);
					}
					TaxConfiguration.ETC_Ledger = ledger;
					AssertMTDNotValidValueError();

					TaxConfiguration.ETC_TaxRealisationMethod = ZString.Empty;
					AssertMTDNotValidValueError(false);
					TaxConfiguration.ETC_TaxRealisationMethod = taxRealisationMethod;
					AssertMTDNotValidValueError();
				}
				else
				{
					TaxConfiguration.ETC_TaxSystemCode = taxSystemShouldBeFound ? taxSystem.Code : ZString.Empty;
					TaxConfiguration.ETC_Ledger = ledger;
					TaxConfiguration.ETC_TaxRealisationMethod = taxRealisationMethod;
					AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);
				}
			}

			void UpdateTaxSystemInRegistry()
			{
				taxSystemsConfigCollection.DeleteAll();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			}

			void AssertMTDNotValidValueError(bool assertError = true)
			{
				if (assertError)
				{
					AssertHasError(TaxConfiguration.ETC_TaxRealisationMethodInfo, "'MDT' is not valid value for entered tax system and ledger combination.");
				}
				else
				{
					AssertNoError(TaxConfiguration.ETC_TaxRealisationMethodInfo, "'MDT' is not valid value for entered tax system and ledger combination.");
				}
			}
		}

		public void TestTestETC_RealisationMethod_ForSPRAP()
		{
			var expectedOnlyPTMAllowedError = "Only 'PTM' realization method is allowed for 'SPR' super type tax system and AP ledger.";
			var expectedPTMNotAllowedError = "'PTM' realization method is allowed only for 'SPR' super type tax system and AP ledger.";

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystemSPR = TestObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxSystemNonSPR = TestObjectCreator.CreateTaxSystem("NSPR", taxSuperType: TaxSuperTypeList.Perceptions.Code);
			taxSystemsConfigCollection.AddRange(new[] { taxSystemSPR, taxSystemNonSPR });
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemSPR.Code;
			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			TaxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);

			foreach (CodeDescriptionPair code in new TaxRealisationMethods())
			{
				if (code.Code == TaxRealisationMethods.PostDateOfMatchTransaction.Code)
				{
					continue;
				}

				TaxConfiguration.ETC_TaxRealisationMethod = code.Code;
				AssertHasError(TaxConfiguration.ETC_TaxRealisationMethodInfo, expectedOnlyPTMAllowedError);
			}

			TaxConfiguration.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			AssertHasError("Changing ledger triggers revalidation", TaxConfiguration.ETC_TaxRealisationMethodInfo, expectedPTMNotAllowedError);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			AssertNoErrors(TaxConfiguration.ETC_TaxRealisationMethodInfo);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemNonSPR.Code;
			AssertHasError("Changing tax system triggers revalidation", TaxConfiguration.ETC_TaxRealisationMethodInfo, expectedPTMNotAllowedError);
		}

		#endregion

		#region ETC_RecordCreationTrigger

		public void TestETC_RecordCreationTrigger()
		{
			TaxConfiguration.ETC_TaxRecordCreationTrigger = ZString.Empty;
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo, "Please enter a Tax Record Creation Trigger.");

			TaxConfiguration.ETC_TaxRecordCreationTrigger = "ABC";
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo, "Enter a valid Tax Record Creation Trigger.");

			TaxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo);

			TaxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo);
		}

		public void TestETC_RecordCreationTrigger_ForSPRAP()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystemSPR = TestObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxSystemNonSPR = TestObjectCreator.CreateTaxSystem("NSPR", taxSuperType: TaxSuperTypeList.Perceptions.Code);
			taxSystemsConfigCollection.AddRange(new[] { taxSystemSPR, taxSystemNonSPR });
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemSPR.Code;
			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			TaxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.PostDate.Code;
			AssertNoErrors(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo);

			TaxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			var expectedError = "Only 'PDT' creation trigger is allowed for 'SPR' super type tax system and AP ledger.";
			AssertHasError(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo, expectedError);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			AssertEquals("Postcondition: ETC_TaxRecordCreationTrigger", TaxRecordCreationTrigger.MatchDate.Code, TaxConfiguration.ETC_TaxRecordCreationTrigger);
			AssertNoErrors("Changing ledger triggers revalidation", TaxConfiguration.ETC_TaxRecordCreationTriggerInfo);

			TaxConfiguration.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			TaxConfiguration.ETC_TaxRecordCreationTrigger = TaxRecordCreationTrigger.MatchDate.Code;
			AssertEquals("Postcondition: ETC_TaxRecordCreationTrigger", TaxRecordCreationTrigger.MatchDate.Code, TaxConfiguration.ETC_TaxRecordCreationTrigger);
			AssertHasError(TaxConfiguration.ETC_TaxRecordCreationTriggerInfo, expectedError);

			TaxConfiguration.ETC_TaxSystemCode = taxSystemNonSPR.Code;
			AssertEquals("Postcondition: ETC_TaxRecordCreationTrigger", TaxRecordCreationTrigger.MatchDate.Code, TaxConfiguration.ETC_TaxRecordCreationTrigger);
			AssertNoErrors("Changing tax system triggers revalidation", TaxConfiguration.ETC_TaxRecordCreationTriggerInfo);
		}

		#endregion

		public void TestETC_CancellationPolicy()
		{
			TaxConfiguration.ETC_CancellationPolicy = ZString.Empty;
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_CancellationPolicyInfo, "Please enter a Cancellation Policy.");

			TaxConfiguration.ETC_CancellationPolicy = "ABC";
			TaxConfiguration.RunPreSaveValidation();
			AssertHasError(TaxConfiguration.ETC_CancellationPolicyInfo, "Enter a valid Cancellation Policy.");

			TaxConfiguration.ETC_CancellationPolicy = CancellationPolicyMethods.CalendarMonth.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_CancellationPolicyInfo);

			TaxConfiguration.ETC_CancellationPolicy = CancellationPolicyMethods.CalendarYear.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_CancellationPolicyInfo);

			TaxConfiguration.ETC_CancellationPolicy = CancellationPolicyMethods.NoRestriction.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_CancellationPolicyInfo);

			TaxConfiguration.ETC_CancellationPolicy = CancellationPolicyMethods.NotAllowed.Code;
			TaxConfiguration.RunPreSaveValidation();
			AssertNoErrors(TaxConfiguration.ETC_CancellationPolicyInfo);
		}

		public void TestETC_RN_NKCountry()
		{
			var expectedError = "Country/Region must be the same as Company country/region.";

			var emptyConfig = Factory.New<AccTaxConfiguration>();
			emptyConfig.ETC_RN_NKCountry = Core.Constants.CountryCodes.Brazil;
			AssertNoErrors(emptyConfig.ETC_RN_NKCountryInfo);

			emptyConfig.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(emptyConfig.ETC_RN_NKCountryInfo);
			emptyConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			AssertHasError(emptyConfig.ETC_RN_NKCountryInfo, expectedError);

			emptyConfig.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertNoErrors(emptyConfig.ETC_RN_NKCountryInfo);
			emptyConfig.ETC_ParentId = GlbBranch.CurrentBranch.PK;
			AssertHasError(emptyConfig.ETC_RN_NKCountryInfo, expectedError);

			emptyConfig.ETC_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			AssertNoErrors(emptyConfig.ETC_RN_NKCountryInfo);

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company1.Branches.AddNew();
			var collection1 = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);
			var taxConfiguration1 = collection1.AddNew();

			taxConfiguration1.RunPreSaveValidation();
			AssertNoErrors(taxConfiguration1.ETC_RN_NKCountryInfo);

			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			taxConfiguration1.RunPreSaveValidation();
			AssertHasError(taxConfiguration1.ETC_RN_NKCountryInfo, expectedError);

			var company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var collection2 = new AccTaxConfigurationCollectionForCompanyOrBranch(company2);
			var taxConfiguration2 = collection2.AddNew();

			taxConfiguration2.RunPreSaveValidation();
			AssertNoErrors(taxConfiguration2.ETC_RN_NKCountryInfo);

			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			taxConfiguration2.RunPreSaveValidation();
			AssertHasError(taxConfiguration2.ETC_RN_NKCountryInfo, expectedError);
		}

		public void TestZeroETC_ThresholdAmountIsNotAllowedForThresholdMethodOtherThanNOT()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			var thresholdMethods = new ETC_ThresholdMethods().GetAllCodes();

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			var mockITaxFrameworkThresholdMethodProvider = new Mock<ITaxFrameworkThresholdMethodProvider>();
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(true);
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(true);

			mockIAccountingCountryFactory.As<IInstanceProvider<ITaxFrameworkThresholdMethodProvider>>().Setup(x => x.Get()).Returns(mockITaxFrameworkThresholdMethodProvider.Object);
			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
				foreach (var thresholdMethod in thresholdMethods.Except(ETC_ThresholdMethods.NoThreshold.Code))
				{
					taxConfig.ETC_ThresholdMethod = thresholdMethod;
					AssertHasErrorContaining(taxConfig.ETC_ThresholdAmountInfo, @"The Threshold Amount of a Tax Configuration must be a number greater than Zero when the Threshold Method is");
				}
			}
		}

		public void TestETC_ThresholdAmountAllGRPTaxConfigurationsOfACompanyShouldHaveSameThresholdAmount()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(company);
			var taxConfig1 = collection.AddNew();
			var taxConfig2 = collection.AddNew();
			var ledgerTypes = new TaxConfigurationLedgers();
			foreach (var ledger in ledgerTypes.GetAllCodes())
			{
				taxConfig1.ETC_Ledger = ledger;
				taxConfig2.ETC_Ledger = ledger;

				AssertWhenGRPTaxConfigurationHaveDifferentThresholdAmount(taxConfig1, taxConfig2);
			}
		}

		public void TestETC_ThresholdAmountAllGRPTaxConfigurationsOfABranchShouldHaveSameThresholdAmount()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(branch);
			var taxConfig1 = collection.AddNew();
			var taxConfig2 = collection.AddNew();
			var ledgerTypes = new TaxConfigurationLedgers();
			foreach (var ledger in ledgerTypes.GetAllCodes())
			{
				taxConfig1.ETC_Ledger = ledger;
				taxConfig2.ETC_Ledger = ledger;
				AssertWhenGRPTaxConfigurationHaveDifferentThresholdAmount(taxConfig1, taxConfig2);
			}
		}

		public void TestETC_ThresholdAmountReValidatedWhenLedgerChanged()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(company);
			var taxConfig1 = collection.AddNew();
			var taxConfig2 = collection.AddNew();
			var ledgerTypes = new TaxConfigurationLedgers();

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = new AccountingTestObjectCreator(Factory).CreateTaxSystem("taxSystem", taxSuperType: TaxSuperTypeList.RetentionInInvoice.Code);
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxConfig1.ETC_TaxSystemCode = taxSystem.Code;
			taxConfig2.ETC_TaxSystemCode = taxSystem.Code;

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			var mockITaxFrameworkThresholdMethodProvider = new Mock<ITaxFrameworkThresholdMethodProvider>();
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(true);

			mockIAccountingCountryFactory.As<IInstanceProvider<ITaxFrameworkThresholdMethodProvider>>().Setup(x => x.Get()).Returns(mockITaxFrameworkThresholdMethodProvider.Object);

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				foreach (var ledger in ledgerTypes.GetAllCodes())
				{
					taxConfig1.ETC_Ledger = ledger;
					taxConfig2.ETC_Ledger = ledger;

					AssertWhenGRPTaxConfigurationHaveDifferentThresholdAmount(taxConfig1, taxConfig2);

					AssertHasError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");

					taxConfig2.ETC_Ledger = ledger == TaxConfigurationLedgers.AccountsPayable.Code ? TaxConfigurationLedgers.AccountsReceivable.Code : TaxConfigurationLedgers.AccountsPayable.Code;

					AssertNoError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");
				}
			}
		}

		void AssertWhenGRPTaxConfigurationHaveDifferentThresholdAmount(AccTaxConfiguration taxConfig1, AccTaxConfiguration taxConfig2)
		{
			taxConfig1.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig2.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig1.ETC_ThresholdAmount = 600.0;
			taxConfig2.ETC_ThresholdAmount = 600.0;

			AssertNoErrors(taxConfig2.ETC_ThresholdAmountInfo);

			taxConfig2.ETC_ThresholdAmount = 500.0;

			AssertHasError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");
		}

		public void TestETC_ThresholdMethodListValidation()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			var mockITaxFrameworkThresholdMethodProvider = new Mock<ITaxFrameworkThresholdMethodProvider>();
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(true);
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(true);

			mockIAccountingCountryFactory.As<IInstanceProvider<ITaxFrameworkThresholdMethodProvider>>().Setup(x => x.Get()).Returns(mockITaxFrameworkThresholdMethodProvider.Object);

			var thresholdMethods = new ETC_ThresholdMethods();
			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
				foreach (var thresholdMethod in thresholdMethods.GetAllCodes())
				{
					AssertETC_ThresholdMethodNoError(thresholdMethod);
				}

				taxConfig.ETC_ThresholdMethod = "ABC";

				AssertHasError(taxConfig.ETC_ThresholdMethodInfo, "Enter a valid Threshold Method.");
			}

			void AssertETC_ThresholdMethodNoError(string thresholdMethod)
			{
				taxConfig.ETC_ThresholdMethod = thresholdMethod;
				AssertNoErrors("Enter a valid Threshold Method.", taxConfig.ETC_ThresholdMethodInfo);
			}
		}

		public void TestETC_ThresholdMethodMandatoryValidation()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_ThresholdMethod = ZString.Empty;
			AssertHasError(taxConfig.ETC_ThresholdMethodInfo, "Please enter a Threshold Method.");
		}

		public void TestETC_ThresholdAmountRevalidatedWhenThresholdMethodChanged()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			taxConfig.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			var message = "The Threshold Amount of a Tax Configuration must be a number greater than Zero when the Threshold Method is 'TRN - Transaction Level Tax Amount'.\r\nPlease review and update the Threshold Method and Threshold Amount values before saving.";
			AssertHasError(taxConfig.ETC_ThresholdAmountInfo, message);

			taxConfig.ETC_ThresholdMethod = ETC_ThresholdMethods.NoThreshold.Code;
			AssertNoError(taxConfig.ETC_ThresholdAmountInfo, message);

			message = "The Threshold Amount of a Tax Configuration must be a number greater than Zero when the Threshold Method is 'TRB - Transaction Level Tax Base Amount'.\r\nPlease review and update the Threshold Method and Threshold Amount values before saving.";

			taxConfig.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			AssertHasError(taxConfig.ETC_ThresholdAmountInfo, message);
		}

		public void TestETC_ThresholdAmountUsingSameBranchLevelGRPTaxConfigurations()
		{
			var company = Factory.New<GlbCompany>();
			var branches = company.Branches;
			var branch1 = branches.AddNew();
			var branch2 = branches.AddNew();

			var taxConfig1 = CreateTaxConfigurationWithTaxSystem();
			var taxConfig2 = CreateTaxConfigurationWithTaxSystem();

			taxConfig1.ETC_ParentId = branch1.PK;
			taxConfig1.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			taxConfig2.ETC_ParentId = branch2.PK;
			taxConfig2.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;

			taxConfig1.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig1.ETC_ThresholdAmount = 400;
			taxConfig2.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig2.ETC_ThresholdAmount = 500;
			AssertNoError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");

			taxConfig2.ETC_ParentId = branch1.PK;
			taxConfig2.Validation.ValidateETC_ThresholdAmount();

			AssertHasError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");
		}

		public void TestETC_ThresholdAmountUsingCompanyBranchLevelGRPTaxConfigurations()
		{
			var company1 = Factory.New<GlbCompany>();
			var company2 = Factory.New<GlbCompany>();

			var taxConfig1 = CreateTaxConfigurationWithTaxSystem();
			var taxConfig2 = CreateTaxConfigurationWithTaxSystem();

			taxConfig1.ETC_ParentId = company1.PK;
			taxConfig1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfig2.ETC_ParentId = company2.PK;
			taxConfig2.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			taxConfig1.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig1.ETC_ThresholdAmount = 400;
			taxConfig2.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig2.ETC_ThresholdAmount = 500;
			AssertNoError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");

			taxConfig2.ETC_ParentId = company1.PK;
			taxConfig2.Validation.ValidateETC_ThresholdAmount();

			AssertHasError(taxConfig2.ETC_ThresholdAmountInfo, "All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving.");
		}

		AccTaxConfiguration CreateTaxConfigurationWithTaxSystem()
		{
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			var taxSystem = new AccountingTestObjectCreator(Factory).CreateTaxSystem("taxSystem", taxSuperType: TaxSuperTypeList.RetentionInInvoice.Code);
			taxSystemsConfigCollection.Add(taxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_TaxSystemCode = taxSystem.Code;
			return taxConfig;
		}

		AccTaxConfiguration TaxConfiguration => taxConfiguration ?? (taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>());
		AccTaxConfiguration taxConfiguration;

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
