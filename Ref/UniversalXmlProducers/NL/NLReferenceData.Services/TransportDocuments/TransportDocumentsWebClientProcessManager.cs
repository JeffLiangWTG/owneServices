namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class TransportDocumentsWebClientProcessManager : CodebookProcessManager
	{
		public TransportDocumentsWebClientProcessManager(ICodebookBuilder transportDocumentBuilder)
		{
			this.transportDocumentBuilder = transportDocumentBuilder;
		}
		readonly ICodebookBuilder transportDocumentBuilder;

		public override string TableNumber => ApplicationConfig.TransportDocumentTableNumber;

		public override string Description => "Transport Documents";

		public override ICodebookBuilder Builder => transportDocumentBuilder;

		public override DownloadManager DownloadManager => new DownloadManager(new WebClientWrapper());
	}
}
