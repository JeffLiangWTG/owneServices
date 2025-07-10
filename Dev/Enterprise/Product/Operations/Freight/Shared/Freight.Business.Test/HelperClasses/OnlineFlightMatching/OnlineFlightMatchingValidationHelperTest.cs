using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OnlineFlightMatchingValidationHelperTest : TestCaseWithFactory
	{
		public void TestGetOnlineFlightMatchStatusWarnings_Matched()
		{
			var flightInfoProvider = GetPopulatedFlightInfoProvider();

			var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);

			AssertEquals(0, warningMessages.Count);
		}

		public void TestGetOnlineFlightMatchStatusWarnings_SpecificWarning_AllWrong()
		{
			var discPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var flightInfoProvider = new TestFlightInformationProvider
			{
				MatchedSchedule = new ScheduleInfo("QF", 69, "SYD", new ZDate(2019, 01, 01), "AKL", new ZDate(2019, 01, 02)),
				OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched,
				VoyageFlight = "QF69",
				DiscPort = discPort,
				LoadPort = loadPort,
				ETD = new ZDateTime(2019, 01, 02),
				ETA = new ZDateTime(2019, 01, 03)
			};

			var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
			AssertEquals("PreAssert", 1, warningMessages.Count);
			AssertEquals(string.Format(@"The following flight details do not match
Load Port: {0}
Departure Date: 01-Jan-19
Discharge Port: {1}
Arrival Date: 02-Jan-19
", discPort.RL_IATA, loadPort.RL_IATA), warningMessages[0]);
		}

		public void TestGetOnlineFlightMatchStatusWarnings_SpecificWarning_SomeWrong()
		{
			var discPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var flightInfoProvider = GetPopulatedFlightInfoProvider();
			flightInfoProvider.OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;
			flightInfoProvider.DiscPort = discPort;
			flightInfoProvider.LoadPort = loadPort;

			var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
			AssertEquals("PreAssert", 1, warningMessages.Count);
			AssertEquals(string.Format(@"The following flight details do not match
Load Port: {0}
Discharge Port: {1}
", discPort.RL_IATA, loadPort.RL_IATA), warningMessages[0]);
		}

		public void TestGetOnlineFlightMatchStatusWarnings_GenericWarning_PartiallyMatched()
		{
			var flightInfoProvider = GetPopulatedFlightInfoProvider();
			flightInfoProvider.MatchedSchedule = ScheduleInfo.Empty;
			flightInfoProvider.OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
			AssertEquals("PreAssert", 1, warningMessages.Count);
			AssertEquals("This Air Routing Leg only partially matches to a Global Flight Schedule flight. This flight may not be fully tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight.", warningMessages[0]);
		}

		public void TestGetOnlineFlightMatchStatusWarnings_GenericWarning_Unmatched()
		{
			var flightInfoProvider = GetPopulatedFlightInfoProvider();
			flightInfoProvider.MatchedSchedule = ScheduleInfo.Empty;
			flightInfoProvider.OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;

			var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
			AssertEquals("PreAssert", 1, warningMessages.Count);
			AssertEquals("This Air Routing Leg does not match any of the Global Flight Schedule flights. This flight may not be tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight.", warningMessages[0]);
		}

		public void TestGetOnlineFlightMatchStatusWarnings_FlightServerConnectionIsSuppressed()
		{
			var flightInfoProvider = GetPopulatedFlightInfoProvider();
			flightInfoProvider.MatchedSchedule = ScheduleInfo.Empty;
			flightInfoProvider.OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;

			var suppressionWarning = "Cannot connect to flight web service. Please wait for 20 minutes and try again.";

			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				flightInfoProvider.Matcher.ServiceRequestManager.OnSuccessfulRequest();
				var warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
				Assert(warningMessages.All(w => w != suppressionWarning));

				flightInfoProvider.Matcher.ServiceRequestManager.OnTimedOutRequest();
				warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
				Assert(warningMessages.Any(w => w == suppressionWarning));

				flightInfoProvider.Matcher.ServiceRequestManager.OnSuccessfulRequest();
				warningMessages = OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(flightInfoProvider);
				Assert(warningMessages.All(w => w != suppressionWarning));
			}
		}

		#region Implementation

		class TestFlightInformationProvider : IFlightInformationProvider
		{
			public ScheduleInfo MatchedSchedule { get; set; }
			public ZString OnlineScheduleStatus { get; set; }
			public string MatchErrorMessage { get; set; }
			public ZString VoyageFlight { get; set; }
			public RefUNLOCO DiscPort { get; set; }
			public RefUNLOCO LoadPort { get; set; }
			public ZDateTime ETD { get; set; }
			public ZDateTime ETA { get; set; }
			public IS8Matcher Matcher { get; set; }
		}

		TestFlightInformationProvider GetPopulatedFlightInfoProvider()
		{
			return new TestFlightInformationProvider
			{
				MatchedSchedule = new ScheduleInfo("QF", 69, "SYD", new ZDate(2019, 01, 01), "AKL", new ZDate(2019, 01, 02)),
				OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched,
				VoyageFlight = "QF69",
				DiscPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"),
				LoadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL"),
				ETD = new ZDateTime(2019, 01, 01),
				ETA = new ZDateTime(2019, 01, 02),
				Matcher = new S8Matcher()
			};
		}

		#endregion
	}
}
