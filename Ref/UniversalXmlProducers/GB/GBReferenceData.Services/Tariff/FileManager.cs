using System.IO;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Tariff
{
	public abstract class FileManager : SharedReferenceData.Services.Tariff.FileManager
	{
		protected override void DownloadFiles(string contentFolder)
		{
			GetDownloadManager()?.RunDownloadProcess(contentFolder);
		}

		protected override string GetContentFolder() => Path.Combine(WebClientDownloadManager.GetWorkingFolder(), "Content");

		protected abstract IDownloadManager GetDownloadManager();
	}

	public class WebClientFileManager : FileManager
	{
		protected override IDownloadManager GetDownloadManager() => new WebClientDownloadManager();
	}
}
