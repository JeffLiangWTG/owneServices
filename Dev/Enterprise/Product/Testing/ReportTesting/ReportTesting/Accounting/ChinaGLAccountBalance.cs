namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("ChinaGLAccountBalance")]
	public class ChinaGLAccountBalance : TemplateTestCase
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
