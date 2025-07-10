using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GLDescriptorPivotValidationTest : AccGLDescriptorPivotValidationTest
	{
		public void TestValidateReportType()
		{
			AssertEquals("TT0", TestAccGLAccountDescriptor1.AJ_ReportType);
			AssertEquals("TT0", TestGLDescriptorPivot0.ReportType);

			AssertNoErrors(TestGLDescriptorPivot0.ReportTypeInfo);
			TestGLDescriptorPivot0.ReportType = "ABC";
			AssertHasError(TestGLDescriptorPivot0.ReportTypeInfo, "The report type 'ABC' is missing in the current language 'ZH-CN'. \r\r\nPlease contact support for assistance with this error.");
		}

		public void TestValidateReportCategory()
		{
			AssertEquals("A01", TestAccGLAccountDescriptor1.AJ_ReportCategory);
			AssertEquals("A01", TestGLDescriptorPivot0.ReportCategory);
			AssertNoErrors(TestGLDescriptorPivot0.ReportCategoryInfo);

			TestGLDescriptorPivot0.ReportCategory = "ABC";
			AssertHasError(TestGLDescriptorPivot0.ReportCategoryInfo, "The category 'ABC' is missing in the report type 'TT0' for the current language. \r\r\nPlease contact support for assistance with this error.");
			TestGLDescriptorPivot0.ReportCategory = "A01";

			AccGLAccountDescriptor testAccGLAccountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testAccGLAccountDescriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor2.AJ_ReportType = "TT0";
			testAccGLAccountDescriptor2.AJ_LocalAccountNumber = "1234.555";

			GLDescriptorPivot testGLDescriptorPivot2 = Factory.NewWithValidTestData<GLDescriptorPivot>();
			testGLDescriptorPivot2.YJ_AJ = testAccGLAccountDescriptor2.PK;
			testGLDescriptorPivot2.YJ_AG = TestGLAccount1.PK;
			testGLDescriptorPivot2.ReportType = "TT0";
			testGLDescriptorPivot2.ReportCategory = "A03";
			AssertNoErrors(testGLDescriptorPivot2.ReportCategoryInfo);

			testGLDescriptorPivot2.ReportCategory = "A01";
			AssertHasError(testGLDescriptorPivot2.ReportCategoryInfo, "The category 'A01' is duplicate in the report type 'TT0' for the current language.");
			testAccGLAccountDescriptor2.AJ_ReportCategory = "A03";

			AccGLAccountDescriptor testAccGLAccountDescriptor3 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testAccGLAccountDescriptor3.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor3.AJ_ReportType = "TT0";
			testAccGLAccountDescriptor3.AJ_LocalAccountNumber = "1234.555";
			testAccGLAccountDescriptor3.AJ_AccountDescription = "Description3";

			GLDescriptorPivot testGLDescriptorPivot3 = Factory.NewWithValidTestData<GLDescriptorPivot>();
			testGLDescriptorPivot3.YJ_AJ = testAccGLAccountDescriptor3.PK;
			testGLDescriptorPivot3.YJ_AG = TestGLAccount1.PK;
			testGLDescriptorPivot3.ReportCategory = "A04";
			AssertHasError(testGLDescriptorPivot3.ReportCategoryInfo, "You cannot have more than one GL local Account '1234.555 - Description3' in the report type 'TT0' for the current language.");
		}

		public void TestValidateReportCategoryWontThrowExceiptionWhenDescriptorIsNull()
		{
			var testAccGLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testAccGLAccountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor.AJ_ReportType = "TT0";
			testAccGLAccountDescriptor.AJ_LocalAccountNumber = "1234.555";

			var testAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testAccGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			testAccGLAccountDescriptor1.AJ_ReportType = "TT0";
			testAccGLAccountDescriptor1.AJ_LocalAccountNumber = "1234.556";

			var testGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var testGLDescriptorPivot = Factory.NewWithValidTestData<GLDescriptorPivot>();
			testGLDescriptorPivot.YJ_AJ = testAccGLAccountDescriptor.PK;
			testGLDescriptorPivot.YJ_AG = testGLAccount.PK;
			testGLDescriptorPivot.ReportType = "TT0";
			testGLDescriptorPivot.ReportCategory = "A01";

			var testGLDescriptorPivot1 = Factory.NewWithValidTestData<GLDescriptorPivot>();
			testGLDescriptorPivot1.YJ_AJ = testAccGLAccountDescriptor1.PK;
			testGLDescriptorPivot1.YJ_AG = testGLAccount.PK;
			testGLDescriptorPivot1.ReportType = "TT0";
			testGLDescriptorPivot1.ReportCategory = "A02";

			testAccGLAccountDescriptor1.Delete();
			var validation = new GLDescriptorPivotValidation(testGLDescriptorPivot);
			AssertNoExceptionThrown("Should not throw null referrence exception.", validation.ValidateReportCategory);
		}

		#region Implementation

		protected AccGLHeader TestGLAccount1;
		protected AccGLAccountDescriptor TestAccGLAccountDescriptor0;
		protected AccGLAccountDescriptor TestAccGLAccountDescriptor1;
		protected GLDescriptorPivot TestGLDescriptorPivot0;

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;

			TestAccGLAccountDescriptor0 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestGLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();

			TestAccGLAccountDescriptor0.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor0.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestAccGLAccountDescriptor0.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			TestAccGLAccountDescriptor0.AJ_LocalAccountNumber = "1234.555";
			TestAccGLAccountDescriptor0.ParentGLHeaderPK = TestGLAccount1.PK;

			TestAccGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			TestAccGLAccountDescriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			TestAccGLAccountDescriptor1.AJ_ReportType = "TT0";
			TestAccGLAccountDescriptor1.AJ_LocalAccountNumber = "1234.555";

			TestGLDescriptorPivot0 = Factory.NewWithValidTestData<GLDescriptorPivot>();
			TestGLDescriptorPivot0.YJ_AJ = TestAccGLAccountDescriptor1.PK;
			TestGLDescriptorPivot0.YJ_AG = TestGLAccount1.PK;
			TestGLDescriptorPivot0.ReportType = "TT0";
			TestGLDescriptorPivot0.ReportCategory = "A01";
			Factory.Save();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}
