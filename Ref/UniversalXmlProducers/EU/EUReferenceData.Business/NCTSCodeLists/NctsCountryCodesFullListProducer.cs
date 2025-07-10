namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCodesFullListProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsCountryCodesFullList();
	}
}
