using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportAdditionalReferenceProducer : UCCImportCodeListProducer<RefCusCodeList>
	{
		public override IUCCImportCodeListDetails CodeListDetail => new ImportAdditionalReference();

		public override int StartingRow => 4;
	}
}
