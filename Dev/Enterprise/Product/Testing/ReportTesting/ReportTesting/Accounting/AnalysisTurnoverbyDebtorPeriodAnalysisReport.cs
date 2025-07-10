namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Analysis - Turnover by Debtor Period Analysis")]
	public class TestAnalysisTurnoverbyDebtorPeriodAnalysisReport : TemplateTestCase
	{
	}

	public class TestAnalysisTurnoverbyDebtorPeriodAnalysisReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Analysis - Turnover by Debtor Period Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"By Debtor, the Turnover by Debtor Period Analysis report provides a multi-period analysis of transactions posted on each Receivable account.
Note, this report supports a 'rolling' analysis of transactions posted to your Receivables ledger.
After nominating a reporting period and selecting the types of transactions (e.g. INV, CRD, REC) to be analyzed, this report will generate a period by period movement analysis for the 12 months up to and including the reporting period.
Extensive filtering and grouping options allow you to review Debtor activity trends across a wide range of criteria. Use this report to review the pattern of transaction movements across your receivable accounts.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAnalysisTurnoverbyDebtorPeriodAnalysisReport();
		}
	}
}
