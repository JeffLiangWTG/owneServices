using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers
{
	internal class FileManagerForTest : FileManager
	{
		public FileManagerForTest(IDownloadManager downloadManager, string contentFolder)
		{
			this.downloadManager = downloadManager;
			this.contentFolder = contentFolder;
		}
		readonly IDownloadManager downloadManager;
		readonly string contentFolder;

		protected override IDownloadManager GetDownloadManager() => downloadManager;

		public IDownloadManager GetDownloadManagerExposed() => GetDownloadManager();
		protected override string GetContentFolder() => contentFolder;
	}

	internal class WebClientFileManagerForTest : WebClientFileManager
	{
		public IDownloadManager GetDownloadManagerExposed() => GetDownloadManager();
		public string GetContentFolderExposed() => GetContentFolder();
	}
}
