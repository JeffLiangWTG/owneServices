namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsGuaranteeTypeCTCProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsGuaranteeTypeCTC();
	}
}
