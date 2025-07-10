using Enterprise.Freight.Agency.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyBookingWebUserVisibleNotesSupportTest : LinerAndAgencyBaseWebUserVisibleNotesSupportTest
	{
		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			return new LinerAndAgencyBookingWebInterfacesHelper(Shipment as TrackingLinerAndAgencyBooking);
		}

		protected override AgencyShipment GetBusinessObjectForHelper()
		{
			return Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
		}
	}
}
