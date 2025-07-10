namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsIncidentCodeProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsIncidentCodeType();
	}
}
