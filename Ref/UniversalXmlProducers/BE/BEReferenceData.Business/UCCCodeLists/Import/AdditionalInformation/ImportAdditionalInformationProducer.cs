using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportAdditionalInformationProducer : UCCImportCodeListProducer<RefCusCodeList>
	{
		public override IUCCImportCodeListDetails CodeListDetail => new ImportAdditionalInformation();

		public override int StartingRow => 4;

		protected override int valueColumn => 5;

		protected override int descriptionENColumn => 6;

		protected override int descriptionFRColumn => 7;

		protected override int descriptionNLColumn => 8;

		protected override int descriptionDEColumn => 9;
	}
}
