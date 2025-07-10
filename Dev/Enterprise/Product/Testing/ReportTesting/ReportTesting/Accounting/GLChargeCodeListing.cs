namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using NUnit.Framework;

	[TemplateName("GL Charge Code Listing")]
	public class TestGLChargeCodeListing : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
