using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingConsolidationLookupsTest : DtbTransportConsolidationLookupsTest
	{
		public void TestBookingsFindBoxList()
		{
			AssertNotEquals("Collection should not be cached.", Lookups.BookingsFindBoxList, Lookups.BookingsFindBoxList);

			// ensure the transportCo is passed to the find-box collection

			Consolidation.Address.OrganisationPK = Helper.CreateOrganisation("ABC").PK;
			var filterDefault = Lookups.BookingsFindBoxList.FilterBusinessObjectDefaults[FilterNameConstants.TransportCompany + ":Property"];
			AssertEquals(Consolidation.Address.OrganisationPK, filterDefault.Value);
		}

		public void TestLocalTransportOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(LocalTransportCollection), Lookups.LocalTransportOrganisations.GetType());
		}

		DtbBookingConsolidationLookups Lookups
		{
			get { return lookups ?? (lookups = new DtbBookingConsolidationLookups(Consolidation)); }
		}

		DtbBookingConsolidation Consolidation
		{
			get { return consolidation ?? (consolidation = Helper.CreateConsolidation()); }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		DtbBookingConsolidationLookups lookups;
		DtbBookingConsolidation consolidation;
		TransportBookingTestHelper helper;
	}
}
