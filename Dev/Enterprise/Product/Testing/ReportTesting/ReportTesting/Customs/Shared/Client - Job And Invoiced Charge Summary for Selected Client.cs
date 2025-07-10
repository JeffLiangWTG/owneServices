namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class ClientJobAndInvoicedChargesSummaryMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Client - Job And Invoiced Charge Summary for Selected Client"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report will list Forwarding and Declaration jobs for a nominated Client (mandatory filter) if there are revenue charges posted for that client (as debtor).
The report will also isolate charges posted for the 'CDS' ratings group (Customs & Disbursement Charges).
The columns displaying in a report can be customized and saved for an individual client so that each time you run the report for that client the customized layout defaults. Use this report in combination with the Report Scheduling tool in order to regularly update your clients on the status of Forwarding and Declaration jobs handled on their behalf.

Note: This report works for both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.ClientJobAndInvoicedChargesSummaryTemplateTest();
		}
	}
}
