namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCountryOutsideCustomsSecurityAgreementArea : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsCountryOutsideCustomsSecurityAgreementArea;

		public string CodeListType => Constants.UccCodeListTypes.NctsCountryOutsideCustomsSecurityAgreementArea;

		public string DataSource => Constants.UccDataSources.NctsCountryOutsideCustomsSecurityAgreementArea;

		public string XmlDataItemForCode => "CountryCode";

	}
}
