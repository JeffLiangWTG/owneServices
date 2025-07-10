namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsXmlErrorCodesProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsXmlErrorCodesType();
	}
}
