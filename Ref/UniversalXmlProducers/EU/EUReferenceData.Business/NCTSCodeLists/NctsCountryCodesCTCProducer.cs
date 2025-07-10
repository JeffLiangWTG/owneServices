namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCodesCTCProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryCodesCTC();
	}
}
