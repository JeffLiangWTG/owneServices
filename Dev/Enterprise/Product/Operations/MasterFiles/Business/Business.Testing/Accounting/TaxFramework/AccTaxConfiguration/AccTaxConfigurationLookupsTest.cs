using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxAmountRoundingMethodsElementByElement()
		{
			var taxConfiguration = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "STD", "MDN" }, taxConfiguration.Lookups.TaxAmountRoundingMethods.GetAllCodes());
		}

		public void TestTaxRecoveryMethodsElementByElement()
		{
			var taxConfig = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "NOR", "REC" }, taxConfig.Lookups.RecoveryMethods.GetAllCodes());
		}

		public void Test_TaxRealisationMethodElementByElement()
		{
			AccTaxConfiguration taxConfig = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "PDT", "MDT", "PTM" }, taxConfig.Lookups.TaxRealisationMethods.GetAllCodes());
		}

		public void Test_LedgerElementByElement()
		{
			AccTaxConfiguration taxConfig = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "AP", "AR" }, taxConfig.Lookups.Ledger.GetAllCodes());
		}

		public void Test_TaxRecordCreationTriggerElementByElement()
		{
			AccTaxConfiguration taxConfig = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "PDT", "MDT" }, taxConfig.Lookups.TaxRecordCreationTrigger.GetAllCodes());
		}

		public void Test_CancellationPolicyMethodsElementByElement()
		{
			AccTaxConfiguration taxConfig = Factory.New<AccTaxConfiguration>();
			AssertArrayEqualsByElements(new[] { "NAL", "CMO", "CYR", "NRE" }, taxConfig.Lookups.CancellationPolicyMethods.GetAllCodes());
		}

		public void Test_TaxSystemsLookups()
		{
			var code_PIB = new CodeDescriptionPair("PIB", "IB Percepcione");
			var code_RIB = new CodeDescriptionPair("RIB", "IB Retencione");
			var code_ISS = new CodeDescriptionPair("ISS", "ISS");

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(Core.Constants.CountryCodes.Argentina, TaxSystemRegistrationLevels.Company.Code, new CodeDescriptionPairList { code_PIB, code_RIB });

			var companyAR = Factory.NewWithValidTestData<GlbCompany>();
			companyAR.SetCountry(Core.Constants.CountryCodes.Argentina);
			var taxConfigurationAR = companyAR.AccTaxConfigurations.AddNew();
			Assert("PIB and RIB Tax System should be in the collection.", taxConfigurationAR.Lookups.TaxSystems.ContainsOnly("PIB", "RIB"));

			var companyBR = Factory.NewWithValidTestData<GlbCompany>();
			companyBR.SetCountry(Core.Constants.CountryCodes.Brazil);
			var taxConfigurationBR = companyBR.AccTaxConfigurations.AddNew();
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(Core.Constants.CountryCodes.Brazil, TaxSystemRegistrationLevels.Company.Code, new CodeDescriptionPairList { });
			AssertEquals("Should be empty because not have Brazilian Company base Tax System.", 0, taxConfigurationBR.Lookups.TaxSystems.Count);

			var branchAU = companyBR.Branches.AddNew();
			branchAU.SetCountry(Core.Constants.CountryCodes.Australia);
			var taxConfigurationAU = branchAU.AccTaxConfigurations.AddNew();
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(Core.Constants.CountryCodes.Brazil, TaxSystemRegistrationLevels.Branch.Code, new CodeDescriptionPairList { code_ISS });
			Assert("Should be use Tax System from company country (not the branch).", taxConfigurationAU.Lookups.TaxSystems.ContainsOnly("ISS"));
		}

		public void Test_TaxAuthoritiesLookups()
		{
			var code_BA = new CodeDescriptionPair("BA", "Buenos Aires");
			var code_CABA = new CodeDescriptionPair("CABA", "Capital Federal");
			var code_RJ = new CodeDescriptionPair("RJ", "Rio de Janeiro");
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.WithGetTaxAuthorities(Core.Constants.CountryCodes.Argentina, null, new CodeDescriptionPairList { code_BA, code_CABA });

			var companyAR = Factory.NewWithValidTestData<GlbCompany>();
			companyAR.SetCountry(Core.Constants.CountryCodes.Argentina);
			var taxConfigurationAR = companyAR.AccTaxConfigurations.AddNew();
			Assert("BA and CABA Tax Authorities should be in the collection.", taxConfigurationAR.Lookups.TaxAuthorities.ContainsOnly("BA", "CABA"));

			var companyBR = Factory.NewWithValidTestData<GlbCompany>();
			companyBR.SetCountry(Core.Constants.CountryCodes.Brazil);
			var taxConfigurationBR = companyBR.AccTaxConfigurations.AddNew();
			mockITaxFrameworkConfigurationHelper.WithGetTaxAuthorities(Core.Constants.CountryCodes.Brazil, null, new CodeDescriptionPairList { code_RJ });
			Assert("RJ Tax Authorities should be in the collection.", taxConfigurationBR.Lookups.TaxAuthorities.ContainsOnly("RJ"));

			var companyAU = Factory.NewWithValidTestData<GlbCompany>();
			companyAU.SetCountry(Core.Constants.CountryCodes.Australia);
			var taxConfigurationAU = companyAU.AccTaxConfigurations.AddNew();
			mockITaxFrameworkConfigurationHelper.WithGetTaxAuthorities(Core.Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { });
			AssertEquals("Should be empty because not have Australian Company base Tax Authorities.", 0, taxConfigurationAU.Lookups.TaxAuthorities.Count);
		}

		public void TestTaxControlAccountsLookups_CompanyConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			config.ETC_ParentId = CompanyABC.PK;

			AssertGLAccountsForCompanyConfig(config.Lookups.TaxControlAccounts);
		}

		public void TestTaxExpenseAccountsLookups_CompanyConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			config.ETC_ParentId = CompanyABC.PK;

			AssertGLAccountsForCompanyConfig(config.Lookups.TaxExpenseAccounts);
		}

		public void TestTaxPendingControlAccountsLookups_CompanyConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			config.ETC_ParentId = CompanyABC.PK;

			AssertGLAccountsForCompanyConfig(config.Lookups.TaxPendingControlAccounts);
		}

		void AssertGLAccountsForCompanyConfig(AccGLHeaderCollection lookups)
		{
			lookups.Load();
			CombineAssertions(() =>
			{
				Assert("Header1: GL account type incorrect", !lookups.Contains(Header1));
				Assert("Header2: GL account type incorrect", !lookups.Contains(Header2));
				Assert("Header3: BSH - Is control account", !lookups.Contains(Header3));
				Assert("Header4: BSH - Ignoring direct posting", lookups.Contains(Header4));
				Assert("Header5: BSH - Company Config - company linked with AccGLHeaderCompanyFilter", lookups.Contains(Header5));
				Assert("Header6: BSH - Valid Case ignoring direct posting", lookups.Contains(Header6));
				Assert("Header7: P&L - Is control account", !lookups.Contains(Header7));
				Assert("Header8: P&L - Ignoring direct posting", lookups.Contains(Header8));
				Assert("Header9: P&L - Branch Config - branch company linked with AccGLHeaderCompanyFilter", !lookups.Contains(Header9));
				Assert("Header10: P&L - Ignoring direct posting", lookups.Contains(Header10));
			});
		}

		public void TestTaxControlAccountsLookups_BranchConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			config.ETC_ParentId = BranchXYZ.PK;

			AssertGLAccountsForBranchConfig(config.Lookups.TaxControlAccounts);
		}

		public void TestTaxExpenseAccountsLookups_BranchConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			config.ETC_ParentId = BranchXYZ.PK;

			AssertGLAccountsForBranchConfig(config.Lookups.TaxExpenseAccounts);
		}

		public void TestTaxPendingControlAccountsLookups_BranchConfig()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			config.ETC_ParentId = BranchXYZ.PK;

			AssertGLAccountsForBranchConfig(config.Lookups.TaxPendingControlAccounts);
		}

		void AssertGLAccountsForBranchConfig(AccGLHeaderCollection lookups)
		{
			lookups.Load();
			CombineAssertions(() =>
			{
				Assert("Header1: GL account type incorrect", !lookups.Contains(Header1));
				Assert("Header2: GL account type incorrect", !lookups.Contains(Header2));
				Assert("Header3: BSH - Is control account", !lookups.Contains(Header3));
				Assert("Header4: BSH - Ignoring direct posting", lookups.Contains(Header4));
				Assert("Header5: BSH - Company Config - company linked with AccGLHeaderCompanyFilter", !lookups.Contains(Header5));
				Assert("Header6: BSH - Valid Case ignoring direct posting", lookups.Contains(Header6));
				Assert("Header7: P&L - Is control account", !lookups.Contains(Header7));
				Assert("Header8: P&L - Ignoring direct posting", lookups.Contains(Header8));
				Assert("Header9: P&L - Branch Config - branch company linked with AccGLHeaderCompanyFilter", lookups.Contains(Header9));
				Assert("Header10: P&L - Ignoring direct posting", lookups.Contains(Header10));
			});
		}

		public void TestTaxControlAccountsLookups_ConfigParentNotFound()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;

			AssertGLAccountsForConfigWithMissingParent(config.Lookups.TaxControlAccounts);
		}

		public void TestTaxExpenseAccountsLookups_ConfigParentNotFound()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;

			AssertGLAccountsForConfigWithMissingParent(config.Lookups.TaxExpenseAccounts);
		}

		public void TestTaxPendingControlAccountsLookups_ConfigParentNotFound()
		{
			SetupCompanyBranchGLAccounts();

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;

			AssertGLAccountsForConfigWithMissingParent(config.Lookups.TaxPendingControlAccounts);
		}

		[ExpectNoExceptions]
		public void TestETC_ThresholdMethodsParameterGetCountryCodeWhenCountryCodeIsEmpty()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			taxConfig.ETC_RN_NKCountry = ZString.Empty;
			var mockIGlobalAccountingCountryFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory();
			_ = taxConfig.Lookups.ETC_ThresholdMethods;
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestETC_ThresholdMethodsGetCountryUsesAccTaxConfigurationCountryCode()
		{
			var countryCode = "AB";
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			taxConfig.ETC_RN_NKCountry = countryCode;
			var mockIGlobalAccountingCountryFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory();
			_ = taxConfig.Lookups.ETC_ThresholdMethods;
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(countryCode));
		}

		public void TestETC_ThresholdMethodsElementsWhenGetCountryFactoryReturnsNull()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory();
			AssertArrayEqualsByElements(new[] { "NOT", "TRN", "TRB" }, taxConfig.Lookups.ETC_ThresholdMethods.GetAllCodes());
		}

		public void TestETC_ThresholdMethodsElementsWhenCountryImplementsThresholdMethodProvider()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			var mockITaxFrameworkThresholdMethodProvider = new Mock<ITaxFrameworkThresholdMethodProvider>();
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(false);
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(false);

			mockIAccountingCountryFactory.As<IInstanceProvider<ITaxFrameworkThresholdMethodProvider>>().Setup(x => x.Get()).Returns(mockITaxFrameworkThresholdMethodProvider.Object);

			var taxSuperTypes = new TaxSuperTypeList().GetAllCodes();
			var defaultExpectedValues = new[] { "NOT - No Threshold" };
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount" }, TaxSuperTypeList.RetentionInInvoice.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount" }, TaxSuperTypeList.Perceptions.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount" }, TaxSuperTypeList.ValueAddedTax.Code);

			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(true);

			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.RetentionInInvoice.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.Perceptions.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.ValueAddedTax.Code);

			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(false);
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(true);

			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "GRP - Transaction Level Group Tax Amount" }, TaxSuperTypeList.RetentionInInvoice.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount" }, TaxSuperTypeList.Perceptions.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount" }, TaxSuperTypeList.ValueAddedTax.Code);

			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelTaxBaseThresholdMethodSupported).Returns(true);
			mockITaxFrameworkThresholdMethodProvider.SetupGet(provider => provider.IsTransactionLevelGroupThresholdMethodSupported).Returns(true);

			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "GRP - Transaction Level Group Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.RetentionInInvoice.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.Perceptions.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.ValueAddedTax.Code);

			foreach (var superType in taxSuperTypes.Except(new[] { TaxSuperTypeList.RetentionInInvoice.Code, TaxSuperTypeList.Perceptions.Code, TaxSuperTypeList.ValueAddedTax.Code }))
			{
				AssertElements(defaultExpectedValues, superType);
			}

			void AssertElements(string[] expectedValues, string superType)
			{
				taxConfig.TaxSystem.TaxSuperType = superType;
				AssertArrayEqualsByElements(expectedValues, taxConfig.Lookups.ETC_ThresholdMethods.Cast<CodeDescriptionPair>().Select(item => item.CodeAndDescription).ToArray());
			}
		}

		public void TestETC_ThresholdMethodsElementsWhenCountryDoesNotImplementsThresholdMethodProvider()
		{
			var taxConfig = CreateTaxConfigurationWithTaxSystem();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var defaultExpectedValues = new[] { "NOT - No Threshold" };
			var taxSuperTypes = new TaxSuperTypeList().GetAllCodes();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.RetentionInInvoice.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.Perceptions.Code);
			AssertElements(new[] { "NOT - No Threshold", "TRN - Transaction Level Tax Amount", "TRB - Transaction Level Tax Base Amount" }, TaxSuperTypeList.ValueAddedTax.Code);
			foreach (var superType in taxSuperTypes.Except(new[] { TaxSuperTypeList.RetentionInInvoice.Code, TaxSuperTypeList.Perceptions.Code, TaxSuperTypeList.ValueAddedTax.Code }))
			{
				AssertElements(defaultExpectedValues, superType);
			}
			void AssertElements(string[] expectedValues, string superType)
			{
				taxConfig.TaxSystem.TaxSuperType = superType;
				AssertArrayEqualsByElements(expectedValues, taxConfig.Lookups.ETC_ThresholdMethods.Cast<CodeDescriptionPair>().Select(item => item.CodeAndDescription).ToArray());
			}
		}

		void AssertGLAccountsForConfigWithMissingParent(AccGLHeaderCollection lookups)
		{
			lookups.Load();
			CombineAssertions(() =>
			{
				Assert("Header1: GL account type incorrect", !lookups.Contains(Header1));
				Assert("Header2: GL account type incorrect", !lookups.Contains(Header2));
				Assert("Header3: BSH - Is control account", !lookups.Contains(Header3));
				Assert("Header4: BSH - Ignoring direct posting", !lookups.Contains(Header4));
				Assert("Header5: BSH - Company Config - company linked with AccGLHeaderCompanyFilter", !lookups.Contains(Header5));
				Assert("Header6: BSH - Valid Case ignoring direct posting", !lookups.Contains(Header6));
				Assert("Header7: P&L - Is control account", !lookups.Contains(Header7));
				Assert("Header8: P&L - Ignoring direct posting", !lookups.Contains(Header8));
				Assert("Header9: P&L - Branch Config - branch company linked with AccGLHeaderCompanyFilter", !lookups.Contains(Header9));
				Assert("Header10: P&L - Ignoring direct posting", !lookups.Contains(Header10));
			});
		}

		void SetupCompanyBranchGLAccounts()
		{
			CompanyABC = Factory.NewWithValidTestData<GlbCompany>();
			CompanyABC.GC_Code = "ABC";

			CompanyXYZ = Factory.NewWithValidTestData<GlbCompany>();
			CompanyXYZ.GC_Code = "XYZ";

			BranchXYZ = Factory.NewWithValidTestData<GlbBranch>();
			BranchXYZ.GB_Code = "XYZ";
			BranchXYZ.GB_GC = CompanyXYZ.PK;

			Header1 = Factory.NewWithValidTestData<AccGLHeader>();
			Header1.AG_AccountType = Core.Constants.AccountType.Total;
			Header1.AG_ControlAccount = false;

			Header2 = Factory.NewWithValidTestData<AccGLHeader>();
			Header2.AG_AccountType = Core.Constants.AccountType.Header;

			Header3 = Factory.NewWithValidTestData<AccGLHeader>();
			Header3.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header3.AG_ControlAccount = true;

			Header4 = Factory.NewWithValidTestData<AccGLHeader>();
			Header4.AG_AccountNum = "400";
			Header4.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header4.AG_ControlAccount = false;
			Header4.AG_IsGlobal = true;
			Header4.AG_DisallowDirectPosting = true;

			Header5 = Factory.NewWithValidTestData<AccGLHeader>();
			Header5.AG_AccountNum = "500";
			Header5.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header5.AG_ControlAccount = false;
			Header5.AG_IsGlobal = false;

			AccGLHeaderCompanyFilter filter = Factory.New<AccGLHeaderCompanyFilter>();
			filter.ACF_AG_Header = Header5.PK;
			filter.ACF_GC_Company = CompanyABC.PK;

			Header6 = Factory.NewWithValidTestData<AccGLHeader>();
			Header6.AG_AccountNum = "600";
			Header6.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Header6.AG_ControlAccount = false;
			Header6.AG_IsGlobal = true;
			Header6.AG_DisallowDirectPosting = false;

			Header7 = Factory.NewWithValidTestData<AccGLHeader>();
			Header7.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Header7.AG_ControlAccount = true;

			Header8 = Factory.NewWithValidTestData<AccGLHeader>();
			Header8.AG_AccountNum = "800";
			Header8.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Header8.AG_ControlAccount = false;
			Header8.AG_IsGlobal = true;
			Header8.AG_DisallowDirectPosting = true;

			Header9 = Factory.NewWithValidTestData<AccGLHeader>();
			Header9.AG_AccountNum = "900";
			Header9.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Header9.AG_ControlAccount = false;
			Header9.AG_IsGlobal = false;

			filter = Factory.New<AccGLHeaderCompanyFilter>();
			filter.ACF_AG_Header = Header9.PK;
			filter.ACF_GC_Company = CompanyXYZ.PK;

			Header10 = Factory.NewWithValidTestData<AccGLHeader>();
			Header10.AG_AccountNum = "1000";
			Header10.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Header10.AG_ControlAccount = false;
			Header10.AG_IsGlobal = true;
			Header10.AG_DisallowDirectPosting = false;

			Factory.Save();
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

		GlbCompany CompanyABC, CompanyXYZ;
		GlbBranch BranchXYZ;
		AccGLHeader Header1, Header2, Header3, Header4, Header5, Header6, Header7, Header8, Header9, Header10;
	}
}
