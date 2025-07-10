namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("ARAP Pending Input and Output Tax Outstanding Transactions")]
	public class TestARAPPendingInputandOutputTaxOutstandingTransactions : TemplateTestCase
	{
	}

	public class TestARAPPendingInputandOutputTaxOutstandingTransactionsMenuSetupReceivables : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.ReceivReports();
			}
		}

		public override string MenuName
		{
			get { return "Pending Input and Output Tax Outstanding Transactions"; }
		}

		public override string Hint
		{
			get
			{
				return

@"This report details unrealized VAT/GST Type tax treatments of Revenue and Cost Charge Lines.
This report identifies Revenue and Cost charge lines on a VAT/GST Cash Basis that are outstanding (not matched) as at a selected date or accounting period.

When 'Cash Basis' Receivables and Payables Charge lines are posted, GST/VAT amounts recorded against the charge line are posted to the General Ledger 'Pending Output Tax Control' and 'Pending Input Tax Control' accounts respectively. Then, when Invoices are subsequently matched, appropriate General Ledger posting events occur moving VAT/GST values from the relevant Pending Output/Input Control accounts to the appropriate Reportable Output / Input VAT/GST Tax Control Accounts.

Use this report to understand, analyze and monitor VAT/GST Pending Output Tax and Pending Input Tax balances.
The report can be used to substantiate the closing balances of General Ledger Pending Output Tax and Pending Input Tax Control accounts.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestARAPPendingInputandOutputTaxOutstandingTransactions();
		}
	}

	public class TestARAPPendingInputandOutputTaxOutstandingTransactionsMenuSetupPayables : TestARAPPendingInputandOutputTaxOutstandingTransactionsMenuSetupReceivables
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.PayablesReports();
			}
		}
	}

	public class TestARAPPendingInputandOutputTaxOutstandingTransactionsMenuSetupGeneralLedger : TestARAPPendingInputandOutputTaxOutstandingTransactionsMenuSetupReceivables
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.GLReports();
			}
		}
	}
}
