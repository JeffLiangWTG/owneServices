using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class HttpClientWithCookieSupportTest
	{
		[Test]
		public void TestGet()
		{
			using (var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK))
			{
				expectedResponse.Content = new StringContent("Mock Response");
				HandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(expectedResponse);

				var requestUri = new Uri("https://example.com");
				var response = Client.Get(requestUri);
				var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("Mock Response", content);
			}
		}

		IHttpClient Client => client ?? (client = new HttpClientWithCookieSupport(HandlerMock.Object));
		HttpClientWithCookieSupport client;

		Mock<HttpMessageHandler> HandlerMock => handlerMock ?? (handlerMock = new Mock<HttpMessageHandler>());
		Mock<HttpMessageHandler> handlerMock;
	}
}
