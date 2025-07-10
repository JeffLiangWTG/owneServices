using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsQueryIdentifierParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsQueryIdentifierParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_QueryIdentifier.xml",
				codeListDetail: new NctsQueryIdentifierType(),
				resultCount: 4)
		{
		}
	}
}
