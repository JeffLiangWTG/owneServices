using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBookingWebInterfacesHelper))]
	sealed class LinerAndAgencyBookingWebDocumentsTest : LinerAndAgencyBaseWebDocumentsTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return new LinerAndAgencyBookingWebInterfacesHelper(Shipment as TrackingLinerAndAgencyBooking);
		}

		protected override AgencyShipment GetBusinessObjectForHelper()
		{
			return Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
		}
	}
}
