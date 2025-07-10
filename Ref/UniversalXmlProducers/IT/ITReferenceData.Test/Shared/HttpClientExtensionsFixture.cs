using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business.Shared;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.Shared
{
	[TestFixture]
	sealed class HttpClientExtensionsFixture
	{
		[Test]
		public async Task GetWithRetryAsync()
		{
			using var httpClient = GetHttpClientWithResponse(HttpStatusCode.OK);
			using var response = await httpClient.GetWithRetryAsync(requestUri);
			var content = await response.Content.ReadAsStringAsync();
			Assert.AreEqual(ResponseContent, content);
		}

		[Test]
		public async Task GetWithRetryAsyncWhenRetries()
		{
			var httpMessageHandlerMock = GetHttpMessageHandlerMockWithRetry();
			using var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			using var response = await httpClient.GetWithRetryAsync(requestUri);
			var content = await response.Content.ReadAsStringAsync();
			Assert.AreEqual(ResponseContent, content);
			httpMessageHandlerMock.Verify();
		}

		[Test]
		public void GetWithRetryAsyncWhenThrows()
		{
			using var httpClient = GetHttpClientWithResponse(HttpStatusCode.NotFound);
			Assert.ThrowsAsync<HttpRequestException>(async () => await httpClient.GetWithRetryAsync(requestUri));
		}

		[Test]
		public async Task PostWithRetryAsync()
		{
			using var httpClient = GetHttpClientWithResponse(HttpStatusCode.OK);
			using var formContent = new FormUrlEncodedContent([]);
			using var response = await httpClient.PostWithRetryAsync(requestUri, formContent);
			var content = await response.Content.ReadAsStringAsync();
			Assert.AreEqual(ResponseContent, content);
		}


		[Test]
		public async Task PostWithRetryAsyncWhenRetries()
		{
			var httpMessageHandlerMock = GetHttpMessageHandlerMockWithRetry();
			using var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			using var formContent = new FormUrlEncodedContent([]);
			using var response = await httpClient.PostWithRetryAsync(requestUri, formContent);
			var content = await response.Content.ReadAsStringAsync();
			Assert.AreEqual(ResponseContent, content);
			httpMessageHandlerMock.Verify();
		}

		[Test]
		public void PostWithRetryAsyncWhenThrows()
		{
			using var httpClient = GetHttpClientWithResponse(HttpStatusCode.NotFound);
			using var formContent = new FormUrlEncodedContent([]);
			Assert.ThrowsAsync<HttpRequestException>(async () => await httpClient.PostWithRetryAsync(requestUri, formContent));
		}

		HttpClient GetHttpClientWithResponse(HttpStatusCode code)
		{
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(() => new HttpResponseMessage(code)
				{
					Content = new StringContent(ResponseContent)
				});

			return new HttpClient(handlerMock.Object);
		}

		Mock<HttpMessageHandler> GetHttpMessageHandlerMockWithRetry()
		{
			var fail = true;

			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(() =>
				{
					if (fail)
					{
						fail = false;

						return new HttpResponseMessage(HttpStatusCode.InternalServerError)
						{
							Content = new StringContent(ResponseContent)
						};
					}
					else
					{
						return new HttpResponseMessage(HttpStatusCode.OK)
						{
							Content = new StringContent(ResponseContent)
						};
					}
				})
				.Verifiable(Times.Exactly(2));

			return handlerMock;
		}


		const string ResponseContent = "RESPONSE";

		static readonly Uri requestUri = new("https://example.com/resource");
	}
}

