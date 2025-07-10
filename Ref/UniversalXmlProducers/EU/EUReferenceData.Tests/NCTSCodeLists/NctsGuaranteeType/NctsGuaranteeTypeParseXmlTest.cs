using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsGuaranteeTypeParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_GuaranteeType.xml",
				codeListDetail: new NctsGuaranteeType(),
				resultCount: 12)
		{
		}
	}
}
