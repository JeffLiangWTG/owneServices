using System;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using WTG.Foundation.Http;
using static Enterprise.Freight.Forwarding.Routing.S8.Business.SoapConstants;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class FlightScheduleClientTest : TestCaseWithFactory
	{
		const string ServerAddress = "http://example.com/";
		const string LogInToken = "ZUBIN12345";

		public void TestConstructor_ShouldThrowArgumentNullException_WhenUriIsNull()
		{
			Uri uri = null;
			AssertExceptionThrown<ArgumentNullException>(() => new FlightScheduleClient(uri));
		}

		#region GetFlight
		public void TestGetFlight_SendCorrectMessageContent()
		{
			var sset = "SSIM";
			var date = new DateTime(2020, 02, 02).ToString("yyyy/mm/dd");
			var airline = "CA";
			var flightNumber = 123;

			SendCorrectMessageContent((client) => client.GetFlight(LogInToken, sset, date, airline, flightNumber),
										string.Format(CultureInfo.InvariantCulture, SoapMessage.FlightWithDateMessageTemplate, nameof(SoapAction.GetFlight), SecurityElement.Escape(LogInToken), sset, date, airline, flightNumber),
										SoapAction.GetFlight);

			SendCorrectMessageContent((client) => client.GetFlight(LogInToken, sset, "", airline, flightNumber),
										string.Format(CultureInfo.InvariantCulture, SoapMessage.FlightWithoutDateMessageTemplate, nameof(SoapAction.GetFlight), SecurityElement.Escape(LogInToken), sset, airline, flightNumber),
										SoapAction.GetFlight);
		}

		public void TestGetFlight_ParseResultCorrectly()
		{
			ParseResultCorrectly((client) => client.GetFlight(LogInToken, "SSIM", "", "CA", 123), MockFlightScheduleClient.RaiseExceptionStatus.DoNotRaiseException, "Result");
			ParseResultCorrectly((client) => client.GetFlight(LogInToken, "SSIM", "", "CA", 123), MockFlightScheduleClient.RaiseExceptionStatus.RaiseInvalidResultTagErrorResponse, null);
		}

		public void TestGetFlight_ShouldThrowArgumentNullException()
		{
			NullArgumentException((client) => client.GetFlight(LogInToken, null, "", "CA", 123));
			NullArgumentException((client) => client.GetFlight(LogInToken, "SSIM", "", null, 123));
		}

		#endregion

		#region SolveRouting

		public void TestSolveRouting_SendCorrectMessageContent()
		{
			SendCorrectMessageContent((client) => client.SolveRouting(LogInToken, "Problem"),
										string.Format(CultureInfo.InvariantCulture, SoapMessage.SolveRoutingMessageTemplate, nameof(SoapAction.SolveRouting), SecurityElement.Escape(LogInToken), "Problem"),
										SoapAction.SolveRouting);
		}

		public void TestSolveRouting_ParseResultCorrectly()
		{
			ParseResultCorrectly((client) => client.SolveRouting(LogInToken, "Problem"), MockFlightScheduleClient.RaiseExceptionStatus.DoNotRaiseException, "Result");
			ParseResultCorrectly((client) => client.SolveRouting(LogInToken, "Problem"), MockFlightScheduleClient.RaiseExceptionStatus.RaiseInvalidResultTagErrorResponse, null);
		}

		public void TestSolveRouting_ShouldThrowArgumentNullException()
		{
			NullArgumentException((client) => client.SolveRouting(LogInToken, null));
		}

		#endregion

		#region LogInS8C
		public void TestLogInS8C_SendCorrectMessageContent()
		{
			var userID = "ID";
			var password = "pas";
			var computer = "";
			var loginID = "loginID";
			var program = "program";
			var clientVersion = "version";

			SendCorrectMessageContent((client) => client.LogInS8C(userID, password, computer, loginID, program, clientVersion),
										string.Format(CultureInfo.InvariantCulture, SoapMessage.LoginMessageTemplate, nameof(SoapAction.LogInS8C), userID, password, computer, loginID, program, clientVersion),
										SoapAction.LogInS8C);
		}

		public void TestLogInS8C_ParseResultCorrectly()
		{
			var userID = "ID";
			var password = "pas";
			var computer = "";
			var loginID = "loginID";
			var program = "program";
			var clientVersion = "version";

			ParseResultCorrectly((client) => client.LogInS8C(userID, password, computer, loginID, program, clientVersion), MockFlightScheduleClient.RaiseExceptionStatus.DoNotRaiseException, MockFlightScheduleClient.MockReturnToken);
			ParseResultCorrectly((client) => client.LogInS8C(userID, password, computer, loginID, program, clientVersion), MockFlightScheduleClient.RaiseExceptionStatus.RaiseInvalidResultTagErrorResponse, null);
		}

		public void TestLogInS8C_ShouldThrowArgumentNullException()
		{
			var userID = "ID";
			var password = "pas";
			var computer = "";
			var loginID = "loginID";
			var program = "program";
			var clientVersion = "version";

			NullArgumentException((client) => client.LogInS8C(null, password, computer, loginID, program, clientVersion));
			NullArgumentException((client) => client.LogInS8C(userID, null, computer, loginID, program, clientVersion));
			NullArgumentException((client) => client.LogInS8C(userID, password, computer, null, program, clientVersion));
			NullArgumentException((client) => client.LogInS8C(userID, password, computer, loginID, null, clientVersion));
		}
		#endregion
	
		#region Implementation
		void SendCorrectMessageContent(Func<IFlightScheduleClient, string> requestCall, string requestBody, string actionName)
		{
			using (var mockServer = new MockFlightScheduleClient())
			{
				var mockHandler = new MockHttpMessageHandler(mockServer);

				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHandler)))
				{
					var client = new FlightScheduleClient(new Uri(ServerAddress));
					var result = requestCall(client);

					AssertEquals(mockHandler.SendAsyncCallCount, 1);
					AssertEquals(mockHandler.AbsoluteUri, ServerAddress);
					AssertEquals(XNode.DeepEquals(XDocument.Parse(mockHandler.ReceivedRequest), XDocument.Parse(requestBody)), true);
					AssertEquals(mockHandler.Headers.GetValues("SOAPAction").First(), actionName);
				}
			}
		}

		void ParseResultCorrectly(Func<IFlightScheduleClient, string> requestCall, MockFlightScheduleClient.RaiseExceptionStatus exceptionStatus, string response)
		{
			using (var mockServer = new MockFlightScheduleClient(response, exceptionStatus))
			{
				var mockHandler = new MockHttpMessageHandler(mockServer);
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHandler)))
				{
					var client = new FlightScheduleClient(new Uri(ServerAddress));
					AssertEquals(requestCall(client), response);
				}
			}
		}

		void NullArgumentException(Func<IFlightScheduleClient, string> requestCall)
		{
			using (var mockServer = new MockFlightScheduleClient())
			{
				var mockHandler = new MockHttpMessageHandler(mockServer);

				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHandler)))
				{
					var client = new FlightScheduleClient(new Uri(ServerAddress));
					AssertExceptionThrown<ArgumentNullException>(() => requestCall(client));
				}
			}
		}

		#endregion
	}
}
