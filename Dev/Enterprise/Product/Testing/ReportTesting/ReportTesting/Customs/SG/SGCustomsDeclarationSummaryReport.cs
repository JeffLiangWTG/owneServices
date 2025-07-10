namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("SG Customs Declaration Summary")]
	public class TestSGCustomsDeclarationSummaryReportTemplate : TemplateTestCase
	{
	}

	public class TestSGCustomsDeclarationSummaryReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "SG Customs Declaration Summary"; }
		}

		public override string Hint
		{
			get { return @"This report lists Declarations"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestSGCustomsDeclarationSummaryReportTemplate();
		}
	}
}
