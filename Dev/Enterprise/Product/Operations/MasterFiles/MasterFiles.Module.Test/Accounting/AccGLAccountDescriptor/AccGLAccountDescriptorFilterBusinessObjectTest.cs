using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGLAccountDescriptorFilterBusinessObject))]
	public class AccGLAccountDescriptorFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccGLAccountDescriptorFilterBusinessObject();
		}

		public void TestParentAccount_ProfitAndLossAccount()
		{
			AssertFilteredForParentAccount(AccountTypeComboBoxConstants.ProfitAndLossAccount);
		}

		public void TestParentAccount_BalanceSheetAccount()
		{
			AssertFilteredForParentAccount(AccountTypeComboBoxConstants.BalanceSheetAccount);
		}

		public void TestParentAccount_NoteAccount()
		{
			AssertFilteredForParentAccount(AccountTypeComboBoxConstants.Note);
		}

		public void AssertFilteredForParentAccount(ZString accountType)
		{
			var testAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountType = accountType;

			testAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor.AJ_ReportCategory = accountType;
			testAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testAccGLAccountDescriptor.AJ_LocalAccountNumber = "1234.555";
			testAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			Factory.Save();

			var gLAccountFilter = new AccGLAccountDescriptorFilterBusinessObject();
			var parentAccountFilter = (ModuleGuidFilter)gLAccountFilter["Parent Account"];
			parentAccountFilter.Property = testGLAccount.PK;
			parentAccountFilter.IsActive = true;
			AssertNoErrors(parentAccountFilter);

			var gLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(gLAccountFilter.Filter);
			AssertNotNull("Filtered for AccGLHeader, Collection should not null", gLAccountDescriptor);

			var testAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			var testGLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount1.AG_AccountType = accountType;

			testAccGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor1.AJ_ReportCategory = accountType;
			testAccGLAccountDescriptor1.AJ_ReportType = "nnn";
			testAccGLAccountDescriptor1.AJ_LocalAccountNumber = "2234.666";
			testAccGLAccountDescriptor1.ParentGLHeaderPK = testGLAccount1.PK;

			AssertNotEquals("Precondition", AccGLAccountDescriptor.ReportTypeCOA, testAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Precondition: Is not COA descriptor, ParentGLHeaderPK should be empty", ZGuid.Empty, testAccGLAccountDescriptor1.ParentGLHeaderPK);
			Factory.Save();

			AccGLAccountDescriptor[] totalGLAccountDescriptor = Factory.Load<AccGLAccountDescriptor>(new ZQuery());
			AssertEquals("Expect 2 GLAccountDescriptors", 2, totalGLAccountDescriptor.Length);

			gLAccountFilter = new AccGLAccountDescriptorFilterBusinessObject();
			((ModuleGuidFilter)gLAccountFilter["Parent Account"]).Property = testGLAccount1.PK;
			((ModuleGuidFilter)gLAccountFilter["Parent Account"]).IsActive = true;
			gLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(gLAccountFilter.Filter);
			AssertNull("Filtered for AccGLHeader, Collection should null", gLAccountDescriptor);
		}

		public void TestReportType()
		{
			AccGLAccountDescriptor testAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			AccGLHeader testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			testAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testAccGLAccountDescriptor.AJ_LocalAccountNumber = "1234.555";
			testAccGLAccountDescriptor.ParentGLHeaderPK = testGLAccount.PK;
			AccGLAccountDescriptor testAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			AccGLHeader testGLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			testGLAccount1.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			testAccGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testAccGLAccountDescriptor1.AJ_ReportType = "nnn";
			testAccGLAccountDescriptor1.AJ_LocalAccountNumber = "2234.666";
			testAccGLAccountDescriptor1.ParentGLHeaderPK = testGLAccount1.PK;

			Factory.Save();

			AccGLAccountDescriptor[] totalGLAccountDescriptor = Factory.Load<AccGLAccountDescriptor>(new ZQuery());
			AssertEquals("Expect 2 GLAccountDescriptors", 2, totalGLAccountDescriptor.Length);

			AccGLAccountDescriptorFilterBusinessObject gLAccountFilter = new AccGLAccountDescriptorFilterBusinessObject();
			AccGLAccountDescriptor[] gLAccountDescriptor = Factory.Load<AccGLAccountDescriptor>(gLAccountFilter.Filter);
			AssertEquals("Expect 1 GLAccountDescriptor with COA ReportType", 1, gLAccountDescriptor.Length);
			AssertEquals("Expected GLAccountDescriptor with COA ReportType ", AccGLAccountDescriptor.ReportTypeCOA, gLAccountDescriptor[0].AJ_ReportType);
		}

		#endregion
	}
}
