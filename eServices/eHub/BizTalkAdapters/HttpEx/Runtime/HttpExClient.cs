using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	[ExcludeFromCodeCoverage]
	class HttpExClient : IHttpExClient, IDisposable
	{
		readonly HttpClient httpClient;

		public HttpExClient(WebRequestHandler handler)
		{
			httpClient = new HttpClient(handler, true);
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
		{
			return await httpClient.SendAsync(requestMessage);
		}

		public void Dispose()
		{
			httpClient?.Dispose();
		}
	}
}
