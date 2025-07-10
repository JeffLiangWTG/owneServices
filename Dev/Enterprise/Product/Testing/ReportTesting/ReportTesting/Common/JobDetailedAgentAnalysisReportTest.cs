namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Detailed Agent Analysis - Forwarding")]
	public class TestJobDetailedAgentAnalysisForwardingReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Analysis by Client then Agent", "Analysis by Agent then Client" };
		}
	}

	public class TestJobDetailedAgentAnalysisForwardingMenuSetupJobCostingModule : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new JobCostingReportModule(); }
		}

		public override string Hint
		{
			get
			{
				return

@"This report identifies TEU, CBM, KG, Job numbers and Profit by Agent. By Consol Agent, it lists Jobs Counts, Volumes and Job Profit totals by Client. 

For the nominated date range, the report selects all jobs that meet the filter criteria. Jobs are then grouped appropriately by Agent: Import freight is recorded against the Consol Sending Agent, Export and Cross-Trade freight is recorded against the Consol Receiving Agent.

By default the report selects jobs based on the date their Accounting Job header was Opened.
Alternatively, the analysis can use the relevant Freight dates associated with each job: Imports ETA, Exports/Cross Trade ETD.

Note: All transactions for a job are included in the Income & Profit calculations. The report is not limited by transaction Post Date. 

By default “Client” is the Local Client on the Accounting Job Header (billing tab) of each Job.

Alternatively, the analysis can define client as Consignee (imports), Consignor (exports) & local client (cross trade).";
			}
		}

		public override string MenuName
		{
			get { return @"Agent Summary Volume & Profit Analysis by Client – Forwarding"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobDetailedAgentAnalysisForwardingReport();
		}
	}

	public class TestJobDetailedAgentAnalysisForwardingMenuSetupForwardingModule : TestJobDetailedAgentAnalysisForwardingMenuSetupJobCostingModule
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
