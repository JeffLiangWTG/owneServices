namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("AP Compliance Documents by Sub Type and Number")]
	class APComplianceDocumentsBySubTypeAndNumber : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
