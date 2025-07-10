using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Freight.ApiClient.Test
{
	[TestFixture]
	public class TestHttpClientApiClientTests
	{
		[Test]
		public async Task TestGetAsync_Success()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("[{'id':1,'name':'John Doe'}]"),
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient);

			// Act
			var response = await apiClient.GetAsync<List<User>>("/users") as ApiResponse<List<User>>;

			// Assert
			response.EnsureSuccessStatusCodeAsync().Wait();
			Assert.That(response.Content, Is.Not.Null);
			Assert.That(response.Content.Count, Is.EqualTo(1));
			Assert.That(response.Content[0].Name, Is.EqualTo("John Doe"));

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Exactly(1),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);
		}

		[Test]
		public async Task TestPostAsync_Success()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.Created,
					Content = new StringContent("{'id':1,'name':'John Doe'}"),
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient);
			var newUser = new User { Name = "John Doe" };
			var jsonPayload = JsonConvert.SerializeObject(newUser);

			// Act
			var response = await apiClient.PostAsync<User>("/users", jsonPayload) as ApiResponse<User>;

			// Assert
			response.EnsureSuccessStatusCodeAsync().Wait();
			Assert.That(response.Content, Is.Not.Null);
			Assert.That(response.Content.Name, Is.EqualTo("John Doe"));

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Exactly(1),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);
		}

		[Test]
		public async Task TestApiException_Create()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.BadRequest,
					Content = new StringContent("{'error':'Bad Request'}"),
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient);

			// Act & Assert
			ApiException exception = null;
			try
			{
				var response = await apiClient.GetAsync<List<User>>("/users") as ApiResponse<List<User>>;
				await response.EnsureSuccessStatusCodeAsync();
			}
			catch (ApiException ex)
			{
				exception = ex;
			}

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(exception.Content, Is.EqualTo("{'error':'Bad Request'}"));

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Exactly(3),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);
		}

		[Test]
		public async Task TestGetAsync_Retry_Success()
		{
			// Arrange
			int requestCount = 0;
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.Returns(() =>
				{
					requestCount++;
					if (requestCount <= 2)
					{
						return Task.FromResult(new HttpResponseMessage
						{
							StatusCode = HttpStatusCode.InternalServerError,
							Content = new StringContent("{'error':'Server Error'}"),
						});
					}
					else
					{
						return Task.FromResult(new HttpResponseMessage
						{
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("[{'id':1,'name':'John Doe'}]"),
						});
					}
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient)
			{
				RetryCount = 2
			};

			// Act
			var response = await apiClient.GetAsync<List<User>>("/users") as ApiResponse<List<User>>;

			// Assert
			response.EnsureSuccessStatusCodeAsync().Wait();
			Assert.That(response.Content, Is.Not.Null);
			Assert.That(response.Content.Count, Is.EqualTo(1));
			Assert.That(response.Content[0].Name, Is.EqualTo("John Doe"));

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Exactly(3),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);
		}

		[Test]
		public async Task TestGetAsync_CustomExceptionFactory()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.BadRequest,
					Content = new StringContent("{'error':'Bad Request'}"),
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient)
			{
				ExceptionFactory = (response) =>
				{
					if (!response.IsSuccessStatusCode)
					{
						return Task.FromResult(new CustomApiException("Custom exception message") as Exception);
					}
					return null;
				}
			};

			// Act & Assert
			CustomApiException exception = null;
			try
			{
				var response = await apiClient.GetAsync<List<User>>("/users") as ApiResponse<List<User>>;
				await response.EnsureSuccessStatusCodeAsync();
			}
			catch (CustomApiException ex)
			{
				exception = ex;
			}

			Assert.That(exception, Is.Not.Null);
			Assert.That(exception.Message, Is.EqualTo("Custom exception message"));

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Exactly(3),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);
		}

		[Test]
		public async Task TestCustomHeader()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) => new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("[{'id':1,'name':'John Doe'}]"),
					RequestMessage = request,
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};

			var apiClient = new HttpClientApiClient(httpClient);
			apiClient.CustomHeaders = new List<(string, string)> { ("Header", "Action") };

			// Act
			var response = await apiClient.GetAsync<List<User>>("/users") as ApiResponse<List<User>>;

			// Assert
			response.EnsureSuccessStatusCodeAsync().Wait();
			Assert.That(response.RequestMessage, Is.Not.Null);
			Assert.That(response.RequestMessage!.Headers.Contains("Header"), Is.True);
			Assert.That(response.RequestMessage.Headers.GetValues("Header").FirstOrDefault(), Is.EqualTo("Action"));
		}
	}

	class User
	{
		public string Name { get; set; }
	}

	[Serializable]
	class CustomApiException : Exception
	{
		public CustomApiException(string message) : base(message) { }

#if NETFRAMEWORK
		protected CustomApiException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
