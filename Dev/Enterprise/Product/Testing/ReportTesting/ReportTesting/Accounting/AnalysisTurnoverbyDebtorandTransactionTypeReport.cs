namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Analysis - Turnover by Debtor and Transaction Type")]
	public class TestAnalysisTurnoverbyDebtorandTransactionTypeReport : TemplateTestCase
	{
	}

	public class TestAnalysisTurnoverbyDebtorandTransactionTypeReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Analysis - Turnover by Debtor And Transaction Type"; }
		}

		public override string Hint
		{
			get
			{
				return

@"By Debtor, the Turnover by Debtor and Transaction Type report will itemize the total value of transactions posted for each transaction type.
Use this report to review, in summary, the movement on each Debtor account by transaction type.
Extensive filtering and grouping options are supported by this report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAnalysisTurnoverbyDebtorandTransactionTypeReport();
		}
	}
}
