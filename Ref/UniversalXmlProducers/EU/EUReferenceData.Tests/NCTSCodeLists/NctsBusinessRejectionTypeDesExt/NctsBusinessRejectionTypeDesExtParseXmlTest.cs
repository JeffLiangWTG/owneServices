using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsBusinessRejectionTypeDesExtParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsBusinessRejectionTypeDesExtParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_BusinessRejectionTypeDesExt.xml",
				codeListDetail: new NctsBusinessRejectionTypeDesExtType(),
				resultCount: 2
			)
		{
		}
	}
}
