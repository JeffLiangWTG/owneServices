namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsNationalityProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsNationality();
	}
}
