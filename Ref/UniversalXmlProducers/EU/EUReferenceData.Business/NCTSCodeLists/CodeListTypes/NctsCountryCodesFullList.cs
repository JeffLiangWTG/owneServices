namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCountryCodesFullList : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsCountryCodeType;

		public string CodeListType => Constants.UccCodeListTypes.CountryCodesFullList;

		public string DataSource => Constants.UccDataSources.CountryCodesFullList;

		public string XmlDataItemForCode => "CountryCode";
	}
}
