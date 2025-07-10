namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Profit Charge Code Summary for Periods")]
	public class TestJobProfitChargeCodeSummaryforPeriods : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}
	}
}
