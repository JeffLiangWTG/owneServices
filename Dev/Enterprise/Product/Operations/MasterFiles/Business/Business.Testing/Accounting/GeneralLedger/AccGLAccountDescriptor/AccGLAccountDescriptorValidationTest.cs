using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccGLAccountDescriptorValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		protected AccGLAccountDescriptor TestAccGLAccountDescriptor;

		protected override void SetUp()
		{
			base.SetUp();
			TestAccGLAccountDescriptor = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as AccGLAccountDescriptor;
		}

		protected Type GetExpectedBusinessObjectType()
		{
			return typeof(AccGLAccountDescriptor);
		}

		#endregion

		#region Test Validation

		public void TestValidateAJ_Language()
		{
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.Afrikaans;
			Assert("Should not have errors since language is valid", !TestAccGLAccountDescriptor.AJ_LanguageInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_Language = ZString.Empty;
			Assert("Should have error since AJ_Language cannot be empty", TestAccGLAccountDescriptor.AJ_LanguageInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.Portuguese;
			Assert("Should not have errors since language is valid", !TestAccGLAccountDescriptor.AJ_LanguageInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_Language = "###";
			Assert("Should have error since description is invalid", TestAccGLAccountDescriptor.AJ_LanguageInfo.HasErrors());
		}

		public void TestValidateAJ_AccountDescription()
		{
			TestAccGLAccountDescriptor.AJ_AccountDescription = "$$$";
			Assert("Should not have errors since there description has been entered", !TestAccGLAccountDescriptor.AJ_AccountDescriptionInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_AccountDescription = ZString.Empty;
			Assert("Should have errors since no description entered", TestAccGLAccountDescriptor.AJ_AccountDescriptionInfo.HasErrors());
		}

		public void TestValidateAJ_ReportCategory()
		{
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Alternate;
			Assert("ALT GL header is now supported", !TestAccGLAccountDescriptor.AJ_ReportCategoryInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_ReportCategory = ZString.Empty;
			Assert("Report type cannot be empty", TestAccGLAccountDescriptor.AJ_ReportCategoryInfo.HasErrors());
		}

		public void TestValidateYJ_AGList()
		{
			// Parent GL Account is filter for P&L and BSH only
			TestAccGLAccountDescriptor.ParentGLHeaderPK_List.Load();
			ZQuery hDRFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.Header);
			AccGLHeader[] hDR_Account = (AccGLHeader[])TestAccGLAccountDescriptor.ParentGLHeaderPK_List.Find(hDRFilter);
			AssertEquals("Should not be any HDR accounts in YJ_AG_List", 0, hDR_Account.Length);

			ZQuery tTLFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.Total);
			AccGLHeader[] tTL_Accounts = (AccGLHeader[])TestAccGLAccountDescriptor.ParentGLHeaderPK_List.Find(tTLFilter);
			AssertEquals("Should not be any TTL accounts in YJ_AGList", 0, tTL_Accounts.Length);
		}

		public void TestValidateAJ_DebitCredit()
		{
			TestAccGLAccountDescriptor.ClearAllNotifications();
			TestAccGLAccountDescriptor.AJ_DebitCredit = ZString.Empty;
			Assert("AJ_DebitCredit should have errors, it cannot be empty", TestAccGLAccountDescriptor.AJ_DebitCreditInfo.HasErrors());
		}

		public void TestValidateAJ_AJ_CarriedForwardAccount()
		{
			// Carried Fwd Account is enable only if the selected Account Type is TTL
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			Assert("AJ_CarriedFwdAccount should be disabled for P&L GL Accounts", TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			Assert("AJ_CarriedFwdAccount should be disabled for BSH GL Accounts", TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;
			Assert("AJ_CarriedFwdAccount should be enabled for TTL GL Accounts", !TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			Assert("AJ_CarriedFwdAccount should be disabled for HDR GL Accounts", TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;
			TestAccGLAccountDescriptor.AJ_ReportCategory = "CFW";
			Assert("AJ_CarriedFwdAccount should be disabled for CFW GL Accounts", TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccountInfo.ReadOnly);
			Assert("AJ_CarriedFwdAccount should be cleared", TestAccGLAccountDescriptor.AJ_AJ_CarriedForwardAccount.IsEmpty);
		}

		public void TestValidateAJ_CarriedFwdAccountList()
		{
			// Carried Fwd Account is filter for CFW only.
			AccGLAccountDescriptor cFW_Descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			cFW_Descriptor.AJ_ReportCategory = "CFW";
			cFW_Descriptor.AJ_Language = Constants.Languages.Afrikaans;
			cFW_Descriptor.AJ_LocalAccountNumber = "3333.333";

			AccGLAccountDescriptor cFW_Descriptor_Malay = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			cFW_Descriptor_Malay.AJ_ReportCategory = "CFW";
			cFW_Descriptor_Malay.AJ_Language = Constants.Languages.Malay;
			cFW_Descriptor_Malay.AJ_LocalAccountNumber = "3333.333";
			Factory.Save();

			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.Malay;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "2222.222";
			TestAccGLAccountDescriptor.AJ_CarriedFwdAccountList.Load();

			AssertEquals("should only be 1 descriptor in the collection", 1, TestAccGLAccountDescriptor.AJ_CarriedFwdAccountList.Count);
			AssertEquals("The descriptor should have language Malay", Constants.Languages.Malay, TestAccGLAccountDescriptor.AJ_CarriedFwdAccountList[0].AJ_Language);

			ZQuery bSHFilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, AccountTypeComboBoxConstants.BalanceSheetAccount);
			AccGLAccountDescriptor[] bSH_Accounts = (AccGLAccountDescriptor[])TestAccGLAccountDescriptor.AJ_CarriedFwdAccountList.Find(bSHFilter);
			AssertEquals("Should be no BSH accounts in CarriedFwdFilter", 0, bSH_Accounts.Length);

			ZQuery pLFilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			AccGLAccountDescriptor[] pL_Accounts = (AccGLAccountDescriptor[])TestAccGLAccountDescriptor.AJ_CarriedFwdAccountList.Find(pLFilter);
			AssertEquals("Should be no P&L accounts in CarriedFwdFilter", 0, pL_Accounts.Length);
		}

		public void TestValidateAJ_TotalLevel()
		{
			// Total Level is enable if the selected Account Type is TTL
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			Assert("Total level should not be enabled for BSH account type", TestAccGLAccountDescriptor.AJ_TotalLevelInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			Assert("Total level should not be enabled for P&L account type", TestAccGLAccountDescriptor.AJ_TotalLevelInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;
			Assert("Total level should be enabled for TTL account type", !TestAccGLAccountDescriptor.AJ_TotalLevelInfo.ReadOnly);
			TestAccGLAccountDescriptor.AJ_TotalLevel = (ZShort)7890;
			Assert("Total level must be less than 999", TestAccGLAccountDescriptor.AJ_TotalLevelInfo.HasErrors());
			TestAccGLAccountDescriptor.AJ_ReportCategory = "CFW";
			Assert("Total level should not be enabled for CFW account type", TestAccGLAccountDescriptor.AJ_TotalLevelInfo.ReadOnly);
		}

		public void TestValidateUniqueLanguageAndLocalAccount()
		{
			// For each language, the Local Account cannot be repeated. Eg. CHS -1001 cannot have two entries.
			AccGLAccountDescriptor savedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			savedDescriptor.AJ_Language = Constants.Languages.Afrikaans;
			savedDescriptor.AJ_LocalAccountNumber = "1000.333";
			Factory.Save();

			AccGLAccountDescriptor unsavedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			unsavedDescriptor.AJ_Language = Constants.Languages.Afrikaans;
			unsavedDescriptor.AJ_LocalAccountNumber = "1000.333";
			Assert("Should be error since language and local account num combination is not unique", unsavedDescriptor.AJ_LocalAccountNumberInfo.HasErrors());

			unsavedDescriptor.AJ_Language = Constants.Languages.Malay;
			unsavedDescriptor.AJ_LocalAccountNumber = "1000.333";
			Assert("Should not be any errors since language and local account num are unique", !unsavedDescriptor.AJ_LocalAccountNumberInfo.HasErrors());

			unsavedDescriptor.AJ_Language = Constants.Languages.Afrikaans;
			unsavedDescriptor.AJ_LocalAccountNumber = "2000.333";
			Assert("Should be no errors since language and local account num are unique", !unsavedDescriptor.AJ_LocalAccountNumberInfo.HasErrors());
		}

		public void TestValidateAll()
		{
			AccGLAccountDescriptor unsavedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			unsavedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			unsavedDescriptor.AJ_Language = Constants.Languages.English;
			unsavedDescriptor.AJ_DebitCredit = "DR";
			unsavedDescriptor.AJ_ReportType = "SSS";
			var validation = new AccGLAccountDescriptorValidation(unsavedDescriptor);
			AssertNoExceptionThrown(() => validation.ValidateAll());
		}

		public void TestValidateYJ_AG()
		{
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			var pAndLQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			TestAccGLAccountDescriptor.ParentGLHeaderPK = Factory.LoadTop1(typeof(AccGLHeader), pAndLQuery).PK;
			Assert("Should not have errors since global GL is not empty", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;
			Assert("P&L GL header must have a global GL account", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			var bshQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.BalanceSheetAccount);
			TestAccGLAccountDescriptor.ParentGLHeaderPK = Factory.LoadTop1(typeof(AccGLHeader), bshQuery).PK;
			Assert("Should not have errors since global GL is not empty", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;
			Assert("BSH header must have a global GL account", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			TestAccGLAccountDescriptor.ParentGLHeaderPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;

			Assert("HDR GL header can have NULL for YJ_AG", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());
			Assert("HDR GL Header should have YJ_AG disabled", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.ReadOnly);

			TestAccGLAccountDescriptor.ParentGLHeaderPK = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.Total;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = ZGuid.Empty;

			Assert("YJ_AG should be disabled for TTL GL Accounts", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.ReadOnly);
			Assert("Total GL accounts can have NULL for YJ_AG", !TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.HasErrors());

			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestAccGLAccountDescriptor.AJ_ReportCategory = "CFW";
			Assert("YJ_AG should be disabled for CFW Accounts", TestAccGLAccountDescriptor.ParentGLHeaderPKInfo.ReadOnly);
			Assert("YJ_AG should be cleared", TestAccGLAccountDescriptor.ParentGLHeaderPK.IsEmpty);
		}

		public void TestValidateLocalAccToParentAccIsOneToOne()
		{
			//For each language, no more than 1 Local Account can reference to the same Parent Account. Eg. 1 Parent Account = 1 Local Account per Language.
			AccGLAccountDescriptor savedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			var pAndLQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			AccGLHeader parentGLAccount = Factory.LoadTop1<AccGLHeader>(pAndLQuery);
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			savedDescriptor.AJ_Language = Constants.Languages.Malay;
			savedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			savedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			savedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			savedDescriptor.AJ_LocalAccountNumber = "###";
			savedDescriptor.ParentGLHeaderPK = parentGLAccount.PK;

			Factory.Save();

			AccGLAccountDescriptor unsavedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			unsavedDescriptor.AJ_Language = Constants.Languages.Malay;
			unsavedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			unsavedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			unsavedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			unsavedDescriptor.ParentGLHeaderPK = parentGLAccount.PK;

			Assert("Expect error since local account to parent account must be one-to-one within language", unsavedDescriptor.ParentGLHeaderPKInfo.HasErrors());

			unsavedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			unsavedDescriptor.AJ_Language = Constants.Languages.Malay;
			unsavedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.HongKong;
			unsavedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			unsavedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			unsavedDescriptor.ParentGLHeaderPK = parentGLAccount.PK;

			Assert("Should be no errors since language has changed", !unsavedDescriptor.ParentGLHeaderPKInfo.HasErrors());

			unsavedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			unsavedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			unsavedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			unsavedDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Assert("Should be no errors since local account is one-to-one with parent account", !unsavedDescriptor.ParentGLHeaderPKInfo.HasErrors());

			unsavedDescriptor.AJ_Language = Constants.Languages.Afrikaans;
			unsavedDescriptor.ParentGLHeaderPK = parentGLAccount.PK;

			Assert("Should be no errors since language has changed", !unsavedDescriptor.ParentGLHeaderPKInfo.HasErrors());
		}

		public void TestValidateParentGLHeaderPK_IsInvalid()
		{
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			descriptor.ParentGLHeaderPK = testGLAccount.PK;

			AssertNotEquals("Precondition", AccountTypeComboBoxConstants.ProfitAndLossAccount, testGLAccount.AG_AccountType);
			AssertHasError(descriptor.ParentGLHeaderPKInfo, "Enter a valid selection.");

			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = testGLAccount.PK;
			AssertNoError(descriptor.ParentGLHeaderPKInfo, "Enter a valid selection.");
		}

		#endregion
	}
}
