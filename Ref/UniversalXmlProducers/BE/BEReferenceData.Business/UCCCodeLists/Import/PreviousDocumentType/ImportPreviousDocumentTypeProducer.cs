using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportPreviousDocumentTypeProducer : UCCImportCodeListProducer<RefCusCodeList>
	{
		public override IUCCImportCodeListDetails CodeListDetail => new ImportPreviousDocumentType();

		public override int StartingRow => 3;
	}
}
