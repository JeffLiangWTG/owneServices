using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CBSAErrorCode
{
	public class CBSAErrorCodeFileDownloader : FileDownloader
	{
		public CBSAErrorCodeFileDownloader(PreProcessChecker checker)
			: base(checker)
		{
			PublicationTime = DateTime.Now;
		}

		protected override bool CheckIsDownloadFileNode(string href) => href.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && href.Contains("error-erreur");

		protected override string RootURL => ApplicationConfig.CBSAEServiceUrl;
	}
}
