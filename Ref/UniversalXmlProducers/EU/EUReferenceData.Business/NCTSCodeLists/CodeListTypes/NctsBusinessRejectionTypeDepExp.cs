namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsBusinessRejectionTypeDepExp : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsBusinessRejectionTypeDepExpCode;

		public string CodeListType => Constants.UccCodeListTypes.BusinessRejectionTypeDepExp;

		public string DataSource => Constants.UccDataSources.BusinessRejectionTypeDepExp;

		public string XmlDataItemForCode => "BusinessRejectionTypeDepExpCode";
	}
}
