namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NctsRejectionCodeDepartureExportType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsRejectionCodeDepartureExport;

		public string CodeListType => Constants.UccCodeListTypes.RejectionCodeDepartureExport;

		public string DataSource => Constants.UccDataSources.RejectionCodeDepartureExport;

		public string XmlDataItemForCode => "RejectionDepartureExportCode";
	}
}
