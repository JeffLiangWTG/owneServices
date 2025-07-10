namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsAdditionalInformationProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsAdditionalInformation();
	}
}
