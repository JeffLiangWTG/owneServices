namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class PreviousDocumentTypesWebClientProcessManager : CodebookProcessManager
	{
		public PreviousDocumentTypesWebClientProcessManager(ICodebookBuilder previousDocumentBuilder)
		{
			this.previousDocumentBuilder = previousDocumentBuilder;
		}
		readonly ICodebookBuilder previousDocumentBuilder;

		public override string TableNumber => ApplicationConfig.PreviousDocumentTableNumber;

		public override string Description => "Previous Documents";

		public override ICodebookBuilder Builder => previousDocumentBuilder;

		public override DownloadManager DownloadManager => new DownloadManager(new WebClientWrapper());
	}
}
