using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsCountryCodesCTC : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsCountryCodesCTC;

		public string CodeListType => Constants.UccCodeListTypes.CountryCodesCTC;

		public string DataSource => Constants.UccDataSources.CountryCodesCTC;

		public string XmlDataItemForCode => "CountryCode";
	}
}
