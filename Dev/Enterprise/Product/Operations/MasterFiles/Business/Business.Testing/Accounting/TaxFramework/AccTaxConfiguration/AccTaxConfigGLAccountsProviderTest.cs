using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxConfigGLAccountsProviderTest : TestCaseWithFactory
	{
		public void TestGetRegistryValue()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			var provider = GetGLAccountsProvider(taxConfig);

			var mock = new Mock<IAccounting>();
			ObjectFactory.Substitute(mock.Object);

			var regValue = Guid.NewGuid();
			mock.SetupGet(o => o.APControlAccount).Returns(regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.APControlAccount));

			regValue = Guid.NewGuid();
			mock.SetupGet(o => o.ARControlAccount).Returns(regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.ARControlAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.TaxTransactionPrepaidAssetControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.TaxTransactionRemittanceLiabilityControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.TaxTransactionExpenseAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.TaxTransactionExpenseAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.TaxTransactionNegativeRevenueAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.TaxTransactionNegativeRevenueAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount));

			regValue = Guid.NewGuid();
			AccountingMasterFilesRegistry.Instance.PendingTaxTransactionRemittanceLiabilityControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			AssertEquals(regValue, provider.GetRegistryValue(GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount));

			AssertEquals(Guid.Empty, provider.GetRegistryValue(GLAccountRegistryType.NotApplicable));
			AssertEquals(Guid.Empty, provider.GetRegistryValue(GLAccountRegistryType.Error));
			AssertExceptionThrown<InvalidOperationException>(() => provider.GetRegistryValue((GLAccountRegistryType)10));
		}

		public void TestGetApplicableGLAccounts_NotApplicable()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			var notApplicable = (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));

			taxConfig.ETC_TaxSystemCode = ZString.Empty;
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
			taxConfig.ETC_TaxSystemCode = "TT";
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
			taxConfig.ETC_TaxSystemCode = "TS";

			taxConfig.ETC_Ledger = ZString.Empty;
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
			taxConfig.ETC_Ledger = LedgerTypes.IncompleteTransactions;
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;

			taxConfig.ETC_TaxRealisationMethod = ZString.Empty;
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
			taxConfig.ETC_TaxRealisationMethod = "RM";
			AssertEquals(notApplicable, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_NoTaxSystem()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			taxSystem.TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;

			var result = (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.Code = "TT";
			mockITaxFrameworkConfigurationHelper.Reset();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case1()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case2()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.RetentionInInvoice.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsReceivable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			taxSystem.TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case3()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.TaxTransactionExpenseAccount, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.SalesTax.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error);

			// Validation is suspended below just to check that GLAccountRegistryType.Error is returned by default for all 4 GL Accounts if none of the checks match for TaxSystem-SuperType/IncludeInInvoiceTotal, Ledger & RealisationMethod
			using (taxConfig.GetValidationSuspender())
			{
				taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			}
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case4()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.TurnoverTax.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsReceivable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.TaxTransactionNegativeRevenueAccount, GLAccountRegistryType.NotApplicable);

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error);

			// Validation is suspended below just to check that GLAccountRegistryType.Error is returned by default for all 4 GL Accounts if none of the checks match for TaxSystem-SuperType/IncludeInInvoiceTotal, Ledger & RealisationMethod
			using (taxConfig.GetValidationSuspender())
			{
				taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			}
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case5()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.SalesTax.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsReceivable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			taxSystem.TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_Case6()
		{
			var taxSystem = GetTaxSystem();
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			var taxConfig = GetTaxConfiguration(taxSystem);
			var provider = GetGLAccountsProvider(taxConfig);

			var result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			AssertNotEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.TaxSuperType = TaxSuperTypeList.RetentionInInvoice.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			taxSystem.TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDateOfMatchTransaction.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			taxSystem.TaxSuperType = TaxSuperTypeList.TurnoverTax.Code;
			taxSystem.IncludeInInvoceTotal = true;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = false;
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			result = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);

			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));

			taxSystem.IncludeInInvoceTotal = true;
			AssertEquals(result, provider.GetApplicableGLAccounts(taxConfig));
		}

		public void TestGetApplicableGLAccounts_APLedger_PostDateRealization_VATSuperType()
		{
			var expectedResult = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			var taxSystem = GetTaxSystem();
			taxSystem.TaxSuperType = TaxSuperTypeList.ValueAddedTax.Code;

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);

			var taxConfig = GetTaxConfiguration(taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;

			var provider = GetGLAccountsProvider(taxConfig);
			var result = provider.GetApplicableGLAccounts(taxConfig);
			AssertEquals(expectedResult, result);
		}

		public void TestGetApplicableGLAccounts_APLedger_MatchDateRealization_VATSuperType()
		{
			var expectedResult = (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);

			var taxSystem = GetTaxSystem();
			taxSystem.TaxSuperType = TaxSuperTypeList.ValueAddedTax.Code;

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);

			var taxConfig = GetTaxConfiguration(taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			var provider = GetGLAccountsProvider(taxConfig);
			var result = provider.GetApplicableGLAccounts(taxConfig);
			AssertEquals(expectedResult, result);
		}

		public void TestGetApplicableGLAccounts_ARLedger_PostDateRealization_VATSuperType()
		{
			var expectedResult = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);

			var taxSystem = GetTaxSystem();
			taxSystem.TaxSuperType = TaxSuperTypeList.ValueAddedTax.Code;

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);

			var taxConfig = GetTaxConfiguration(taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsReceivable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.PostDate.Code;

			var provider = GetGLAccountsProvider(taxConfig);
			var result = provider.GetApplicableGLAccounts(taxConfig);
			AssertEquals(expectedResult, result);
		}

		public void TestGetApplicableGLAccounts_ARLedger_MatchDateRealization_VATSuperType()
		{
			var expectedResult = (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);

			var taxSystem = GetTaxSystem();
			taxSystem.TaxSuperType = TaxSuperTypeList.ValueAddedTax.Code;

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			UpdateTaxSystemMockSetup(mockITaxFrameworkConfigurationHelper, taxSystem);

			var taxConfig = GetTaxConfiguration(taxSystem);
			taxConfig.ETC_Ledger = LedgerTypes.AccountsReceivable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;

			var provider = GetGLAccountsProvider(taxConfig);
			var result = provider.GetApplicableGLAccounts(taxConfig);
			AssertEquals(expectedResult, result);
		}

		void UpdateTaxSystemMockSetup(Mock<ITaxFrameworkConfigurationHelper> mockHelper, TaxSystemsConfiguration taxSystem)
		{
			mockHelper.WithGetTaxSystem(Factory, taxSystem).WithGetTaxSystems(taxSystem);
		}

		public TaxSystemsConfiguration GetTaxSystem()
		{
			return new AccountingTestObjectCreator(Factory).CreateTaxSystem("TS");
		}

		public AccTaxConfiguration GetTaxConfiguration(TaxSystemsConfiguration taxSystem)
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfig.ETC_RN_NKCountry = taxSystem.Country;
			taxConfig.ETC_TaxSystemCode = taxSystem.Code;
			taxConfig.ETC_Ledger = LedgerTypes.AccountsPayable;
			taxConfig.ETC_TaxRealisationMethod = TaxRealisationMethods.MatchDate.Code;
			return taxConfig;
		}

		public IAccTaxConfigGLAccountsProvider GetGLAccountsProvider(AccTaxConfiguration taxConfig)
		{
			var provider = taxConfig.GLAccountsProvider_ExposedForTestOnly;
			AssertEquals("Enterprise.MasterFiles.Business.AccTaxConfigGLAccountsProvider", provider.GetType().ToString());
			return provider;
		}
	}
}
