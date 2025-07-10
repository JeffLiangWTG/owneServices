using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.OnlineSailingSchedules.Exceptions;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.Freight.OnlineSailingSchedules.ServiceRequestManager;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class PortCallProviderTest : TestCaseWithFactory
	{
		public void TestGetPortCall_RequestManager_ShouldNotBeNull()
		{
			var portCallProvider = new PortCallProvider();
			AssertExceptionThrown(typeof(ArgumentNullException), () => portCallProvider.GetPortCall(string.Empty, null));
		}

		public void TestGetPortCall_IsSuppressed_ShouldNotTryToAccessService()
		{
			var requestManager = new Mock<IServiceRequestManager>();

			requestManager.Setup(x => x.IsSuppressed()).Returns(true);

			var portCallProvider = new PortCallProviderForTesting
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new Exception("Should not try to access HttpClient");
				}
			};

			AssertNull(portCallProvider.GetPortCall(string.Empty, requestManager.Object).Items);
		}

		public void TestGetPortCall_TimeoutSetOnHttpClient_OnSuccessfullRequest()
		{
			var requestManager = new Mock<IServiceRequestManager>();

			requestManager.Setup(x => x.IsSuppressed()).Returns(false);
			requestManager.Setup(x => x.RequestTimeout).Returns(TimeSpan.FromSeconds(8));
			requestManager.Setup(x => x.OnSuccessfulRequest());

			var portCallProvider = new PortCallProviderForTesting
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					AssertEquals(TimeSpan.FromSeconds(8), httpClient.Timeout);
					return new HttpResponseMessage(HttpStatusCode.Accepted);
				}
			};

			AssertNull(portCallProvider.GetPortCall(string.Empty, requestManager.Object).Items);
		}

		public void TestGetPortCall_SslConnectionError()
		{
			var callCount = 0;
			var portCallProvider = new PortCallProviderForTesting()
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
			var requestManager = new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertNotNull(portCallProvider.GetPortCall(string.Empty, requestManager));

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(3, callCount);

			var expectedNotification = "Could not create SSL/TLS secure channel. Please try again later.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetPortCall_ServerProtocolViolation_Test()
		{
			var callCount = 0;
			var portCallProvider = new PortCallProviderForTesting()
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					callCount++;
					throw new HttpRequestException(string.Empty,
						new WebException(string.Empty,
							WebExceptionStatus.ServerProtocolViolation));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertNotNull(portCallProvider.GetPortCall(string.Empty, requestManager));

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(3, callCount);

			var expectedNotification = "The server committed a protocol violation. Please try again later.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetPortCall_ForbiddenException_Test()
		{
			var callCount = 0;
			var portCallProvider = new PortCallProviderForTesting()
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					callCount++;
					throw new HttpRequestException(string.Empty,
						new WebException("The remote server returned an error: (403) Forbidden.",
							WebExceptionStatus.ProtocolError));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);

			ErrorReporter.Clear();
			AssertNotNull(portCallProvider.GetPortCall(string.Empty, requestManager));

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(3, callCount);

			var expectedNotification = "Unable to connect to Global Sailing Schedules Server. Please check your connection. Firewalls/Proxies may restrict your connection.";
			AssertEquals(expectedNotification, notifications.AsString.TrimEnd('\r', '\n'));
		}

		public void TestGetPortCall_HandleException()
		{
			var portCallProvider = new PortCallProviderForTesting
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw new Exception("Unknown exception",
						new WebException("Some web problem", new Exception("more info"), WebExceptionStatus.PipelineFailure, null));
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new PortCallServiceRequestManager(notifications)
			{
				SuppressUntilUtc = ZDateTime.Now.AddDays(-1)
			};

			portCallProvider.GetPortCall(string.Empty, requestManager);

			AssertEquals("Error occurred while loading Port Call data from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			AssertEquals(1, ErrorReporter.TotalErrorCount);

			AssertContains("Error occurred while loading Port Call data from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/portcalls?filter=
ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			Assert(ErrorReporter.LastExceptionReported.Data.Contains("Search Parameters"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Header"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Request"));
			Assert(!ErrorReporter.LastExceptionReported.Data.Contains("Response Status"));

			ErrorReporter.Clear();
		}

		public void TestGetPortCall_HandleException_WithResponse()
		{
			var portCallProvider = new PortCallProviderForTesting()
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						ReasonPhrase = "Bad request",
						Content = new StringContent(""),
					};
					message.Headers.Add("Connection", "close");
					message.Headers.Add("Cache-Control", "no-cache");
					message.RequestMessage = new HttpRequestMessage(new HttpMethod("Get"), requestUri);
					message.StatusCode = HttpStatusCode.BadRequest;
					return message;
				}
			};
			var notifications = new NotificationBuffer();
			var requestManager = new PortCallServiceRequestManager(notifications)
			{
				SuppressUntilUtc = ZDateTime.Now.AddDays(-1)
			};

			portCallProvider.GetPortCall("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", requestManager);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Error occurred while loading Port Call data from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", ErrorReporter.LastExceptionReported.Data["Search Parameters"].ToString());
			AssertContains(@"Connection: close
Cache-Control: no-cache", ErrorReporter.LastExceptionReported.Data["Response Header"].ToString());
			AssertContains("Method: Get, RequestUri:", ErrorReporter.LastExceptionReported.Data["Response Request"].ToString());
			AssertContains("BadRequest", ErrorReporter.LastExceptionReported.Data["Response Status"].ToString());

			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/portcalls?filter=", ErrorReporter.LastMessageReported);
			AssertContains("ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			AssertEquals("Error occurred while loading Port Call data from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			ErrorReporter.Clear();
		}

		public void TestGetPortCall_HandleException_InvalidJsonResponse()
		{
			ErrorReporter.Clear();

			var portCallProvider = new PortCallProviderForTesting()
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
			var requestManager = new PortCallServiceRequestManager(notifications)
			{
				SuppressUntilUtc = ZDateTime.Now.AddDays(-1)
			};

			portCallProvider.GetPortCall("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", requestManager);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Error occurred while loading Port Call data from Global Schedules service.", ErrorReporter.LastMessageReported);
			AssertContains("Exception thrown", "'/' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", ErrorReporter.LastMessageReported);
			AssertContains("Data is reported", "//{}", ErrorReporter.LastMessageReported);
			AssertContains("LoadPort=BRSSZ&EtdFrom=2016-11-30&DischargePort=CATOR&EtaTo=2016-12-21&CarrierCode=MAEU", ErrorReporter.LastExceptionReported.Data["Search Parameters"].ToString());
			AssertContains(@"Pragma: no-cache
Cache-Control: no-store, no-cache, max-age=0", ErrorReporter.LastExceptionReported.Data["Response Header"].ToString());
			AssertContains("Method: Get, RequestUri:", ErrorReporter.LastExceptionReported.Data["Response Request"].ToString());
			AssertContains("OK", ErrorReporter.LastExceptionReported.Data["Response Status"].ToString());

			AssertContains(@"Url: https://gss.wisegrid.net/api/v2/portcalls?filter=", ErrorReporter.LastMessageReported);
			AssertContains("ServicePointManager.SecurityProtocol: ", ErrorReporter.LastMessageReported);

			AssertEquals("Error occurred while loading Port Call data from Global Schedules service.", notifications.AsString.TrimEnd('\r', '\n'));

			ErrorReporter.Clear();
		}

		public void TestGetPortCall_InternalServerError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.InternalServerError);
		}

		public void TestGetPortCall_BadRequestWithDeserializableMessage()
		{
			var badRequestMessage = "Serializable bad request";
			var content = JsonSerializer.Serialize(new ErrorResult() { Detail = badRequestMessage });
			AssertPortCallBadRequestResponse(content, badRequestMessage, typeof(BadRequestException));
		}

		public void TestGetPortCall_BadRequestWithNotDeserializableMessage()
		{
			var content = "[{\"Message\":\"Serializable bad request\"},{\"Message\":\"Serializable bad request\"},{\"Message\":\"Serializable bad request\"}]";
			AssertPortCallBadRequestResponse(content, "The JSON value could not be converted to Enterprise.Freight.OnlineSailingSchedules.ServiceRequestManager.ErrorResult. Path: $ | LineNumber: 0 | BytePositionInLine: 1. happened for [{\"Message\":\"Serializable bad request\"},{\"Message\":\"Serializable bad request\"},{\"Message\":\"Serializable bad request\"}]", typeof(BadRequestException));
		}

		public void TestGetPortCall_BadRequestWithEmptyMessage()
		{
			var content = "";
			AssertPortCallBadRequestResponse(content, "The input does not contain any JSON tokens. Expected the input to start with a valid JSON token, when isFinalBlock is true. Path: $ | LineNumber: 0 | BytePositionInLine: 0. happened for ", typeof(BadRequestException));
		}

		public void TestGetPortCall_BadRequestWithSerializedEmptyMessage()
		{
			var content = JsonSerializer.Serialize("");
			AssertPortCallBadRequestResponse(content, "The JSON value could not be converted to Enterprise.Freight.OnlineSailingSchedules.ServiceRequestManager.ErrorResult. Path: $ | LineNumber: 0 | BytePositionInLine: 2. happened for \"\"", typeof(BadRequestException));
		}

		public void TestGetPortCall_NotImplementedError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.NotImplemented);
		}

		public void TestGetPortCall_BadGatewayError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.BadGateway);
		}

		public void TestGetPortCall_ServiceUnavailableError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.ServiceUnavailable);
		}

		public void TestGetPortCall_GatewayTimeoutError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.GatewayTimeout);
		}

		public void TestGetPortCall_HttpVersionNotSupportedError()
		{
			CheckServerErrorExceptionHandling(HttpStatusCode.HttpVersionNotSupported);
		}

		void CheckServerErrorExceptionHandling(HttpStatusCode statusCode)
		{
			ErrorReporter.Clear();

			var routesProvider = new PortCallProviderForTesting()
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(statusCode)
					{
						ReasonPhrase = "internal server error",
						Content = new StringContent("Detailed error message of internal server error")
					};
					return message;
				}
			};

			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			routesProvider.GetPortCall("", requestManager);

			AssertEquals(requestManager.ExceptionErrorMessage, notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals("Should not report exception for " + statusCode, 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		void AssertExceptionReportedOnce(UserInitiatedServiceRequestManager requestManager, NotificationBuffer notifications, string exceptionContent, Type exceptionType)
		{
			AssertEquals(requestManager.ExceptionErrorMessage, notifications.AsString.TrimEnd('\r', '\n'));
			AssertEquals("Should not report exception for " + HttpStatusCode.BadRequest, 1, ErrorReporter.TotalErrorCount);
			AssertEquals(exceptionType, ErrorReporter.LastExceptionReported.InnerException.GetType());
			AssertEquals(exceptionContent, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		void AssertPortCallBadRequestResponse(string badRequestContent, string exceptionMessage, Type exceptionType)
		{
			var portCallsProvider = new PortCallProviderForTesting()
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						ReasonPhrase = "bad request",
						Content = new StringContent(badRequestContent)
					};
					return message;
				}
			};
			var notifications = new NotificationBuffer();
			var requestManager = new UserInitiatedServiceRequestManager(notifications);
			portCallsProvider.GetPortCall("", requestManager);
			AssertExceptionReportedOnce(requestManager, notifications, exceptionMessage, exceptionType);
		}
	}

	public class PortCallProviderForTesting : PortCallProvider
	{
		public Func<IApiClient, string, HttpResponseMessage> MockResponseGetter;

		protected override IApiResponse<PortCall.PortCall> TryGetResponse(string endpoint, IApiClient client)
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
