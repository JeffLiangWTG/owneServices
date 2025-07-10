using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsGuaranteeTypeCTC : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsGuaranteeTypeCTCCode;

		public string CodeListType => Constants.UccCodeListTypes.GuaranteeTypeCTC;

		public string DataSource => Constants.UccDataSources.GuaranteeTypeCTC;

		public string XmlDataItemForCode => "GuaranteeTypeCode";
	}
}
