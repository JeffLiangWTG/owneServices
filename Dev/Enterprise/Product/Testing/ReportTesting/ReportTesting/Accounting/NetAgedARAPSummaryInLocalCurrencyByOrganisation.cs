namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Net Aged ARAP Summary In Local Currency By Organisation")]
	public class TestNetAgedARAPSummaryInLocalCurrencyByOrganisation : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
