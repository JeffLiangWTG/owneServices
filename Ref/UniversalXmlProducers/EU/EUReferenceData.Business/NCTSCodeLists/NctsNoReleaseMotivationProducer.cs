namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsNoReleaseMotivationProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsNoReleaseMotivationType();
	}
}
