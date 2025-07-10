namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class AdditionalReferenceWebClientProcessManager : CodebookProcessManager
	{
		public AdditionalReferenceWebClientProcessManager(ICodebookBuilder additionalReferenceBuilder)
		{
			this.additionalReferenceBuilder = additionalReferenceBuilder;
		}
		readonly ICodebookBuilder additionalReferenceBuilder;

		public override string TableNumber => ApplicationConfig.AdditionalReferenceTableNumber;

		public override string Description => "Additional References";

		public override ICodebookBuilder Builder => additionalReferenceBuilder;

		public override DownloadManager DownloadManager => new DownloadManager(new WebClientWrapper());
	}
}
