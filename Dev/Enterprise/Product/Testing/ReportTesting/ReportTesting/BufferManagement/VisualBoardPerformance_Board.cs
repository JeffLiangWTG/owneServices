namespace Enterprise.ReportTesting.BufferManagement
{
	using ZArchitecture.Modules;

	[TemplateName("Visual Board Performance - Board")]
	public class TestVisualBoardPerformance_BoardReport : TemplateTestCase
	{
	}

	public class TestVisualBoardPerformance_BoardReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.BMReports(); }
		}

		public override string MenuName
		{
			get { return "Visual Board Performance - Board"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVisualBoardPerformance_BoardReport();
		}
	}
}
