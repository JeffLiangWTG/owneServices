using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsNoReleaseMotivationParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsNoReleaseMotivationParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_NoReleaseMotivation.xml",
				codeListDetail: new NctsNoReleaseMotivationType(),
				resultCount: 12
			)
		{
		}
	}
}
