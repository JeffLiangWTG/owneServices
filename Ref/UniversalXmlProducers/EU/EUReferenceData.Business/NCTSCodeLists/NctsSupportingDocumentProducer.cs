namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsSupportingDocumentProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsSupportingDocument();
	}
}
