namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsPreviousDocumentUnionGoodsProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsPreviousDocumentUnionGoods();
	}
}
