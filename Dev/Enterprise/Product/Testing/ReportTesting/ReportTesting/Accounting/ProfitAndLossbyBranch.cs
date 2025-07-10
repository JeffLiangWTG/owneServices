namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Profit And Loss by Branch")]
	public class TestProfitAndLossbyBranch : TemplateTestCase
	{
		//This reprot has two data sources, profit/loss data and retained earnings data
		//Because of this reason, this report is not suitable for CSV/XML Export and should not have ColumnHeadings
		protected override bool ReportRequiresColumnHeadings => false;

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
