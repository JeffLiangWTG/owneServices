namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AR Aged Outstanding Transactions - Summary in Local Currency")]
	public class TestARAgedOutstandingSummaryInLocalCurrencyReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
