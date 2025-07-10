namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AR Invoices with Related AP Transaction Details")]
	public class InvoiceDetailByReceivableAccountWithRelatedAPInvoicePaymentStatusTemplateTest : TemplateTestCase
	{
	}

	public class InvoiceDetailByReceivableAccountWithRelatedAPInvoicePaymentStatusReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Invoice Detail by Receivable Account with Related AP Invoice Payment Status"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"This report can be used to identify each AR Invoice, Credit Note or Adjustment Note transaction's job related Payable Invoices or Credit Notes.
The report lists all transaction lines within each AR Invoice, Credit Note or Adjustment Note matching the AR filters nominated.
Then against each transaction line, the report also identifies any related Accounts Payable transaction lines and their current Paid / Not paid status.
An Accounts Receivable transaction line is ""related"" to an Accounts Payable transaction line when both lines have the same Job, Charge Code, Line Branch and Line Department.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new InvoiceDetailByReceivableAccountWithRelatedAPInvoicePaymentStatusTemplateTest();
		}
	}
}
