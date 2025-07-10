using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;

sealed class Taric4FileDownloader : ITaric4FileDownloader
{
	readonly IFtpClient ftpClient;
	readonly IWebDriverHelperFactory webDriverHelperFactory;
	readonly ITaric4FileDownloaderConfig config;

	public Taric4FileDownloader(ITaric4FileDownloaderConfig config = null, IFtpClient ftpClient = null, IWebDriverHelperFactory webDriverHelperFactory = null)
	{
		this.config = config ?? new Taric4FileDownloaderConfig();
		this.ftpClient = ftpClient ?? new FtpClient(this.config.FtpSeleniumAddress);
		this.webDriverHelperFactory = webDriverHelperFactory ?? new WebDriverHelperFactory();
	}

	public string DownloadTaric4BaseFile(string destinationDirectoryPath)
	{
		string fileName;
		string ftpDirectory = config.FtpSeleniumDirectory;
		PrepareFtpDirectory(ftpDirectory);
		using (var webDriver = webDriverHelperFactory.Create(config.SeleniumTariffDownloadFullPath))
		{
			InitializeFileUploadToFtp(webDriver, config.LoadIntervalInSeconds);
			fileName = WaitForFileIsUploadedToFtp(ftpDirectory, config.WaitIntervalInSeconds);
		}
		DownloadAndRemoveFileFromFtp(fileName, ftpDirectory, destinationDirectoryPath);

		return fileName;
	}

	void PrepareFtpDirectory(string directoryName)
	{
		var contentNames = ftpClient.GetDirectoryContent();
		if (!contentNames.Contains(directoryName))
		{
			ftpClient.CreateDirectory(directoryName);
		}
		else
		{
			var zipFiles = ftpClient.GetDirectoryContent(directoryName)
				.Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
			foreach (var fileName in zipFiles)
			{
				ftpClient.DeleteFile(fileName, directoryName);
			}

			var anyZipFileExists = ftpClient.GetDirectoryContent(directoryName)
				.Any(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
			if (anyZipFileExists)
			{
				throw new InvalidOperationException($"There are zip files in the FTP directory {directoryName}. Please remove them manually.");
			}
		}
	}

	void InitializeFileUploadToFtp(IWebDriverHelper webDriver, int loadIntervalInSeconds)
	{
		webDriver.GetWebPage(config.Taric4Url, loadIntervalInSeconds);
		webDriver.GetWebPageByLinkId(config.Taric4CheckboxId);
		Thread.Sleep(1000 * loadIntervalInSeconds); // needs some time to execute action on checkboxId element
		webDriver.GetWebPageByLinkId(config.Taric4DownloadButtonId);
	}

	string WaitForFileIsUploadedToFtp(string directoryName, int waitIntervalInSeconds)
	{
		string result;
		while (!TryGetFirstFtpFileEndsWith(".zip", out result))
		{
			Thread.Sleep(1000 * waitIntervalInSeconds);
		}

		return result;

		bool TryGetFirstFtpFileEndsWith(string value, out string fileName) =>
			(fileName = ftpClient.GetDirectoryContent(directoryName)
				.FirstOrDefault(x => x.EndsWith(value, StringComparison.OrdinalIgnoreCase))) != null;
	}

	void DownloadAndRemoveFileFromFtp(string fileName, string ftpDirectory, string destinationDirectoryPath)
	{
		if (!Directory.Exists(destinationDirectoryPath))
		{
			Directory.CreateDirectory(destinationDirectoryPath);
		}
		ftpClient.DownloadFile(fileName, ftpDirectory, destinationDirectoryPath);
		ftpClient.DeleteFile(fileName, ftpDirectory);
	}
}
