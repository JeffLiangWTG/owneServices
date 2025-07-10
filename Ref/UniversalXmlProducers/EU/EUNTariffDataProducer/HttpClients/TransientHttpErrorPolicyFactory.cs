using System;
using System.Collections.Generic;
using System.Net.Http;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class TransientHttpErrorPolicyFactory
	{
		internal const int RetryCount = 3;
		internal static readonly TimeSpan RetryFirstDelay = TimeSpan.FromSeconds(10);

		internal static IEnumerable<TimeSpan> SleepDurations(int retryCount, TimeSpan retryFirstDelay) => Backoff.DecorrelatedJitterBackoffV2(
						medianFirstRetryDelay: retryFirstDelay,
						retryCount: retryCount);

		public static AsyncRetryPolicy<HttpResponseMessage> CreateTransientHttpErrorPolicy() => CreateTransientHttpErrorPolicy(RetryCount, RetryFirstDelay);

		public static AsyncRetryPolicy<HttpResponseMessage> CreateTransientHttpErrorPolicy(int retryCount, TimeSpan retryFirstDelay) =>
			HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(response => response?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				.WaitAndRetryAsync(SleepDurations(retryCount, retryFirstDelay));
	}
}
