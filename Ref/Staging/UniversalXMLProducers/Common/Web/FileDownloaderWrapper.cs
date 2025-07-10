using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.Staging.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web
{
	public class FileDownloaderWrapper : IFileDownloaderWrapper
	{
		public FileDownloaderWrapper()
		{
		}

		public FileDownloaderWrapper(HttpClient httpClientOverride, HttpClientRetryHandler httpClientRetryHandlerOverride)
		{
			this.httpClientOverride = httpClientOverride;
			this.httpClientRetryHandlerOverride = httpClientRetryHandlerOverride;
		}

		public FileDownloaderWrapper(HttpClient httpClientOverride)
		{
			this.httpClientOverride = httpClientOverride;
		}

		public bool DownloadFile(string remoteUrl, string localFilePath)
		{
			return DownloadFile(remoteUrl, localFilePath, null);
		}

		public bool DownloadFile(string remoteUrl, string localFilePath, DateTime? lastModifiedDate)
		{
			var fileDownloader = GetFileDownloader(remoteUrl);

			var lastModified = lastModifiedDate.HasValue ? lastModifiedDate.Value : fileDownloader.GetCreationTime();

			if (File.Exists(localFilePath))
			{
				var localFileInfo = new FileInfo(localFilePath);

				if (DateTime.Compare(localFileInfo.LastWriteTime, lastModified) >= 0 && localFileInfo.Length > 0)
				{
					return true;
				}
				File.Delete(localFilePath);
			}

			using (var input = fileDownloader.GetFileStream())
			using (var output = File.Create(localFilePath))
			{
				input.GetResponseStream().CopyTo(output);
			}

			var info = new FileInfo(localFilePath);
			info.CreationTime = lastModified;
			info.LastWriteTime = lastModified;

			return info.Length != 0 && File.Exists(localFilePath);
		}

		public string[] DownloadAndExtract(string remoteUrl, string localFilePath)
		{
			return DownloadAndExtract(remoteUrl, localFilePath, null);
		}

		public string[] DownloadAndExtract(string remoteUrl, string localFilePath, DateTime? lastModifiedDate)
		{
			if (DownloadFile(remoteUrl, localFilePath, lastModifiedDate))
			{
				return ExtractLocalZipFile(localFilePath);
			}
			return null;
		}

		public string[] ExtractLocalZipFile(string filePath, string destinationPath = "")
		{
			return FileDownloader.ExtractZipFile(filePath, destinationPath);
		}

		public FileInfo GetFileInfo(string localFilePath)
		{
			return new FileInfo(localFilePath);
		}

		public byte[] DownloadFile(string remoteUrl)
		{
			var fileDownloader = GetFileDownloader(remoteUrl);

			using (var input = fileDownloader.GetFileStream())
			using (var output = new MemoryStream())
			{
				input.GetResponseStream().CopyTo(output);
				return output.ToArray();
			}
		}

		protected virtual IFileDownloader GetFileDownloader(string remoteUrl)
		{
			return new FileDownloader(new Uri(remoteUrl), UserAgent, httpClientOverride, httpClientRetryHandlerOverride);
		}

		readonly HttpClient httpClientOverride;
		readonly HttpClientRetryHandler httpClientRetryHandlerOverride;
		const string UserAgent = "WiseTech Tools";
	}
}
