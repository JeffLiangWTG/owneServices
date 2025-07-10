using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Customs Entry Lines Report")]
	public class TestZACustomsEntryLinesReportTemplate : TemplateTestCase { }

	public class TestZACustomsEntryLinesReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Customs Entry Lines Report";

		public override string Hint => "Customs Entry Line details Report";

		protected override TemplateTestCase GetTemplateTestCase() => new TestZACustomsEntryLinesReportTemplate();
	}
}
