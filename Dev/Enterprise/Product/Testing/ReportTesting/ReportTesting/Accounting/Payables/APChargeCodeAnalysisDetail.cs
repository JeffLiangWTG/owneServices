namespace Enterprise.ReportTesting.Accounting.Payables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AP Charge Code Analysis - Detail")]
	public class APChargeCodeAnalysisDetailTemplateTest : TemplateTestCase
	{
	}

	public class APChargeCodeAnalysisDetailReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
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

					@"The Payables Charge Code Analysis Detail report is used to itemize and analyze turnover by charge code and creditor. This report itemizes the charge lines posted on Payables Invoice, Credit Note and Adjustment Note transactions.

Use this report to itemize payables transaction lines by charge code and creditor posted to your Payables Ledger in a selected accounting period or date range.

The report supports three different layout styles: By Charge Code; By Transaction Type; By Creditor.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new APChargeCodeAnalysisDetailTemplateTest();
		}
	}
}
