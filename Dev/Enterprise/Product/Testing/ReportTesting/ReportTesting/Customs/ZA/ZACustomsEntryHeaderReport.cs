using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Customs Entry Header Report")]
	public class TestZACustomsEntryHeaderReportTemplate : TemplateTestCase { }

	public class TestZACustomsEntryHeaderReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Customs Entry Header Report";

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase() => new TestZACustomsEntryHeaderReportTemplate();
	}
}
