namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsRejectionCodeDestinationExitType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;
		public string CodeType => Constants.ZZRefCusCodeList.NctsRejectionCodeDestinationExitCode;
		public string CodeListType => Constants.UccCodeListTypes.RejectionCodeDestinationExit;
		public string DataSource => Constants.UccDataSources.RejectionCodeDestinationExit;
		public string XmlDataItemForCode => "RejectionDestinationExitCode";
	}
}
