namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AR Aged Outstanding Transactions - Detail in Local Currency")]
	public class TestARAgedOutstandingDetailInLocalCurrencyReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
