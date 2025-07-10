namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using NUnit.Framework;

	[TemplateName("Job Profit Charge Code Summary for Periods - Iteration by all branches")]
	public class TestJobProfitChargeCodeSummaryforPeriodsIterationbyallbranches : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "JobProfitChargeCodeSummaryBr" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}
	}
}
