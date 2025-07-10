using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace CargoWise.RefDbRepo.ITReferenceData.Test
{
	internal static class HttpClientTestHelper
	{
		public static HttpClient GetHttpClientWithContent(string content)
		{
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					SendAsync,
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(content)
				});

			return new HttpClient(handlerMock.Object);
		}

		public static HttpClient GetHttpClientWithResponseMessage(HttpResponseMessage responseMessage)
		{
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					SendAsync,
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(() => responseMessage);

			return new HttpClient(handlerMock.Object);
		}

		const string SendAsync = "SendAsync";
	}
}
