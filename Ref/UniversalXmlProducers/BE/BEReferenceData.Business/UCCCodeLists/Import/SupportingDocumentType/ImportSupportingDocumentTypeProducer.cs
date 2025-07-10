using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportSupportingDocumentTypeProducer : UCCImportCodeListProducer<RefCusCodeList>
	{
		public override IUCCImportCodeListDetails CodeListDetail => new ImportSupportingDocumentType();

		public override int StartingRow => 4;
	}
}
