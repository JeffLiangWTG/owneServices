namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Job Profit Charge Code Summary for Periods - Iteration by all departments")]
	public class TestJobProfitChargeCodeSummaryforPeriodsIterationbyalldepartments : TemplateTestCase
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
