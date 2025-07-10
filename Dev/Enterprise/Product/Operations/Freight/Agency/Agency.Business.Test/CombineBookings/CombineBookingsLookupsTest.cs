using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class CombineBookingsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBookings()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "1234";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			OrgHeader bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			MasterBooking.JS_PackingMode = "";
			AssertNoDefaults(Combine.Lookups.Bookings);
			MasterBooking.JS_PackingMode = Constants.ContainerModes.FCL;
			MasterBooking.JS_JX = sailing.PK;
			MasterBooking.JS_RL_NKOrigin = "AUSYD";
			MasterBooking.JS_RL_NKDestination = "GBLON";
			MasterBooking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			MasterBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			MasterBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			IFilterBusinessObjectDefaultsProvider provider = Combine.Lookups.Bookings;
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.CargoType, "Property", (ZString)Constants.ContainerModes.FCL);
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.VoyageVessel, "VoyageFlightNo", (ZString)"1234");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.VoyageVessel, "Vessel", (ZString)"MAJAPAHIT");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.LoadDischarge, "Property1", (ZString)"AUBNE");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.LoadDischarge, "Property2", (ZString)"NLAMS");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.OriginDestination, "Property1", (ZString)"AUSYD");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.OriginDestination, "Property2", (ZString)"GBLON");
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.BookingParty, "Property", bookingParty.PK);
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.ConsignorConsignee, "Property1", consignor.PK);
			AssertHasDefault(provider, AgencyShipmentDefaultFilterProviderTest.ConsignorConsignee, "Property2", consignee.PK);
		}

		#region Implementation
		public CombineBookings Combine
		{
			get
			{
				return combine ?? (combine = new CombineBookings(MasterBooking));
			}
		}

		CombineBookings combine;
		public AgencyBooking MasterBooking
		{
			get
			{
				return masterBooking ?? (masterBooking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking masterBooking;
		#endregion
	}
}
