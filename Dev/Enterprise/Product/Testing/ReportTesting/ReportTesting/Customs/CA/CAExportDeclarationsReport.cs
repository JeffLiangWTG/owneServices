namespace Enterprise.ReportTesting.Customs.CA
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("CA Export Declaration Report")]
	public class TestCAExportDeclarationsReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}

	public class TestCAExportDeclarationsReportMenuSetup : ReportTestCase
	{
		public override string MenuName => "CA Export Declaration Report";

		public override string Hint => "This report makes export declaration information available.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCAExportDeclarationsReport();
		}

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}
