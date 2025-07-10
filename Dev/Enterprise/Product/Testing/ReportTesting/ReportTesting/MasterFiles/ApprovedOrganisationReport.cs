namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Organisation - Known-Approved-Regulated Organisations")]
	public class TestApprovedOrganisationTemplate : TemplateTestCase
	{
	}

	public class TestApprovedOrganisationReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Known-Approved-Regulated Organizations"; }
		}

		public override string Hint
		{
			get { return "List of Approved/Known Shippers, Account Consignors and Regulated Agents. Includes approval information including any documentary requirements and applicable issue and expiry dates."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestApprovedOrganisationTemplate();
		}
	}
}
