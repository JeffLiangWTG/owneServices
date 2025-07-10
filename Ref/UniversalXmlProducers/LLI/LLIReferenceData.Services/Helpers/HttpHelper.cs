using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services.Helpers;

public static class HttpHelper
{
	const int MaxRetryCount = 3;

	readonly static IAsyncPolicy<HttpResponseMessage> RetryPolicy = HttpPolicyExtensions
		.HandleTransientHttpError()
		.WaitAndRetryAsync(MaxRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

	public static async Task<HttpResponseMessage> GetAsync(HttpClient client, Uri uri)
	{
		return await RetryPolicy.ExecuteAsync(async () =>
		{
			var response = await client.GetAsync(uri);
			response.EnsureSuccessStatusCode();
			return response;
		});
	}

	public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(HttpClient client, Uri uri, T jsonObject)
	{
		return await RetryPolicy.ExecuteAsync(async () =>
		{
			var response = await client.PostAsJsonAsync(uri, jsonObject);
			response.EnsureSuccessStatusCode();
			return response;
		});
	}
}
