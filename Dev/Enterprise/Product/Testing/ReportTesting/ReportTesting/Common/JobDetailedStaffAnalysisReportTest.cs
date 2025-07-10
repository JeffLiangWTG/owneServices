namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Detailed Staff Analysis - Forwarding")]
	public class TestJobDetailedStaffAnalysisForwardingReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Vol&ProfitSummaryByClient-Fwdg" };
		}
	}

	public class TestJobDetailedStaffAnalysisForwardingMenuSetupJobCostingModule : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new JobCostingReportModule(); }
		}

		public override string Hint
		{
			get
			{
				return @"This report identifies Jobs, Volumes and Job Profit totals by Client. 

It supports analysis of TEU, CBM, KG, Job numbers and Profit by client.

For the nominated date range, the report selects all jobs that meet the filter criteria. 

By default the report selects jobs based on the date their Accounting Job header was Opened.
Alternatively, you can run this analysis using the relevant Freight dates associated with your job: Imports ETA, Exports/Cross Trade ETD. The registration date will be used when ETA/ETD date is empty.

Note: Only recognized charges recorded against a job will be included in the Income & Profit calculations. This report is not limited by transaction Post Date.

By default “Client” is the Local Client on the Accounting Job Header (billing tab) of each Job.

Alternatively, you can elect to run this analysis by Consignee (imports), Consignor (exports) & local client (for cross trade).

Nominate a Staff Grouping option to measure client performance by staff member.";
			}
		}

		public override string MenuName
		{
			get { return @"Client Summary Volume & Profit Analysis – Forwarding"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobDetailedStaffAnalysisForwardingReport();
		}
	}

	public class TestJobDetailedStaffAnalysisForwardingMenuSetupForwardingModule : TestJobDetailedStaffAnalysisForwardingMenuSetupJobCostingModule
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new ForwardingReportsModule();
			}
		}
	}
}
