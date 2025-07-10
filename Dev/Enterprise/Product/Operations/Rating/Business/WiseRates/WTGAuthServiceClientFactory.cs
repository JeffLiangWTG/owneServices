namespace Enterprise.Rating.Business
{
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using AuthenticationService.Client;
	using CargoWise.Common;
	using Enterprise.Integration.Rating;
	using Polly;
	using Polly.Extensions.Http;
	using Polly.Retry;

	public class WTGAuthServiceClientFactory : IWTGAuthServciceClientFactory
	{
		public IWTGAuthServiceClient Create(string serviceURL)
		{
			Argument.NotNull(!string.IsNullOrEmpty(serviceURL), nameof(serviceURL));

			return new WTGAuthServiceClient(serviceURL, new RetryMessageHandler());
		}

		class RetryMessageHandler : DelegatingHandler
		{
			public RetryMessageHandler() : base(new HttpClientHandler()) { }

			readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
				Policy
				.Handle<HttpRequestException>()
				.OrTransientHttpError()
				.WaitAndRetryAsync(new[]
				{
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(3),
				});

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
				=> RetryPolicy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
		}
	}
}
