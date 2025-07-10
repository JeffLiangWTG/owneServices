using System.Net.Http;
using Microsoft.Extensions.Http;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class FileDownloaderHttpClientFactory
	{
		public static HttpClient CreateHttpClient()
		{
			var retryPolicy = TransientHttpErrorPolicyFactory.CreateTransientHttpErrorPolicy();

#pragma warning disable CA2000 // Dispose objects before losing scope
			var handler = new PolicyHttpMessageHandler(retryPolicy)
			{
				InnerHandler = new SocketsHttpHandler
				{
					PooledConnectionLifetime = Constants.DownloadTimeout
				}
			};
#pragma warning restore CA2000 // Dispose objects before losing scope

			return new HttpClient(handler) { Timeout = Constants.DownloadTimeout };
		}
	}
}
