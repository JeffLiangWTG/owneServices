using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WarehouseDynamicWorkOrderEventParentFinder : WhsDocketEventParentFinder<WhsDynamicWorkOrder>
	{
		internal WarehouseDynamicWorkOrderEventParentFinder(BusinessObjectFactory factory, WarehouseDynamicWorkOrderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			var eventValueObject = (IXmlEventValueObject)xmlEvent;

			var dynamicWorkOrderReferences = new WhsDynamicWorkOrderReferences
			{
				ExternalReference = eventValueObject.Context.OrderNumber,
				ExternalReferenceSplit = eventValueObject.Context.OrderNumberSplit,
			};

			var bestMatch = new WhsDynamicWorkOrderLastResortMatcher(factory, dynamicWorkOrderReferences, logger).GetBestMatch();
			return bestMatch == null ? null : new BusinessObject[] { bestMatch };
		}
	}
}
