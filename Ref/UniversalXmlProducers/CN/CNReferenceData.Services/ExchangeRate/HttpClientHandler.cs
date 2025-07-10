using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public class HttpClientHandler : IHttpHandler
	{
		static HttpClient Client => new HttpClient();

		public async Task<HttpResponseMessage> GetAsync(string url)
		{
			var client = Client;
			try
			{
				client.Timeout = TimeSpan.FromSeconds(300);
				return await client.GetAsync(new Uri(url));
			}
			catch (TaskCanceledException)
			{
				throw new TimeoutException($"Post request has timed out. The server didn't response within {client.Timeout.TotalSeconds} seconds.");
			}
		}
	}
}
