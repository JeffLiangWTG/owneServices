using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyBookingShipmentEventParentFinderTest : AgencyShipmentEventParentFinderTest<AgencyBooking>
	{
		protected override IEventDataContextManager GetNewEventDataContextManager()
		{
			return new AgencyBookingDataContextManager();
		}

		protected override ZString GetShipmentType()
		{
			return "Agency Booking";
		}
	}
}
