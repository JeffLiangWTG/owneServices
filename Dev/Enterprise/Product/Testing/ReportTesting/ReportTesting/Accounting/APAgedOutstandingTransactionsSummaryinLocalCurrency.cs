namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AP Aged Outstanding Transactions - Summary in Local Currency")]
	public class TestAPAgedOutstandingTransactionsSummaryinLocalCurrency : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
