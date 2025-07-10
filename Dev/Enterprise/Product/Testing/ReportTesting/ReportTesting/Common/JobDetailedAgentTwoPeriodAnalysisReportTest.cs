namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Detailed Agent Analysis for two Periods - Forwarding")]
	public class TestJobDetailedAgentTwoPeriodAnalysisReportReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Analysis by Client", "Analysis by Agent and Client" };
		}
	}

	public class TestJobDetailedAgentTwoPeriodAnalysisReportMenuSetupJobCostingModule : ReportTestCase
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
@"For two comparative periods, this report identifies TEU, CBM, KG, Job Counts and Profit by Client. Sorting and Column Customization options mean that you can tailor the analysis to your needs. 

By default the Period 1 and Period 2 date ranges select jobs by Job Open date.
Alternatively, the analysis can use the freight dates associated with each job: Import ETA; Export/Cross Trade ETD.

By default “Client” is the Local Client on the Accounting Job Header (billing tab).
Alternatively, the analysis can define client as Consignee (imp), Consignor (exp) & local client (cross trade).

Layouts:  
Use the ""Analysis by Client"" layout to generate totals by client.
Use the ""Analysis by Agent and Client"" layout to report client performance by Agent.
Note: Import freight will be reported under the Consol Sending Agent; Export and Cross-Trade freight will be reported under the Receiving Agent.

The report is not limited by transaction Post Date. Any Profit calculation includes all transactions on a job.";
			}
		}

		public override string MenuName
		{
			get { return @"Client Summary Two Period Volume & Profit Analysis - Forwarding"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobDetailedAgentTwoPeriodAnalysisReportReport();
		}
	}

	public class TestJobDetailedAgentTwoPeriodAnalysisReportMenuSetupForwardingModule : TestJobDetailedAgentTwoPeriodAnalysisReportMenuSetupJobCostingModule
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
