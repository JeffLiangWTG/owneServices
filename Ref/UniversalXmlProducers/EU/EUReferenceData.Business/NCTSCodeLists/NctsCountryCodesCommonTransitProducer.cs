namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCodesCommonTransitProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryCodesCommonTransit();
	}
}
