namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCodesCommunityProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryCodesCommunityType();
	}
}
