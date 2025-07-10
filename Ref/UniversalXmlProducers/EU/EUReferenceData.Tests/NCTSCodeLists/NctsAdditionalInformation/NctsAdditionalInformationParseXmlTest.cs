using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsAdditionalInformationParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsAdditionalInformationParseXmlTest()
			: base(
			"RD_NCTS-P5_AdditionalInformation.xml",
			new NctsAdditionalInformation(),
			4
			)
		{
		}
	}
}
