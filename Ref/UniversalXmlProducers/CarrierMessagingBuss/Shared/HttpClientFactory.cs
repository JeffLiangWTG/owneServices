using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public class HttpClientFactory : IHttpClientFactory
	{
		public HttpClient CreateClient()
		{
			var client = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(10)
			};
			client.DefaultRequestHeaders.Add("User-Agent", "RefDbRepo.CarrierMessagingBuss");
			return client;
		}
	}
}
