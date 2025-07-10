using System.Linq;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("CashBookRegisterReport2in1")]
	class TestCashBookRegisterReport : TemplateTestCase
	{
		public void TestSortCondition()
		{
			var sheetForSorting = Report.XlInterface.WorkSheets.First(x => x.SheetName == "Sort");
			AssertContains("CashBookDate, AH_TransactionBelongsToGroup, TransactionSubType, Reference", sheetForSorting.ToString());
			AssertContains("PostDate, AH_TransactionBelongsToGroup, TransactionSubType, Reference", sheetForSorting.ToString());
		}

		public void TestDateSource()
		{
			var expectSql = "AH_TransactionBelongsToGroup FROM Report_CashBookRegister";
			AssertContains(expectSql, Report.XlInterface.WorkSheets[0].ToString());
			AssertContains(expectSql, Report.XlInterface.WorkSheets[1].ToString());
		}
	}

	public class TestCashBookRegisterReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.CashBookReports(); }
		}

		public override string MenuName
		{
			get { return "Cash Book Register Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Cash Book Register report is a listing of all cashbook transactions and deposits. For a selected date range this report will list all Deposits, Payments and Transfers posted to a selected bank account.
 
The filter options assist you in documenting and reconciling the transaction history of each bank account.

Two optional templates are available:
1. Cash Book Register - this is a listing of transactions in the bank account currency
2. Register with Local Equivalent - this lists transactions in both the bank account currency and the local currency equivalent value of each posted transaction.

For each transaction listed, the following dates are identified in the report: Transaction date, Post Date and Bank Statement Reconciled (cleared) date.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCashBookRegisterReport();
		}
	}
}
