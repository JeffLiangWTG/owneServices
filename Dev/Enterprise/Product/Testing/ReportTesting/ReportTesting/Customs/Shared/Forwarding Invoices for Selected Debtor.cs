namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class TestForwardingInvoicesForSelectedDebtorMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Invoicing - Invoices Posted for Selected Debtor"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Invoice Summary Report shows a list of accounting job related AR invoices and Credit Notes posted for a selected Debtor in a given date range.
For each Job Related Receivables Invoice and Credit Note listed, the report also identifies key freight details (e.g. consignee, consignor, shipment/declaration, consol, volume etc).

Note: This report includes both FORWARDING and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.TestForwardingInvoicesForSelectedDebtor();
		}
	}
}
