using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using WTG.Foundation.Http;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class S8ClientTest : TestCaseWithFactory
	{
		public const string TimeOutExceptionMessage = "The operation has timed out";
		public const string UnderlyingconnectionWasClosedMessage = "The underlying connection was closed";
		public const string Token = "ZUBIN123";

		public void TestDispose()
		{
			using (var mockServer = new MockFlightScheduleClient())
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var client = new S8Client())
			{
				AssertEquals("Should be registered.", true, DisposableLeakListener.Instance.IsRegistered(client));
			}
		}
		public void TestSecureCertificateCheckCallbackDoesNotFill()
		{
			var expectedResult = new MethodCallResult<int>(345);
			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "FlightNumber=345" },
				ParseResults = new List<MethodCallResult<int>>() { expectedResult },
				LoginToken = Token
			})
			{
				var result = client.TestRequest();
				AssertResult(expectedResult, result);
				AssertNull(client.ServerCertificateValidationResult);
			}
		}

		#region constructor

		public void TestCurrentUrlNullShouldSetToPrimary()
		{
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://abc.com"))
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Faillover.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://def.com"))
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			using (var client = new S8Client())
			{
				AssertEquals(FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value, FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value);
			}
		}

		public void TestCurrentUrlNotPrimaryOrFailoverShouldSetToPrimary()
		{
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://abc.com"))
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Faillover.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://def.com"))
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://invalidurl.com"))
			using (var client = new S8Client())
			{
				AssertEquals(FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value, FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value);
			}
		}

		#endregion

		#region Login

		public void TestLoginCalledAtTheFirstTime()
		{
			var expectedResult = new MethodCallResult<int>(345);

			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "FlightNumber=345" },
				ParseResults = new List<MethodCallResult<int>>() { expectedResult },
				LoginToken = Token
			})
			{
				FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

				AssertEquals(false, client.LoginCalled);

				var result = client.TestRequest();

				AssertEquals(true, client.LoginCalled);
				AssertEquals(Token, FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.Value);

				AssertResult(expectedResult, result);
			}
		}

		public void TestLoginCalledOnce()
		{
			var expectedResult1 = new MethodCallResult<int>(345);
			var expectedResult2 = new MethodCallResult<int>(567);

			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "FlightNumber=345", "FlightNumber=567" },
				ParseResults = new List<MethodCallResult<int>>() { expectedResult1, expectedResult2 },
				LoginToken = Token
			})
			{
				FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

				AssertEquals(false, client.LoginCalled);

				var result = client.TestRequest();

				AssertEquals(true, client.LoginCalled);
				AssertEquals(Token, FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.Value);

				AssertResult(expectedResult1, result);

				client.LoginCalled = false;

				result = client.TestRequest();

				AssertEquals(false, client.LoginCalled);
				AssertResult(expectedResult2, result);
			}
		}

		public void TestLoginCalledWhenServiceCallReturnsError()
		{
			var expectedResult = new MethodCallResult<int>(345);

			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "#Error", "FlightNumber=345" },
				ParseResults = new List<MethodCallResult<int>>() { expectedResult },
				LoginToken = Token
			})
			{
				AssertEquals(false, client.LoginCalled);

				var result = client.TestRequest();

				AssertEquals(true, client.LoginCalled);
				AssertResult(expectedResult, result);
			}
		}

		public void TestLoginError()
		{
			var expectedResult = new MethodCallResult<int>("Login Failed");

			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "FlightNumber=345" },
				ParseResults = new List<MethodCallResult<int>>() { new MethodCallResult<int>(345) },
				LoginToken = "#Login Failed"
			})
			{
				AssertEquals(false, client.LoginCalled);

				FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				var result = client.TestRequest();

				AssertEquals(true, client.LoginCalled);
				AssertEquals("", FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.Value);

				AssertEquals(false, result.Succeeded);
				AssertContains("Login Failed", result.ErrorMessage);
			}
		}

		public void TestLoginFailForInstalledSSLNotSignedTrustedAuthority()
		{
			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseLoginFailForInstalledSSLNotSignedTrustedAuthorityResponse))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var client = new S8Client())
			{
				var result = client.GetFlight(new FlightRequest(new ZDate(2018, 7, 30), "QF", 1));

				AssertEquals(false, result.Succeeded);
				AssertContains(MockFlightScheduleClient.LoginFailForInstalledSSLNotSignedTrustedAuthority, result.ErrorMessage);
			}
			ErrorReporter.Clear();
		}

		#endregion

		#region Solve routing

		public void TestSolveRouting()
		{
			var solveRoutingResult = "R 18/07/30 10:49:20 (CargoWise One) SSIM TRO NTL 18/08/01 ..2.H...0.. . . . . .\n000 TRO NTL  0:20   11:00   11:20 FP           <TRO   NTL     11:00   11:20 FP   712    J32     0    74                                        ..3.... 18/08/01 18/09/26 J> \n!0 Time:  0.00   2   0.01 (SYDWP-SS8C-1,W3WP) [In cache]";
			using (var mockServer = new MockFlightScheduleClient(solveRoutingResult))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var client = new S8Client())
			{
				var result = client.SolveRouting(new RoutingRequest(Factory));

				AssertEquals(true, result.Succeeded);
				AssertEquals(solveRoutingResult, result.Result);
			}
		}

		public void TestSolveRouting_DoNotUseSuppression()
		{
			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				var s8ServiceRequestManager = new S8ServiceRequestManager();

				using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseWebException))
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
				using (var client = new S8Client(null, s8ServiceRequestManager))
				{
					var result = client.SolveRouting(new RoutingRequest(Factory));

					AssertEquals(false, result.Succeeded);
					AssertContains(MockFlightScheduleClient.WebException, result.ErrorMessage);
					AssertNotEquals("S8LoginSuppressionUntilUtc should be updated.", DateTime.MinValue, s8ServiceRequestManager.SuppressUntilUtc);

					var result2 = client.SolveRouting(new RoutingRequest(Factory));

					AssertEquals(false, result2.Succeeded);
					AssertContains(MockFlightScheduleClient.WebException, result.ErrorMessage);
					AssertNotEquals("S8LoginSuppressionUntilUtc should be updated.", DateTime.MinValue, s8ServiceRequestManager.SuppressUntilUtc);
					AssertNotContains("Suppression should not be used for SolveRouting", "Login is suppressed", result2.ErrorMessage);
				}

				ErrorReporter.Clear();
			}
		}

		public void TestSolveRouting_ShouldLoginAgain_WhenExceptionMessageIsSignInInvalidOrExpired()
		{
			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseSignInInvalidOrExpiredExceptionOneTime))
			{
				var mockHandler = new MockHttpMessageHandler(mockServer);
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => mockHandler)))
				using (var s8Client = new S8Client())
				{
					s8Client.SolveRouting(new RoutingRequest(Factory));
					AssertEquals("developer exception not expected", 0, ErrorReporter.TotalErrorCount);
					AssertEquals(2, mockServer.LoginCount);
				}
			}
			ErrorReporter.Clear();
		}

		public void TestSolveRouting_ShouldInformUser_WhenAirportIsNotValidAndUpdateStatusIsNotNull()
		{
			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseInvalidAirportParseErrorResponse ))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var s8Client = new S8Client(UpdateProgress))
			{
				var routingRequest = new RoutingRequest(Factory);
				routingRequest.OriginUNLOCOCode = "CNSHA";
				routingRequest.DestinationUNLOCOCode = "USLAX";
				var result = s8Client.SolveRouting(routingRequest);
				AssertEquals("developer exception not expected", 0, ErrorReporter.TotalErrorCount);
				AssertEquals(string.Format(ServerParseErrorHandlerTest.InvalidAirportMessage, "Origin", routingRequest.OriginUNLOCO.Code), result.ErrorMessage);
			}

			ErrorReporter.Clear();
		}

		public void TestSolveRouting_ShouldInformUser_WhenAirlineIsNotValidAndUpdateStatusIsNotNull()
		{
			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseInvalidAirlineParseErrorResponse))
			{
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
				using (var s8Client = new S8Client(UpdateProgress))
				{
					var result = s8Client.SolveRouting(new RoutingRequest(Factory));
					AssertEquals("developer exception not expected", 0, ErrorReporter.TotalErrorCount);
					AssertEquals(ServerParseErrorHandlerTest.InvalidAirlineMessage, result.ErrorMessage);
				}
			}

			ErrorReporter.Clear();
		}

		#endregion

		#region GetFlight

		public void TestGetFlight()
		{
			var getFlightServiceCallResult = "GetFlight         ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0001 2018/07/26 2018/10/06 1234567  1 J SYD 15:55 0 SIN 22:15 0 388\nQF 0001 2018/07/26 2018/10/06 1234567  2 J SIN 23:55 0 LHR 06:55 1 388\n";
			using (var mockServer = new MockFlightScheduleClient(getFlightServiceCallResult))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var client = new S8Client())
			{
				var result = client.GetFlight(new FlightRequest(new ZDate(2018, 7, 30), "QF", 1));
				AssertEquals(true, result.Succeeded);

				var flight = result.Result;

				AssertEquals(new ZDate(2018, 7, 30), flight.RequestDate);
				AssertEquals("QF", flight.Airline);
				AssertEquals(1, flight.FlightNumber);

				AssertEquals(2, flight.Legs.Count);
				var legs = flight.Legs;

				AssertEquals(flight.Airline, legs[0].Airline);
				AssertEquals(flight.Airline, legs[1].Airline);

				AssertEquals(flight.FlightNumber, legs[0].FlightNumber);
				AssertEquals(flight.FlightNumber, legs[1].FlightNumber);

				AssertEquals(1, legs[0].LegNumber);
				AssertEquals(2, legs[1].LegNumber);

				AssertEquals(legs[0].Destination, legs[1].Origin);
			}
		}

		public void TestGetFlight_WhenLoginIsSuppressed()
		{
			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				var s8ServiceRequestManager = new S8ServiceRequestManager();

				using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseWebException))
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
				using (var client = new S8Client(null, s8ServiceRequestManager))
				{
					var result = client.GetFlight(new FlightRequest(new ZDate(2018, 7, 30), "QF", 1));

					AssertEquals(false, result.Succeeded);
					AssertContains(MockFlightScheduleClient.WebException, result.ErrorMessage);

					var result2 = client.GetFlight(new FlightRequest(new ZDate(2018, 7, 30), "QF", 1));

					AssertEquals(false, result2.Succeeded);
					AssertContains("Login is suppressed", result2.ErrorMessage);
				}

				AssertNotEquals("S8LoginSuppressionUntilUtc should be updated.", DateTime.MinValue, s8ServiceRequestManager.SuppressUntilUtc);
				AssertEquals("Primary token should be reset", string.Empty, FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.Value);
				AssertEquals("Fail-Over token should be reset", string.Empty, FreightDataRegistry.Instance.S8CargoWebServiceSessionFailOverToken.Value);

				ErrorReporter.Clear();
			}
		}

		public void TestGetFlight_WhenLoginIsSuppressed_AndRetrySucceeded()
		{
			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				var s8ServiceRequestManager = new S8ServiceRequestManager();
				var getFlightServiceCallResult = "GetFlight         ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0001 2018/07/26 2018/10/06 1234567  1 J SYD 15:55 0 SIN 22:15 0 388\nQF 0001 2018/07/26 2018/10/06 1234567  2 J SIN 23:55 0 LHR 06:55 1 388\n";
				using (var mockServer = new MockFlightScheduleClient(getFlightServiceCallResult))
				using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
				using (var client = new S8Client())
				{
					var result = client.GetFlight(new FlightRequest(new ZDate(2018, 7, 30), "QF", 1));

					AssertEquals(true, result.Succeeded);
				}

				AssertEquals("S8LoginSuppressionUntilUtc should be reset to MinValue.", DateTime.MinValue, s8ServiceRequestManager.SuppressUntilUtc);
			}
		}

		public void TestGetFlight_ShouldSupressS8Request_WhenErrorIsTimeoutAgain()
		{
			using (var mockServer = new MockFlightScheduleClient(null, MockFlightScheduleClient.RaiseExceptionStatus.RaiseTimeoutExceptionInGetFlightMoreThanOneTime))
			using (ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => new MockHttpMessageHandler(mockServer))))
			using (var s8Client = new S8Client(status => { }))
			{
				s8Client.GetFlight(new FlightRequest(new ZDate(2020, 11, 05), "CX", 2049));
				var result = s8Client.GetFlight(new FlightRequest(new ZDate(2020, 11, 05), "CX", 2049));
				AssertEquals("Login is suppressed", result.ErrorMessage);
			}
		}

		#endregion

		#region Request

		public void TestRequestServiceFailure()
		{
			using (var client = new S8ClientForGeneralTest
			{
				ServiceCallResults = new List<string>() { "#Error1", "#Error2" },
				ParseResults = new List<MethodCallResult<int>>() { new MethodCallResult<int>(345) },
				LoginToken = "ZUBIN12345"
			})
			{
				AssertEquals(false, client.LoginCalled);

				var result = client.TestRequest();

				AssertEquals(true, client.LoginCalled);

				AssertEquals(false, result.Succeeded);
				AssertContains("Error2", result.ErrorMessage);
			}
		}

		#endregion

		#region Helper Methods and classes

		void UpdateProgress(string status)
		{
		}

		void AssertResult(MethodCallResult<int> expectedResult, MethodCallResult<int> actualResult)
		{
			AssertEquals(expectedResult.Succeeded, actualResult.Succeeded);

			if (actualResult.Succeeded)
			{
				AssertEquals(expectedResult.Result, actualResult.Result);
			}
			else
			{
				AssertEquals(expectedResult.ErrorMessage, actualResult.ErrorMessage);
			}
		}
		#endregion
	}
}
