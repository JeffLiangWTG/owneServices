using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class FileDownloader
	{
		public static async Task<bool> TryDownload(IHttpClientHelper httpClientHelper, string downloadUrl, string filePath)
		{
			return await TryDownloadCore((url) => httpClientHelper.GetAsync(url), downloadUrl, filePath);
		}

		public static async Task<bool> TryDownload(IWebSourceProvider sourceProvider, string downloadUrl, string filePath)
		{
			return await TryDownloadCore((url) => sourceProvider.GetAsync(url), downloadUrl, filePath);
		}

		static async Task<bool> TryDownloadCore(Func<string, Task<Stream>> downloadFunc, string downloadUrl, string filePath)
		{
			bool isDownloaded = false;

			using (var stream = await downloadFunc(downloadUrl))
			{
				if (stream != null)
				{
					using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
					{
						stream.CopyTo(fileStream);
					}
					isDownloaded = File.Exists(filePath);
				}
				if (!isDownloaded)
				{
					File.Delete(filePath);
					ErrorWriter.WriteError($"Failed to download. Download URL: {downloadUrl}. File Path: {filePath}.");
				}
			}

			return isDownloaded;
		}
	}
}
