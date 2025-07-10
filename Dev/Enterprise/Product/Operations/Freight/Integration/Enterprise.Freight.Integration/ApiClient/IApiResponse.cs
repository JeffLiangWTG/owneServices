using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Enterprise.Freight.Integration.ApiClient
{
	public interface IApiResponse<T> : IApiResponse
	{
		T Content { get; }

		Task<IApiResponse<T>> EnsureSuccessStatusCodeAsync(bool throwErrorRegardlessOfStatusCode = false);
	}

	public interface IApiResponse : IDisposable
	{
		HttpResponseHeaders Headers { get; }

		HttpContentHeaders ContentHeaders { get; }

		bool IsSuccessStatusCode { get; }

		HttpStatusCode StatusCode { get; }

		string ReasonPhrase { get; }

		HttpRequestMessage RequestMessage { get; }

		Version Version { get; }

		Exception Error { get; }

		HttpResponseMessage Response { get; }
	}
}
