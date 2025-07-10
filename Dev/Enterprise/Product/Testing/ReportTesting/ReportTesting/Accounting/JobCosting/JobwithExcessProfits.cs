namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Job Profit - Job with Excess Profits")]
	public class TestJobwithExcessProfitsReportTest : TemplateTestCase
	{
	}

	public class TestJobwithExcessProfitsReportTestMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Exception Reporting - Jobs with Excess Profits"; }
		}

		public override string Hint
		{
			get
			{
				return

@"This report produces a list of Jobs for your login company that have profits in a selected posted date range that are in excess of a nominated ""amount"".
The filter options available in this report mean that you can limit the analysis to profits for a selected Job Branch, Job Department, Transaction Branch, Transaction Department, Local Client, Job Status, Charge Code or Charge Group.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobwithExcessProfitsReportTest();
		}
	}
}
