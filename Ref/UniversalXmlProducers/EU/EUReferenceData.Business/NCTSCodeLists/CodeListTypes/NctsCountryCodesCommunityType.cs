using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCountryCodesCommunityType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsCountryCodesCommunity;

		public string CodeListType => Constants.UccCodeListTypes.CountryCodesCommunity;

		public string DataSource => Constants.UccDataSources.CountryCodesCommunity;

		public string XmlDataItemForCode => "CountryCode";
	}
}
