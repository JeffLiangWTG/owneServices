using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class DownloadManager : IDisposable
	{
		public DownloadManager(IWebClientWrapper webClient)
		{
			WebClient = webClient;
		}
		readonly IWebClientWrapper WebClient;

		public DownloadManager(IWebDriverHelperWrapper webDriverHelper)
		{
			WebDriverHelper = webDriverHelper;
		}
		readonly IWebDriverHelperWrapper WebDriverHelper;

		public List<string> DownloadCodeBook(string downloadUrl, string workingFolder)
		{
			try
			{
				var localZipFile = Path.Combine(workingFolder, "CodeBook.zip");
				var files = new List<string>();

				DownloadManagerHelper.PrepareEnvironment(workingFolder);

				if (WebClient.DownloadFile(downloadUrl, localZipFile))
				{
					files = WebClient.ExtractLocalZipFile(localZipFile).ToList<string>();

					DownloadManagerHelper.RemoveUnexpectedFiles(".xml", files);
				}

				return files;
			}
			catch (Exception ex)
			{
				throw new ProcessingException($"Processing of {downloadUrl} failed. ", ex);
			}
		}

		public List<string> DownloadFilesAsExcel(string downloadUrl)
		{
			try
			{
				return WebDriverHelper.DownloadFiles(downloadUrl);
			}
			catch (Exception ex)
			{
				throw new ProcessingException($"Processing of {downloadUrl} failed. ", ex);
			}
		}

		public XmlDocument DownloadFileAsXmlDocument(string downloadUrl)
		{
			try
			{
				return WebClient.DownloadFileAsXmlDocument(downloadUrl);
			}
			catch (Exception ex)
			{
				throw new ProcessingException($"Processing of {downloadUrl} failed. ", ex);
			}
		}

		public List<string> DownloadNewFiles(List<TariffDownloadElement> downloadElements, string workingFolder)
		{
			DownloadManagerHelper.PrepareEnvironment(workingFolder);
			DownloadManagerHelper.RemoveUnexpectedFiles(downloadElements.Select(x => x.FileName).ToList<string>(), Directory.GetFiles(workingFolder).ToList());
			var files = new List<string>();

			foreach (var downloadElement in downloadElements)
			{
				try
				{
					var localFile = Path.Combine(workingFolder, downloadElement.FileName);
					if (!File.Exists(localFile) && WebClient.DownloadFile(downloadElement.Url, localFile))
					{
						files.Add(localFile);
					}
				}
				catch (Exception ex)
				{
					throw new ProcessingException($"Processing of {downloadElement.Url} failed. ", ex);
				}
			}
			return files;
		}

		public void Dispose()
		{
			(WebClient as IDisposable)?.Dispose();
			(WebDriverHelper as IDisposable)?.Dispose();
		}
	}
}
