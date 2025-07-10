namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GL Trial Balance Mulitlingual  Report")]
	public class TestGLTrialBalanceMulitlingualReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
