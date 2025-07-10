using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("AR Aged Outstanding Transactions - Detail in Invoice Currency")]
	public class TestARAgedOutstandingDetailInInvoiceCurrencyReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			((ISingleAccountingPeriodFieldUnitTestHelper)Report.FilterCollection["Period"]).SinglePeriod = 200606;
			RunReport();
		}
	}
}
