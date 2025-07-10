namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("ARAP Aged Summary In Local Currency By Organisation By Book Type")]
	public class TestARAPAgedSummaryInLocalCurrencyByOrganisationByBookType : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
