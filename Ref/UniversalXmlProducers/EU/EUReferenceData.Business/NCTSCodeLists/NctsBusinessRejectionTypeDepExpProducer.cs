namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsBusinessRejectionTypeDepExpProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsBusinessRejectionTypeDepExp();
	}
}
