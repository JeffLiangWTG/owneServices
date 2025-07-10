namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Job Profit - All Job Types")]
	public class TestJobProfitAll : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestJobProfitAllJobTypesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - All Job Types"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"This report specifically targets the analysis of Job Profits for invoicing jobs irrespective of the type of operations job they are associated with.

When running the report users can select one of three layouts. The layout style selected determines the type of analysis and level of detail included in the report. Using this report you can now produce reports that analyze Revenue, WIP, Costs, Accruals and Job Profit by:

· Charge Code for each Job
· Totals for each Job
· Transaction Details by Job";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitAll();
		}
	}
}
