using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public sealed class HttpClientHandler : IHttpHandler, IDisposable
	{
		static HttpClient _client = new HttpClient();

		public HttpResponseMessage Get(string url)
		{
			var response = GetAsync(url);
			return response?.Result;
		}

		public async Task<HttpResponseMessage> GetAsync(string url)
		{
			return await _client.GetAsync(new Uri(url));
		}

		public HttpResponseMessage Post(string url, HttpContent content)
		{
			var response = PostAsync(url, content);
			return response?.Result;
		}

		public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
		{
			return await _client.PostAsync(new Uri(url), content);
		}

		public void SetSecureConnection(bool requireSecureConnection)
		{
			if (requireSecureConnection)
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
				ServicePointManager.DefaultConnectionLimit = Configuration.ApplicationConfig.BatchSize > Configuration.ApplicationConfig.ConnectionLimit ? Configuration.ApplicationConfig.ConnectionLimit : Configuration.ApplicationConfig.BatchSize;
				ServicePointManager.UseNagleAlgorithm = false;
			}
		}

		public void Dispose()
		{
			if (_client == null)
			{
				return;
			}

			_client.Dispose();
			_client = null;
		}
	}
}
