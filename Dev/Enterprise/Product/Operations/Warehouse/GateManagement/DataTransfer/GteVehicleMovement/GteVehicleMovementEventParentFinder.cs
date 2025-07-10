using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GteVehicleMovementEventParentFinder : EventParentFinder
	{
		public GteVehicleMovementEventParentFinder(GteVehicleMovementDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
	: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return null;
		}
	}
}
