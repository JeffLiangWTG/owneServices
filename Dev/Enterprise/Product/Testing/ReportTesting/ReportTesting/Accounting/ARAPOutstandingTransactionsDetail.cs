namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Accounting.Module;

	[TemplateName("Outstanding AR and AP Transactions Listing Report with Ops Ref")]
	public class ARAPOutstandingTransactionsDetailTemplateTest : TemplateTestCase
	{
	}

	public abstract class ARAPOutstandingTransactionsDetailReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Net – AR & AP Outstanding Transactions Detail"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"This report itemizes outstanding Accounts Receivable and Accounts Payable transactions.
By reporting both Payable and the Receivable Ledger outstanding items, the report can be used to identify the net outstanding balance with each AR / AP organization.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ARAPOutstandingTransactionsDetailTemplateTest();
		}
	}
}
