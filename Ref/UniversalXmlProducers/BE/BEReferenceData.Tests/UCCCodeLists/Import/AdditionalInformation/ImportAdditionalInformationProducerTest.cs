namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class ImportAdditionalInformationProducerTest : UCCImportCodeListProducerAbstractTest<ImportAdditionalInformationProducer>
	{
		protected override string CodeListType => Constants.UccCodeListTypes.AdditionalInformation;

		protected override string CodeType => Constants.ZZRefCusCodeList.UccImportAdditionalInformation;

		protected override string DataSource => Constants.DataSources.BeAdditionalInformation;

		protected override string UccCodeListName => Constants.UccCodeListNames.AdditionalInformationCodes;
	}
}
