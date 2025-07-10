namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	public class TestJobProfitForwardingMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Forwarding & Customs"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"The Job Profit – Forwarding & Customs Report can be used to analyze Revenue, WIP, Costs, Accruals and Profits on Forwarding Shipments, Declarations and Consols.

This menu supports a wide range of freight Job filters (e.g. transport mode, ETA/ETD, carrier, agent).
Other filter options include branch, department, local client, sales rep, transaction posted dates.
The level of summary or detail in the final report is determined by your TEMPLATE SELECTION.

Options include analysis by consol or job at a summary, charge code or transaction detail level.
You can limit the report to only including transactions in a posted date range. The report will then be limited to identifying the MOVEMENT IN PROFIT posted in the selected transaction date range.

NB:  Jobs attached to more than one consol may be listed more than once in the Consol Analysis reports.
NB: This report includes both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Accounting.TestJobProfitForwarding();
		}
	}
}
