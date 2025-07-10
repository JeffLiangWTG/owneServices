namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Bank Reconciliation - Last Reconciliation Dates")]
	public class TestBankReconciliationLastReconciliationDates : TemplateTestCase
	{
	}

	public class TestBankReconciliationLastReconciliationDatesMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return "Bank Reconciliation - Last Reconciliation Dates"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"Use this report to quickly identify the current bank reconciliation status of each bank account active in your login company.
This report will list ALL active bank accounts in your login company.
For each active bank account this report will identify the last general ledger post date, last bank statement date and last bank statement closing balance used in the bank reconciliation of each account.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestBankReconciliationLastReconciliationDates();
		}
	}
}
