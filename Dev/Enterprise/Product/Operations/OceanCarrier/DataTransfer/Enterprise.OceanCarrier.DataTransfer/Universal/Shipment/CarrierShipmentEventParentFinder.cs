using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.OceanCarrier.DataTransfer.Universal
{
	public class CarrierShipmentEventParentFinder : EventParentFinder
	{
		public CarrierShipmentEventParentFinder(BusinessObjectFactory factory, CarrierShipmentDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return null;
		}
	}
}
