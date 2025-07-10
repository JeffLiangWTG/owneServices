namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("SG Customs Major Exporter Scheme - Inward Summary Report")]
	public class TestSGCustomsMajorExporterSchemeInwardSummaryReportTemplate : TemplateTestCase
	{
	}

	public class TestSGCustomsMajorExporterSchemeInwardSummaryReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "SG Customs Major Exporter Scheme - Inward Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"The report produces a listing of Singapore Customs Declarations that fall under the Major Exporter Scheme.
For a specified date range the report identifies Major Exporter Scheme declaration details.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestSGCustomsMajorExporterSchemeInwardSummaryReportTemplate();
		}
	}
}
