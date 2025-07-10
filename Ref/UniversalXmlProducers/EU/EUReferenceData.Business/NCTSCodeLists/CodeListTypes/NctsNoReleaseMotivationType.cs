namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsNoReleaseMotivationType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsNoReleaseMotivationCode;

		public string CodeListType => Constants.UccCodeListTypes.NoReleaseMotivation;

		public string DataSource => Constants.UccDataSources.NoReleaseMotivation;

		public string XmlDataItemForCode => "NoReleaseMotivationCode";
	}
}
