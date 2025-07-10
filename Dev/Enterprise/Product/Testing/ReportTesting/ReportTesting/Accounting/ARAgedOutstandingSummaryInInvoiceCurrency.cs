namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AR Aged Outstanding Transactions - Summary in Invoice Currency")]
	public class TestARAgedOutstandingSummaryInInvoiceCurrencyReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
