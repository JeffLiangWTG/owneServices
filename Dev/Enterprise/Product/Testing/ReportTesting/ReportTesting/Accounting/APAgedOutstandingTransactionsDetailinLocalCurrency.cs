namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AP Aged Outstanding Transactions - Detail in Local Currency")]
	public class TestAPAgedOutstandingTransactionsDetailinLocalCurrency : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
