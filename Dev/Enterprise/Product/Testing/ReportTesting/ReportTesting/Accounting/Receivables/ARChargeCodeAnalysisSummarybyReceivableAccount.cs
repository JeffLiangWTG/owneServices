namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AR Charge Code Analysis - Summary by Receivable Account")]
	public class ARChargeCodeAnalysisSummarybyReceivableAccountTemplateTest : TemplateTestCase
	{
	}

	public class ARChargeCodeAnalysisSummarybyReceivableAccountReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Analysis - Summary by Receivable Account"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Receivables Charge Code Analysis Summary by Receivable Account report is used to analyze total turnover by receivable account and charge code. The report analyses AR  Invoice, Credit Note and Adjustment Note transactions at the Transaction Line / Charge Code level.
Use this report to identify the major revenue streams by Debtor posted to your Receivables Ledger in a selected accounting period or date range.
This report also supports SIX Charge Group Analysis options (Customs, Forwarding, Warehouse, CFS, Transport and Other).  When you select any of the Charge Group Analysis filter options the report will include extra columns that dissect and total charges across charge groups relevant to each charge code.
This optional dissection supports a deeper analysis of you key revenue streams.
By Debtor account, this report will itemize transactions posted within a nominated date range.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ARChargeCodeAnalysisSummarybyReceivableAccountTemplateTest();
		}
	}
}
