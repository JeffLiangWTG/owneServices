using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportTransportDocumentTypeProducer : UCCImportCodeListProducer<RefCusCodeList>
	{
		public override IUCCImportCodeListDetails CodeListDetail => new ImportTransportDocumentType();

		public override int StartingRow => 3;
	}
}
