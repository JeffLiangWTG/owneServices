namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsGuaranteeTypeEUNonTIRProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsGuaranteeTypeEUNonTIR();
	}
}
