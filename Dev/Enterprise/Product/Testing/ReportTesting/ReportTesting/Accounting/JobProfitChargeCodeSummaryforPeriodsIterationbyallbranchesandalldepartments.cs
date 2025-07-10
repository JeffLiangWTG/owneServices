namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Job Profit Charge Code Summary for Periods - Iteration by all branches and all departments")]
	public class TestJobProfitChargeCodeSummaryforPeriodsIterationbyallbranchesandalldepartments : TemplateTestCase
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
