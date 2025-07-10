using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGLHeaderFilterBusinessObject))]
	sealed class AccGLHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccGLHeaderFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		#endregion

		#region Filters

		public void TestCashFlowTypeFilter()
		{
			AccGLHeader testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader testGLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();

			testGLHeader1.AG_CashFlowType = "XXX";
			testGLHeader2.AG_CashFlowType = "O01";
			testGLHeader3.AG_CashFlowType = "CSH";

			Factory.Save();

			AccGLHeaderFilterBusinessObject filter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)filter["Cash Flow Type"]).Property = "CSH";
			((ModuleTextFilter)filter["Cash Flow Type"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(filter.Filter);

			AssertCollectionNotContains(testGLHeader1, glHeaders);
			AssertCollectionNotContains(testGLHeader2, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);
		}

		public void TestLanguageFilter()
		{
			AccGLHeader testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader testGLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();

			AccGLAccountDescriptor testDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testDescriptor1.AJ_Language = Core.Constants.Languages.Malay;
			testDescriptor1.ParentGLHeaderPK = testGLHeader1.PK;

			AccGLAccountDescriptor testDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testDescriptor2.AJ_Language = Core.Constants.Languages.Malay;
			testDescriptor2.ParentGLHeaderPK = testGLHeader2.PK;

			AccGLAccountDescriptor testDescriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testDescriptor3.AJ_Language = Core.Constants.Languages.Afrikaans;
			testDescriptor3.ParentGLHeaderPK = testGLHeader3.PK;

			Factory.Save();

			AccGLHeaderFilterBusinessObject languageFilter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)languageFilter["Language"]).Property = Core.Constants.Languages.Malay;
			((ModuleTextFilter)languageFilter["Language"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(languageFilter.Filter);

			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);
			AssertCollectionNotContains(testGLHeader3, glHeaders);
		}

		public void TestUnmappedAccountsFilter()
		{
			AccGLHeader unmappedGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			unmappedGLHeader.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;

			AccGLHeader mappedGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			mappedGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			AccGLHeader unmappedTTLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			unmappedTTLHeader.AG_AccountType = AccountTypeComboBoxConstants.Total;

			AccGLAccountDescriptor testGLDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testGLDescriptor.AJ_Language = Constants.Languages.Malay;
			testGLDescriptor.ParentGLHeaderPK = mappedGLHeader.PK;

			Factory.Save();

			AccGLHeaderFilterBusinessObject unmappedFilter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)unmappedFilter["Language"]).Property = Constants.Languages.Malay;
			((ModuleTextFilter)unmappedFilter["Language"]).IsActive = true;
			((ModuleFlagsFilter)unmappedFilter["Unmapped accounts"]).Property0 = true;
			((ModuleFlagsFilter)unmappedFilter["Unmapped accounts"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(unmappedFilter.Filter);

			AssertCollectionContains(unmappedGLHeader, glHeaders);
			AssertCollectionNotContains(mappedGLHeader, glHeaders);
			AssertCollectionNotContains(unmappedTTLHeader, glHeaders);
		}

		public void TestSubAccountTypeFilterWithMultipleSubAccounts()
		{
			var testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLHeader4 = Factory.NewWithValidTestData<AccGLHeader>();

			var testGLHeaderSubAccount1_1 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount1_2 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount2_1 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount2_2 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount3_1 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount3_2 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
			var testGLHeaderSubAccount3_3 = Factory.NewWithValidTestData<AccGLHeaderSubAccount>();

			testGLHeaderSubAccount1_1.ASA_AG = testGLHeader1.PK;
			testGLHeaderSubAccount1_1.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.Organization);
			testGLHeaderSubAccount1_2.ASA_AG = testGLHeader1.PK;
			testGLHeaderSubAccount1_2.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.SalesGroup);

			testGLHeaderSubAccount2_1.ASA_AG = testGLHeader2.PK;
			testGLHeaderSubAccount2_1.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.SalesGroup);
			testGLHeaderSubAccount2_2.ASA_AG = testGLHeader2.PK;
			testGLHeaderSubAccount2_2.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.StaffAndResources);

			testGLHeaderSubAccount3_1.ASA_AG = testGLHeader3.PK;
			testGLHeaderSubAccount3_1.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.StaffAndResources);
			testGLHeaderSubAccount3_2.ASA_AG = testGLHeader3.PK;
			testGLHeaderSubAccount3_2.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.StaffGroup);
			testGLHeaderSubAccount3_3.ASA_AG = testGLHeader3.PK;
			testGLHeaderSubAccount3_3.ASA_SubClass = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(Core.Constants.SubAccountType.Organization);
			Factory.Save();

			AccGLHeaderFilterBusinessObject subAccountFilter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).Property = Core.Constants.SubAccountType.Organization;
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(subAccountFilter.Filter);

			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionNotContains(testGLHeader2, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);
			AssertCollectionNotContains(testGLHeader4, glHeaders);

			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).Property = Core.Constants.SubAccountType.SalesGroup;
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(subAccountFilter.Filter);

			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);
			AssertCollectionNotContains(testGLHeader3, glHeaders);
			AssertCollectionNotContains(testGLHeader4, glHeaders);

			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).Property = Core.Constants.SubAccountType.StaffGroup;
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(subAccountFilter.Filter);

			AssertCollectionNotContains(testGLHeader1, glHeaders);
			AssertCollectionNotContains(testGLHeader2, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);
			AssertCollectionNotContains(testGLHeader4, glHeaders);

			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).Property = Core.Constants.SubAccountType.StaffAndResources;
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(subAccountFilter.Filter);

			AssertCollectionNotContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);
			AssertCollectionNotContains(testGLHeader4, glHeaders);

			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).Property = "ALL";
			((ModuleTextFilter)subAccountFilter["Sub Account Type"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(subAccountFilter.Filter);

			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);
			AssertCollectionContains(testGLHeader4, glHeaders);
		}

		public void TestCompanyFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CCC";
			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "DDD";

			var testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader1.AG_IsGlobal = false;
			var filter1ForHeader1 = testGLHeader1.CompanyFilters.AddNew();
			filter1ForHeader1.ACF_GC_Company = company1.PK;
			var filter2ForHeader1 = testGLHeader1.CompanyFilters.AddNew();
			filter2ForHeader1.ACF_GC_Company = company2.PK;

			var testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader2.AG_IsGlobal = false;
			var filter1ForHeader2 = testGLHeader2.CompanyFilters.AddNew();
			filter1ForHeader2.ACF_GC_Company = company3.PK;

			var testGLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader3.AG_IsGlobal = false;
			var filter1ForHeader3 = testGLHeader3.CompanyFilters.AddNew();
			filter1ForHeader3.ACF_GC_Company = company1.PK;

			var testGLHeader4 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader4.AG_IsGlobal = true;

			Factory.Save();

			var companyFilter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)companyFilter["Company Filter"]).Property = "AAA";
			((ModuleTextFilter)companyFilter["Company Filter"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader3, glHeaders);

			((ModuleTextFilter)companyFilter["Company Filter"]).Property = "BBB";
			((ModuleTextFilter)companyFilter["Company Filter"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionContains(testGLHeader1, glHeaders);

			((ModuleTextFilter)companyFilter["Company Filter"]).Property = "CCC";
			((ModuleTextFilter)companyFilter["Company Filter"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionContains(testGLHeader2, glHeaders);

			((ModuleTextFilter)companyFilter["Company Filter"]).Property = "DDD";
			((ModuleTextFilter)companyFilter["Company Filter"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);
			AssertEquals(0, glHeaders.Count);

			((ModuleTextFilter)companyFilter["Company Filter"]).Property = "ALL";
			((ModuleTextFilter)companyFilter["Company Filter"]).IsActive = true;
			glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);

			AssertCollectionNotContains(testGLHeader1, glHeaders);
			AssertCollectionNotContains(testGLHeader2, glHeaders);
			AssertCollectionNotContains(testGLHeader3, glHeaders);
			AssertCollectionContains(testGLHeader4, glHeaders);
		}

		public void TestGlobalStatusFilter()
		{
			var testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader1.AG_IsGlobal = true;
			var testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader2.AG_IsGlobal = false;
			Factory.Save();

			var companyFilter = new AccGLHeaderFilterBusinessObject();
			((ModuleTextFilter)companyFilter["Global Status"]).Property = "ALL";
			((ModuleTextFilter)companyFilter["Global Status"]).IsActive = true;
			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);

			((ModuleTextFilter)companyFilter["Global Status"]).Property = "NOT GLOBAL";
			((ModuleTextFilter)companyFilter["Global Status"]).IsActive = true;
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionNotContains(testGLHeader1, glHeaders);
			AssertCollectionContains(testGLHeader2, glHeaders);

			((ModuleTextFilter)companyFilter["Global Status"]).Property = "GLOBAL";
			((ModuleTextFilter)companyFilter["Global Status"]).IsActive = true;
			glHeaders.Load(companyFilter.Filter);
			AssertCollectionContains(testGLHeader1, glHeaders);
			AssertCollectionNotContains(testGLHeader2, glHeaders);
		}

		public void TestLocalAccountCodeFilter()
		{
			SetUpTestDataForLocalAccountFilter();

			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			var filterBizO = new AccGLHeaderFilterBusinessObject();
			var localAccountFilter = (ModuleTextFilter)filterBizO["Local Account Code"];

			localAccountFilter.Property = "0000";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("2 GL Headers with local account should be found", 2, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1, testGLHeader2 }, glHeaders);

			localAccountFilter.Property = "00000001";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with local account code '00000001' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1 }, glHeaders);

			localAccountFilter.Property = "3333";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("No GL Header's local account code starts with '3333'", 0, glHeaders.Count);
		}

		public void TestLocalAccountDescriptionFilter()
		{
			SetUpTestDataForLocalAccountFilter();

			AccGLHeaderCollection glHeaders = new AccGLHeaderCollection(Factory);
			var filterBizO = new AccGLHeaderFilterBusinessObject();
			var localAccountFilter = (ModuleTextFilter)filterBizO["Local Account Description"];

			localAccountFilter.Property = "Local Account Description";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("2 GL Headers with local account should be found", 2, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1, testGLHeader2 }, glHeaders);

			localAccountFilter.Property = "Local Account Description 1";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with local account description 'Local Account Description 1' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1 }, glHeaders);

			localAccountFilter.Property = "Global Account Description";
			localAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("No GL Header's local account description starts with 'Global Account Description'", 0, glHeaders.Count);
		}

		public void TestAccountTypeFilter()
		{
			var accountTypeFilter = new AccGLHeaderFilterBusinessObject();
			var expectedCodes_NoteGLAccountEnabled = new string[]
			{
				"ALL",
				Constants.AccountType.BalanceSheetAccount,
				Constants.AccountType.ProfitAndLossAccount,
				Constants.AccountType.Total,
				Constants.AccountType.Header,
				Constants.AccountType.Consolidation,
				Constants.AccountType.Alternate,
				Constants.AccountType.Note,
				Constants.AccountType.Undefined
			};

			var accountTypeList = ((ModuleTextFilter)accountTypeFilter["Account Type"]).List;
			var accountTypeActualCodesArray = (accountTypeList as CodeDescriptionPairList).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes_NoteGLAccountEnabled, accountTypeActualCodesArray);
			AssertEquals("AccountTypeList.Count should be 9", 9, accountTypeList.Count);
		}

		AccGLHeader testGLHeader1;
		AccGLHeader testGLHeader2;

		void SetUpTestDataForLocalAccountFilter()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			GlbStaff.CurrentUser.GS_WorkingLanguage = Constants.Languages.ChineseSimplified;

			testGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader1.AG_AccountNum = "1111.11.11";
			testGLHeader1.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			var testGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testGLAccountDescriptor1.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			testGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			testGLAccountDescriptor1.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testGLAccountDescriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLAccountDescriptor1.ParentGLHeaderPK = testGLHeader1.PK;
			testGLAccountDescriptor1.AJ_LocalAccountNumber = "00000001";
			testGLAccountDescriptor1.AJ_AccountDescription = "Local Account Description 1";

			testGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader2.AG_AccountNum = "2222.22.22";
			testGLHeader2.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			var testGLAccountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testGLAccountDescriptor2.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			testGLAccountDescriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			testGLAccountDescriptor2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testGLAccountDescriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLAccountDescriptor2.ParentGLHeaderPK = testGLHeader2.PK;
			testGLAccountDescriptor2.AJ_LocalAccountNumber = "00000002";
			testGLAccountDescriptor2.AJ_AccountDescription = "Local Account Description 2";

			Factory.Save();
		}

		public void TestAlternateAccount()
		{
			SetUpTestDataForAlternateAccountFilter();

			var glHeaders = new AccGLHeaderCollection(Factory);
			var filterBizO = new AccGLHeaderFilterBusinessObject();
			var alternateGLAccountFilter = (ModuleTextFilter)filterBizO["Alternate Account"];

			glHeaders.Load(filterBizO.Filter);
			Assert(glHeaders.GetPKs().Contains(testGLHeader1.PK));
			Assert(glHeaders.GetPKs().Contains(testGLHeader2.PK));

			alternateGLAccountFilter.Property = "10.00.1000";
			alternateGLAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account '10.00.1000' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1.PK }, glHeaders.GetPKs());

			alternateGLAccountFilter.Property = "20.00.1000";
			alternateGLAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account '20.00.1000' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader2.PK }, glHeaders.GetPKs());

			alternateGLAccountFilter.Property = "3333";
			alternateGLAccountFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("No GL Header's Alternate Account starts with '3333'", 0, glHeaders.Count);

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			filterBizO = new AccGLHeaderFilterBusinessObject();
			AssertNull(filterBizO["Alternate Account"]);
		}

		public void TestAlternateAccountName()
		{
			SetUpTestDataForAlternateAccountFilter();

			var glHeaders = new AccGLHeaderCollection(Factory);
			var filterBizO = new AccGLHeaderFilterBusinessObject();
			var alternateGLAccountNameFilter = (ModuleTextFilter)filterBizO["Alternate Account Name"];

			glHeaders.Load(filterBizO.Filter);
			Assert(glHeaders.GetPKs().Contains(testGLHeader1.PK));
			Assert(glHeaders.GetPKs().Contains(testGLHeader2.PK));

			alternateGLAccountNameFilter.Property = "AlternateGLAccount1";
			alternateGLAccountNameFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account Name 'AlternateGLAccount1' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader1.PK }, glHeaders.GetPKs());

			alternateGLAccountNameFilter.Property = "AlternateGLAccount2";
			alternateGLAccountNameFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account Name 'AlternateGLAccount2' should be found", 1, glHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { testGLHeader2.PK }, glHeaders.GetPKs());

			alternateGLAccountNameFilter.Property = "3333";
			alternateGLAccountNameFilter.IsActive = true;
			glHeaders.Load(filterBizO.Filter);
			AssertEquals("No GL Header's Alternate Account Name starts with '3333'", 0, glHeaders.Count);

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			filterBizO = new AccGLHeaderFilterBusinessObject();
			AssertNull(filterBizO["Alternate Account Name"]);
		}

		void SetUpTestDataForAlternateAccountFilter()
		{
			SetUpTestDataForLocalAccountFilter();

			var creator = new AccountingTestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT", "Management Reporting", true, true);
			creator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			var chart2 = creator.CreateAlternateChart("TRR", "Management Reporting");
			creator.CreateAccAlternateChartFormat(chart2, 1, "X", "tier 1");
			Factory.Save();

			var alternateGLAccountForGLHeader1 = Factory.NewWithValidTestData<AccAlternateGLAccount>();
			alternateGLAccountForGLHeader1.AGA_AAC_AlternateChart = chart.PK;
			alternateGLAccountForGLHeader1.AGA_AccountNum = "10.00.1000";
			alternateGLAccountForGLHeader1.AGA_Description = "AlternateGLAccount1";
			var attribute = Factory.NewWithValidTestData<AccAlternateGLAccountAttribute>();
			attribute.AAA_AG_GLHeader = testGLHeader1.PK;
			attribute.AAA_AGA_AlternateGLAccount = alternateGLAccountForGLHeader1.PK;
			attribute.AAA_Sequence = 1;
			attribute.AAA_Attribute = "OCG";
			attribute.AAA_Value = "OCG";

			var alternateGLAccountForGLHeader2 = Factory.NewWithValidTestData<AccAlternateGLAccount>();
			alternateGLAccountForGLHeader2.AGA_AAC_AlternateChart = chart.PK;
			alternateGLAccountForGLHeader2.AGA_AccountNum = "20.00.1000";
			alternateGLAccountForGLHeader2.AGA_Description = "AlternateGLAccount2";
			attribute = Factory.NewWithValidTestData<AccAlternateGLAccountAttribute>();
			attribute.AAA_AG_GLHeader = testGLHeader2.PK;
			attribute.AAA_AGA_AlternateGLAccount = alternateGLAccountForGLHeader2.PK;
			attribute.AAA_Sequence = 1;
			attribute.AAA_Attribute = "OCG";
			attribute.AAA_Value = "OCG";

			var alternateGLAccountForGLHeader3 = creator.CreateAccAlternateGlAccount(chart2.PK, "30.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount For GLHeader2");
			creator.CreateAccAlternateGlAccountAttribute(alternateGLAccountForGLHeader3, testGLHeader2.PK, 1, "OCG", "OCG");

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());
		}

		#endregion
	}
}
