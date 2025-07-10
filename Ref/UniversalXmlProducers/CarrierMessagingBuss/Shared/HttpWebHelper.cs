using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public class HttpWebHelper<T>(IHttpClientFactory httpClientFactory) : IHttpWebHelper<T> where T : class
	{
		private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

		public async Task<T> GetAsync(string url, string token)
		{
			using var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
			var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				throw new InvalidOperationException($"Api response is invalid. Status Code: {response.StatusCode}, Content: {errorContent}");
			}

			var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Response content stream is null.");

			var result = await JsonSerializer.DeserializeAsync<T>(responseStream).ConfigureAwait(false);
			return result ?? throw new InvalidOperationException("Deserialized object is null.");
		}
	}
}
