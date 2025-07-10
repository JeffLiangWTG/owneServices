namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GL Summary For China")]
	public class TestGLSummaryCNReport : TemplateTestCase
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
