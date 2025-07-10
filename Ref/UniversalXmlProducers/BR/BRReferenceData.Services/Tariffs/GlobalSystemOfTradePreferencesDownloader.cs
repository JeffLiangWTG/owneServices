using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class GlobalSystemOfTradePreferencesDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			var response = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_GSTP_DUTY").Value)?.Result;

			if (response != null && response.IsSuccessStatusCode)
			{
				return response.Content.ReadAsByteArrayAsync()?.Result;
			}

			return null;
		}
	}
}
