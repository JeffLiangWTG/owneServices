namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GL Trial Balance Report")]
	public class TestGLTrialBalanceReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			RunReport();
		}
	}
}
