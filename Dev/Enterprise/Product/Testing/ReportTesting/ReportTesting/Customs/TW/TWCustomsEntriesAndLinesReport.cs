using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Customs.TW
{
	[TemplateName("TW Customs Entries And Lines")]
	public class TWCustomsEntriesAndLinesReportTemplate : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportDefaultFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportEntryNumber()
		{
			PrepareReportForRender();
			((TextField)Report.FilterCollection["Entry Number"]).ValueAsStringForSerialisation = "EN96944490";
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
		public void TestReportShipmentTypeIMP()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Shipment Type"]).ValueAsStringForSerialisation = "IMP";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportShipmentTypeEXP()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Shipment Type"]).ValueAsStringForSerialisation = "EXP";
			RunReport();
		}
	}
}
