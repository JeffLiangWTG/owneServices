namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsFunctionalErrorCodesIeCA : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsFunctionalErrorCodesIeCA;

		public string CodeListType => Constants.UccCodeListTypes.FunctionalErrorCodesIeCA;

		public string DataSource => Constants.UccDataSources.FunctionalErrorCodesIeCA;

		public string XmlDataItemForCode => "Code";
	}
}
