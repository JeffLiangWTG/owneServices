using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.Shared
{
	public static class HttpClientExtensions
	{
		const int MaxRetryCount = 3;

		static readonly AsyncPolicy<HttpResponseMessage> RetryPolicy = HttpPolicyExtensions
			.HandleTransientHttpError()
			.WaitAndRetryAsync(MaxRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

		public static async Task<HttpResponseMessage> GetWithRetryAsync(this HttpClient client, Uri requestUri)
		{
			return await RetryPolicy.ExecuteAsync(async () =>
			{
				var response = await client.GetAsync(requestUri);
				response.EnsureSuccessStatusCode();
				return response;
			});
		}

		public static async Task<HttpResponseMessage> PostWithRetryAsync(this HttpClient client, Uri requestUri, HttpContent content)
		{
			return await RetryPolicy.ExecuteAsync(async () =>
			{
				var response = await client.PostAsync(requestUri, content);
				response.EnsureSuccessStatusCode();
				return response;
			});
		}
	}
}
