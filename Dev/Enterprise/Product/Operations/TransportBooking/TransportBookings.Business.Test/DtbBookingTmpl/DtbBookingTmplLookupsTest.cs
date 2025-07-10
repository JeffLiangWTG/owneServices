using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingTmplLookupsTest : DtbTransportTmplLookupsTest
	{
		public void TestDirections()
		{
			var template = Factory.New<DtbBookingTmpl>();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).Directions, template.Lookups.Directions);
		}

		public void TestRatingFreightModes()
		{
			var template = Factory.New<DtbBookingTmpl>();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).RatingFreightModes.List, template.Lookups.RatingFreightModes);
		}
	}
}
