namespace Enterprise.ReportTesting.Customs.ZA
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("ZA Product Listing Report")]
	public class ZAProductListingReportTemplate : TemplateTestCase
	{
	}

	public class ZAProductListingReportTest : ReportTestCase
	{
		public override string MenuName => "ZA Product Listing Report";

		public override string Hint => @"This report lists the current products for South African companies";

		protected override TemplateTestCase GetTemplateTestCase() => new ZAProductListingReportTemplate();

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustFilesReports();
	}
}
