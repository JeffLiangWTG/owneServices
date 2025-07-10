namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("China Posting Report For Bank")]
	public class TestChinaPostingReportForBank : TemplateTestCase
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
