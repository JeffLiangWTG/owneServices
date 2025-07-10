namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.ReportTesting;

	[TemplateName("Outstanding WIP & ACR Transactions")]
	public class TestOutstandingWIPACRTransactionsReport : TemplateTestCase
	{
	}

	public class TestOutstandingWIPACRTransactionsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Forwarding.Module.ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "WIP Rev & Accrued Costs - Outstanding Transactions"; }
		}

		public override string Hint
		{
			get
			{
				return
						@"The WIP Revenue & Accrued Costs - Outstanding Transactions report itemizes each outstanding WIP and Accrued Cost transaction as at the end of the selected accounting period.  
This report can be used to manage outstanding WIP and Accrual transactions. 
It can be used to substantiate the closing balances of your General Ledger WIP and Accrued Cost Control at the end of each accounting period.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOutstandingWIPACRTransactionsReport();
		}
	}
}
