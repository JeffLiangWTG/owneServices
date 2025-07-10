using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsXmlErrorCodesParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsXmlErrorCodesParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_XmlErrorCodes.xml",
				codeListDetail: new NctsXmlErrorCodesType(),
				resultCount: 15
			)
		{
		}
	}
}
