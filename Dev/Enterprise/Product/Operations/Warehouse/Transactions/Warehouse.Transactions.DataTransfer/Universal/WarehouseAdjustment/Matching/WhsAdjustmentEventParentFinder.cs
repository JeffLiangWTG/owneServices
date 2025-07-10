using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsAdjustmentEventParentFinder : EventParentFinder
	{
		internal WhsAdjustmentEventParentFinder(BusinessObjectFactory factory, WarehouseAdjustmentDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var eventValueObject = (IXmlEventValueObject)xmlEvent;
			var whsDocketReferences = new WhsAdjustmentReferences();
			whsDocketReferences.AdjustmentReference = eventValueObject.Context.AdjustmentReference;

			var bestMatch = new WhsAdjustmentMatcher(factory, whsDocketReferences, logger).GetBestMatch();

			return bestMatch == null ? null : new BusinessObject[] { bestMatch };
		}
	}
}
