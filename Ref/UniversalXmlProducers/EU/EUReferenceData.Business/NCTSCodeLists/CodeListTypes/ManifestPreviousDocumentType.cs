namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class ManifestPreviousDocumentType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsP5PreviousDocumentCode;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentType;

		public string DataSource => Constants.UccDataSources.PreviousDocumentsManifest;

		public string XmlDataItemForCode => "PreviousDocumentTypeCode";
	}
}
