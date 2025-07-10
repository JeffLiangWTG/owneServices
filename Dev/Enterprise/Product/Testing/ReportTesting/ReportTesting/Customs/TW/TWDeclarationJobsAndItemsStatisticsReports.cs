using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Customs.TW
{
	[TemplateName("TW Declaration Jobs And Items Statistics")]
	public class TestTWDeclarationJobsAndItemsStatisticsReportTemplate : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportDefaultFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportShipmentTypeAll()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Shipment Type"]).ValueAsStringForSerialisation = "ALL";
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
		public void TestReportCountByCreateUser()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Count by"]).ValueAsStringForSerialisation = "C";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportCountByLastEditUser()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Count by"]).ValueAsStringForSerialisation = "L";
			RunReport();
		}
	}
}
