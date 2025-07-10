namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsIncidentCodeType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsIncidentCode;

		public string CodeListType => Constants.UccCodeListTypes.IncidentCode;

		public string DataSource => Constants.UccDataSources.IncidentCode;

		public string XmlDataItemForCode => "Code";
	}
}
