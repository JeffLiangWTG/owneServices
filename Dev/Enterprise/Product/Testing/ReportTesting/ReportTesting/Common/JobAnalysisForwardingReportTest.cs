namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Accounting.Module;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Analysis - Forwarding")]
	public class TestJobAnalysisForwardingReport : TemplateTestCase
	{
	}

	public class TestJobAnalysisForwardingReportMenuSetupForwarding : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Client Summary Analysis – Forwarding"; }
		}

		public override string Hint
		{
			get
			{
				return

@"This report identifies Job, Income & Job Profit totals by Client for two comparative date ranges.

For the nominated date ranges, the report selects jobs that meet the filter criteria. 

By default the report selects jobs by the date the Accounting Job header was Opened.
Alternatively, you can run this analysis using the relevant Freight dates associated with your job: Imports ETA, Exports/Cross Trade ETD. The registration date will be used when ETA/ETD date is empty.

Note: All transactions for a job will be included in the Income & Profit calculations. The report is not limited by recognition date. The report also counts jobs with no transactions.

By default “Client” is the Local Client of each Job.

Alternatively, you can elect to run this analysis by Consignee (imports), Consignor (exports) & local client (for cross trade).

Nominate a Staff Grouping option to measure client performance by staff member e.g. Sales Rep on each Job;Sales Rep on each client’s Staff Assignment.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobAnalysisForwardingReport();
		}
	}

	public class TestJobAnalysisForwardingReportMenuSetupAccounting : TestJobAnalysisForwardingReportMenuSetupForwarding
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new JobCostingReportModule();
			}
		}
	}
}
