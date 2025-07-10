using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccGLDescriptorPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateYJ_AG()
		{
			//For each language, no more than 1 Local Account can reference to the same Parent Account. Eg. 1 Parent Account = 1 Local Account per Language.
			AccGLAccountDescriptor savedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			AccGLHeader parentGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();

			savedDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			savedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			savedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			savedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			savedDescriptor.AJ_LocalAccountNumber = "1234.555";

			AccGLDescriptorPivot testAccGLDescriptorPivot = Factory.New<AccGLDescriptorPivot>();
			testAccGLDescriptorPivot.YJ_AJ = savedDescriptor.PK;
			testAccGLDescriptorPivot.YJ_AG = CargoWise.Types.ZGuid.Empty;
			Assert("This descriptor must reference a global GL account", testAccGLDescriptorPivot.YJ_AGInfo.HasErrors());
			testAccGLDescriptorPivot.YJ_AG = parentGLAccount.PK;
			Assert("Should be no errors ", !testAccGLDescriptorPivot.YJ_AGInfo.HasErrors());

			Factory.Save();

			AccGLAccountDescriptor unsavedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			unsavedDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			unsavedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			unsavedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			unsavedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;

			AccGLDescriptorPivot unTestAccGLDescriptorPivot = Factory.New<AccGLDescriptorPivot>();
			unTestAccGLDescriptorPivot.YJ_AJ = unsavedDescriptor.PK;
			unTestAccGLDescriptorPivot.YJ_AG = parentGLAccount.PK;
			Assert("Expect error since local account to parent account must be one-to-one within language", unTestAccGLDescriptorPivot.YJ_AGInfo.HasErrors());

			unTestAccGLDescriptorPivot.YJ_AG = testGLAccount.PK;
			Assert("Should be no errors since local account is one-to-one with parent account", !unTestAccGLDescriptorPivot.YJ_AGInfo.HasErrors());
		}

		public void TestValidateParentAccountNotTheSame()
		{
			AccGLAccountDescriptor savedDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();

			var pAndLQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			AccGLHeader parentGLAccount = Factory.LoadTop1<AccGLHeader>(pAndLQuery);
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			savedDescriptor.AJ_Language = Constants.Languages.Malay;
			savedDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			savedDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			savedDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			savedDescriptor.AJ_LocalAccountNumber = "1234.123";
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
	}
}
