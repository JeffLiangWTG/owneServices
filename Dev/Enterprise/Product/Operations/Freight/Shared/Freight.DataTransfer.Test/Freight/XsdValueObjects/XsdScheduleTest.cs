using CargoWise.EntityFramework.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdScheduleTest : TestCaseWithFactory
	{
		public void TestGetSailings_WhenScheduleNull()
		{
			AssertEquals("When no schedule, should have zero sailings", 0, XsdSchedule.GetSailings(null).Length);
		}

		public void TestGetSailings()
		{
			Xsd.Schedule schedule = new Xsd.Schedule();

			Xsd.ScheduleSailing sailing = new Xsd.ScheduleSailing();
			schedule.Item = sailing;
			AssertEquals("When sailings collection null", 0, XsdSchedule.GetSailings(schedule).Length);
			sailing.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			sailing.Sailings.AddNew();
			AssertEquals("When sailings populated", 1, XsdSchedule.GetSailings(schedule).Length);

			Xsd.ScheduleRoadRailFlight flight = new Xsd.ScheduleRoadRailFlight();
			schedule.Item = flight;
			AssertEquals("When flights collection null", 0, XsdSchedule.GetSailings(schedule).Length);
			flight.Flights = new Xsd.FlightWithLoadDischargePortsAndConsolsCollection();
			flight.Flights.AddNew();
			AssertEquals("When flights populated", 1, XsdSchedule.GetSailings(schedule).Length);
		}

		public void TestGetVessel()
		{
			Xsd.Schedule schedule = new Xsd.Schedule();

			Xsd.ScheduleSailing sailing = new Xsd.ScheduleSailing();
			schedule.Item = sailing;
			AssertEquals("No Vessel on sailing", "", XsdSchedule.GetVessel(schedule));
			sailing.VesselName = "vessel";
			AssertEquals("LoadPort on sailing", "vessel", XsdSchedule.GetVessel(schedule));

			Xsd.ScheduleRoadRailFlight flight = new Xsd.ScheduleRoadRailFlight();
			schedule.Item = flight;
			AssertEquals("No Vessel on flight", "", XsdSchedule.GetVessel(schedule));
			flight.FlightNoJourneyNoTruckRegNo = "this is a decoy value";
			AssertEquals("LoadPort on flight", "", XsdSchedule.GetVessel(schedule));
		}

		public void TestGetVoyage()
		{
			Xsd.Schedule schedule = new Xsd.Schedule();

			Xsd.ScheduleSailing sailing = new Xsd.ScheduleSailing();
			schedule.Item = sailing;
			AssertEquals("No Voyage on sailing", "", XsdSchedule.GetVoyage(schedule));
			sailing.VoyageNo = "voyage";
			AssertEquals("LoadPort on sailing", "voyage", XsdSchedule.GetVoyage(schedule));

			Xsd.ScheduleRoadRailFlight flight = new Xsd.ScheduleRoadRailFlight();
			schedule.Item = flight;
			AssertEquals("No Voyage on flight", "", XsdSchedule.GetVoyage(schedule));
			flight.FlightNoJourneyNoTruckRegNo = "voyage";
			AssertEquals("LoadPort on flight", "voyage", XsdSchedule.GetVoyage(schedule));
		}
	}
}
