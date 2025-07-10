namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Cash Book Deposit Batch Listing")]
	public class TestCashBookDepositBatchListingReport : TemplateTestCase
	{
	}

	public class TestCashBookDepositBatchListingReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return "Cash Book Deposit Batch Listing"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"The Cash Book Listing Report produces a list of Deposit Batches in your Login Company.
For each deposit batch, the report generates an itemized listing of each receipt included in the deposit.
Filter options let you limit the report to deposits for a selected bank account, selected date range, or deposit batch number range.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCashBookDepositBatchListingReport();
		}
	}
}
