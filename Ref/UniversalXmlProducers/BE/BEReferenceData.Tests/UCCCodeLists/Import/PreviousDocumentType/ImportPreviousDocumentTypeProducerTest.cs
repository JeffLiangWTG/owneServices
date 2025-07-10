namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class ImportPreviousDocumentTypeProducerTest : UCCImportCodeListProducerAbstractTest<ImportPreviousDocumentTypeProducer>
	{
		protected override string CodeListType => Constants.UccCodeListTypes.PreviousDocumentType;

		protected override string CodeType => Constants.ZZRefCusCodeList.UccImportPreviousDocumentType;

		protected override string DataSource => Constants.DataSources.BePreviousDocuments;

		protected override string UccCodeListName => Constants.UccCodeListNames.PreviousDocumentCodesCL214;
	}
}
