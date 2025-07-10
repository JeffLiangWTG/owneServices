namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class ImportSupportingDocumentTypeProducerTest : UCCImportCodeListProducerAbstractTest<ImportSupportingDocumentTypeProducer>
	{
		protected override string CodeListType => Constants.UccCodeListTypes.SupportingDocumentType;

		protected override string CodeType => Constants.ZZRefCusCodeList.UccImportSupportingDocumentType;

		protected override string DataSource => Constants.DataSources.BeSupportingDocuments;

		protected override string UccCodeListName => Constants.UccCodeListNames.SupportingDocumentCodes;
	}
}
