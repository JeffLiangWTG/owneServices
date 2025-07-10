namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("ARAP Compliance Sequences Voided Number List")]
	public class TestComplianceSequencesVoidedNumberListReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}
}
