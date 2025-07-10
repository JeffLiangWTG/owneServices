namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("NZ GST Summary Report")]
	public class TestNZGSTSummaryReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
