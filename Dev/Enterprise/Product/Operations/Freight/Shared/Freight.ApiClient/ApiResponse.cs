using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Enterprise.Freight.Integration.ApiClient;

namespace Enterprise.Freight.ApiClient
{
	public sealed class ApiResponse<T> : IApiResponse<T>
	{
		bool disposed;

		public ApiResponse(HttpResponseMessage response, T content, IApiClient apiClient, Exception error = null)
		{
			Response = response ?? throw new ArgumentNullException(nameof(response));
			Error = error;
			Content = content;
			ApiClient = apiClient;
		}

		public T Content { get; }

		public HttpResponseMessage Response { get; }

		public IApiClient ApiClient { get; }

		public HttpResponseHeaders Headers => Response.Headers;

		public HttpContentHeaders ContentHeaders => Response.Content?.Headers;

		public bool IsSuccessStatusCode => Response.IsSuccessStatusCode;

		public string ReasonPhrase => Response.ReasonPhrase;

		public HttpRequestMessage RequestMessage => Response.RequestMessage;

		public HttpStatusCode StatusCode => Response.StatusCode;

		public Version Version => Response.Version;

		public Exception Error { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<IApiResponse<T>> EnsureSuccessStatusCodeAsync(bool throwErrorRegardlessOfStatusCode = false)
		{
			if (throwErrorRegardlessOfStatusCode && Error != null)
			{
				Dispose();
				throw Error;
			}
			if (!IsSuccessStatusCode)
			{
				var exception = Error ?? await ApiException.Create(Response.RequestMessage, Response.RequestMessage?.Method, Response, ApiClient).ConfigureAwait(false);
				Dispose();
				throw exception;
			}

			return this;
		}

		void Dispose(bool disposing)
		{
			if (!disposing || disposed)
			{
				return;
			}

			disposed = true;

			Response.Dispose();
		}
	}
}
