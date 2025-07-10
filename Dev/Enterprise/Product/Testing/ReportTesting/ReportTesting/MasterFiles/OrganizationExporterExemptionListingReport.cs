namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Exporter Exemption Management")]
	public class OrganizationExporterExemptionListingTemplateTest : TemplateTestCase
	{
	}

	public class OrganizationExporterExemptionListingReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Listing of Exporter Exemption Documents"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report will list all 'EXV' document issue in Italy (IT) that expired within the specified date range. This report can optionally be filtered by Organization's Controlling Branch and Document Type.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OrganizationExporterExemptionListingTemplateTest();
		}
	}
}
