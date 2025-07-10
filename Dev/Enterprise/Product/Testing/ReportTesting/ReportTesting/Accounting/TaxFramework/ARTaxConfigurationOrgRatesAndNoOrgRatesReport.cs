using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Accounting.TaxFramework
{
	[TemplateName("Organization - AR Tax Configuration Rates")]
	public class ARTaxConfigurationOrganizationRatesReport : TemplateTestCase
	{
	}

	public class TaxConfigurationOrganizationRatesReport_Receivables : ReportTestCase
	{
		public override string MenuName => "Organization - A/R Tax Configuration Rates";

		public override string Hint => @"The Organization – A/R Tax Configuration Rates report is used to review Tax Configuration rates recorded against Receivables Organizations for AR Ledger Tax Configurations that support organization specific tax rates.
This report supports two optional templates.
The AR Rates template returns all rates recorded against Receivables Organizations with a start date in the nominated date range for the selected AR Ledger Tax Configuration.
The AR No Rate template returns a list of Receivables Organizations assigned the nominated AR Tax Configuration and no Rate with a start date in the nominated date range.
Use this report to identify the rates recorded against Receivables Organizations with a specific Tax Configuration assigned.
This report is relevant in Login Companies using Tax Configurations that permit Organization specific rates.";

		public override ZEmbeddedModule ModuleToTest => new MasterDataReports();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ARTaxConfigurationOrganizationRatesReport();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "TF=Y");
	}
}
