namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AR Charge Code Analysis - Detail")]
	public class ARChargeCodeAnalysisDetailTemplateTest : TemplateTestCase
	{
	}

	public class ARChargeCodeAnalysisDetailReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Analysis - Detail"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"The Receivables Charge Code Analysis Detail report is used to itemize and analyze turnover by charge code and debtor. This report itemizes the charge lines posted on Receivables Invoice, Credit Note and Adjustment Note transactions.

Use this report to itemize receivables transaction lines by charge code and debtor posted to your Receivables Ledger in a selected accounting period or date range.

The report supports three different layout styles: By Charge Code; By Transaction Type; By Debtor.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ARChargeCodeAnalysisDetailTemplateTest();
		}
	}
}
