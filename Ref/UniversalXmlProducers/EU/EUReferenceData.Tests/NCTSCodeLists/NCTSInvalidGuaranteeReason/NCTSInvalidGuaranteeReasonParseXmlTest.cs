using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NCTSInvalidGuaranteeReasonParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NCTSInvalidGuaranteeReasonParseXmlTest() : base(
				inputFileName: "RD_NCTS-P5_InvalidGuaranteeReason.xml",
				codeListDetail: new NCTSReleaseNotification(),
				resultCount: 12)
		{
			
		}
	}
}
