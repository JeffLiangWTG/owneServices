using System;
using System.Net;
using CargoWise.Common.Testing;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class MockFlightScheduleClient : IFlightScheduleClient
	{
		public enum RaiseExceptionStatus
		{
			DoNotRaiseException,
			RaiseWebException,
			RaiseGeneralExceptionInLogin,
			RaiseTimeoutExceptionInLoginOneTime,
			RaiseTimeoutExceptionInLoginMoreThanOneTime,
			RaiseTimeoutExceptionInGetFlightOneTime,
			RaiseTimeoutExceptionInGetFlightMoreThanOneTime,
			RaiseUnderlyingConnectionWasClosedExceptionInGetFlight,
			RaiseTimeoutExceptionInSolveRoutingOneTime,
			RaiseTimeoutExceptionInSolveRoutingMoreThanOneTime,
			RaiseUnderlyingConnectionWasClosedExceptionInSolveRouting,
			RaiseSignInInvalidOrExpiredExceptionOneTime,
			RaiseInvalidAirlineParseErrorResponse,
			RaiseInvalidAirportParseErrorResponse,
			RaiseInvalidEquipmentParseErrorResponse,
			RaiseDatabaseDisconnectionErrorInGetFlight,
			RaiseInvalidResultTagErrorResponse,
			RaiseLoginFailForInstalledSSLNotSignedTrustedAuthorityResponse
		}

		public const string ValidResponse = "Result";
		public const string WebException = "Web Exception";
		public const string ParseErrorInvalidAirline = "Parse error: Invalid airline: MA";
		public const string ParseErrorInvalidAirport = "Parse error: Invalid airport: SHA";
		public const string ParseErrorInvalidEquipment = "Parse error: Invalid equipment: Boeing737";
		public const string LoginFailForInstalledSSLNotSignedTrustedAuthority = "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.";
		public const string MockReturnToken = "MOCKTOKEN";

		public int LoginCount;

		bool timeoutHasRaisedInLogin;
		bool timeOutHasRaisedInSolveRouting;
		bool timeoutHasRaisedInGetFlight;
		bool loginHasCalled;
		bool invalidOrExpiredExceptionRaised;

		public RaiseExceptionStatus CurrentRaiseExceptionStatus { get; private set; }
		public string ResponseContent { get; private set; }
		public string EndPoint { get; private set; }

		public MockFlightScheduleClient(string response = ValidResponse, RaiseExceptionStatus raiseException = RaiseExceptionStatus.DoNotRaiseException)
		{
			CurrentRaiseExceptionStatus = raiseException;
			ResponseContent = response;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public string SolveRouting(string token, string problem)
		{
			EndPoint = "SolveRouting";

			try
			{
				return BuildS8ServerResponse(SolveRouting());
			}
			catch (Exception)
			{
				throw;
			}
		}

		public string GetFlight(string token, string sset, string date, string airline, int flight)
		{
			EndPoint = "GetFlight";

			try
			{
				return BuildS8ServerResponse(GetFlight());
			}
			catch (Exception)
			{
				throw;
			}
		}

		public string LogInS8C(string userID, string password, string computer, string loginID, string program, string clientVersion)
		{
			EndPoint = "LogInS8C";

			try
			{
				return BuildS8ServerResponse(LogInS8C());
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#region Implementation
		string SolveRouting()
		{
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInSolveRoutingMoreThanOneTime)
			{
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInSolveRoutingOneTime && !timeOutHasRaisedInSolveRouting)
			{
				timeOutHasRaisedInSolveRouting = true;
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseUnderlyingConnectionWasClosedExceptionInSolveRouting)
			{
				throw new WebException(S8ClientTest.UnderlyingconnectionWasClosedMessage, WebExceptionStatus.ConnectionClosed);
			}
			if(CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseWebException)
			{
				throw new WebException(WebException);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseSignInInvalidOrExpiredExceptionOneTime && !invalidOrExpiredExceptionRaised)
			{
				invalidOrExpiredExceptionRaised = true;
				return $"#{S8Client.SignInInvalidOrExpired}";
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseInvalidAirlineParseErrorResponse)
			{
				return $"#{ParseErrorInvalidAirline}";
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseInvalidAirportParseErrorResponse)
			{
				return $"#{ParseErrorInvalidAirport}";
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseInvalidEquipmentParseErrorResponse)
			{
				return $"#{ParseErrorInvalidEquipment}";
			}
			return ResponseContent;
		}

		string GetFlight()
		{
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseDatabaseDisconnectionErrorInGetFlight)
			{
				var ex = SqlExceptionCreator.NewSqlException("Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.");
				throw ex;
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInGetFlightMoreThanOneTime)
			{
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInGetFlightOneTime && timeoutHasRaisedInGetFlight)
			{
				timeoutHasRaisedInGetFlight = true;
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseUnderlyingConnectionWasClosedExceptionInGetFlight)
			{
				throw new WebException(S8ClientTest.UnderlyingconnectionWasClosedMessage, WebExceptionStatus.ConnectionClosed);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseWebException)
			{
				throw new WebException(WebException);
			}

			return ResponseContent;
		}

		string LogInS8C()
		{
			LoginCount++;

			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInLoginMoreThanOneTime)
			{
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseTimeoutExceptionInLoginOneTime && timeoutHasRaisedInLogin)
			{
				timeoutHasRaisedInLogin = true;
				throw new WebException(S8ClientTest.TimeOutExceptionMessage, WebExceptionStatus.Timeout);
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseGeneralExceptionInLogin)
			{
				if (!loginHasCalled)
				{
					loginHasCalled = true;
					throw new Exception("First exception");
				}

				throw new Exception("Second exception");
			}
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseWebException)
			{
				throw new WebException(WebException);
			}
			if(CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseLoginFailForInstalledSSLNotSignedTrustedAuthorityResponse)
			{
				throw new WebException(LoginFailForInstalledSSLNotSignedTrustedAuthority);
			}

			return MockReturnToken;
		}

		string BuildS8ServerResponse(string response)
		{
			var tagName = EndPoint;
			if (CurrentRaiseExceptionStatus == RaiseExceptionStatus.RaiseInvalidResultTagErrorResponse)
			{
				tagName = "Tag";
			}

			return $@"<s:Envelope xmlns:s = ""http://schemas.xmlsoap.org/soap/envelope/"">
				<s:Body>
					<{tagName}Response xmlns = ""S8CWebSv"">
						<{tagName}Result>{response}</{tagName}Result>
					</{tagName}Response>
				</s:Body>
			</s:Envelope>";
		}
		#endregion
	}
}
