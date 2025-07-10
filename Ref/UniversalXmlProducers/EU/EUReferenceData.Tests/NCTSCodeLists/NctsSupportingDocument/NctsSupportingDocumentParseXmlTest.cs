using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsSupportingDocumentParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsSupportingDocumentParseXmlTest()
			: base(
			"RD_NCTS-P5_SupportingDocumentType.xml",
			new NctsSupportingDocument(),
			31
			)
		{
		}
	}
}
