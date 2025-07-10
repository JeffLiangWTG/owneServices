using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Company - Accounting Tax Configuration Profile Report")]
	public class CompanyAccountingTaxConfigurationProfileTemplateTest : TemplateTestCase
	{
	}

	public class CompanyAccountingTaxConfigurationProfileReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return @"The Tax Configuration Profile report returns a list of Accounting Tax Configurations recorded against Company and Branch records. 
Tax Configurations define the non-VAT/GST type tax systems enabled for use in a Login Company.
Use this report to review the Tax System and Tax Authority configurations recorded against Company and Branch records in your database.
When a Tax Configuration is configured against the Branch record, the report identifies both the relevant Company and Branch of the Tax Configuration.
When a Tax Configuration is configured against the Company record, the report identifies the relevant Company and leaves the Branch Column empty. 
This report is only relevant when Tax Transaction accounting behavior (examples: Perceptions, Retention in Invoice, Standard Payment Basis Retention) is enabled in the Application Version deployed against the database."; }
		}

		public override string MenuName
		{
			get { return "Company - Accounting Tax Configuration Profile Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new CompanyAccountingTaxConfigurationProfileTemplateTest();
		}
	}
}
