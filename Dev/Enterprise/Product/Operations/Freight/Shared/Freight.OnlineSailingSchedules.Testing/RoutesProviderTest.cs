using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class RoutesProviderTests : TestCaseWithFactory
	{
		public void TestGetRoutes_RequestManager_ShouldNotBeNull()
		{
			var routesProvider = new RoutesProvider(Factory);
			AssertExceptionThrown(typeof(ArgumentNullException), () => routesProvider.GetRoutes(string.Empty, null));
		}

		public void TestGetRoutes_IsSuppressed_ShouldNotTryToAccessService()
		{
			var requestManager = new Mock<IServiceRequestManager>(MockBehavior.Strict);

			requestManager.Setup(r => r.IsSuppressed())
				.Returns(true);

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new Exception("Should not try to access HttpClient");
				}
			};

			AssertEquals(0, routesProvider.GetRoutes(string.Empty, requestManager.Object).Length);
		}

		public void TestGetRoutes_TimeoutSetOnHttpClient_OnSuccessfullRequest()
		{
			var requestManager = new Mock<IServiceRequestManager>(MockBehavior.Strict);

			requestManager.Setup(r => r.IsSuppressed())
				.Returns(false);

			const int requestTimeoutSeconds = 8;

			requestManager.Setup(r => r.RequestTimeout)
				.Returns(TimeSpan.FromSeconds(requestTimeoutSeconds));

			requestManager.Setup(r => r.OnSuccessfulRequest());

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					AssertEquals(TimeSpan.FromSeconds(requestTimeoutSeconds), httpClient.Timeout);

					return new HttpResponseMessage(HttpStatusCode.Accepted)
					{
						Content = new StringContent("[]")
					};
				}
			};

			AssertEquals(0, routesProvider.GetRoutes(string.Empty, requestManager.Object).Length);
		}

		public void TestGetRoutes_TooManyRecordsReturned()
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
					{
#pragma warning disable WTG2007    // Testing functionality.
						ReasonPhrase = "The content length is more than 2000000",
						Content = new StringContent(string.Empty)
#pragma warning restore WTG2007 // Testing functionality.
					};
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes(string.Empty, requestManager);

			AssertEquals("Too many records were returned. Please modify the filters to narrow down your search.", notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetRoutes_NumberOfRecordsReturnedLimit()
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
#pragma warning disable WTG2007 // Testing functionality.
						ReasonPhrase = "More than 500 records returned.",
						Content = new StringContent("{\"title\":\"MaxRecordsReached\", " +
							"\"status\":400,\"detail\":\"More than 500 records returned.\",\"traceId\":\"00-c6490eda589e264988fb4a56e054279e-0da4f3d613fa1045-00\"}")
#pragma warning restore WTG2007 // Testing functionality.
					};
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes(string.Empty, requestManager);

			AssertEquals("Too many records were returned. Please modify the filters to narrow down your search.", notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetRoutes_ScacNotSupported()
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
#pragma warning disable WTG2007 // Testing functionality. 
						ReasonPhrase = "One or more validation errors occurred.",
						Content = new StringContent("{\"title\":\"One or more validation errors occurred.\"," +
							"\"status\":400,\"traceId\":\"00-d8b5cd9edd9ada4d8485ae3ccf18b26b-5da34a0c39aac04e-00\",\"errors\":{ \"CarrierCode\":[\"Carrier SCAC is not supported.\"]}}")
#pragma warning restore WTG2007 // Testing functionality. 
					};
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes(string.Empty, requestManager);

			AssertEquals("Carrier SCAC is not supported.", notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetRoutes_InternalServerError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.InternalServerError);
		}

		public void TestGetRoutes_NotImplementedError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.NotImplemented);
		}

		public void TestGetRoutes_BadGatewayError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.BadGateway);
		}

		public void TestGetRoutes_ServiceUnavailableError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.ServiceUnavailable);
		}

		public void TestGetRoutes_GatewayTimeoutError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.GatewayTimeout);
		}

		public void TestGetRoutes_HttpVersionNotSupportedError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.HttpVersionNotSupported);
		}

		void CheckServerErrorExceptionHandling(HttpStatusCode statusCode)
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(statusCode)
					{
#pragma warning disable WTG2007 // Testing functionality. 
						ReasonPhrase = "internal server error",
						Content = new StringContent("Detailed error message of internal server error")
#pragma warning restore WTG2007 // Testing functionality. 
					};
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes(string.Empty, requestManager);

			AssertEquals(requestManager.ExceptionErrorMessage, notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals("Should not report exception for " + statusCode, 0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestGetRoutes_ProxyAuthenticationRequired()
		{
			GetRoutes_ProxyAuthenticationRequired_Test(isAutoInitiatedServiceRequestTest: true);
			GetRoutes_ProxyAuthenticationRequired_Test(isAutoInitiatedServiceRequestTest: false);
		}

		void GetRoutes_ProxyAuthenticationRequired_Test(bool isAutoInitiatedServiceRequestTest)
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new HttpRequestException(string.Empty, new WebException("The remote server returned an error: (407) Proxy Authentication Required."));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = isAutoInitiatedServiceRequestTest
				? new AutoInitiatedServiceRequestManager(notifications)
				: (OnlineScheduleServiceRequestManager)new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertEquals(0, routesProvider.GetRoutes("LoadPort=IDJKT&EtdFrom=2016-12-19&DischargePort=USLAX&EtaFrom=2016-12-19", requestManager).Length);

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var expectedNotification = isAutoInitiatedServiceRequestTest ? string.Empty : "Please configure your proxy server to be able to use Global Sailing Schedules.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetRoutes_InvalidRemoteCertificate()
		{
			GetRoutes_InvalidRemoteCertificate_Test(isAutoInitiatedServiceRequestTest: true);
			GetRoutes_InvalidRemoteCertificate_Test(isAutoInitiatedServiceRequestTest: false);
		}

		void GetRoutes_InvalidRemoteCertificate_Test(bool isAutoInitiatedServiceRequestTest)
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new HttpRequestException(string.Empty,
						new WebException("The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.",
						new AuthenticationException("The remote certificate is invalid according to the validation procedure.")));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = isAutoInitiatedServiceRequestTest
				? new AutoInitiatedServiceRequestManager(notifications)
				: (OnlineScheduleServiceRequestManager)new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertEquals(0, routesProvider.GetRoutes("LoadPort=IDJKT&EtdFrom=2016-12-19&DischargePort=USLAX&EtaFrom=2016-12-19", requestManager).Length);

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var expectedNotification = isAutoInitiatedServiceRequestTest ? string.Empty : "Please check that your certificate is installed and enabled or check your proxy server settings to be able to use Global Sailing Schedules.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetRoutes_ServerProtocolViolation()
		{
			GetRoutes_ServerProtocolViolation(isAutoInitiatedServiceRequestTest: true);
			GetRoutes_ServerProtocolViolation(isAutoInitiatedServiceRequestTest: false);
		}

		void GetRoutes_ServerProtocolViolation(bool isAutoInitiatedServiceRequestTest)
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new HttpRequestException(string.Empty,
						new WebException(string.Empty, WebExceptionStatus.ServerProtocolViolation));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = isAutoInitiatedServiceRequestTest
				? new AutoInitiatedServiceRequestManager(notifications)
				: (OnlineScheduleServiceRequestManager)new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertEquals(0, routesProvider.GetRoutes("LoadPort=IDJKT&EtdFrom=2016-12-19&DischargePort=USLAX&EtaFrom=2016-12-19", requestManager).Length);

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var expectedNotification = isAutoInitiatedServiceRequestTest ? string.Empty : "The server committed a protocol violation. Please try again later.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetRoutes_ForbiddenException()
		{
			GetRoutes_ForbiddenException(isAutoInitiatedServiceRequestTest: true);
			GetRoutes_ForbiddenException(isAutoInitiatedServiceRequestTest: false);
		}

		void GetRoutes_ForbiddenException(bool isAutoInitiatedServiceRequestTest)
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new HttpRequestException(string.Empty,
						new WebException("The remote server returned an error: (403) Forbidden.", WebExceptionStatus.ProtocolError));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = isAutoInitiatedServiceRequestTest
				? new AutoInitiatedServiceRequestManager(notifications)
				: (OnlineScheduleServiceRequestManager)new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertEquals(0, routesProvider.GetRoutes("LoadPort=IDJKT&EtdFrom=2016-12-19&DischargePort=USLAX&EtaFrom=2016-12-19", requestManager).Length);

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var expectedNotification = isAutoInitiatedServiceRequestTest ? string.Empty : "Unable to connect to Global Sailing Schedules Server. Please check your connection. Firewalls/Proxies may restrict your connection.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetRoutes_SslConnectionError()
		{
			GetRoutes_SslConnectionError_Test(isAutoInitiatedServiceRequestTest: true);
			GetRoutes_SslConnectionError_Test(isAutoInitiatedServiceRequestTest: false);
		}

		void GetRoutes_SslConnectionError_Test(bool isAutoInitiatedServiceRequestTest)
		{
			var callCount = 0;
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					callCount++;
					throw new HttpRequestException(string.Empty,
						new WebException(
							"The request was aborted: Could not create SSL/TLS secure channel.",
							WebExceptionStatus.SecureChannelFailure));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = isAutoInitiatedServiceRequestTest
				? new AutoInitiatedServiceRequestManager(notifications)
				: (OnlineScheduleServiceRequestManager)new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertEquals(0, routesProvider.GetRoutes("LoadPort=IDJKT&EtdFrom=2016-12-19&DischargePort=USLAX&EtaFrom=2016-12-19", requestManager).Length);

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(3, callCount);

			var expectedNotification = isAutoInitiatedServiceRequestTest ? string.Empty : "Could not create SSL/TLS secure channel. Please try again later.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetRoutes_HandleException()
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new Exception("Unknown exception",
						new WebException("Some web problem", new Exception("more info"), WebExceptionStatus.PipelineFailure, null));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes(string.Empty, requestManager);

			AssertEquals("Error occurred while loading routes from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			AssertEquals(1, ErrorReporter.TotalErrorCount);

			AssertContains("Error occurred while loading routes from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/routes/search?
ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			Assert(ErrorReporter.LastExceptionReported.Data.Contains("Search Parameters"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Header"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Request"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Status"));

			ErrorReporter.Clear();
		}

		public void TestGetRoutes_HandleException_WithResponse()
		{
			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
#pragma warning disable WTG2007 // Testing Functionality
						ReasonPhrase = string.Empty,
						Content = new StringContent("{\"Message\":\"More than 500 records returned.\"}")
#pragma warning restore WTG2007 // Testing Functionality
					};
					message.Headers.Add("Connection", "close");
					message.Headers.Add("Cache-Control", "no-cache");
					message.RequestMessage = new HttpRequestMessage(new HttpMethod("Get"), requestUri);
					message.StatusCode = HttpStatusCode.BadRequest;
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", requestManager);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Error occurred while loading routes from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", ErrorReporter.LastExceptionReported.Data["Search Parameters"].ToString());
			AssertContains(@"Connection: close
Cache-Control: no-cache", ErrorReporter.LastExceptionReported.Data["Response Header"].ToString());
			AssertContains("Method: Get, RequestUri:", ErrorReporter.LastExceptionReported.Data["Response Request"].ToString());
			AssertContains("BadRequest", ErrorReporter.LastExceptionReported.Data["Response Status"].ToString());

			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/routes/search?", ErrorReporter.LastMessageReported);
			AssertContains("ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			AssertEquals("Error occurred while loading routes from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			ErrorReporter.Clear();
		}

		public void TestGetRoutes_HandleException_InvalidJsonResponse()
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent("//{}")
					};
					message.Headers.Add("Pragma", "no-cache");
					message.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
					message.RequestMessage = new HttpRequestMessage(new HttpMethod("Get"), requestUri);
					message.StatusCode = HttpStatusCode.OK;
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", requestManager);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Error occurred while loading routes from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains("Exception thrown", "'/' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", ErrorReporter.LastMessageReported);
			AssertContains("Data is reported", "//{}", ErrorReporter.LastMessageReported);
			AssertContains("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", ErrorReporter.LastExceptionReported.Data["Search Parameters"].ToString());
			AssertContains(@"Pragma: no-cache
Cache-Control: no-store, no-cache, max-age=0", ErrorReporter.LastExceptionReported.Data["Response Header"].ToString());
			AssertContains("Method: Get, RequestUri:", ErrorReporter.LastExceptionReported.Data["Response Request"].ToString());
			AssertContains("OK", ErrorReporter.LastExceptionReported.Data["Response Status"].ToString());

			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/routes/search?", ErrorReporter.LastMessageReported);
			AssertContains("ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			AssertEquals("Error occurred while loading routes from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestGetRoutes_HandleException_Forbidden()
		{
			ErrorReporter.Clear();

			var routesProvider = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.Forbidden)
					{
						Content = new StringContent("Proxy issue")
					};
					message.Headers.Add("Pragma", "no-cache");
					message.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
					message.RequestMessage = new HttpRequestMessage(new HttpMethod("Get"), requestUri);
					message.StatusCode = HttpStatusCode.Forbidden;
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetRoutes("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", requestManager);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Error occurred while loading routes from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", ErrorReporter.LastExceptionReported.Data["Search Parameters"].ToString());
			AssertContains(@"Pragma: no-cache
Cache-Control: no-store, no-cache, max-age=0", ErrorReporter.LastExceptionReported.Data["Response Header"].ToString());
			AssertContains("Method: Get, RequestUri:", ErrorReporter.LastExceptionReported.Data["Response Request"].ToString());
			AssertContains("Forbidden", ErrorReporter.LastExceptionReported.Data["Response Status"].ToString());

			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/routes/search?", ErrorReporter.LastMessageReported);
			AssertContains("ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			AssertEquals("Error occurred while loading routes from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			ErrorReporter.Clear();
		}
	}

	class RoutesProviderForTesting : RoutesProvider
	{
		public RoutesProviderForTesting(BusinessObjectFactory factory) : base(factory)
		{
		}

		public Func<IApiClient, string, HttpResponseMessage> MockResponseGetter;

		protected override IApiResponse<ServiceModel.Route[]> TryGetResponse(string endpoint, IApiClient client)
		{
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(() => MockResponseGetter.Invoke(client, endpoint));

			var httpClient = new HttpClient(handlerMock.Object)
			{
				BaseAddress = new Uri("http://test.com/"),
			};
			var apiClient = ObjectFactory.Get<IApiClient>("HttpClient", httpClient);
			apiClient.Timeout = client.Timeout;
			apiClient.ExceptionFactory = client.ExceptionFactory;
			apiClient.ContentSerializer = client.ContentSerializer;
			return base.TryGetResponse(endpoint, apiClient);
		}
	}
}
