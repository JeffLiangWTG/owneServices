namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Freight.Integration;
	using Moq;
	using NUnit.Framework;
	using FlightScheduleStatus = Core.Constants.FlightScheduleStatus;

	public class S8MatcherTestCase : TransactionedTestCase
	{
		public void TestMatchError_GetFlightError()
		{
			ErrorReporter.Clear();

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlightResultError);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlightResultError);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unknown, result.ScheduleStatus);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("S8Matcher_GetScheduleInfosCore_ErrorMessage", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestMatchError_GetFlightNotFound()
		{
			ErrorReporter.Clear();

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlightResultNotFound);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlightResultNotFound);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestMatchError_GetLoginIsSuppressed()
		{
			ErrorReporter.Clear();

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetLoginIsSuppressed);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetLoginIsSuppressed);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcher = new S8Matcher();
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestMatchError_GetFlightDateNotMatch()
		{
			ErrorReporter.Clear();

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1115QF568Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1114QF568Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("Q́F"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unknown, result.ScheduleStatus);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(@"Current flight schedule is:
Airline: QF
Flight Number: 568
Origin : PER
Departure Date: 14-Nov-18 00:00:00
Destination : SYD
Arrival Date: 15-Nov-18 00:00:00
The request date is: 14-Nov-18 00:00:00
But in the response, the airline 'QF', flight number '568' or request date '15-Nov-18 00:00:00' does not match.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestMatchError_GetFlightEmptyLegs()
		{
			ErrorReporter.Clear();

			var flight1114QF568ResultWithEmptyLegs = GetFlight1114QF568Result;
			flight1114QF568ResultWithEmptyLegs.Result.Legs.Clear();

			var flight1115QF568ResultWithEmptyLegs = GetFlight1115QF568Result;
			flight1115QF568ResultWithEmptyLegs.Result.Legs.Clear();

			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(flight1114QF568ResultWithEmptyLegs);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(flight1115QF568ResultWithEmptyLegs);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unknown, result.ScheduleStatus);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(@"Current flight schedule is:
Airline: QF
Flight Number: 568
Origin : PER
Departure Date: 14-Nov-18 00:00:00
Destination : SYD
Arrival Date: 15-Nov-18 00:00:00
The request date is: 14-Nov-18 00:00:00
The number of legs is 0.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestMatchError_ShouldNotReportError_WhenErrorIsTimeOut()
		{
			using (var s8Client = new S8Client(status => { }))
			{
				var matcherForTest = new S8MatcherForTest(s8Client);
				var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
				matcherForTest.Match(scheduleToMatch);
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestMatchError_ShouldNotReportError_WhenErrorIsDisconnectionFromDatabase()
		{
			using (var s8Client = new S8Client(status => { }))
			{
				var matcherForTest = new S8MatcherForTest(s8Client);
				var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
				matcherForTest.Match(scheduleToMatch);
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestMatchError_ShouldNotReportError_WhenErrorUnableToRetrieveLoginMutex()
		{
			using (var s8Client = new S8ClientForMutexTest())
			{
				var matcherForTest = new S8MatcherForTest(s8Client);
				var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
				matcherForTest.Match(scheduleToMatch);
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestMatchError_ShouldNotReportError_WhenErrorIsTheUnderlyingConnectionWasClosed()
		{
			using (var s8Client = new S8Client(status => { }))
			{
				var matcherForTest = new S8MatcherForTest(s8Client);
				var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
				matcherForTest.Match(scheduleToMatch);
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestMatchQF568_WithDepartureDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1114QF568Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, ZString.Empty, new ZDate(2018, 11, 14), ZString.Empty, ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1114QF568Result.Result.Legs[0], result);
		}

		public void TestMatchQF568_WithArrivalDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/15" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1115QF568Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1114QF568Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, ZString.Empty, ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1114QF568Result.Result.Legs[0], result);
		}

		public void TestMatchQF568_Successfully()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/14" && request.Airline == "QF" && request.FlightNumber == 568)))
				.Returns(GetFlight1114QF568Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 568, new ZString("PER"), new ZDate(2018, 11, 14), new ZString("SYD"), new ZDate(2018, 11, 15));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1114QF568Result.Result.Legs[0], result);
		}

		public void TestMatchQF1_WithDepartureDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), ZString.Empty, ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);
		}

		public void TestMatchQF1_WithDepartureDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), new ZDate(2018, 11, 12), ZString.Empty, ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SIN"), new ZDate(2018, 11, 12), ZString.Empty, ZDate.Empty);
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithDepartureDateAndArrivalDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), ZString.Empty, new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), ZString.Empty, new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithDepartureDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), new ZString("SIN"), ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), new ZString("LHR"), ZDate.Empty);
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithArrivalDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);
		}

		public void TestMatchQF1_WithArrivalDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/13" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1113QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SIN"), ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithArrivalDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/13" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1113QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, ZDate.Empty, new ZString("SIN"), new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, ZDate.Empty, new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithDepartureDateAndArrivalDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), new ZDate(2018, 11, 12), ZString.Empty, new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), new ZDate(2018, 11, 12), ZString.Empty, new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithDepartureDateAndArrivalDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), new ZString("SIN"), new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, ZString.Empty, new ZDate(2018, 11, 12), new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_WithArrivalDateAndOriginAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/13" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1113QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/13" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1113QF1Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), ZDate.Empty, new ZString("SIN"), new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SIN"), ZDate.Empty, new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), ZDate.Empty, new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF1_Successfully()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/12" && request.Airline == "QF" && request.FlightNumber == 1)))
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result)
				.Returns(GetFlight1112QF1Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), new ZDate(2018, 11, 12), new ZString("SIN"), new ZDate(2018, 11, 12));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SIN"), new ZDate(2018, 11, 12), new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 1, new ZString("SYD"), new ZDate(2018, 11, 12), new ZString("LHR"), new ZDate(2018, 11, 13));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1112QF1Result.Result.Legs[0], GetFlight1112QF1Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithDepartureDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), ZString.Empty, ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);
		}

		public void TestMatchQF2_WithDepartureDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), new ZDate(2018, 11, 7), ZString.Empty, ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("SIN"), new ZDate(2018, 11, 8), ZString.Empty, ZDate.Empty);
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithDepartureDateAndArrivalDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result)
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), ZString.Empty, new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), ZString.Empty, new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithDepartureDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.SetupSequence(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result)
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), new ZString("SIN"), ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), new ZString("SYD"), ZDate.Empty);
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithArrivalDate()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);
		}

		public void TestMatchQF2_WithArrivalDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/09" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1109QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("SIN"), ZDate.Empty, ZString.Empty, new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithArrivalDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/09" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1109QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, ZDate.Empty, new ZString("SIN"), new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, ZDate.Empty, new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithDepartureDateAndArrivalDateAndOrigin()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), new ZDate(2018, 11, 7), ZString.Empty, new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("SIN"), new ZDate(2018, 11, 8), ZString.Empty, new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), new ZDate(2018, 11, 7), ZString.Empty, new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithDepartureDateAndArrivalDateAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), new ZString("SIN"), new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 8), new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_WithArrivalDateAndOriginAndDestination()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/09" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1109QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/09" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1109QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), ZDate.Empty, new ZString("SIN"), new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("SIN"), ZDate.Empty, new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), ZDate.Empty, new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.PartiallyMatched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatchQF2_Successfully()
		{
			var s8ClientWrapper = new Mock<IS8Client>();
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1108QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			s8ClientWrapper.Setup(x => x.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlight1107QF2Result);
			var s8Client = s8ClientWrapper.Object;

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), new ZDate(2018, 11, 7), new ZString("SIN"), new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(s8Client);
			var result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("SIN"), new ZDate(2018, 11, 8), new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[1], result);

			scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, new ZString("LHR"), new ZDate(2018, 11, 7), new ZString("SYD"), new ZDate(2018, 11, 9));
			matcherForTest = new S8MatcherForTest(s8Client);
			result = matcherForTest.Match(scheduleToMatch);
			AssertEquals(FlightScheduleStatus.Matched, result.ScheduleStatus);
			AssertScheduleInfo(GetFlight1107QF2Result.Result.Legs[0], GetFlight1107QF2Result.Result.Legs[1], result);
		}

		public void TestMatch_WithOutRangeDepartureDate()
		{
			var clientMock = new Mock<IS8Client>();
			clientMock.Setup(c => c.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlightResultErrorInDate);

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), new ZString("SIN"), ZDate.Empty);
			var matcherForTest = new S8MatcherForTest(clientMock.Object);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertEquals("Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.", result.MatchErrorMessage);
		}

		public void TestMatch_WithOutRangeArrivalDate()
		{
			var clientMock = new Mock<IS8Client>();
			clientMock.Setup(c => c.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/07" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlightResultErrorInDate);
			clientMock.Setup(c => c.GetFlight(It.Is<FlightRequest>(request => request.Date == "2018/11/08" && request.Airline == "QF" && request.FlightNumber == 2)))
				.Returns(GetFlightResultErrorInDate);

			var scheduleToMatch = new ScheduleInfo(new ZString("QF"), 2, ZString.Empty, new ZDate(2018, 11, 7), ZString.Empty, new ZDate(2018, 11, 8));
			var matcherForTest = new S8MatcherForTest(clientMock.Object);
			var result = matcherForTest.Match(scheduleToMatch);

			AssertEquals(FlightScheduleStatus.Unmatched, result.ScheduleStatus);
			AssertEquals("Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.", result.MatchErrorMessage);
		}

		void AssertScheduleInfo(FlightLegInformation leg, IS8MatchResult result)
		{
			var scheduleInfo = result.MatchedSchedule;

			Assertion.AssertEquals(leg.Airline, scheduleInfo.Carrier);
			AssertEquals(leg.FlightNumber, scheduleInfo.FlightNumber);
			Assertion.AssertEquals(leg.Origin, scheduleInfo.Origin);
			AssertEquals(leg.DepartureDate, scheduleInfo.DepartureDate);
			Assertion.AssertEquals(leg.Destination, scheduleInfo.Destination);
			AssertEquals(leg.ArrivalDate, scheduleInfo.ArrivalDate);
		}

		void AssertScheduleInfo(FlightLegInformation legOrigin, FlightLegInformation legDestination, IS8MatchResult result)
		{
			var scheduleInfo = result.MatchedSchedule;

			Assertion.AssertEquals(legOrigin.Airline, scheduleInfo.Carrier);
			AssertEquals(legOrigin.FlightNumber, scheduleInfo.FlightNumber);
			Assertion.AssertEquals(legOrigin.Origin, scheduleInfo.Origin);
			AssertEquals(legOrigin.DepartureDate, scheduleInfo.DepartureDate);
			Assertion.AssertEquals(legDestination.Destination, scheduleInfo.Destination);
			AssertEquals(legDestination.ArrivalDate, scheduleInfo.ArrivalDate);
		}

		const string WebServiceResponse1114QF568 = "GetFlight         ( \"SSIM\", \"2018/11/14\", \"QF\", 568 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0568 2018/10/29 2019/01/31 1.34...  1 J PER 23:15 0 SYD 06:25 1 332\n";
		const string WebServiceResponse1115QF568 = "GetFlight         ( \"SSIM\", \"2018/11/15\", \"QF\", 568 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0568 2018/10/29 2019/01/31 1.34...  1 J PER 23:15 0 SYD 06:25 1 332\n";

		const string WebServiceResponse1112QF1 = "GetFlight         ( \"SSIM\", \"2018/11/12\", \"QF\", 1 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0001 2018/10/28 2019/03/29 1234567  1 J SYD 17:00 0 SIN 22:05 0 388\nQF 0001 2018/10/28 2019/03/29 1234567  2 J SIN 23:55 0 LHR 06:15 1 388\n";
		const string WebServiceResponse1113QF1 = "GetFlight         ( \"SSIM\", \"2018/11/13\", \"QF\", 1 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0001 2018/10/28 2019/03/29 1234567  1 J SYD 17:00 0 SIN 22:05 0 388\nQF 0001 2018/10/28 2019/03/29 1234567  2 J SIN 23:55 0 LHR 06:15 1 388\n";

		const string WebServiceResponse1107QF2 = "GetFlight         ( \"SSIM\", \"2018/11/07\", \"QF\", 2 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0002 2018/10/28 2019/03/29 1234567  1 J LHR 20:45 0 SIN 17:35 1 388\nQF 0002 2018/10/28 2019/03/29 1234567  2 J SIN 19:15 1 SYD 06:15 2 388\n";
		const string WebServiceResponse1108QF2 = "GetFlight         ( \"SSIM\", \"2018/11/08\", \"QF\", 2 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0002 2018/10/28 2019/03/29 1234567  1 J LHR 20:45 0 SIN 17:35 1 388\nQF 0002 2018/10/28 2019/03/29 1234567  2 J SIN 19:15 1 SYD 06:15 2 388\n";
		const string WebServiceResponse1109QF2 = "GetFlight         ( \"SSIM\", \"2018/11/09\", \"QF\", 2 )\nAL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\nQF 0002 2018/10/28 2019/03/29 1234567  1 J LHR 20:45 0 SIN 17:35 1 388\nQF 0002 2018/10/28 2019/03/29 1234567  2 J SIN 19:15 1 SYD 06:15 2 388\n";

		MethodCallResult<FlightInformation> GetFlight1114QF568Result => FlightInformation.Parse(WebServiceResponse1114QF568);
		MethodCallResult<FlightInformation> GetFlight1115QF568Result => FlightInformation.Parse(WebServiceResponse1115QF568);

		MethodCallResult<FlightInformation> GetFlight1112QF1Result => FlightInformation.Parse(WebServiceResponse1112QF1);
		MethodCallResult<FlightInformation> GetFlight1113QF1Result => FlightInformation.Parse(WebServiceResponse1113QF1);

		MethodCallResult<FlightInformation> GetFlight1107QF2Result => FlightInformation.Parse(WebServiceResponse1107QF2);
		MethodCallResult<FlightInformation> GetFlight1108QF2Result => FlightInformation.Parse(WebServiceResponse1108QF2);
		MethodCallResult<FlightInformation> GetFlight1109QF2Result => FlightInformation.Parse(WebServiceResponse1109QF2);

		MethodCallResult<FlightInformation> GetFlightResultError => new MethodCallResult<FlightInformation>("GetFlight Error");
		MethodCallResult<FlightInformation> GetFlightResultNotFound => new MethodCallResult<FlightInformation>("Flight not found.", shouldBeReported: false);
		MethodCallResult<FlightInformation> GetLoginIsSuppressed => new MethodCallResult<FlightInformation>("Login is suppressed");

		MethodCallResult<FlightInformation> GetFlightResultErrorInDate => new MethodCallResult<FlightInformation>("#Error in Date");

		class S8MatcherForTest : S8Matcher
		{
			readonly IS8Client s8Client;
			readonly IServiceRequestManager requestManager;

			public S8MatcherForTest(IS8Client s8Client)
			{
				this.s8Client = s8Client;
				requestManager = new S8ServiceRequestManager();
			}

			public override IS8MatchResult Match(ScheduleInfo scheduleToMatch)
			{
				var internalMatcher = new S8MatcherInternalWithMockClient(scheduleToMatch, s8Client, ShouldReportError(), requestManager);
				return internalMatcher.Match();
			}

			protected override bool ShouldReportError()
			{
				return true;
			}
		}

		class S8MatcherInternalWithMockClient : S8MatcherInternal
		{
			readonly IS8Client s8Client;

			public S8MatcherInternalWithMockClient(ScheduleInfo scheduleToMatch, IS8Client s8Client, bool shouldReportError, IServiceRequestManager requestManager) : base(scheduleToMatch, shouldReportError, requestManager)
			{
				this.s8Client = s8Client;
			}

			protected override IS8Client GetNewClient()
			{
				return s8Client;
			}
		}
	}
}
