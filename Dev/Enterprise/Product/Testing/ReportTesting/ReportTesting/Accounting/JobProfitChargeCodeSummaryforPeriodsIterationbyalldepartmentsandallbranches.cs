namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Job Profit Charge Code Summary for Periods - Iteration by all departments and all branches")]
	public class TestJobProfitChargeCodeSummaryforPeriodsIterationbyalldepartmentsandallbranches : TemplateTestCase
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
