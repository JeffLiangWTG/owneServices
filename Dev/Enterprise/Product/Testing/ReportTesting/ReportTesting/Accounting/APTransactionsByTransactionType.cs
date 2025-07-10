namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AP Transactions By Transaction Type")]
	public class TestAPTransactionsByTransactionType : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}

	public class TestAPTransactionsbyTransactionTypeReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Transactions by Transaction Type"; }
		}

		public override string Hint
		{
			get
			{
				return @"By Transaction Type, this report will itemize transactions posted within a nominated date range. 
It will provide totals for each Transaction Type.
To run this report, you must at a minimum nominate both a posted date range AND the transaction types to be reported (e.g. INV, CRD, REC).

There are eleven optional display columns including options to display of Tax (GST/VAT); job references; exchange rates; due dates; payment status.
 
NOTE: This report will document the movement in the Payables Control account when every transaction type is selected and ONLY the period filter is used.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAPTransactionsByTransactionType();
		}
	}
}
