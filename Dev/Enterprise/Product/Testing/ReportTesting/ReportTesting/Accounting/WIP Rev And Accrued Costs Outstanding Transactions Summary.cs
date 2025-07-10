namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Outstanding WIP And ACR Transactions Summary")]
	public class TestWIPRevAndAccruedCostsOutstandingTransactionsSummary : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestWIPRevAndAccruedCostsOutstandingTransactionsSummaryMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "WIP Rev & Accrued Costs - Outstanding Transactions Summary"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"The WIP Revenue & Accrued Costs - Outstanding Transactions Summary report identifies the total value of outstanding WIP and Accrued Cost transactions by Charge Code, as at the end of a selected accounting period.  
This report can be used to manage outstanding WIP and Accrual balances.
It can be used to substantiate the closing balances of your General Ledger WIP and Accrued Cost Control accounts at the end of each accounting period. 
Group by options in the report allow you to summarize outstanding balances by Charge Code, Transaction Branch, Transaction Department, and by combination of Transaction Branch and Transaction Department.";
			}
		}
		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWIPRevAndAccruedCostsOutstandingTransactionsSummary();
		}
	}
}
