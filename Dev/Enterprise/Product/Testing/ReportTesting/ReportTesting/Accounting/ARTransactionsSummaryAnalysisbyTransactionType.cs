namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AR Transactions Summary Analysis by Transaction Type")]
	public class TestARTransactionsSummaryAnalysisbyTransactionType : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
