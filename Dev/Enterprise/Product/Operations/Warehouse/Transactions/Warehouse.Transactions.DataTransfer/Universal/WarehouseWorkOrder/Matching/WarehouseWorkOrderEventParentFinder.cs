using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WarehouseWorkOrderEventParentFinder : WhsDocketEventParentFinder<WhsWorkOrder>
	{
		internal WarehouseWorkOrderEventParentFinder(BusinessObjectFactory factory, WarehouseWorkOrderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			var eventValueObject = (IXmlEventValueObject)xmlEvent;

			var workOrderReferences = new WhsWorkOrderReferences
			{
				ExternalReference = eventValueObject.Context.OrderNumber,
				ExternalReferenceSplit = eventValueObject.Context.OrderNumberSplit,
				References = GetAdditionalReferencesForMatching(factory, xmlEvent),
			};

			var bestMatch = new WhsWorkOrderLastResortMatcher(factory, workOrderReferences, logger).GetBestMatch();
			return bestMatch == null ? null : new BusinessObject[] { bestMatch };
		}
	}
}
