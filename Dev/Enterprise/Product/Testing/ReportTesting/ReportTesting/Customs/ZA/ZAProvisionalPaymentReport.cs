namespace Enterprise.ReportTesting.Customs.ZA
{
	using Enterprise.Customs.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("ZA Provisional Payment Report")]
	public class ZAProvisionalPaymentReportTemplate : TemplateTestCase
	{
	}

	public class ZAProvisionalPaymentReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();
		public override string MenuName => "Provisional Payment Report";

		public override string Hint => "This report makes ZA Provisional Payment information available.";

		protected override TemplateTestCase GetTemplateTestCase() => new ZAProvisionalPaymentReportTemplate();
	}
}
