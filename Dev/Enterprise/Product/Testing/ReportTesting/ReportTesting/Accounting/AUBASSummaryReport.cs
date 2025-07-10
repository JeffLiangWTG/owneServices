namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AU BAS Summary Report")]
	public class TestAUBASSummaryReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
