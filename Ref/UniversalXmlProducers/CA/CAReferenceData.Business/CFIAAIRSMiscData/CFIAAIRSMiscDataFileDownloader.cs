using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSMiscData
{
	public class CFIAAIRSMiscDataFileDownloader : FileDownloader
	{
		public CFIAAIRSMiscDataFileDownloader(PreProcessChecker checker)
			: base(checker)
		{
		}

		protected override bool CheckIsDownloadFileNode(string href) => href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && href.Contains("airs_miscellaneous");

		protected override string RootURL => ApplicationConfig.InspectionCanadaCA;
	}
}
