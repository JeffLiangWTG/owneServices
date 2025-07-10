using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public sealed class HttpClientHandler : IHttpHandler, IDisposable
	{
		static System.Net.Http.HttpClientHandler _handler;
		static HttpClient _client;
		readonly TimeSpan[] _retryIntervals;
		readonly bool _useRetry;
		readonly bool _simulateBrowser;

		public HttpClientHandler(bool useRetry = false, TimeSpan[] retryIntervals = null, bool simulateBrowser = false)
		{
			_handler = _handler ?? new System.Net.Http.HttpClientHandler();
			_handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
			_handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

			_client = _client ?? new HttpClient(_handler);
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
			_client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			_client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

			if (useRetry)
			{
				_useRetry = useRetry;
				_retryIntervals = retryIntervals ?? new TimeSpan[]
				{
					TimeSpan.FromSeconds(0.5),
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(1.5),
				};
			}

			if (simulateBrowser)
			{
				_simulateBrowser = simulateBrowser;
				_client.DefaultRequestHeaders.Connection.Add("keep-alive");
				_client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			if (_simulateBrowser)
			{
				_client.AssignBrowserHeaders();
			}

			if (!_useRetry)
			{
				return await _client.GetAsync(url);
			}

			for (var retry = 0; retry <= _retryIntervals.Length; retry++)
			{
				try
				{
					return await _client.GetAsync(url);
				}
				catch (Exception ex)
				{
					if (retry == _retryIntervals.Length)
					{
						var err = $"Fail to get {url} after retry {retry} times, exception: {ex.Message}";
						Console.Error.WriteLine(err);
						throw;
					}
					Thread.Sleep(_retryIntervals[retry]);
				}
			}

			return default;
		}

		public void Dispose()
		{
			if (_client != null)
			{
				_client.Dispose();
				_client = null;
			}

			if (_handler != null)
			{
				_handler.Dispose();
				_handler = null;
			}
		}
	}
}
