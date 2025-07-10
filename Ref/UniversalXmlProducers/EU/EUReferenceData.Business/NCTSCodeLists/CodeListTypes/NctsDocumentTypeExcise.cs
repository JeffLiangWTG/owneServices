namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsDocumentTypeExcise : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsDocumentTypeExcise;

		public string CodeListType => Constants.UccCodeListTypes.DocumentTypeExcise;

		public string DataSource => Constants.UccDataSources.DocumentTypeExcise;

		public string XmlDataItemForCode => "PreviousDocumentTypeCode";
	}
}
