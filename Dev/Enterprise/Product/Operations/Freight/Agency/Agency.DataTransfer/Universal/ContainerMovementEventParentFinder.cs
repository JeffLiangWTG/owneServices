using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class ContainerMovementEventParentFinder : EventParentFinder
	{
		public ContainerMovementEventParentFinder(BusinessObjectFactory factory, ContainerMovementDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			//We never expect to import universal event for container movement, only for container stock.
			return null;
		}
	}
}
