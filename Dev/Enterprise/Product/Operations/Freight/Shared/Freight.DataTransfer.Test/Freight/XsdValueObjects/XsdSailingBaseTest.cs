using CargoWise.EntityFramework.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdSailingBaseTest : TestCaseWithFactory
	{
		public void TestGetLoadPort()
		{
			Xsd.SailingWithLoadDischargePorts sailing = new Xsd.SailingWithLoadDischargePorts();
			AssertEquals("No LoadPort on sailing", "", XsdSailingBase.GetLoadPort(sailing));
			sailing.LoadPort = "AUMEL";
			AssertEquals("LoadPort on sailing", "AUMEL", XsdSailingBase.GetLoadPort(sailing));

			Xsd.FlightWithLoadDischargePorts flight = new Xsd.FlightWithLoadDischargePorts();
			AssertEquals("No LoadPort on sailing", "", XsdSailingBase.GetLoadPort(flight));
			flight.LoadPort = "AUMEL";
			AssertEquals("LoadPort on sailing", "AUMEL", XsdSailingBase.GetLoadPort(flight));
		}

		public void TestGetDischargePort()
		{
			Xsd.SailingWithLoadDischargePorts sailing = new Xsd.SailingWithLoadDischargePorts();
			AssertEquals("No DischargePort on sailing", "", XsdSailingBase.GetDischargePort(sailing));
			sailing.DischargePort = "AUMEL";
			AssertEquals("DischargePort on sailing", "AUMEL", XsdSailingBase.GetDischargePort(sailing));

			Xsd.FlightWithLoadDischargePorts flight = new Xsd.FlightWithLoadDischargePorts();
			AssertEquals("No DischargePort on sailing", "", XsdSailingBase.GetDischargePort(flight));
			flight.DischargePort = "AUMEL";
			AssertEquals("DischargePort on sailing", "AUMEL", XsdSailingBase.GetDischargePort(flight));
		}

		public void TestGetVoyage()
		{
			Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
			AssertEquals("No Voyage on sailing", "", XsdSailingBase.GetVoyage(sailing));
			sailing.VoyageNo = "voyage";
			AssertEquals("LoadPort on sailing", "voyage", XsdSailingBase.GetVoyage(sailing));

			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			AssertEquals("No Voyage on flight", "", XsdSailingBase.GetVoyage(flight));
			flight.FlightNoJourneyNoTruckRegNo = "voyage";
			AssertEquals("LoadPort on flight", "voyage", XsdSailingBase.GetVoyage(flight));
		}
	}
}
