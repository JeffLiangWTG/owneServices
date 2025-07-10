using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public sealed class HttpClientWithCookieSupport : IHttpClient
	{
		readonly HttpClientHandler httpHandler;
		readonly HttpClient client;

		public HttpClientWithCookieSupport(HttpMessageHandler handler = null)
		{
			httpHandler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseCookies = true,
				CheckCertificateRevocationList = true
			};
			client = new HttpClient(handler ?? httpHandler);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
		}

		HttpResponseMessage IHttpClient.Get(Uri requestUri)
		{
			return client.GetAsync(requestUri).GetAwaiter().GetResult();
		}

		void IDisposable.Dispose()
		{
			client?.Dispose();
			httpHandler?.Dispose();
		}
	}
}
