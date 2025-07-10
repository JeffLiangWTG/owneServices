namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Profit Loss Budget Periods Analysis")]
	public class ProfitLossBudgetPeriodsAnalysis : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}

		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}
}
