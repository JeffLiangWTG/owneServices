namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AP Transactions by Compliance Sub Type and Compliance Number")]
	class TestTransactionsByComplianceSubTypeAndNumber : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
