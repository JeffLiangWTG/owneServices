using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsPreviousDocumentParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsPreviousDocumentParseXmlTest()
			: base(
			"RD_NCTS-P5_PreviousDocumentType.xml",
			new NctsPreviousDocumentType(),
			34
			)
		{
		}
	}
}
