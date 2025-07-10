using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsReleaseTypeParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsReleaseTypeParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_ReleaseType.xml",
				codeListDetail: new NctsReleaseType(),
				resultCount: 2)
		{
		}
	}
}
