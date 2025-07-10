namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AP Aged Outstanding Transactions - Summary in Invoice Currency")]
	public class TestAPAgedOutstandingTransactionsSummaryinInvoiceCurrency : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
