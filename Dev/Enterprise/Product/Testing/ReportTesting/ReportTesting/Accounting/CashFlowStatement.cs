namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using NUnit.Framework;

	[TemplateName("Cash Flow Statement")]
	public class TestCashFlowStatement : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Cash Flow Statement" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
