namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class ManifestPreviousDocumentTypeProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new ManifestPreviousDocumentType();
	}
}
