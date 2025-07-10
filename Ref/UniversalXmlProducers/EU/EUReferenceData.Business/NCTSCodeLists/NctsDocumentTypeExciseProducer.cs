namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsDocumentTypeExciseProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsDocumentTypeExcise();
	}
}
