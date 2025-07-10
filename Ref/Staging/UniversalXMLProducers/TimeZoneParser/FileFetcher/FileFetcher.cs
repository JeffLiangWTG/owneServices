using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public static class FileFetcher
	{
		static async Task<string> GetLatestTimeZoneDBLink()
		{
			using (var client = new HttpClient())
			{
				return await client.GetStringAsync(new Uri(ApplicationConfig.LatestTimeZoneDbLink));
			}
		}

		public static FileDetails DownloadTimeZoneDatabase()
		{
			string latestTimeZoneLink = GetLatestTimeZoneDBLink().Result;

			var webFileDownloader = new FileDownloaderWrapper();

			var filenamePath = Path.Combine(ApplicationConfig.LocalDownloadFolder, latestTimeZoneLink.Replace(ApplicationConfig.TimeZoneDownloadUrl, string.Empty));
			var downloadsFolder = ApplicationConfig.LocalDownloadFolder;
			if (!Directory.Exists(downloadsFolder))
			{
				Directory.CreateDirectory(downloadsFolder);
			}
			if (webFileDownloader.DownloadFile(latestTimeZoneLink, filenamePath))
			{
				return new FileDetails { FileLink = filenamePath, FileDateTime = GetCreationTime(latestTimeZoneLink) };
			}
			return new FileDetails();
		}

		static DateTime GetCreationTime(string url)
		{
			var fileDownloader = new FileDownloader(new Uri(url));
			return fileDownloader.GetCreationTime();
		}
	}
}
