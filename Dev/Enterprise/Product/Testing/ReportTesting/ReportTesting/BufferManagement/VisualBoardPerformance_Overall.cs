namespace Enterprise.ReportTesting.BufferManagement
{
	using ZArchitecture.Modules;

	[TemplateName("Visual Board Performance - Overall")]
	public class TestVisualBoardPerformance_OverallReport : TemplateTestCase
	{
	}

	public class TestVisualBoardPerformance_OverallReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.BMReports(); }
		}

		public override string MenuName
		{
			get { return "Visual Board Performance - Overall"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVisualBoardPerformance_OverallReport();
		}
	}
}
