namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TemplateName("AR Transaction Lines by Compliance Sub Type and Compliance Number")]
	class TestARTransactionLinesByComplianceSubTypesAndNumber : TemplateTestCase
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
