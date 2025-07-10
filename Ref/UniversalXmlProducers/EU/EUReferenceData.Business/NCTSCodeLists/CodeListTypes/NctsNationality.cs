using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsNationality : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsNationalityCode;

		public string CodeListType => Constants.UccCodeListTypes.Nationality;

		public string DataSource => Constants.UccDataSources.Nationality;

		public string XmlDataItemForCode => "CountryCode";
	}
}
