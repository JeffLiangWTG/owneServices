using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeEUNonTIRParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsGuaranteeTypeEUNonTIRParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_GuaranteeTypeEUNonTIR.xml",
				codeListDetail: new NctsGuaranteeTypeEUNonTIR(),
				resultCount: 9)
		{
		}
	}
}
