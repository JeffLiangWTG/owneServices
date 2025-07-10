using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsTransportDocumentParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsTransportDocumentParseXmlTest()
			: base(
			"RD_NCTS-P5_TransportDocumentType.xml",
			new NctsTransportDocument(),
			17
			)
		{
		}
	}
}
