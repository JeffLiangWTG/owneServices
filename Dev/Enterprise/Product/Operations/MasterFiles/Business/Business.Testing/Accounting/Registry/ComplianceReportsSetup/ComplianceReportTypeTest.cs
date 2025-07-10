using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportType))]
	class ComplianceReportTypeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateReportType()
		{
			AssertNoErrors("Precondition: Category should not have errors.", BizObj.ReportTypeInfo);

			BizObj.ReportType = "";
			AssertHasError(BizObj.ReportTypeInfo, "Please enter a Report Type.");
			BizObj.ReportType = "ABC";
			AssertNoErrors(BizObj.ReportTypeInfo);
		}

		public void TestValidateReportTypeWithUserDefined()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			var list = new ComplianceReportTypeCollection();
			AssertEquals(ZString.Empty, list.CodesAsString);
			ComplianceReportType complianceReportType = list.AddNew();
			list.RegistryName = "ComplianceReportsSetupsUserDefined";
			AssertNoErrors("Precondition: Category should not have errors.", complianceReportType.ReportTypeInfo);

			complianceReportType.ReportType = "";
			AssertHasError(complianceReportType.ReportTypeInfo, "Please enter a Report Type.");
			complianceReportType.ReportType = "BSH";
			AssertEquals(complianceReportType.RowErrors.ToMessageListString(), "The report type cannot be BSH, P&L, PLM, VAT, SPA, PLA, SSE, TT0, because these are system default Settings.");
			complianceReportType.ReportType = "ABC";
			AssertNoErrors(complianceReportType.ReportTypeInfo);

			ComplianceReportType complianceReportType2 = list.AddNew();

			complianceReportType2.ReportType = "ABC";
			AssertEquals(complianceReportType2.RowErrors.ToMessageListString(), "Duplicate Report Type");
			complianceReportType2.ReportType = "CBA";
			AssertNoErrors(complianceReportType2.ReportTypeInfo);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.ReportType = "";
			BizObj.ClearAllNotifications();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.ReportTypeInfo);
		}

		public void TestConstructionAndProperties()
		{
			ComplianceReportsSetupCategoryCollection reportTypeCategories = new ComplianceReportsSetupCategoryCollection("TT1", Constants.CountryCodes.China);
			ComplianceReportType bizObj = new ComplianceReportType("TT1", "Test Desc", reportTypeCategories, Constants.CountryCodes.China);

			AssertEquals("TT1", bizObj.ReportType);
			AssertEquals("Test Desc", bizObj.ReportTypeDescription);
			AssertEquals("CN", bizObj.CountryCode);
			AssertEquals(reportTypeCategories, bizObj.ReportTypeCategories);
		}
		public void TestConstructionAndAddDefaultReportTypes()
		{
			ComplianceReportsSetupCategoryCollection reportTypeCategories1 = new ComplianceReportsSetupCategoryCollection("TT1", Constants.CountryCodes.China);
			ComplianceReportType bizObj1 = new ComplianceReportType();
			bizObj1.AddDefaultReportTypes("TT1", (NoResString)"Test Desc", reportTypeCategories1, Constants.CountryCodes.China);

			AssertEquals("TT1", bizObj1.ReportType);
			AssertEquals("Test Desc", bizObj1.ReportTypeDescription);
			AssertEquals("CN", bizObj1.CountryCode);
			AssertEquals(reportTypeCategories1, bizObj1.ReportTypeCategories);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceReportType("TT1", (NoResString)"Test Desc", null, Constants.CountryCodes.China);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceReportType();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ComplianceReportType BizObj
		{
			get { return (ComplianceReportType)base.BizObj; }
		}

		#endregion

	}
}
