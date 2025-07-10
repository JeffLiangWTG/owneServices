namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsFunctionalErrorCodesIeCAProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsFunctionalErrorCodesIeCA();
	}
}
