namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.DocumentEngine.RuntimeOptions;
	using NUnit.Framework;

	[TemplateName("Analysis - Local Client by Period")]
	public class TestAnalysisLocalClientByPeriodReport : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}

		public override bool ReportPassesSortOrderAsParameter
		{
			get
			{
				return true;
			}
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesWithOnlyRequiredFilters()
		{
			//NB By default this tests Job Count, No GorupBy using invoice dates
			PrepareReportForRender();
			((ISingleAccountingPeriodFieldUnitTestHelper)Report.FilterCollection["Period"]).SinglePeriod = 200606;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSPCompilesWithOperationsDates()
		{
			PrepareReportForRender();
			((ISingleAccountingPeriodFieldUnitTestHelper)Report.FilterCollection["Period"]).SinglePeriod = 200606;
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Report By"]).ValueAsStringForSerialisation = "O";
			RunReport();
		}
	}
}
