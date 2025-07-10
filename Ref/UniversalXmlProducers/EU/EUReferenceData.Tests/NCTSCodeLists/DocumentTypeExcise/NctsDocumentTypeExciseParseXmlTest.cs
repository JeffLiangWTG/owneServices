using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.DocumentTypeExcise
{
	sealed class NctsDocumentTypeExciseParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsDocumentTypeExciseParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_DocumentTypeExcise.xml",
				codeListDetail: new NctsDocumentTypeExcise(),
				resultCount: 2
			)
		{
		}
	}
}
