namespace CargoWise.RefDbRepo.NLReferenceData.Services
{

	public class AdditionalInformationWebClientProcessManager : CodebookProcessManager
	{
		public AdditionalInformationWebClientProcessManager(ICodebookBuilder codebookBuilder)
		{
			this.codebookBuilder = codebookBuilder;
		}
		readonly ICodebookBuilder codebookBuilder;

		public override string TableNumber => ApplicationConfig.AdditionalInformationTableNumber;

		public override string Description => "Additional Information";

		public override ICodebookBuilder Builder => codebookBuilder;

		public override DownloadManager DownloadManager => new DownloadManager(new WebClientWrapper());
	}
}
