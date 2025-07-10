namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCountryCodesCommonTransit : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsCountryCodesCommonTransit;

		public string CodeListType => Constants.UccCodeListTypes.CountryCodesCommonTransit;

		public string DataSource => Constants.UccDataSources.CountryCodesCommonTransit;

		public string XmlDataItemForCode => "CountryCode";
	}
}
