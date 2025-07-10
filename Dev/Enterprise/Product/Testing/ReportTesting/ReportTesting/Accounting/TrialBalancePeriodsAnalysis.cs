namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Trial Balance Periods Analysis")]
	public class TestTrialBalancePeriodsAnalysis : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
