using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyShipmentDefaultFilterProviderTest : DefaultFilterProviderTest<AgencyShipmentDefaultFilterProvider>
	{
		public const string VoyageVessel = "Voyage / Vessel";
		public const string ContainerNumber = "Container #";
		public const string CargoType = "Cargo Type";
		public const string BookingParty = "Booking Party";
		public const string ConsignorConsignee = "Consignor / Consignee";
		public const string LoadDischarge = "Load / Discharge";
		public const string OriginDestination = "Origin / Destination";
		public const string ETD = "ETD";
		public const string ETA = "ETA";
		public void TestVessel()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, VoyageVessel);
			Provider.Vessel = "MAJAPAHIT";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, VoyageVessel, "Vessel", (ZString)"MAJAPAHIT");
		}

		public void TestVoyage()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, VoyageVessel);
			Provider.Voyage = "voy";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, VoyageVessel, "VoyageFlightNo", (ZString)"voy");
		}

		public void TestBookingParty()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, BookingParty);
			Provider.BookingParty = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, BookingParty, "Property", Provider.BookingParty);
		}

		public void TestConsignor()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ConsignorConsignee);
			Provider.Consignor = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ConsignorConsignee, "Property1", Provider.Consignor);
		}

		public void TestConsignee()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ConsignorConsignee);
			Provider.Consignee = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ConsignorConsignee, "Property2", Provider.Consignee);
		}

		public void TestContainerNum()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ContainerNumber);
			Provider.ContainerNumber = "FAKE4100011";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ContainerNumber, "Property", (ZString)"FAKE4100011");
		}

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, CargoType);
			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, CargoType, "Property", (ZString)Core.Constants.ContainerModes.FCL);
		}

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);
			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property1", HomePort);
		}

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);
			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property2", HomePort);
		}

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);
			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property1", HomePort);
		}

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);
			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property2", HomePort);
		}

		public void TestETDFrom()
		{
			ZDateTime today = ZDateTime.Today;
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);
			Provider.ETDFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property1", today);
		}

		public void TestETDTo()
		{
			ZDateTime today = ZDateTime.Today;
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);
			Provider.ETDTo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property2", today);
		}

		public void TestETAFrom()
		{
			ZDateTime today = ZDateTime.Today;
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);
			Provider.ETAFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property1", today);
		}

		public void TestETATo()
		{
			ZDateTime today = ZDateTime.Today;
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);
			Provider.ETATo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property2", today);
		}
	}
}
