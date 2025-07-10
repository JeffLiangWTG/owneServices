namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class ImportAdditionalReferenceProducerTest : UCCImportCodeListProducerAbstractTest<ImportAdditionalReferenceProducer>
	{
		protected override string CodeListType => Constants.UccCodeListTypes.AdditionalReference;

		protected override string CodeType => Constants.ZZRefCusCodeList.UccImportAdditionalReference;

		protected override string DataSource => Constants.DataSources.BeAdditionalReference;

		protected override string UccCodeListName => Constants.UccCodeListNames.AdditionalReferenceCodes;
	}
}
