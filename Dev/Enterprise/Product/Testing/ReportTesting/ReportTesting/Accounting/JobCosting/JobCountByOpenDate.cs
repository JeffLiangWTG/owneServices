using Enterprise.Accounting.Module;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Count by Open Date")]
	public class TestJobCountByOpenDateReport : TemplateTestCase
	{
	}

	public class TestJobCountByOpenDateReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Count by Open Date"; }
		}

		public override string Hint
		{
			get
			{
				return

@"For a nominated Job Opened Date Range, this report will return a count of Accounting Job Headers opened each day.
Totals by Branch and / or Department can be returned, depending on your choice of optional template.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobCountByOpenDateReport();
		}
	}
}
