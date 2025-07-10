using Enterprise.Accounting.Module;

namespace Enterprise.ReportTesting.Accounting.Receivables
{
	[TemplateName("AR Outstanding Balances Currency Summary")]
	public class AROutstandingBalancesCurrencySummaryTemplateTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}

	public class AROutstandingBalancesCurrencySummaryReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Outstanding Balances Currency Summary"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"The Outstanding Balances Currency Summary report identifies the total receivables balance outstanding by currency at the end of a selected accounting period.

The report supports the management of exchange exposure at the end of each accounting period.  The report identifies the average exchange rate at which receivable balances are carried in the subsidiary ledger. Users can then compare this historical book value to their end of accounting period rates.  

This report can be used as a working paper from which to calculate exchange provisions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AROutstandingBalancesCurrencySummaryTemplateTest();
		}
	}
}
