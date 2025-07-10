using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Enterprise.Freight.AIS.Testing
{
	sealed class ResilientDelegatingHandlerTestCase : TestCase
	{
		public void TestSendAsync_RetriesWhenTransientErrorObserved()
		{
			// Arrange
			var referenceRetryIntervals = new[]
			{
				TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)
			};

			var httpRequestTimeStamps = new List<DateTime>();
			var innerHandlerMock = new Mock<HttpMessageHandler>();
			innerHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable }))
				.Callback(() => httpRequestTimeStamps.Add(DateTime.Now));

			var resilientDelegatingHandler = new ResilientDelegatingHandler
			{
				InnerHandler = innerHandlerMock.Object
			};

			var httpClient = new HttpClient(resilientDelegatingHandler);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels"));

			SystemClock.SleepAsync = (sleepTime, cancellationToken) =>
			{
				var task = Task.Delay(sleepTime, cancellationToken);
				task.Wait(cancellationToken);
				return Task.CompletedTask;
			};

			// Act
			httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

			// Assert
			CombineAssertions("Retry policy: first attempt + 2 retries", () =>
			{
				AssertEquals("3 retries expected.", 3, httpRequestTimeStamps.Count);

				for (int i = 1; i < httpRequestTimeStamps.Count; i++)
				{
					var actualWaitInterval = httpRequestTimeStamps[i] - httpRequestTimeStamps[i - 1];
					var expectedWaitInterval = referenceRetryIntervals[i - 1];
					AssertGreaterThan($"Retry attempt #{i} must not be made earlier than expected.",
						actualWaitInterval, expectedWaitInterval);
				}
			});
		}

		public void TestSendAsync_BreaksTheCircuitWhenTooManyErrorsObserved()
		{
			// Arrange
			var innerHandlerMock = new Mock<HttpMessageHandler>();
			innerHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable }));

			var resilientDelegatingHandler = new ResilientDelegatingHandler
			{
				InnerHandler = innerHandlerMock.Object
			};

			var httpClient = new HttpClient(resilientDelegatingHandler);

			var totalTimeSlept = 0;
			SystemClock.SleepAsync = (sleepTime, cancellationToken) =>
			{
				totalTimeSlept += sleepTime.Seconds;
				return Task.CompletedTask;
			};

			// Act and Assert
			CombineAssertions("Circuit Breaker policy: exception should be thrown once the circuit-breaker is open after 5 consecutive exceptions.", () =>
			{
				AssertNoExceptionThrown(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/1")))
					.GetAwaiter().GetResult());

				AssertNoExceptionThrown(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/2")))
					.GetAwaiter().GetResult());

				AssertNoExceptionThrown(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/3")))
					.GetAwaiter().GetResult());

				AssertNoExceptionThrown(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/4")))
					.GetAwaiter().GetResult());

				AssertNoExceptionThrown(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/5")))
					.GetAwaiter().GetResult());

				AssertExceptionThrown<BrokenCircuitException<HttpResponseMessage>>(() => httpClient
					.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels/6")))
					.GetAwaiter().GetResult());

				AssertEquals("Retry policy: each of first 5 requests waits for 4 (1+3) seconds", 20, totalTimeSlept);
			});
		}

		protected override void TearDown()
		{
			base.TearDown();

			ResilientDelegatingHandler.ResetCircuitBreaker();
		}
	}
}
