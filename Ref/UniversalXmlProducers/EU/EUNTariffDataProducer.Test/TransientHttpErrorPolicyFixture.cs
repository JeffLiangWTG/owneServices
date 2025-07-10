using System;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class TransientHttpErrorPolicyFixture
	{
		readonly HttpStatusCode[] transientErrors = {
			HttpStatusCode.InternalServerError,
			HttpStatusCode.RequestTimeout,
			HttpStatusCode.BadGateway,
			HttpStatusCode.GatewayTimeout,
			HttpStatusCode.ServiceUnavailable,
			HttpStatusCode.TooManyRequests
		};

		readonly HttpStatusCode[] permanentErrors = {
			HttpStatusCode.NotFound,
			HttpStatusCode.Forbidden
		};

		readonly TimeSpan testRetryDelay = TimeSpan.FromMilliseconds(5);

		Task<HttpResponseMessage> ClientStub(HttpStatusCode[] statusCodes, ref int invocationCount)
		{
			var statusCode = invocationCount < statusCodes.Length
				? statusCodes[invocationCount]
				: HttpStatusCode.OK;

			invocationCount++;

			return Task.FromResult(new HttpResponseMessage(statusCode));
		}

		[Test]
		public async Task TransientHttpErrorPolicy_WithTransientErrorsResolvedDuringRetries_ShouldReturnSuccess()
		{
			int retryCount = transientErrors.Length;
			int invocationCount = 0;
			var policy = TransientHttpErrorPolicyFactory.CreateTransientHttpErrorPolicy(retryCount, testRetryDelay);

			var result = await policy.ExecuteAsync(() => ClientStub(transientErrors, ref invocationCount));

			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(invocationCount, Is.EqualTo(retryCount + 1));
		}

		[Test]
		public async Task TransientHttpErrorPolicy_WithTransientErrorsNotResolvedDuringRetries_ShouldReturnLastAttemptedResult()
		{
			int retryCount = transientErrors.Length - 1;
			int invocationCount = 0;
			var policy = TransientHttpErrorPolicyFactory.CreateTransientHttpErrorPolicy(retryCount, testRetryDelay);

			var result = await policy.ExecuteAsync(() => ClientStub(transientErrors, ref invocationCount));

			Assert.That(result.StatusCode, Is.EqualTo(transientErrors[retryCount]));
			Assert.That(invocationCount, Is.EqualTo(retryCount + 1));
		}

		[Test]
		public void TransientHttpErrorPolicy_WithTransientErrorsNotResolvedDuringRetries_ShouldStopRetryingInReasonableTime()
		{
			var totalDuration = TransientHttpErrorPolicyFactory
				.SleepDurations(TransientHttpErrorPolicyFactory.RetryCount, TransientHttpErrorPolicyFactory.RetryFirstDelay)
				.Take(TransientHttpErrorPolicyFactory.RetryCount)
				.Aggregate((acc, t) => acc.Add(t));

			var expectedMaximumTotalWait = TimeSpan.FromSeconds(60);
			Assert.That(totalDuration, Is.LessThan(expectedMaximumTotalWait));
		}

		[Test]
		public async Task TransientHttpErrorPolicy_WithPermanentError_ShouldStopRetrying()
		{
			var transientErrorsCount = 2;
			var transientAndPermanentErrors = transientErrors.Take(transientErrorsCount).Concat(permanentErrors).ToArray();
			int retryCount = transientAndPermanentErrors.Length + 1;
			int invocationCount = 0;
			var policy = TransientHttpErrorPolicyFactory.CreateTransientHttpErrorPolicy(retryCount, testRetryDelay);

			var result = await policy.ExecuteAsync(() => ClientStub(transientAndPermanentErrors, ref invocationCount));

			Assert.That(result.StatusCode, Is.EqualTo(permanentErrors.First()));
			Assert.That(invocationCount, Is.EqualTo(transientErrorsCount + 1));
		}
	}
}
