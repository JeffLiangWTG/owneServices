namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Job Profit - Forwarding")]
	public class TestJobProfitForwarding : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesForBJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "B");
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesForSJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "S");
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesForAllJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "All");
			RunReport();
		}
	}

	public class TestJobProfitForwardingMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Forwarding"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"The Job Profit – Forwarding Report can be used to analyze Revenue, WIP, Costs, Accruals and Profits on Forwarding Shipments, Declarations and Consols.

This menu supports a wide range of freight Job filters (e.g. transport mode, ETA/ETD, carrier, agent).

Other filter options include branch, department, local client, sales rep, transaction posted dates.

The level of summary or detail in the final report is determined by your TEMPLATE SELECTION.

Options include analysis by consol or job at a summary, charge code or transaction detail level.

You can limit the report to only including transactions in a posted date range. The report will then be limited to identifying the MOVEMENT IN PROFIT posted in the selected transaction date range.

NB:  Jobs attached to more than one consol may be listed more than once in the Consol Analysis reports.

NB:  The Job Profit - Forwarding reports list the financial profit recorded against the Job Invoicing Tab attached to forwarding shipments";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitForwarding();
		}
	}
}
