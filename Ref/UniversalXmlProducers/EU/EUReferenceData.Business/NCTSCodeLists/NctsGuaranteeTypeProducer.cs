namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsGuaranteeTypeProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsGuaranteeType();
	}
}
