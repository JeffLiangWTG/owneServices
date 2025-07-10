namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsAdditionalReferenceProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsAdditionalReference();
	}
}
