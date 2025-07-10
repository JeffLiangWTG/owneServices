namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AR Transactions by Compliance Sub Type and Compliance Number")]
	public class TestTransactionsByComplianceSubTypeAndNumberReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
