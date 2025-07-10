namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class SupportingDocumentTypesWebClientProcessManager : CodebookProcessManager
	{
		public SupportingDocumentTypesWebClientProcessManager(ICodebookBuilder supportingDocumentBuilder)
		{
			this.supportingDocumentBuilder = supportingDocumentBuilder;
		}
		readonly ICodebookBuilder supportingDocumentBuilder;

		public override string TableNumber => ApplicationConfig.SupportingDocumentTableNumber;

		public override string Description => "Supporting Documents";

		public override ICodebookBuilder Builder => supportingDocumentBuilder;

		public override DownloadManager DownloadManager => new DownloadManager(new WebClientWrapper());
	}
}
