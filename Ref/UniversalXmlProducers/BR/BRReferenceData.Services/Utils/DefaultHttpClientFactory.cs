using System.Net.Http;
using System;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public sealed class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
	{
		private readonly Lazy<HttpMessageHandler> _handlerLazy = new Lazy<HttpMessageHandler>(() => new HttpClientHandler());

		public HttpClient CreateClient(string name) => new HttpClient(_handlerLazy.Value, disposeHandler: false);

		public void Dispose()
		{
			if (_handlerLazy.IsValueCreated)
			{
				_handlerLazy.Value.Dispose();
			}
		}
	}
}
