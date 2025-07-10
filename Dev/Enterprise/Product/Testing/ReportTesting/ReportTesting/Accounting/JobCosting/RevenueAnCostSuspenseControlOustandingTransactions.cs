namespace Enterprise.ReportTesting.Accounting
{
	public class RevenueAndCostSuspenseControlOustandingTransactionsMenuItem : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Revenue and Cost Suspense Control - Outstanding Transactions"; }
		}

		public override string Hint
		{
			get { return MenuHint; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new RevenueAndCostSuspenseControlOustandingTransactionsTemplate();
		}

		const string MenuHint = @"The Revenue and Cost Suspense Control  - Outstanding Transactions report itemizes each revenue and cost item held in suspense as at the end of the selected accounting period.  
Transactions in 'suspense' are transactions posted on or before the nominated date/period. 
Those transactions, however, are not recognized in general ledger profit and loss reports until a future period. 
This report can be used to substantiate the closing balances of your General Ledger Revenue Suspense and Cost Suspense Control accounts at the end of each accounting period.";
	}

	[TemplateName("Revenue and Cost Suspense Control - Outstanding Transactions")]
	public class RevenueAndCostSuspenseControlOustandingTransactionsTemplate : TemplateTestCase
	{
	}
}
