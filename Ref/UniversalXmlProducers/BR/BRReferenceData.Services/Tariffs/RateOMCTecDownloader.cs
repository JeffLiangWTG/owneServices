using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class RateOMCTecDownloader
	{
		public static byte[] DownloadOMCTecFile(HttpClient client)
		{
			var url = ConfigurationProvider.Configuration.GetSection("URL_OMC_TEC").Value;

			var fileContent = client.GetByteArrayAsyncEx(url)?.Result;
			if (fileContent == null || fileContent.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download the OMC Tec file from: {url}");
			}
			return fileContent;
		}
	}
}
