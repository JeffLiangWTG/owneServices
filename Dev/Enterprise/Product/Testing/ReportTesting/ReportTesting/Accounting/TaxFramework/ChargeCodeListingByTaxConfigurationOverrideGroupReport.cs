using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Accounting.TaxFramework
{
	[TemplateName("Charge Code Listing by Tax Configuration Override Group Report")]
	public class ChargeCodeListingByTaxConfigurationOverrideGroupTemplateTest : TemplateTestCase
	{
	}

	public class ChargeCodeListingByTaxConfigurationOverrideGroupReportTest : ReportTestCase
	{
		public override string MenuName => "Charge Code Listing by Tax Configuration Override Group";

		public override string Hint => @"The Charge Code Listing by Tax Configuration Override Group Report lists Charge Codes attached to Tax Configuration Override Groups.
Tax Configuration Override Groups define when Tax Transaction records will be created when posting Costs or Revenues.
Use this report to identify Charge Codes linked to Tax Configuration Override Groups.
Only Charge Codes linked to Tax Configuration Override Groups are returned by the report.
Details returned include the Charge Code, linked Tax Configuration Override Groups, and the Tax Configuration, Service Code and Tax Rates (if any) defined against the Tax Framework Configuration tab of each Override group listed.";

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new AccountReports();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ChargeCodeListingByTaxConfigurationOverrideGroupTemplateTest();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "TF=Y");
	}
}
