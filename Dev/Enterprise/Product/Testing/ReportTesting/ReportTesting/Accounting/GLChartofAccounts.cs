namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using NUnit.Framework;

	[TemplateName("GL Chart of Accounts")]
	public class TestGLChartofAccounts : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "GL Chart of Accounts" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
