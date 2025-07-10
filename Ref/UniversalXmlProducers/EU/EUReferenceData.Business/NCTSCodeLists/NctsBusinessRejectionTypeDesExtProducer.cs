namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsBusinessRejectionTypeDesExtProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsBusinessRejectionTypeDesExtType();
	}
}
