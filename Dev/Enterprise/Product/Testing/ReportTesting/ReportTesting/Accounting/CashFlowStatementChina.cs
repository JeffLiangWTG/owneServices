namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Cash Flow Statement China")]
	public class TestCashFlowStatementChina : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
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
