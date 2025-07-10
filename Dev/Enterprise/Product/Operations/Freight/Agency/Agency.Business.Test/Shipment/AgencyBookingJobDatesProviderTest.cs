namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyBookingJobDatesProviderTest : AgencyShipmentJobDatesProviderTest
	{
		protected override AgencyShipment GetAgencyShipment()
		{
			return Factory.New<AgencyBooking>();
		}
	}
}
