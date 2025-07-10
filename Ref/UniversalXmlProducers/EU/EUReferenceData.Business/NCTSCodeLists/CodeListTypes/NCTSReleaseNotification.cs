namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NCTSReleaseNotification : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NCTSReleaseNotificationCode;

		public string CodeListType => Constants.UccCodeListTypes.ReleaseNotification;

		public string DataSource => Constants.UccDataSources.ReleaseNotification;

		public string XmlDataItemForCode => "ReleaseNotificationCode";
	}
}
