using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsAdditionalReferenceParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsAdditionalReferenceParseXmlTest()
			: base(
			"RD_NCTS-P5_AdditionalReference.xml",
			new NctsAdditionalReference(),
			47
			)
		{
		}
	}
}
