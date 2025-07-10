using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRejectionCodeDepartureExportParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsRejectionCodeDepartureExportParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_RejectionCodeDepartureExport.xml",
				codeListDetail: new NctsRejectionCodeDepartureExportType(),
				resultCount: 3
			)
		{
		}
	}
}
