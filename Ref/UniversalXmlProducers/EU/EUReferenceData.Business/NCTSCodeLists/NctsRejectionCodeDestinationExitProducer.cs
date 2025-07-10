namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsRejectionCodeDestinationExitProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsRejectionCodeDestinationExitType();
	}
}
