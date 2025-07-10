namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsPreviousDocumentProducer : NctsManyToOneCodeListXMLProducer
	{
		protected override IUCCExportCodeListDetail[] CodeListDetails => new IUCCExportCodeListDetail[] { new NctsPreviousDocumentType(), new NctsPreviousDocumentExportType() };
	}
}
