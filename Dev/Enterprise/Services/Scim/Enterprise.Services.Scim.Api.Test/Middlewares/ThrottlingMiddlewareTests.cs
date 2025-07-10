using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using CargoWise.Async;
using Enterprise.Services.Scim.Api.Config;
using Enterprise.Services.Scim.Api.Middlewares;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test.Middlewares
{
	public class ThrottlingMiddlewareTests : TestCase
	{
		Mock<OwinMiddleware> nextMiddlewareMock;
		ThrottlingMiddleware middleware;

		Mock<IOwinContext> contextMock;
		Mock<IOwinRequest> requestMock;
		StubResponse stubResponse;

		public void TestInvoke_ShouldThrottle_WhenRequestLimitExceeded()
		{
			// Arrange
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("1.1.1.1");
			var clientIp = "1.1.1.1";
			HttpRuntime.Cache.Insert(clientIp, 11, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

			// Act
			GetAsyncRequest(middleware, contextMock.Object);
			stubResponse.InvokeOnSendingHeaders();

			// Assert
			AssertEquals(11, HttpRuntime.Cache[clientIp]);
			AssertEquals(429, stubResponse.StatusCode);
			AssertEquals("Too many requests", stubResponse.ReasonPhrase);
			AssertCollectionContains("Retry-After", stubResponse.Headers.Keys);
		}

		public void TestInvoke_ShouldThrottle_WhenRequestLimitExceeded_Multiple()
		{
			// Arrange
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("1.1.1.1, 2.2.2.2, 3.3.3.3");
			var clientIp = "3.3.3.3";
			HttpRuntime.Cache.Insert(clientIp, 11, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

			// Act
			GetAsyncRequest(middleware, contextMock.Object);
			stubResponse.InvokeOnSendingHeaders();

			// Assert
			AssertEquals(11, HttpRuntime.Cache[clientIp]);
			AssertEquals(429, stubResponse.StatusCode);
			AssertEquals("Too many requests", stubResponse.ReasonPhrase);
			AssertCollectionContains("Retry-After", stubResponse.Headers.Keys);
		}

		public void TestInvoke_ShouldNotThrottle_WhenRequestLimitNotExceeded_NewEntry()
		{
			// Arrange
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("1.1.1.1");
			var clientIp = "1.1.1.1";

			// Act
			GetAsyncRequest(middleware, contextMock.Object);

			// Assert
			nextMiddlewareMock.Verify(n => n.Invoke(contextMock.Object), Times.Once);
			AssertEquals(1, HttpRuntime.Cache[clientIp]);
			Thread.Sleep(1500);
			AssertEquals(null, HttpRuntime.Cache[clientIp]);
		}

		public void TestInvoke_ShouldNotThrottle_WhenRequestLimitNotExceeded_ExistingEntry()
		{
			// Arrange
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("1.1.1.1");
			var clientIp = "1.1.1.1";
			HttpRuntime.Cache.Insert(clientIp, 1, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

			// Act
			GetAsyncRequest(middleware, contextMock.Object);
			stubResponse.InvokeOnSendingHeaders();

			// Assert
			nextMiddlewareMock.Verify(n => n.Invoke(contextMock.Object), Times.Once);
			AssertEquals(2, HttpRuntime.Cache[clientIp]);
			Thread.Sleep(1500);
			AssertEquals(null, HttpRuntime.Cache[clientIp]);
		}

		public void TestInvoke_ShouldNotThrottle_WhenClientIpIsAllowed()
		{
			// Arrange
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("127.0.0.1");
			var clientIp = "127.0.0.1";

			// Act
			GetAsyncRequest(middleware, contextMock.Object);
			stubResponse.InvokeOnSendingHeaders();

			// Assert
			AssertEquals(null, HttpRuntime.Cache[clientIp]);
			nextMiddlewareMock.Verify(n => n.Invoke(contextMock.Object), Times.Once);
		}

		void GetAsyncRequest(ThrottlingMiddleware requestBuilder, IOwinContext context)
		{
			AsyncHelper.RunTask(() => requestBuilder.Invoke(context), CancellationToken.None, "async").GetAwaiter().GetResult();
		}

		protected override void SetUp()
		{
			var appSettingsMock = new Mock<IAppSettings>();
			appSettingsMock.Setup(a => a.ThrottleTimeInSeconds).Returns(1);
			appSettingsMock.Setup(a => a.ThrottleMaxRequestCount).Returns(10);
			appSettingsMock.Setup(a => a.ThrottleSkippedIps).Returns("127.0.0.1");
			appSettingsMock.Setup(a => a.SafelistSkippedIps).Returns("127.0.0.1");

			nextMiddlewareMock = new Mock<OwinMiddleware>(null);
			middleware = new ThrottlingMiddleware(nextMiddlewareMock.Object, appSettingsMock.Object);
			contextMock = new Mock<IOwinContext>();
			requestMock = new Mock<IOwinRequest>();
			requestMock.Setup(r => r.RemoteIpAddress).Returns("127.0.0.1");
			requestMock.Setup(r => r.Headers["X-Forwarded-For"]).Returns("127.0.0.1");
			contextMock.Setup(c => c.Request).Returns(requestMock.Object);
			stubResponse = new StubResponse(contextMock.Object);
			contextMock.Setup(c => c.Response).Returns(stubResponse);

			base.SetUp();
		}
		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}
	}

	class StubResponse(IOwinContext context) : IOwinResponse
	{
		Action<object> _callback;
		object _state;

		// Stubimplementation that only supports one callback and state
		public void OnSendingHeaders(Action<object> callback, object state)
		{
			_callback = callback;
			_state = state;
		}

		/// <summary>
		/// Simulates that another middleware has started writing to the stream.
		/// </summary>
		public void InvokeOnSendingHeaders()
		{
			_callback?.Invoke(_state);
		}

		#region Unimplemented methods

		public void Redirect(string location)
		{
			throw new NotImplementedException();
		}

		public void Write(string text)
		{
			throw new NotImplementedException();
		}

		public void Write(byte[] data)
		{
			throw new NotImplementedException();
		}

		public void Write(byte[] data, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(string text)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(string text, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(byte[] data)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(byte[] data, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task WriteAsync(byte[] data, int offset, int count, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public T Get<T>(string key)
		{
			throw new NotImplementedException();
		}

		public IOwinResponse Set<T>(string key, T value)
		{
			throw new NotImplementedException();
		}

		#endregion

		public IDictionary<string, object> Environment { get; private set; }
		public IOwinContext Context { get; private set; } = context;
		public int StatusCode { get; set; }
		public string ReasonPhrase { get; set; }
		public string Protocol { get; set; }
		public IHeaderDictionary Headers { get; private set; } = new HeaderDictionary(new Dictionary<string, string[]>());
		public ResponseCookieCollection Cookies { get; private set; }
		public long? ContentLength { get; set; }
		public string ContentType { get; set; }
		public DateTimeOffset? Expires { get; set; }
		public string ETag { get; set; }
		public Stream Body { get; set; }
	}
}
