using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging;
using Enterprise.Registry.Business;
using WTG.Foundation.Http;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.AirlineMessaging
{
	public class PelicanApiClientTest : TestCaseWithFactory
	{
		public void TestXmlSentToServer()
		{
			var handler = new MockHttpMessageHandlerForTest(0);
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler)))
			{
				var responses = new List<string>();

				for (var i = 0; i < 10; i++)
				{
					var sender = new PelicanApiClient(new Uri("http://wtg.com"));
					var response = sender.Post("<xml></xml>", new CancellationToken());

					responses.Add(response.Content);
				}

				AssertEquals(10, handler.receivedRequestBodies.Count);
				AssertEquals(true, handler.receivedRequestBodies.All(x => string.Equals(x, "<xml></xml>")));
				AssertEquals(10, responses.Count);
			}
		}

		public void TestRetry()
		{
			RetryTestCase(retryAttempts: 0, serverFailureCount: 0, expectedOkResponse: true);
			RetryTestCase(retryAttempts: 0, serverFailureCount: 1, expectedOkResponse: false);
			RetryTestCase(retryAttempts: 3, serverFailureCount: 1, expectedOkResponse: true);
			RetryTestCase(retryAttempts: 3, serverFailureCount: 3, expectedOkResponse: true);
			RetryTestCase(retryAttempts: 3, serverFailureCount: 4, expectedOkResponse: false);
		}

		void RetryTestCase(int retryAttempts, int serverFailureCount, bool expectedOkResponse)
		{
			using (FreightDataRegistry.Instance.PelicanApiRetryAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, retryAttempts))
			{
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandlerForTest(serverFailureCount))))
				{
					var sender = new PelicanApiClient(new Uri("http://wtg.com"));
					var response = sender.Post("", new CancellationToken());

					AssertEquals($"Request should {(expectedOkResponse ? "succeed" : "fail")}",
						expectedOkResponse ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, response.StatusCode);
					AssertEquals(expectedOkResponse ? $"This is request #{Math.Min(retryAttempts, serverFailureCount) + 1}" : null, response.Content);
				}
			}
		}

		public void TestTimeout()
		{
			Exception exception = null;
			TimeSpan delay;
			var timeoutSettingInSeconds = 3;

			using (FreightDataRegistry.Instance.PelicanApiTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeoutSettingInSeconds))
			{
				var sw = new Stopwatch();
				try
				{
					// This IP url should be unreachable so it should hit timeout
					var sender = new PelicanApiClient(new Uri("http://123.123.13.13"));
					sw.Start();
					sender.Post("", new CancellationToken());
				}
				catch (Exception ex)
				{
					exception = ex;
				}
				sw.Stop();
				delay = sw.Elapsed;
			}

			Assert("Request should timeout", exception != null && exception is TaskCanceledException);
			Assert("Request should wait for timeout setting", delay >= TimeSpan.FromSeconds(timeoutSettingInSeconds) && delay < TimeSpan.FromSeconds(timeoutSettingInSeconds + 1));
		}

		class MockHttpMessageHandlerForTest : HttpMessageHandler
		{
			readonly int failureCount;
			int requestCount;
			public List<string> receivedRequestBodies;

			public MockHttpMessageHandlerForTest(int failureCount)
			{
				this.failureCount = failureCount;
				receivedRequestBodies = new List<string>();
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (request.Content != null)
				{
					var requestBody = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					receivedRequestBodies.Add(requestBody);
				}

				var response = new HttpResponseMessage
				{
					Content = new StringContent($"This is request #{++requestCount}")
				};
				response.StatusCode = requestCount <= failureCount ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;

				return await Task.FromResult(response);
			}
		}
	}
}
