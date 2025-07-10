using System;
using System.IO;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public abstract class FileManager : SharedReferenceData.Services.Tariff.FileManager
	{
		protected override void DownloadFiles(string contentFolder)
		{
			GetDownloadManager()?.RunDownloadProcess(contentFolder, DateTime.Now);
		}

		protected override string GetContentFolder() => Path.Combine(WebDriverDownloadManager.GetWorkingFolder(), "Content");

		protected abstract IDownloadManager GetDownloadManager();
	}

	public class WebDriverFileManager : FileManager
	{
		protected override IDownloadManager GetDownloadManager() => new WebDriverDownloadManager();
	}
}
