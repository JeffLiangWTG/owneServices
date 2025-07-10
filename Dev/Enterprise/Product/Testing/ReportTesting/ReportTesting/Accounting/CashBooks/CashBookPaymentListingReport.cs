namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Cash Book Payment Listing")]
	public class TestCashBookPaymentListingReport : TemplateTestCase
	{
	}

	public class TestCashBookPaymentListingReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return "Cash Book Payment Listing Report"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"Use the Cash Book Payment Listing report to detail payments by Bank Account and Payment Method.
This report generates a listing of all PAY and DPY transactions in your login company with posted dates falling within a nominated date range.
Additional filter options also let you limit the report to only listing transactions for a selected bank account, transaction type (DPY or PAY), payment method (e.g. CHQ, EFT), or cheque book.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCashBookPaymentListingReport();
		}
	}
}
