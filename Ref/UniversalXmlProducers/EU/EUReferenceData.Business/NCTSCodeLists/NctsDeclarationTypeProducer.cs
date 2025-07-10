namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsDeclarationTypeProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NctsDeclarationType();
	}
}
