namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Cash Flow Modified GL Account")]
	public class TestCashFlowModifiedGLAccount : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
