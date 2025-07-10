using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeCTCParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsGuaranteeTypeCTCParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_GuaranteeTypeCTC.xml",
				codeListDetail: new NctsGuaranteeTypeCTC(),
				resultCount: 10)
		{
		}
	}
}
