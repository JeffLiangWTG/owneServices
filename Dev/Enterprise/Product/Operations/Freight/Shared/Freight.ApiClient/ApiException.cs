using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Enterprise.Freight.Integration.ApiClient;

namespace Enterprise.Freight.ApiClient
{
	[Serializable]
	public class ApiException : Exception
	{
		public HttpStatusCode StatusCode { get; }

		public string ReasonPhrase { get; }

		public HttpResponseHeaders Headers { get; }

		public HttpMethod HttpMethod { get; }

		public HttpRequestMessage RequestMessage { get; }

		public HttpContentHeaders ContentHeaders { get; private set; }

		public string Content { get; private set; }

		public bool HasContent => !string.IsNullOrWhiteSpace(Content);

		public IApiClient ApiClient { get; }

#if NETFRAMEWORK
		protected ApiException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		protected ApiException(HttpRequestMessage message, HttpMethod httpMethod, string content, HttpStatusCode statusCode, string reasonPhrase, HttpResponseHeaders headers, IApiClient apiClient, Exception innerException = null)
			: this(CreateMessage(statusCode, reasonPhrase, message.RequestUri), message, httpMethod, content, statusCode, reasonPhrase, headers, apiClient)
		{
		}

		protected ApiException(string exceptionMessage, HttpRequestMessage message, HttpMethod httpMethod, string content, HttpStatusCode statusCode, string reasonPhrase, HttpResponseHeaders headers, IApiClient apiClient, Exception innerException = null)
			: base(exceptionMessage, innerException)
		{
			RequestMessage = message;
			HttpMethod = httpMethod;
			StatusCode = statusCode;
			ReasonPhrase = reasonPhrase;
			Headers = headers;
			Content = content;
			ApiClient = apiClient;
		}

		public async Task<T> GetContentAsAsync<T>() => HasContent ?
				await ApiClient.ContentSerializer.FromHttpContentAsync<T>(new StringContent(Content)).ConfigureAwait(false) :
				default;

		public static Task<ApiException> Create(HttpRequestMessage message, HttpMethod httpMethod, HttpResponseMessage response, IApiClient apiClient, Exception innerException = null)
		{
			var exceptionMessage = CreateMessage(response.StatusCode, response.ReasonPhrase, message.RequestUri);
			return Create(exceptionMessage, message, httpMethod, response, apiClient, innerException);
		}

		public static async Task<ApiException> Create(string exceptionMessage, HttpRequestMessage message, HttpMethod httpMethod, HttpResponseMessage response, IApiClient apiClient, Exception innerException = null)
		{
			var exception = new ApiException(exceptionMessage, message, httpMethod, null, response.StatusCode, response.ReasonPhrase, response.Headers, apiClient, innerException);

			if (response.Content == null)
			{
				return exception;
			}

			try
			{
				exception.ContentHeaders = response.Content.Headers;
				var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				exception.Content = content;
				response.Content.Dispose();
			}
			catch
			{
				// NB: We're already handling an exception at this point,
				// so we want to make sure we don't throw another one
				// that hides the real error.
			}

			return exception;
		}

		static string CreateMessage(HttpStatusCode statusCode, string reasonPhrase, Uri uri) =>
			$"Response status code does not indicate success: {(int)statusCode} ({reasonPhrase}). Request endpoint: {uri}";
	}
}
