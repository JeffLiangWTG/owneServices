namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsXmlErrorCodesType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsXmlErrorCodes;

		public string CodeListType => Constants.UccCodeListTypes.XmlErrorCodes;

		public string DataSource => Constants.UccDataSources.XmlErrorCodes;

		public string XmlDataItemForCode => "XmlErrorCodesCode";
	}
}
