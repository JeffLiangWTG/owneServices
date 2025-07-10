using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Customs.TW
{
	[TemplateName("TW Import Tax And Fees Payment List")]
	public class TestTWImportTaxAndFeesPaymentListReportTemplate : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportDefaultFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportTransportModeAll()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transport Mode"]).ValueAsStringForSerialisation = "ALL";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportTransportModeAIR()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transport Mode"]).ValueAsStringForSerialisation = "AIR";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportTransportModeSEA()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transport Mode"]).ValueAsStringForSerialisation = "SEA";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportPaymentTypeAll()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Payment Type"]).ValueAsStringForSerialisation = "All";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportPaymentTypeN5110()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Payment Type"]).ValueAsStringForSerialisation = "N5110";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportPaymentTypeN5111()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Payment Type"]).ValueAsStringForSerialisation = "N5111";
			RunReport();
		}
	}
}
