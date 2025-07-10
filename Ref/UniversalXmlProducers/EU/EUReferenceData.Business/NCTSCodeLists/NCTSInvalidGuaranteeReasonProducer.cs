namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NCTSInvalidGuaranteeReasonProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NCTSInvalidGuaranteeReason();
	}
}
