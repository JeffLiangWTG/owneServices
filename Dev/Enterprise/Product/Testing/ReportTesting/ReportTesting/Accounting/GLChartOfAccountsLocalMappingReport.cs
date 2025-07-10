namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GL Chart Of Accounts Local Mapping Report")]
	public class TestGLChartOfAccountsLocalMappingReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
