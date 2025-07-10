namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Bank Reconciliation")]
	public class TestBankReconciliationTemplate : TemplateTestCase
	{
	}

	public class TestBankReconciliationReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return @"Bank Reconciliation"; }
		}

		public override string Hint
		{
			get
			{
				return @"This reporting option generates bank reconciliation documentation for the latest bank reconciliation saved against a nominated bank account.
When running this report you can choose to print any combination of the following: A Summary Bank Reconciliation Report, A Detailed Bank Reconciliation Report itemizing each outstanding item, a Listing of Transactions reconciled / cleared / ticked against the bank statement date.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestBankReconciliationTemplate();
		}
	}
}
