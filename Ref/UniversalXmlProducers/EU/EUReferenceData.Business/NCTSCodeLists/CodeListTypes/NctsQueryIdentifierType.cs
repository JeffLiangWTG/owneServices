namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsQueryIdentifierType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsQueryIdentifierCodeType;

		public string CodeListType => Constants.UccCodeListTypes.QueryIdentifier;

		public string DataSource => Constants.UccDataSources.QueryIdentifier;

		public string XmlDataItemForCode => "QueryIdentifier";
	}
}
