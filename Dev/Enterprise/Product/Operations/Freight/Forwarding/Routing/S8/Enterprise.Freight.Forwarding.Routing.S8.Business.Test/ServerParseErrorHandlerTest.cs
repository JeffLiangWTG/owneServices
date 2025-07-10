using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class ServerParseErrorHandlerTest : TestCaseWithFactory
	{
		public const string InvalidAirlineMessage = "Selected Airline is not a valid supported Carrier"; // Developer Only
		public const string InvalidAirportMessage = "{0} airport <{1}> has an invalid IATA code"; // Developer Only

		public void TestResponseContainsParseError_ShouldNotBeTrue_WhenServerResponseIsNull()
		{
			var handler = new ServerParseErrorHandler(null, null);
			Assert(!handler.ResponseContainsParseError);
		}

		public void TestResponseContainsParseError_ShouldNotBeTrue_WhenServerResponseDoesNotContainParseError()
		{
			var serverResponse =
				@"R 21/01/22 02:18:13 (CargoWise One) SSIM SYD LHR 21/01/22 .C2.T...0.B AA . . . .
100 SYD LHR 38:10   10:15 1 + 13:25 AA AA        2021 / 01 / 21 2099 / 12 / 31 1.3.567 < SYD   LAX     10:15   06:10 AA  9712    77W     0  7487                                        1234567 21 / 01 / 21 99 / 12 / 31 F > < LAX   LHR     18:55 1 + 13:25 AA  9707    77W     0  5456                                        1.3.567 21 / 01 / 21 99 / 12 / 31 F >
							   100 SYD LHR 62:10   10:15 2 + 13:25 AA AA        2021 / 01 / 21 2099 / 12 / 30 .2.4567 < SYD   LAX     10:15   06:10 AA  9712    77W     0  7487                                        1234567 21 / 01 / 21 99 / 12 / 31 F > < LAX   LHR   1 + 18:55 2 + 13:25 AA  9707    77W     0  5456                                        1.3.567 21 / 01 / 21 99 / 12 / 31 F >
																100 SYD LHR 37:10   11:15 1 + 13:25 AA AA        2021 / 01 / 21 2021 / 02 / 09....5.7 < SYD 1 LAX B   11:15   06:15 AA    72    77W     0  7487                                        .2.45.7 21 / 01 / 21 21 / 02 / 09 J > < LAX   LHR     18:55 1 + 13:25 AA  9707    77W     0  5456                                        1.3.567 21 / 01 / 21 99 / 12 / 31 F >
																							  100 SYD LHR 61:10   11:15 2 + 13:25 AA AA        2021 / 01 / 21 2021 / 02 / 09 .2.45.7 < SYD 1 LAX B   11:15   06:15 AA    72    77W     0  7487                                        .2.45.7 21 / 01 / 21 21 / 02 / 09 J > < LAX   LHR   1 + 18:55 2 + 13:25 AA  9707    77W     0  5456                                        1.3.567 21 / 01 / 21 99 / 12 / 31 F >
																															   !0 Time:
			0.01  14   0.12(SYDWP - SS8C - 1, W3WP)[In cache]";
			var handler = new ServerParseErrorHandler(serverResponse, new RoutingRequest(Factory));
			Assert(!handler.ResponseContainsParseError);
		}

		public void TestResponseContainsParseError_ShouldBeTrue_WhenServerResponseContainsParseError()
		{
			var handler = new ServerParseErrorHandler(ServerParseErrorHandler.ParseError, null);
			Assert(handler.ResponseContainsParseError);
		}

		public void TestErrorType_ShouldBeInvalidAirline_WhenServerResponseContainsParseErrorAndInvalidAirline()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: {ServerParseErrorHandler.InvalidAirline}", new RoutingRequest(Factory));
			AssertEquals(ServerParseErrorHandler.ParseErrorMessageType.InvalidAirline, handler.ErrorType);
		}

		public void TestErrorType_ShouldBeInvalidAirport_WhenServerResponseContainsParseErrorAndInvalidAirport()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: INVALID AIRPORT", new RoutingRequest(Factory));
			AssertEquals(ServerParseErrorHandler.ParseErrorMessageType.InvalidAirport, handler.ErrorType);
		}

		public void TestErrorType_ShouldBeNone_WhenServerResponseContainsParseErrorAndInvalidEquipment()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: INVALID EQUIPMENT", new RoutingRequest(Factory));
			AssertEquals(ServerParseErrorHandler.ParseErrorMessageType.None, handler.ErrorType);
		}

		public void TestShouldReportError_ShouldBeFalse_WhenServerResponseContainsParseErrorAndInvalidAirline()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: {ServerParseErrorHandler.InvalidAirline}", new RoutingRequest(Factory));
			Assert(!handler.ShouldReportError);
		}

		public void TestShouldReportError_ShouldBeFalse_WhenServerResponseContainsParseErrorAndInvalidAirport()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: INVALID AIRPORT", new RoutingRequest(Factory));
			Assert(!handler.ShouldReportError);
		}

		public void TestShouldReportError_ShouldBeTrue_WhenServerResponseContainsParseErrorAndInvalidEquipment()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: INVALID EQUIPMENT", new RoutingRequest(Factory));
			Assert(handler.ShouldReportError);
		}

		public void TestMessage_ShouldBeInvalidAirlineMessage_WhenServerResponseContainsParseErrorAndInvalidAirline()
		{
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: {ServerParseErrorHandler.InvalidAirline}", new RoutingRequest(Factory));
			AssertEquals(InvalidAirlineMessage, handler.Message);
		}

		public void TestMessage_ShouldBeInvalidAirportMessage_WhenServerResponseContainsParseErrorAndInvalidAirport()
		{
			var request = new RoutingRequest(Factory);
			request.OriginUNLOCOCode = "CNSHA";
			request.DestinationUNLOCOCode = "USLAX";
			var handler = new ServerParseErrorHandler($"#{ServerParseErrorHandler.ParseError}: INVALID AIRPORT: SHA ", request);
			AssertEquals(string.Format(InvalidAirportMessage, "Origin", request.OriginUNLOCO.Code), handler.Message);
		}

		public void TestMessage_ShouldBeOriginalServerResponse_WhenServerResponseContainsParseErrorAndInvalidEquipment()
		{
			var originalMessage = $"#{ServerParseErrorHandler.ParseError}: INVALID EQUIPMENT";
			var handler = new ServerParseErrorHandler(originalMessage, new RoutingRequest(Factory));
			AssertEquals(originalMessage, handler.Message);
		}
	}
}
