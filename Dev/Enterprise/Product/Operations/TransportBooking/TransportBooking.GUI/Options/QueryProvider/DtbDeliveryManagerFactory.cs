using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.GUI.Options.QueryProvider
{
	class DtbDeliveryManagerFactory : IDtbDeliveryManagerFactory
	{
		public IDtbDeliveryManager CreateManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager)
		{
			return new DtbDeliveryManager(factory, parent, direction, combineContainers, logger, errorManager);
		}
	}
}
