using Enterprise.DocumentEngine.RuntimeOptions;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Customs.TW
{
	[TemplateName("TW Volume Ranking")]
	public class TestTWVolumeRankingReportTemplate : TemplateTestCase
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
		public void TestReportShipmentTypeTRN()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Shipment Type"]).ValueAsStringForSerialisation = "TRN";
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
		public void TestReportGroupBySupplier()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Supplier";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByImporter()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Importer";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByDeclarationType()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Declaration Type";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByOfficeOfReceipt()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Office of Receipt";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByExportLocation()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Export Location";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByPortOfOrigin()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Port of Origin";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByFinalDestination()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Final Destination";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByCreateUser()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Create User";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByLastEditUser()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Group By"]).ValueAsStringForSerialisation = "Last Edit User";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSortByVolumeOfEntries()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Sort By"]).ValueAsStringForSerialisation = "Volume of Entries";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSortByNetWeight()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Sort By"]).ValueAsStringForSerialisation = "Net Weight";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSortByGrossWeight()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Sort By"]).ValueAsStringForSerialisation = "Gross Weight";
			RunReport();
		}
	}
}
