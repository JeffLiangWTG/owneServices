namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsTransportDocumentProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsTransportDocument();
	}
}
