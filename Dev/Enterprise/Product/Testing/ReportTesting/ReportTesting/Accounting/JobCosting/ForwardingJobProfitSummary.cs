namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Forwarding Job Profit Summary")]
	public class TestForwardingJobProfitSummaryReportTest : TemplateTestCase
	{
	}

	public class TestForwardingJobProfitSummaryReportTestMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Forwarding and Customs Summary"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Job Profit - Forwarding and Customs Summary by Job report can be used to review Revenue, WIP, Costs, Accruals and Profit movements on Forwarding and Customs jobs.  
This menu supports a wide range of freight Job filters (e.g. transport mode, ETA/ETD, carrier, agent).
Other filter options include branch, department, local client, sales rep, transaction posted dates.
This report is summarized. It identifies total movements by job.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwardingJobProfitSummaryReportTest();
		}
	}
}
