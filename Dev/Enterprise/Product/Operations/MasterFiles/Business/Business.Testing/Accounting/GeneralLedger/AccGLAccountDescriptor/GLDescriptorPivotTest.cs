using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLDescriptorPivot))]
	class GLDescriptorPivotTest : AccGLDescriptorPivotTest
	{
		public void TestCurrentCompanyNonChina()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.Australia);
			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AccGLAccountDescriptor testAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testAccGLAccountDescriptor1.AJ_Language = Constants.Languages.EnglishAmerican;
			testAccGLAccountDescriptor1.AJ_ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			testAccGLAccountDescriptor1.AJ_LocalAccountNumber = "1234.555";

			GLDescriptorPivot testGLDescriptorPivot1 = Factory.New<GLDescriptorPivot>();
			testGLDescriptorPivot1.YJ_AJ = testAccGLAccountDescriptor1.PK;
			testGLDescriptorPivot1.YJ_AG = TestGLAccount.PK;
			testGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss;
			testGLDescriptorPivot1.ReportCategory = BalanceSheet_China.Codes.D01;

			AssertNotNull(testGLDescriptorPivot1.ReportType_List);
			AssertEquals(0, testGLDescriptorPivot1.ReportType_List.Count);

			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, testAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals(BalanceSheet_China.Codes.D01, testAccGLAccountDescriptor1.AJ_ReportCategory);
			AssertEquals(ZString.Empty, testGLDescriptorPivot1.ReportTypeDescription);
			AssertEquals(ZString.Empty, testGLDescriptorPivot1.ReportCategoryDescription);

			AssertHasError(testGLDescriptorPivot1.ReportTypeInfo, "The report type 'P&L' is missing in the current language 'EN-US'. \r\r\nPlease contact support for assistance with this error.");
			AssertHasError(testGLDescriptorPivot1.ReportCategoryInfo, "The category '" + BalanceSheet_China.Codes.D01 + "' is missing in the report type 'P&L' for the current language. \r\r\nPlease contact support for assistance with this error.");
		}

		public void TestReportType_List()
		{
			AssertNotNull(TestGLDescriptorPivot1.ReportType_List);
			AssertEquals(8, TestGLDescriptorPivot1.ReportType_List.Count);
		}

		public void TestReportType()
		{
			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Balance Sheet", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Profit And Loss for Year", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Profit And Loss for Monthly", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Statement of Provision for Impairments of Asset", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("VAT Detailed Report", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Profit and Loss Appropriation", TestGLDescriptorPivot1.ReportTypeDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement;
			AssertEquals(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("Statement of Shareholders' Equity", TestGLDescriptorPivot1.ReportTypeDescription);
		}

		public void TestReportCategory_List()
		{
			TestGLDescriptorPivot1.ReportType = ZString.Empty;
			AssertEquals(0, TestGLDescriptorPivot1.ReportCategory_List.Count);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			AssertNotNull(TestGLDescriptorPivot1.ReportCategory_List);
			AssertEquals(82, TestGLDescriptorPivot1.ReportCategory_List.Count);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss;
			AssertNotNull(TestGLDescriptorPivot1.ReportCategory_List);
			AssertEquals(33, TestGLDescriptorPivot1.ReportCategory_List.Count);
		}

		public void TestReportCategory()
		{
			AssertEquals(BalanceSheet_China.Codes.D01, TestAccGLAccountDescriptor1.AJ_ReportCategory);
			AssertEquals(BalanceSheet_China.Descriptions.D01, TestGLDescriptorPivot1.ReportCategoryDescription);

			TestGLDescriptorPivot1.ReportCategory = BalanceSheet_China.Codes.D03;
			AssertEquals(BalanceSheet_China.Codes.D03, TestAccGLAccountDescriptor1.AJ_ReportCategory);
			AssertEquals(BalanceSheet_China.Descriptions.D03, TestGLDescriptorPivot1.ReportCategoryDescription);

			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss;
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, TestGLDescriptorPivot1.ReportCategory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return TestGLDescriptorPivot1;
		}

		protected AccGLAccountDescriptor TestAccGLAccountDescriptor1;
		protected GLDescriptorPivot TestGLDescriptorPivot1;

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;

			base.SetUp();

			TestAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor1.AJ_ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			TestAccGLAccountDescriptor1.AJ_LocalAccountNumber = "1234.555";

			TestGLDescriptorPivot1 = Factory.New<GLDescriptorPivot>();
			TestGLDescriptorPivot1.YJ_AJ = TestAccGLAccountDescriptor1.PK;
			TestGLDescriptorPivot1.YJ_AG = TestGLAccount.PK;
			TestGLDescriptorPivot1.ReportType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			TestGLDescriptorPivot1.ReportCategory = BalanceSheet_China.Codes.D01;
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}
