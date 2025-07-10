using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLDescriptorPivot))]
	public class AccGLDescriptorPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAccGLDescriptorPivot()
		{
			AssertNotNull("AccGLDescriptorPivot should be not null.", TestAccGLDescriptorPivot1);
			AssertEquals(TestAccGLAccountDescriptor, TestAccGLDescriptorPivot1.GLAccountDescriptor);
			AssertEquals(TestGLAccount, TestAccGLDescriptorPivot1.GLHeader);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return TestAccGLDescriptorPivot1;
		}

		protected AccGLAccountDescriptor TestAccGLAccountDescriptor;
		protected AccGLHeader TestGLAccount;
		protected AccGLDescriptorPivot TestAccGLDescriptorPivot1;

		protected override void SetUp()
		{
			base.SetUp();
			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			TestGLAccount.AG_AccountNum = "1234.56.78";
			TestGLAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;

			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "1234.555";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = TestGLAccount.PK;
			Factory.Save();
			TestAccGLDescriptorPivot1 = TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA;
		}

		#endregion
	}
}
