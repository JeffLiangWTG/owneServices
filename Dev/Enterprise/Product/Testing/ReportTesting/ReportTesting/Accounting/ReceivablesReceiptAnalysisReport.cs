namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Receivables Receipt Analysis Report")]
	class TestReceivablesReceiptAnalysisReport : TemplateTestCase
	{
	}

	public class TestReceivablesReceiptAnalysisReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Receivables Receipt Analysis Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report analyses AR Receipt (REC) transactions.
For a selected Post Date Range, this report lists Receipts posted in your Receivables ledger.
For each Receipt listed the report will also identify the transactions paid (matched) by that receipt.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestReceivablesReceiptAnalysisReport();
		}
	}
}
