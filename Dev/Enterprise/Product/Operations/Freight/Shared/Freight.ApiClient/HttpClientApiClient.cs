using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Freight.Integration.ApiClient;
using Newtonsoft.Json;
using Polly;
using WTG.Foundation.Http;

namespace Enterprise.Freight.ApiClient
{
	public class HttpClientApiClient : IApiClient, IDisposable
	{
		public HttpClient Client { get; private set; }

		public HttpClientApiClient(string baseUrl)
		{
			Client = CreateHttpClient(baseUrl);
			ExceptionFactory = new DefaultApiExceptionFactory(this).CreateAsync;
			ContentSerializer = new DefalutJsonSerializer(this);
		}

		public HttpClientApiClient(string baseUrl, HttpMessageHandler handler)
		{
			Client = CreateHttpClient(baseUrl, handler);
			ExceptionFactory = new DefaultApiExceptionFactory(this).CreateAsync;
			ContentSerializer = new DefalutJsonSerializer(this);
		}

		HttpClient CreateHttpClient(string baseUrl, HttpMessageHandler handler = default)
		{
			if (string.IsNullOrWhiteSpace(baseUrl))
			{
				throw new ArgumentException(
					$"`{nameof(baseUrl)}` must not be null or whitespace.",
					nameof(baseUrl));
			}
			var client = handler == null ? ObjectFactory.Get<IHttpClientFactory>().Create() : ObjectFactory.Get<IHttpClientFactory>().CreateNew(handler);
			client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
			return client;
		}

		public HttpClientApiClient(HttpClient httpClient)
		{
			Client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			ExceptionFactory = new DefaultApiExceptionFactory(this).CreateAsync;
			ContentSerializer = new DefalutJsonSerializer(this);
		}

		public TimeSpan Timeout
		{
			get => Client.Timeout;
			set
			{
				if (value != TimeSpan.MaxValue)
				{
					Client.Timeout = value;
				}
			}
		}
		public int RetryCount { get; set; } = 2;
		public string MediaType { get; set; }
		public List<(string, string)> CustomHeaders {  get; set; }  = new List<(string, string)>();
		public List<string> Accept { get; set; } = new List<string> { "application/json" };
		public (string, string)? AccessToken
		{
			get => _accessToken;
			set
			{
				_accessToken = value;
				if (_accessToken.HasValue)
				{
					Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_accessToken?.Item1, _accessToken?.Item2);
				}
				else
				{
					Client.DefaultRequestHeaders.Authorization = null;
				}
			}
		}
		(string, string)? _accessToken;

		public Func<HttpResponseMessage, Task<Exception>> ExceptionFactory { get; set; }

		public IHttpContentSerializer ContentSerializer { get; set; }

		public Func<HttpResponseMessage, bool> RetryPredicate { get; set; } = (HttpResponseMessage response) => !response.IsSuccessStatusCode;

		public Task<IApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
		{
			return SendAsync<TResponse>(endpoint, HttpMethod.Get, null, cancellationToken);
		}

		public Task<IApiResponse<TResponse>> PostAsync<TResponse>(string endpoint, string request, CancellationToken cancellationToken = default)
		{
			return SendAsync<TResponse>(endpoint, HttpMethod.Post, request, cancellationToken);
		}

		async Task<IApiResponse<TResponse>> SendAsync<TResponse>(string endpoint, HttpMethod method, string request, CancellationToken cancellationToken = default)
		{
			var retryPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>().OrResult(RetryPredicate)
				.RetryAsync(RetryCount);
			var response = await retryPolicy.ExecuteAsync(() => SendRequestAsync<TResponse>(method, endpoint, request, cancellationToken)).ConfigureAwait(false);
			Exception e = null;
			try
			{
				e = await ExceptionFactory(response).ConfigureAwait(false);
			}
			catch (Exception fe)
			{
				e = fe;
			}
			var body = default(TResponse);
			try
			{
				body = (e == null && response.Content != null) ? await ContentSerializer.FromHttpContentAsync<TResponse>(response.Content, cancellationToken).ConfigureAwait(false) : default;
			}
			catch (Exception ex)
			{
				e = await ContentSerializer.SerializerErrorHandler(ex, response);
			}
			return new ApiResponse<TResponse>(response, body, this, e);
		}

		async Task<HttpResponseMessage> SendRequestAsync<TResponse>(HttpMethod method, string endpoint, string requestBody,
			CancellationToken cancellationToken)
		{
			var requestMessage = new HttpRequestMessage(method, endpoint)
			{
				Content = requestBody != null ? new StringContent(requestBody, Encoding.UTF8, MediaType) : null,
			};

			foreach (var acceptHeader in Accept)
			{
				requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
			}

			foreach (var customHeader in CustomHeaders)
			{
				if (customHeader.Item1 != null && customHeader.Item2 != null)
				{
					requestMessage.Headers.TryAddWithoutValidation(customHeader.Item1, customHeader.Item2);
				}
			}

			return await Client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
		}

		public void Dispose()
		{
			((IDisposable)Client).Dispose();
		}
	}

	public class DefalutJsonSerializer : IHttpContentSerializer
	{
		readonly IApiClient apiClient;

		public DefalutJsonSerializer(IApiClient httpClientApiClient)
		{
			this.apiClient = httpClientApiClient;
		}

		public async Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
		{
			var data = await content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<T>(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception reporting")]
		public async Task<Exception> SerializerErrorHandler(Exception e, HttpResponseMessage message)
		{
			return await ApiException.Create("An error occured deserializing the response.", message?.RequestMessage, message?.RequestMessage?.Method, message, apiClient, e);
		}
	}

	public class DefaultApiExceptionFactory
	{
		static readonly Task<Exception> NullTask = Task.FromResult<Exception>(null);

		readonly HttpClientApiClient apiClient;

		public DefaultApiExceptionFactory(HttpClientApiClient apiClient)
		{
			this.apiClient = apiClient;
		}

		public Task<Exception> CreateAsync(HttpResponseMessage responseMessage)
		{
			if (!responseMessage.IsSuccessStatusCode)
			{
				return CreateExceptionAsync(responseMessage, apiClient);
			}
			else
			{
				return NullTask;
			}
		}

		static async Task<Exception> CreateExceptionAsync(HttpResponseMessage responseMessage, HttpClientApiClient apiClient)
		{
			var requestMessage = responseMessage.RequestMessage;
			var method = requestMessage?.Method;

			return await ApiException.Create(requestMessage, method, responseMessage, apiClient).ConfigureAwait(false);
		}
	}
}
