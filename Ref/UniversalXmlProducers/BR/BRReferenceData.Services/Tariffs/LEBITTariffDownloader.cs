using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class LEBITTariffDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			var response = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_LEBIT_LIST").Value)?.Result;

			if (response != null && response.IsSuccessStatusCode)
			{
				return response.Content.ReadAsByteArrayAsync()?.Result;
			}

			return null;
		}
	}
}
