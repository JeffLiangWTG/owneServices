namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsReleaseType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NCTSReleaseTypeCode;

		public string CodeListType => Constants.UccCodeListTypes.ReleaseType;

		public string DataSource => Constants.UccDataSources.ReleaseType;

		public string XmlDataItemForCode => "ReleaseTypeCode";
	}
}
