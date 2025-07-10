namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsGuaranteeType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsGuaranteeTypeCode;

		public string CodeListType => Constants.UccCodeListTypes.GuaranteeType;

		public string DataSource => Constants.UccDataSources.GuaranteeType;

		public string XmlDataItemForCode => "GuaranteeTypeCode";
	}
}
