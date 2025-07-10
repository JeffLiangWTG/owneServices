namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("BalanceSheet")]
	public class TestBalanceSheet : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}
	}
}
