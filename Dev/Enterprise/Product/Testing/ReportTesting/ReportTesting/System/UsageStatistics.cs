namespace Enterprise.ReportTesting.SystemTest
{
	[TemplateName("Resource Efficiency Statistics Report")]
	public class TestUsageStatisticsReport : TemplateTestCase
	{
	}

	public class UsageStatisticsReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.UserAdminReports(); }
		}

		public override string MenuName
		{
			get { return "Resource Efficiency Statistics"; }
		}

		public override string Hint
		{
			get { return "The Resource Efficiency Statistics report shows a listing of Statistic data gathered by service task."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUsageStatisticsReport();
		}
	}
}
