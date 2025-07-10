namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Job Profit - MAWB Stock")]
	public class TestJobProfitMAWBStock : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestJobProfitMAWBStockMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - MAWB Stock"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report specifically targets the analysis of Job Profits for invoicing jobs attached directly to MAWB Stock. It supports an extensive range of freight and invoicing job filters as well as three layout options.

The layout style selected when running the report determines the type of analysis and level of detail included in the report. Using this report you can now produce reports that analyze Revenue, WIP, Costs, Accruals and Job Profit by:

· Charge Code for each Job
· Totals for each Job
· Transaction Details by Job

";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitMAWBStock();
		}
	}
}
