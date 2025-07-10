using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsPreviousDocumentUnionGoodsParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsPreviousDocumentUnionGoodsParseXmlTest()
			: base(inputFileName: "RD_NCTS-P5_PreviousDocumentUnionGoods.xml", codeListDetail: new NctsPreviousDocumentUnionGoods(), resultCount: 4)
		{ }
	}
}
