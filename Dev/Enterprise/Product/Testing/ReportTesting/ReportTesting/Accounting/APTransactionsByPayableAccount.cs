namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AP Transactions by Payable Account")]
	public class TestAPTransactionsByPayableAccountTemplate : TemplateTestCase
	{
		public TestAPTransactionsByPayableAccountTemplate()
		{
		}
	}

	public class TestAPTransactionsByPayableAccountReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Transactions by Payable Account"; }
		}

		public override string Hint
		{
			get
			{
				return @"By Creditor account, this report will itemize transactions posted within a nominated date range. 
It will provide totals for each Creditor account.
To run this report, you must at a minimum nominate both a posted date range AND the transaction types to be reported (e.g. INV, CRD, PAY).

There are eleven optional display columns including options to display of Tax (GST/VAT); job references; exchange rates; due dates; payment status.

This report supports analysis of transactions by Creditor, Creditor Group, Consolidation Category, Accounts Relationship, Settlement Group, Transaction Branch, Payment Status, Sales Rep, Customer Service Rep, and Credit Controller.    
 
NOTE: This report will document the movement in the Payables Control account when every transaction type is selected and ONLY the period filter is used.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAPTransactionsByPayableAccountTemplate();
		}
	}
}
