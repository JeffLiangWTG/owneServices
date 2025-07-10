namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsBusinessRejectionTypeDesExtType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsBusinessRejectionTypeDesExt;

		public string CodeListType => Constants.UccCodeListTypes.BusinessRejectionTypeDesExt;

		public string DataSource => Constants.UccDataSources.BusinessRejectionTypeDesExt;

		public string XmlDataItemForCode => "BusinessRejectionTypeDesExtCode";
	}
}
