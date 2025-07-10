using System;
using Polly.Retry;
using Polly;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class FileFetcherRetry
	{
		public static T Execute<T>(Func<T> action, int retryCount = 3, int retryDelayInSeconds = 5)
		{
			ArgumentNullException.ThrowIfNull(action);

			T result = default(T);

			var pipeline = new ResiliencePipelineBuilder()
				.AddRetry(
					new RetryStrategyOptions
					{
						ShouldHandle = new PredicateBuilder()
							.Handle<ArgumentException>(x => x.Message?.Contains("Cannot find last modification datetime") is true),
						MaxRetryAttempts = retryCount,
						Delay = TimeSpan.FromSeconds(retryDelayInSeconds),
						BackoffType = DelayBackoffType.Exponential,
						UseJitter = true,
					})
				.Build();

			pipeline.Execute(() => { result = action(); });

			return result;
		}
	}
}
