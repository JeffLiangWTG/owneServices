namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Cash Book Reconciled Items By Statement Date")]
	public class TestCashBookReconciledItemsByStatementDateTemplate : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestCashBookReconciledItemsByStatementDateReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return "Cash Book Reconciled Items By Statement Date"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"The Cash Book Reconciled Items by Statement Date report allows you to generate a list of items cleared (ticked) through your Bank Reconciliation module.
This report identifies both the Statement date you cleared the transaction against, and the transaction's original post date. Use this report to generate a list of items that have cleared your bank account.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCashBookReconciledItemsByStatementDateTemplate();
		}
	}
}
