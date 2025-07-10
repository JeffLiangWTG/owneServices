namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.ReportTesting;
	using Enterprise.ReportTesting.Accounting;

	public class TestForwardingJobProfitAnalysisReportTestMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Forwarding.Module.ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit – Forwarding and Customs Transaction Detail"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Job Profit – Forwarding and Customs Transaction Detail report can be used to itemize Revenue, WIP, Costs, Accruals and Profit movements  on Forwarding and Customs jobs.  
This menu supports a wide range of freight Job filters (e.g. transport mode, ETA/ETD, carrier, agent).
Other filter options include branch, department, local client, sales rep, transaction posted dates.
This report is a detail report.  It itemizes each recognized transaction line on a job.
Options include analysis by consol or job at a summary, charge code or transaction detail level.
You can limit the report to only including transactions and profit movements recognized in a specific date range.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwardingJobProfitAnalysisReportTest();
		}
	}
}
