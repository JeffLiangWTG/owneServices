namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsRejectionCodeDepartureExportProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsRejectionCodeDepartureExportType();
	}
}
