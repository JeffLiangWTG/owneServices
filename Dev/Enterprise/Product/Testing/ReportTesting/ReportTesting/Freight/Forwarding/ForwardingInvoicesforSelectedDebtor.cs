namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Forwarding Invoices for Selected Debtor")]
	public class TestForwardingInvoicesForSelectedDebtor : TemplateTestCase
	{
	}

	public class TestForwardingInvoicesForSelectedDebtorMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
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

@"The Invoicing - Invoices Posted for Selected Debtor Report shows a list of job related AR invoices and Credit Notes posted for a selected Debtor in a given date range.

For each Job Related Receivables Invoice and Credit Note listed, the report also identifies key freight details (e.g. consignee, consignor, shipment, consol, volume etc).";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwardingInvoicesForSelectedDebtor();
		}
	}
}
