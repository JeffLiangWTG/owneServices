using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsPreviousDocumentUnionGoodsProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsPreviousDocumentUnionGoodsProducer>
	{
		protected override NctsPreviousDocumentUnionGoodsProducer Producer => new NctsPreviousDocumentUnionGoodsProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_PreviousDocumentUnionGoods.zip";

		protected override string InputFileName => "RD_NCTS-P5_PreviousDocumentUnionGoods.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL178.xml";
	}
}
