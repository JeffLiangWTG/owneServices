using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class WebSourceProvider : IWebSourceProvider, IDisposable
	{
		public WebSourceProvider(HttpMessageHandler handler, bool isParallelMode)
		{
			this.isParallelMode = isParallelMode;
			client = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler)));
		}

		readonly HttpClient client;

		readonly bool isParallelMode;

		bool IWebSourceProvider.IsParallelMode => isParallelMode;

		public async Task<string> GetPageAsync(string url)
		{
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
			{
				requestMessage.Headers.Add("User-Agent", "WiseTech Tools");
				var result = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
				return await result.Content.ReadAsStringAsync();
			}
		}

		public async Task<Stream> GetAsync(string url)
		{
			return await (await client.GetAsync(new Uri(url))).Content.ReadAsStreamAsync();
		}

		public void Dispose()
		{
			client?.Dispose();
		}
	}
}
