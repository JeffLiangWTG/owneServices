namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("ARAP Transactions Summary By Organization And Country")]
	public class TestARAPTransactionsSummaryByOrganizationAndCountry : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
