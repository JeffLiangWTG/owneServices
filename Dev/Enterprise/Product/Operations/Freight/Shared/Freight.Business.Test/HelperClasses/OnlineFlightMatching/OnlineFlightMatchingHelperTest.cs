using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	public class OnlineFlightMatchingHelperTest : TestCaseWithFactory
	{
		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestTryMatchAgainstOnlineFlights()
		{
			RunTestWithMockedS8Matcher(() =>
			{
				var flightInfoProvider = GetPopulatedFlightInfoProvider();
				var onlineFlightMatchingHelper = new OnlineFlightMatchingHelper();
				var (errorMessage, resultSchedule, resultStatus) = onlineFlightMatchingHelper.MatchAgainstOnlineFlights(flightInfoProvider);

				AssertEquals(Constants.FlightScheduleStatus.Matched, resultStatus);
			});
		}

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestTryMatchAgainstOnlineFlights_WithFlightNumberSuffix()
		{
			RunTestWithMockedS8Matcher(() =>
			{
				var flightInfoProvider = GetPopulatedFlightInfoProvider('S');
				var onlineFlightMatchingHelper = new OnlineFlightMatchingHelper();
				var (errorMessage, resultSchedule, resultStatus) = onlineFlightMatchingHelper.MatchAgainstOnlineFlights(flightInfoProvider);

				AssertEquals(Constants.FlightScheduleStatus.Matched, resultStatus);
			});
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

		TestFlightInformationProvider GetPopulatedFlightInfoProvider(char? suffix = null)
		{
			return new TestFlightInformationProvider
			{
				VoyageFlight = "QF69" + suffix.GetValueOrDefault(),
				DiscPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"),
				LoadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL"),
				ETD = ZDate.Today,
				ETA = ZDate.Today.AddDays(1)
			};
		}

		public static void RunTestWithMockedS8Matcher(Action test, string expectedStatus = Constants.FlightScheduleStatus.Matched, ZString aircraftType = default)
		{
			var result = new Mock<IS8MatchResult>(MockBehavior.Strict);
			result.Setup(r => r.ScheduleStatus).Returns(expectedStatus);
			result.Setup(r => r.MatchedSchedule).Returns(new ScheduleInfo("QF", 69, "SYD", ZDate.Today, "AKL", ZDate.Today.AddDays(1), aircraftType: aircraftType));
			result.Setup(r => r.MatchErrorMessage).Returns("");

			var matcher = new Mock<IS8Matcher>(MockBehavior.Strict);
			matcher.Setup(m => m.Match(It.IsAny<ScheduleInfo>())).Returns(result.Object);
			matcher.SetupGet(m => m.ServiceRequestManager).Returns(new S8ServiceRequestManager());

			using (Enterprise.MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTestAttribute.Enable())
			using (ObjectFactory.Substitute(matcher.Object))
			{
				test();
			}
		}

		#endregion
	}
}
