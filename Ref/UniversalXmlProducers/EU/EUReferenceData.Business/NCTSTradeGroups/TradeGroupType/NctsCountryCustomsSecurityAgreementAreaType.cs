namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCustomsSecurityAgreementAreaType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string CodeType => Constants.ZZRefCusTradeCountry.NctsCountryCustomsSecurityAgreementArea;

		public string CodeListType => Constants.UccCodeListTypes.CountryCustomsSecurityAgreementArea;

		public string DataSource => Constants.UccDataSources.CountryCustomsSecurityAgreementArea;

		public string Domain => Constants.UccConstants.NCTSDomain;

		public string XmlDataItemForCode => "CountryCode";
	}
}
