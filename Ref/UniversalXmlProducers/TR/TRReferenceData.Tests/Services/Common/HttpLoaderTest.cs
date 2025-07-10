using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using NUnit.Framework;
using Moq.Protected;
using Moq;
using System.Threading;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Common
{
    class HttpLoaderTest
    {
		[Test]
		public async Task TestHttpLoader_LoadAsync_WithSuccessResponse_ReturnsContent()
		{
			var expectedContent = "Hello, world!";
			using (var mockHttpClient = CreateMockHttpClient(HttpStatusCode.OK, expectedContent))
			using (var loader = new HttpLoader(new Uri("https://abc.com"), mockHttpClient))
			{
				var result = await loader.LoadAsync();
				Assert.AreEqual(expectedContent, result);
			}
		}


		[TestCase(HttpStatusCode.InternalServerError)]
		[TestCase(HttpStatusCode.BadRequest)]
		[TestCase(HttpStatusCode.NotFound)]
		public async Task TestHttpLoader_LoadAsync_WithErrorStatus_ReturnsNull(HttpStatusCode statusCode)
		{
			using (var httpClient = CreateMockHttpClient(statusCode))
			using (var loader = new HttpLoader(new Uri("https://abc.com"), httpClient))
			{
				var result = await loader.LoadAsync();
				Assert.IsNull(result);
			}
		}

		[TestCase(typeof(TaskCanceledException))]
		[TestCase(typeof(HttpRequestException))]
		public async Task TestHttpLoader_LoadAsync_WithException_ReturnsNull(Type exceptionType)
		{
			var exception = (Exception)Activator.CreateInstance(exceptionType);

			using (var httpClient = CreateMockHttpClient(HttpStatusCode.OK, exception: exception))
			using (var loader = new HttpLoader(new Uri("https://example.com"), httpClient))
			{
				var result = await loader.LoadAsync();
				Assert.IsNull(result);
			}
		}

		HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseBody = "", Exception exception = null)
		{
			var mockHandler = new Mock<HttpMessageHandler>();

			if (exception != null)
			{
				mockHandler
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>()
					)
					.ThrowsAsync(exception);
			}
			else
			{
				mockHandler
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>()
					)
					.ReturnsAsync(() => CreateHttpResponseMessage(statusCode, responseBody));
			}

			return new HttpClient(mockHandler.Object);
		}

		static HttpResponseMessage CreateHttpResponseMessage(HttpStatusCode statusCode, string responseBody)
		{
			var responseMessage = new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(responseBody)
			};

			return responseMessage;
		}
	}
}
