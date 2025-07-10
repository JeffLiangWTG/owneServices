using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class TariffIPITableDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			var url = ConfigurationProvider.Configuration.GetSection("URL_IPI_TABLE").Value;

			var fileContent = client.GetByteArrayAsyncEx(url)?.Result;
			if (fileContent == null || fileContent.Length == 0)
			{
				throw new InvalidOperationException($"The application was unable to download the OMC Tec file from: {url}");
			}
			return fileContent;
		}
	}
}
