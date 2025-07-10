namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Payables - Analysis Turnover Creditor Period Analysis")]
	public class TestPayablesAnalysisTurnoverCreditorPeriodAnalysisTemplate : TemplateTestCase
	{
	}

	public class TestPayablesAnalysisTurnoverCreditorPeriodAnalysisReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Analysis - Turnover by Creditor Period Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return

@"By Creditor, the Turnover by Creditor Period Analysis report provides a multi-period analysis of transactions posted on each Payable account.
Note, this report supports a 'rolling' analysis of transactions posted to your Payables ledger.
After nominating a reporting period and selecting the types of transactions (e.g. INV, CRD, REC) to be analyzed, this report will generate a period by period movement analysis for the 12 months up to and including the reporting period.
Extensive filtering and grouping options allow you to review Creditor activity trends across a wide range of criteria. Use this report to review the pattern of transaction movements across your payable accounts.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPayablesAnalysisTurnoverCreditorPeriodAnalysisTemplate();
		}
	}
}
