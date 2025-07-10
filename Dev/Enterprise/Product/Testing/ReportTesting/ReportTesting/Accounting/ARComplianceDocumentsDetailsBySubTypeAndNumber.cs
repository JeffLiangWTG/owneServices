namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TemplateName("AR Compliance Documents Details by Sub Type and Number")]
	class TestARComplianceDocumentsDetailsBySubTypeAndNumber : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();
		}
	}
}
