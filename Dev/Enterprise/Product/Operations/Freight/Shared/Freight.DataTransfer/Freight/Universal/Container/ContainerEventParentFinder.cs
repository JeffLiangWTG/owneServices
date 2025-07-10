using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	internal class ContainerEventParentFinder : EventParentFinder
	{
		public ContainerEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger) : base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return null;
		}
	}
}
