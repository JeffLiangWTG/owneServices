using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReportConfigurationPivotCollection))]
	class ReportConfigurationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<ReportConfigurationPivotCollection>
	{
		public void TestUpdateGLHeader()
		{
			TestAccGLDescriptorPivotCollection = TestAccGLAccountDescriptor.ReportConfigurationPivotCollection;
			TestGLDescriptorPivot = TestAccGLDescriptorPivotCollection.AddNew();
			TestGLDescriptorPivot.YJ_AG = TestGLAccount.PK;
			TestGLDescriptorPivot.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			TestGLDescriptorPivot.ReportCategory = BalanceSheet_China.Codes.D01;

			Factory.Save();

			AssertEquals(TestGLDescriptorPivot, TestAccGLDescriptorPivotCollection[0]);
			AssertEquals(TestAccGLDescriptorPivot.YJ_AG, TestGLAccount.PK);
			AssertEquals(TestAccGLDescriptorPivotCollection.AdditionalFilter, new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, TestGLAccount.PK));

			var changedGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			changedGLAccount.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestAccGLAccountDescriptor.ParentGLHeaderPK = changedGLAccount.PK;

			AssertEquals("The Pivot YJ_AG should be update to ChangeGLAccount.PK", TestAccGLDescriptorPivot.YJ_AG, changedGLAccount.PK);
			AssertEquals("The AdditionalFilter should be updated", TestAccGLDescriptorPivotCollection.AdditionalFilter, new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, changedGLAccount.PK));
		}

		public void TestGetAccGLDescriptorPivotAndFilter()
		{
			TestAccGLDescriptorPivotCollection = new ReportConfigurationPivotCollection(Factory, TestAccGLAccountDescriptor);
			TestGLDescriptorPivot = TestAccGLDescriptorPivotCollection.AddNew();
			TestGLDescriptorPivot.YJ_AG = TestGLAccount.PK;
			TestGLDescriptorPivot.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			TestGLDescriptorPivot.ReportCategory = BalanceSheet_China.Codes.D01;
			Factory.Save();

			AssertEquals(TestGLDescriptorPivot, TestAccGLDescriptorPivotCollection[0]);
			AssertNotEquals(TestGLDescriptorPivot1, TestAccGLAccountDescriptor.AccGLDescriptorPivotCOA);
		}

		public void TestSetDefaultsForNewChild()
		{
			TestAccGLDescriptorPivotCollection = new ReportConfigurationPivotCollection(Factory, TestAccGLAccountDescriptor);
			TestGLDescriptorPivot = TestAccGLDescriptorPivotCollection.AddNew();

			AssertNotEquals(TestAccGLAccountDescriptor.PK, TestGLDescriptorPivot.YJ_AJ);
			AssertEquals(TestAccGLAccountDescriptor.AJ_Language, TestGLDescriptorPivot.GLAccountDescriptor.AJ_Language);
			AssertEquals(TestAccGLAccountDescriptor.AJ_LocalAccountNumber, TestGLDescriptorPivot.GLAccountDescriptor.AJ_LocalAccountNumber);
			AssertEquals(TestAccGLAccountDescriptor.AJ_AccountDescription, TestGLDescriptorPivot.GLAccountDescriptor.AJ_AccountDescription);
			AssertNull(TestGLDescriptorPivot.GLAccountDescriptor.AccGLDescriptorPivotCOA);
			AssertNotNull(TestGLDescriptorPivot.GLAccountDescriptor);
			AssertEquals(TestGLAccount, TestGLDescriptorPivot.GLHeader);

			var reportTypeList = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value;
			AssertEquals(reportTypeList[0].ReportType, TestGLDescriptorPivot.ReportType);
			TestGLDescriptorPivot.ReportCategory = reportTypeList.GetReportTypeCategoriesFromCode(TestGLDescriptorPivot.ReportType)[0].Category;

			Factory.Save();

			var testGLDescriptorPivot1 = TestAccGLDescriptorPivotCollection.AddNew();
			AssertEquals(reportTypeList[1].ReportType, testGLDescriptorPivot1.ReportType);
			AssertEquals(TestGLAccount, testGLDescriptorPivot1.GLHeader);
			AssertEquals(TestAccGLAccountDescriptor.AJ_Language, testGLDescriptorPivot1.GLAccountDescriptor.AJ_Language);
			AssertEquals(TestAccGLAccountDescriptor.AJ_LocalAccountNumber, testGLDescriptorPivot1.GLAccountDescriptor.AJ_LocalAccountNumber);
			AssertEquals(TestAccGLAccountDescriptor.AJ_AccountDescription, testGLDescriptorPivot1.GLAccountDescriptor.AJ_AccountDescription);
		}

		public void TestDeletedAccGLDescriptorCOA()
		{
			var testAccGLDescriptorPivotCollection = GetCollectionToTest();
			AssertEquals(0, testAccGLDescriptorPivotCollection.Count);
			TestGLDescriptorPivot = testAccGLDescriptorPivotCollection.AddNew();
			AssertEquals(1, testAccGLDescriptorPivotCollection.Count);

			var newFactory = new BusinessObjectFactory();
			var newFactoryAccGLAccountDescriptor = newFactory.Load<AccGLAccountDescriptor>(TestAccGLAccountDescriptor.PK);
			newFactoryAccGLAccountDescriptor.Delete();
			newFactory.Save();

			Factory.Save();
			AssertEquals(0, testAccGLDescriptorPivotCollection.Count);
		}

		public void TestAfterNewElement_ComplianceReportsSetupsUserDefinedNotCopyCategoriesToComplianceReportsSetupsCN()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;

			var list = new ComplianceReportTypeCollection();
			ComplianceReportType complianceReportType1 = list.AddNew();
			complianceReportType1.ReportType = "EFG";

			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Precondition", AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value.Count, 1);
			AssertEquals("Precondition", AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.Count, 8);

			var collection = GetCollectionToTest();
			collection.AddNew();

			AssertEquals(AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.Count, 8);
			AssertNotEquals(AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.Count, 9);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		#region Implementation

		protected GLDescriptorPivot TestGLDescriptorPivot1;
		protected GLDescriptorPivot TestGLDescriptorPivot;
		protected ReportConfigurationPivotCollection TestAccGLDescriptorPivotCollection;
		protected AccGLAccountDescriptor TestAccGLAccountDescriptor;
		protected AccGLHeader TestGLAccount;
		protected AccGLDescriptorPivot TestAccGLDescriptorPivot;

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override ReportConfigurationPivotCollection GetCollectionToTest()
		{
			return new ReportConfigurationPivotCollection(Factory, TestAccGLAccountDescriptor ?? Factory.NewWithValidTestData<AccGLAccountDescriptor>());
		}

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

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			Factory.Save();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}
