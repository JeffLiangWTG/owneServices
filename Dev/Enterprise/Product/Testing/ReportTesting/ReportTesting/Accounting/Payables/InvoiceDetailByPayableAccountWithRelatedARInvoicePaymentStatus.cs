namespace Enterprise.ReportTesting.Accounting.Payables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AP Invoices with Related AR Transaction Details")]
	public class InvoiceDetailByPayableAccountWithRelatedARInvoicePaymentStatusTemplateTest : TemplateTestCase
	{
	}

	public class InvoiceDetailByPayableAccountWithRelatedARInvoicePaymentStatusReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Invoice Detail by Payable Account with Related AR Invoice Payment Status"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"This report can be used to identify each AP Invoice, Credit Note or Adjustment Note transaction's job related Receivable Invoices or Credit Notes.
The report lists all transaction lines within each AP Invoice, Credit Note or Adjustment Note matching the AP filters nominated.
Then against each transaction line, the report also identifies any related Accounts Receivable transaction lines and their current Paid / Not paid status.
An Accounts Payable transaction line is ""related"" to an Accounts Receivable transaction line when both lines have the same Job, Charge Code, Line Branch and Line Department.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new InvoiceDetailByPayableAccountWithRelatedARInvoicePaymentStatusTemplateTest();
		}
	}
}
