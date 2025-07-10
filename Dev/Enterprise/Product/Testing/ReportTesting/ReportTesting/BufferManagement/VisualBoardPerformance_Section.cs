namespace Enterprise.ReportTesting.BufferManagement
{
	using ZArchitecture.Modules;

	[TemplateName("Visual Board Performance - Section")]
	public class TestVisualBoardPerformance_SectionReport : TemplateTestCase
	{
	}

	public class TestVisualBoardPerformance_SectionReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.BMReports(); }
		}

		public override string MenuName
		{
			get { return "Visual Board Performance - Section"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVisualBoardPerformance_SectionReport();
		}
	}
}
