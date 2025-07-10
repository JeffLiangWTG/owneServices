namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	[TemplateName("Transaction Summary VAT Analysis By Receivable Account")]
	public class TestTransactionSummaryVATAnalysisByReceivableAccount : TemplateTestCase
	{
	}

	public class TransactionSummaryVATAnalysisByReceivableAccountMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Transaction Summary VAT Analysis by Receivable Account"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report can assist in the preparation of your company’s European Community cross-border sales reporting (i.e. EU Recapitulative Statements, EC Sales lists).";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTransactionSummaryVATAnalysisByReceivableAccount();
		}
	}
}
