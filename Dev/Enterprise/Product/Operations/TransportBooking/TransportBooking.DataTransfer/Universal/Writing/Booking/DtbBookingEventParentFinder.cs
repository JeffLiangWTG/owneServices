using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingEventParentFinder : EventParentFinder
	{
		public DtbBookingEventParentFinder(BusinessObjectFactory factory, DtbBookingDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			// if this is changed, update DtbConfirmationEventParentFinder to not fall back to booking.
			return null;
		}
	}
}
