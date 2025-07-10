namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Analysis - Turnover by Debtor with Prior Year Comparison")]
	public class TestAnalysisTurnoverbyDebtorwithPriorYearComparisonReport : TemplateTestCase
	{
	}

	public class TestAnalysisTurnoverbyDebtorwithPriorYearComparisonReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Analysis - Turnover by Debtor with Prior Year Comparison"; }
		}

		public override string Hint
		{
			get
			{
				return @"Turnover by Debtor with Prior Year Comparison lists, by debtor, the total turnover value of all the transaction types selected. The report shows values for the selected accounting period and year-to-date, with the prior year same period, prior year YTD and prior year total comparison. This is a local currency report, values reported are the local currency equivalent values of all transactions. Extensive filtering, sorting and grouping options are provided to allow Debtor activity analysis by a wide range of criteria.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAnalysisTurnoverbyDebtorwithPriorYearComparisonReport();
		}
	}
}
