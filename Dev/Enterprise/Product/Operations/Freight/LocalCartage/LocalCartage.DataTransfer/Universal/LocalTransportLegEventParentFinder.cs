using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class LocalTransportLegEventParentFinder : EventParentFinder
	{
		internal LocalTransportLegEventParentFinder(BusinessObjectFactory factory, LocalTransportLegDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return null;
		}
	}
}
