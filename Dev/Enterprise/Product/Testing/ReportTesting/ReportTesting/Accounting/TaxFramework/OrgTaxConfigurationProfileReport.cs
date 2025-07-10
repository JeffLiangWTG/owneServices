using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Accounting.TaxFramework
{
	[TemplateName("Organization - AR Tax Configuration Profile")]
	public class AROrgTaxConfigurationProfileReport : TemplateTestCase
	{
	}

	[TemplateName("Organization - AP Tax Configuration Profile")]
	public class APOrgTaxConfigurationProfileReport : TemplateTestCase
	{
	}

	public class OrgTaxConfigurationProfileReport_Receivables : ReportTestCase
	{
		public override string MenuName => "Organization - A/R Tax Configuration Profile";

		public override string Hint => @"The Organization - A/R Tax Configuration Profile report lists the details of each A/R Tax Configuration recorded against Receivables Organizations in your Login Company.";

		public override ZEmbeddedModule ModuleToTest => new Enterprise.MasterFiles.Module.MasterDataReports();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AROrgTaxConfigurationProfileReport();
		}
	}

	public class OrgTaxConfigurationProfileReport_Payables : ReportTestCase
	{
		public override string MenuName => "Organization - A/P Tax Configuration Profile";

		public override string Hint => @"The Organization - A/P Tax Configuration Profile report lists the details of each A/P Tax Configuration recorded against Payables Organizations in your Login Company.";

		public override ZEmbeddedModule ModuleToTest => new Enterprise.MasterFiles.Module.MasterDataReports();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new APOrgTaxConfigurationProfileReport();
		}
	}
}
