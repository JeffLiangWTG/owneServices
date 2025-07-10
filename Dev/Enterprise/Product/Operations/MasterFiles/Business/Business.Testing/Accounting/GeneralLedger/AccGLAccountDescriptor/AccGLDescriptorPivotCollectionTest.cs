using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLDescriptorPivotCollection))]
	class AccGLDescriptorPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<AccGLDescriptorPivotCollection>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			TestGLAccount.AG_AccountNum = "1111.22.33";
			TestGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			TestAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor.AJ_LocalAccountNumber = "8888.888";
			TestAccGLAccountDescriptor.ParentGLHeaderPK = TestGLAccount.PK;
			TestAccGLDescriptorPivot = TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA;
			Factory.Save();
		}

		protected override AccGLDescriptorPivotCollection GetCollectionToTest()
		{
			return new AccGLDescriptorPivotCollection(Factory, TestAccGLAccountDescriptor ?? Factory.NewWithValidTestData<AccGLAccountDescriptor>());
		}
		protected AccGLAccountDescriptor TestAccGLAccountDescriptor;
		protected AccGLHeader TestGLAccount;
		protected AccGLDescriptorPivot TestAccGLDescriptorPivot;

		#endregion

		public void TestConstructors()
		{
			AccGLDescriptorPivotCollection collection1 = new AccGLDescriptorPivotCollection(Factory, TestAccGLAccountDescriptor);
			AssertEquals(1, collection1.Count);
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new AccGLDescriptorPivotCollection(Factory, null); });
		}

		public void TestMapToGLHeader()
		{
			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);

			TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.MapToGLHeader(ZGuid.Empty);
			AssertEquals(0, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);

			TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.MapToGLHeader(TestGLAccount.PK);
			AssertEquals(1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOACollection.Count);
			AssertEquals(TestAccGLAccountDescriptor.ParentGLHeaderPK, TestGLAccount.PK);
		}
	}
}
