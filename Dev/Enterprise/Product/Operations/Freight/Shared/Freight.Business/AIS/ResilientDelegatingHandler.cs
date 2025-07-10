using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Wrap;

namespace Enterprise.Freight.AIS
{
	public class ResilientDelegatingHandler : DelegatingHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return AsyncPolicy.Value.ExecuteAsync(token => base.SendAsync(request, token), cancellationToken, continueOnCapturedContext: true);
		}

		static readonly Overridable<IAsyncPolicy<HttpResponseMessage>> AsyncPolicy = new Overridable<IAsyncPolicy<HttpResponseMessage>>(BuildHttpResiliencyPolicy());

		static IAsyncPolicy<HttpResponseMessage> BuildHttpResiliencyPolicy()
		{
			var circuitBreakerPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromMinutes(5));
			var retryPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.WaitAndRetryAsync(new[]
				{
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(3),
				});

			return circuitBreakerPolicy.WrapAsync(retryPolicy);
		}

		internal static void ResetCircuitBreaker()
		{
			if ((AsyncPolicy.Value as AsyncPolicyWrap<HttpResponseMessage>)?.Outer is AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreaker)
			{
				circuitBreaker.Reset();
			}
		}
	}
}
