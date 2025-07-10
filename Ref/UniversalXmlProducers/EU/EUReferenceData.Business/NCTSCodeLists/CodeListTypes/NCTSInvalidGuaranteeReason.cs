namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NCTSInvalidGuaranteeReason : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NCTSInvalidGuaranteeReasonCode;

		public string CodeListType => Constants.UccCodeListTypes.InvalidGuaranteeReason;

		public string DataSource => Constants.UccDataSources.InvalidGuaranteeReason;

		public string XmlDataItemForCode => "InvalidGuaranteeReasonCode";
	}
}
