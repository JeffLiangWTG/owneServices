using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbDeliveryManagerFactory
	{
		IDtbDeliveryManager CreateManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager);
	}
}
