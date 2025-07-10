namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Profit Loss Period Analysis")]
	public class TestProfitLossPeriodAnalysis : TemplateTestCase
	{
	}

	public class TestProfitLossPeriodAnalysisMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReports(); }
		}

		public override string MenuName
		{
			get { return "Profit and Loss Periods Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"The 12 Period Profit & Loss Report is a report that lists the movements in the General Ledger's Profit and Loss accounts in a selected financial year. 

For each P&L account selected, this report displays: 
  - net movement in each accounting period in the selected accounting YEAR up to & including the selected accounting Period 
  - the YTD closing balance (i.e cumulative value) of each GL account 
  - DR balances and net DR movements are displayed as NEGATIVE values (in the P&L DR's equate to expenses) 
  - CR balances and net CR movements are displayed as POSITIVE values (in the P&L CR's equate to incomes)

The report can be filtered by Accounting period, Department and Branch.  By default the report only lists those accounts with financial activity. When running this report, you can also choose to include all general ledger accounts, including those with no transactions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestProfitLossPeriodAnalysis();
		}
	}
}
