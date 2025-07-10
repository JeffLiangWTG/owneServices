using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRejectionCodeDestinationExitParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsRejectionCodeDestinationExitParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_RejectionCodeDestinationExit.xml",
				codeListDetail: new NctsRejectionCodeDestinationExitType(),
				resultCount: 4
			)
		{
		}
	}
}
