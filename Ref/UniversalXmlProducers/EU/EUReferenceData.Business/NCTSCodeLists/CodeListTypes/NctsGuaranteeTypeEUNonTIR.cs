using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsGuaranteeTypeEUNonTIR : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsGuaranteeTypeEUNonTIRCode;

		public string CodeListType => Constants.UccCodeListTypes.GuaranteeTypeEUNonTIR;

		public string DataSource => Constants.UccDataSources.GuaranteeTypeEUNonTIR;

		public string XmlDataItemForCode => "GuaranteeTypeCode";
	}
}
