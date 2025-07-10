using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsDeclarationTypeParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsDeclarationTypeParseXmlTest()
			: base(
			"RD_NCTS-P5_DeclarationType.xml",
			new NctsDeclarationType(),
			6
			)
		{
		}
	}
}
