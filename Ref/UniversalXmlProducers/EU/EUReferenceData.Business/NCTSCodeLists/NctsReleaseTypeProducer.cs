namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsReleaseTypeProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsReleaseType();
	}
}
