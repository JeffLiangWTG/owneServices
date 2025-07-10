using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.GateManagement.Integration.Interfaces
{
	public interface IGateManagementFacilityDataContextManager : IShipmentDataContextManager
	{
		BusinessObject GetLinkedEntity(UniversalObjectFactory factory, IGteGateMovementBooking gateMovementBooking);
	}
}
