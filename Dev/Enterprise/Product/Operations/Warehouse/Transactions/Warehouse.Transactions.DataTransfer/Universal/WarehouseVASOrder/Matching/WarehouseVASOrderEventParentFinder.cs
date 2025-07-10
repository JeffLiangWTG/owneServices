using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WarehouseVASOrderEventParentFinder : EventParentFinder
	{
		internal WarehouseVASOrderEventParentFinder(BusinessObjectFactory factory, WarehouseVASOrderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return null;
		}
	}
}