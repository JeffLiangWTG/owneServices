namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Exception Reporting - Jobs with Losses")]
	public class TestExceptionReportingJobsWithLosses : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
