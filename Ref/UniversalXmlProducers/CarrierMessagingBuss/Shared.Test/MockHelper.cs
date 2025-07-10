using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Moq;
using Moq.Protected;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	public sealed class MockHelper
	{
		public static HttpClient GetMockedHttpClient(HttpResponseMessage responseMessage, string uri)
			=> GetMockedHttpClient(() => responseMessage, uri);

		public static HttpClient GetMockedHttpClient(Func<HttpResponseMessage> getResponseMessage, string uri)
		{
			var handlerMock = new Mock<HttpMessageHandler>();

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(getResponseMessage);

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri(uri)
			};

			return httpClient;
		}

		public static IHttpClientFactory GetMockedHttpClientFactory(HttpClient httpClient)
		{
			var factoryMock = new Mock<IHttpClientFactory>();
			factoryMock
				.Setup(x => x.CreateClient())
				.Returns(httpClient);
			return factoryMock.Object;
		}

		public static IAccessTokenProvider GetTokenProvider(string token)
		{
			var tokenProviderMock = new Mock<IAccessTokenProvider>();
			tokenProviderMock
				.Setup(x => x.GetAccessToken())
				.Returns(token);
			return tokenProviderMock.Object;
		}

		public static IHttpWebHelper<T> GetHttpWebHelper<T>(Action<string, string> action, Func<Task<T>> getResponseResult) where T : class
		{
			var httpWebHelperMock = new Mock<IHttpWebHelper<T>>();
			httpWebHelperMock
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(getResponseResult)
				.Callback(action);
			return httpWebHelperMock.Object;
		}
	}
}
