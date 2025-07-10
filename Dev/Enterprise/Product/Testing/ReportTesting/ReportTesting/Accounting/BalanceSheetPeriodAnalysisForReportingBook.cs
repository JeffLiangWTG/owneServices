using NUnit.Framework;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Balance Sheet - Period Analysis For Reporting book")]
	public class TestBalanceSheetPeriodAnalysisForReportingBook : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
