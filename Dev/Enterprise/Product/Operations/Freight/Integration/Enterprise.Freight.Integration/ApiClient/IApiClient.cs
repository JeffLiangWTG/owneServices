using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Freight.Integration.ApiClient
{
	public interface IApiClient : IDisposable
	{
		HttpClient Client { get; }
		TimeSpan Timeout { get; set; }
		int RetryCount { get; set; }
		string MediaType { get; set; }
		List<string> Accept { get; set; }
		List<(string, string)> CustomHeaders { get; set; }
		(string, string)? AccessToken { get; set; }
		IHttpContentSerializer ContentSerializer { get; set; }
		Func<HttpResponseMessage, Task<Exception>> ExceptionFactory { get; set; }

		Task<IApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
		Task<IApiResponse<TResponse>> PostAsync<TResponse>(string endpoint, string request, CancellationToken cancellationToken = default);
	}
}
