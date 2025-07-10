namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsRoleOfRequesterType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsRoleOfRequesterCodeType;

		public string CodeListType => Constants.UccCodeListTypes.RoleOfRequester;

		public string DataSource => Constants.UccDataSources.RoleOfRequester;

		public string XmlDataItemForCode => "Code";
	}
}
