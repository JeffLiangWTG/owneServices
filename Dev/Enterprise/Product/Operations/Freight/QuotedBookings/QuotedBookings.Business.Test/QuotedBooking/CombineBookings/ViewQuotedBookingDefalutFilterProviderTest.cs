using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBookingDefalutFilterProvider))]
	public class ViewQuotedBookingDefalutFilterProviderTest : DefaultFilterProviderTest<ViewQuotedBookingDefalutFilterProvider>
	{
		public void TestVessel()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel);
			Provider.Vessel = "MAJAPAHIT";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel, "Vessel", (ZString)"MAJAPAHIT");
		}

		public void TestVoyage()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel);
			Provider.Voyage = "voy";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel, "VoyageFlightNo", (ZString)"voy");
		}

		public void TestBookingParty()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.BookingParty);
			Provider.BookingParty = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.BookingParty, "Property", Provider.BookingParty);
		}

		public void TestCarrier()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.BookingParty);
			Provider.Carrier = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.Carrier, "Property", Provider.Carrier);
		}

		public void TestClient()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.Client);
			Provider.Client = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.Client, "Property", Provider.Client);
		}

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.TransportMode);
			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.TransportMode, "Property", (ZString)Core.Constants.TransportModes.Sea);
		}

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.ContainerMode);
			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.ContainerMode, "Property", (ZString)Core.Constants.ContainerModes.FCL);
		}

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge);
			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge, "Property1", HomePort);
		}

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge);
			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge, "Property2", HomePort);
		}

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination);
			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination, "Property1", HomePort);
		}

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination);
			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination, "Property2", HomePort);
		}

		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.QuotedBookings;
			}
		}
	}
}
