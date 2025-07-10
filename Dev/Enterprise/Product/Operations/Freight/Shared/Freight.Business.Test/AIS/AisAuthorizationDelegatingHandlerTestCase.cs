using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Freight.AIS.Testing
{
	sealed class AisAuthorizationDelegatingHandlerTestCase : TestCase
	{
		public void TestSendAsync_SetsAuthorizationHeader()
		{
			// Arrange
			var innerHandlerMock = new Mock<HttpMessageHandler>();
			innerHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage()));

			var vesselMovementsUrlGeneratorMock = new Mock<IVesselMovementsUrlGenerator>();
			vesselMovementsUrlGeneratorMock
				.Setup(generator => generator.GetTokenAsync(It.IsAny<string>(), It.IsAny<OrgContact>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new VesselMovementsAuthTokenResult
				{
					Token = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.MyAccount, "Token")
				}));

			var sut = new AisAuthorizationDelegatingHandler(vesselMovementsUrlGeneratorMock.Object)
			{
				InnerHandler = innerHandlerMock.Object
			};

			var httpClient = new HttpClient(sut);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels"));

			// Act
			httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

			// Assert
			AssertEquals("HTTP request Authorization header must be set.",
				new AuthenticationHeaderValue("Bearer", "Token"),
				httpRequestMessage.Headers.Authorization);
		}

		[ExpectExceptionMessage(typeof(HttpRequestException), "Oops.")]
		public void TestSendAsync_ThrowsHttpRequestException_WhenTokenCannotBeReceivedDueToError()
		{
			// Arrange
			var innerHandlerMock = new Mock<HttpMessageHandler>();
			innerHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage()));

			var vesselMovementsUrlGeneratorMock = new Mock<IVesselMovementsUrlGenerator>();
			vesselMovementsUrlGeneratorMock
				.Setup(generator => generator.GetTokenAsync(It.IsAny<string>(), It.IsAny<OrgContact>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new VesselMovementsAuthTokenResult
				{
					ErrorMessage = "Oops."
				}));

			var sut = new AisAuthorizationDelegatingHandler(vesselMovementsUrlGeneratorMock.Object)
			{
				InnerHandler = innerHandlerMock.Object
			};

			var httpClient = new HttpClient(sut);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels"));

			// Act
			httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
		}

		[ExpectExceptionMessage(typeof(HttpRequestException), "Authentication required.")]
		public void TestSendAsync_ThrowsHttpRequestException_WhenUrlRedirectResponseIsReceived()
		{
			// Arrange
			var innerHandlerMock = new Mock<HttpMessageHandler>();
			innerHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage()));

			var vesselMovementsUrlGeneratorMock = new Mock<IVesselMovementsUrlGenerator>();
			vesselMovementsUrlGeneratorMock
				.Setup(generator => generator.GetTokenAsync(It.IsAny<string>(), It.IsAny<OrgContact>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new VesselMovementsAuthTokenResult
				{
					RedirectUrl = new Uri("https://identityprovider.wtg.local/authorize")
				}));

			var sut = new AisAuthorizationDelegatingHandler(vesselMovementsUrlGeneratorMock.Object)
			{
				InnerHandler = innerHandlerMock.Object
			};

			var httpClient = new HttpClient(sut);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://api.wtg.local/v1/vessels"));

			// Act
			httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
		}
	}
}
