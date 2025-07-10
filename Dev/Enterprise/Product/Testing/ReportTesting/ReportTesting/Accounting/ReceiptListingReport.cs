namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Receipt Listing Report")]
	public class TestReceiptListingReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
