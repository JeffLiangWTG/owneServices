namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class ImportTransportDocumentTypeProducerTest : UCCImportCodeListProducerAbstractTest<ImportTransportDocumentTypeProducer>
	{
		protected override string CodeListType => Constants.UccCodeListTypes.TransportDocumentType;

		protected override string CodeType => Constants.ZZRefCusCodeList.UccImportTransportDocumentType;

		protected override string DataSource => Constants.DataSources.BeTransportDocuments;

		protected override string UccCodeListName => Constants.UccCodeListNames.TransportDocumentCodes;
	}
}
