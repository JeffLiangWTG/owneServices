namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AP Compliance Documents Details by Sub Type and Number")]
	class TestAPComplianceDocumentsDetailsBySubTypeAndNumber : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
