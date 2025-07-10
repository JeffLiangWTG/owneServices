namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Exception Reporting - Jobs with No Posted Revenue or Costs")]
	public class TestExceptionReportingJobsWithNoPostedRevenueOrCosts : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportSPCompilesForBJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "B");
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesForSJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "S");
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesForAllJobTypes()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			SetMultipleChoiceFilterValue("Job Type", "All");
			RunReport();
		}
	}

	public class TestExceptionReportingJobsWithNoPostedRevenueOrCostsMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Exception Reporting - Jobs with No Posted Revenue or Costs"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"This report produces a list of Jobs for your login company that have never had any revenue (REV) or cost (CST) transactions posted.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestExceptionReportingJobsWithNoPostedRevenueOrCosts();
		}
	}
}
