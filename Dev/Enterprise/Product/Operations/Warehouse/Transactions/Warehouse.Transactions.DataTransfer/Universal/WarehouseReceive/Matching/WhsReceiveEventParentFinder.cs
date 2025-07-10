using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsReceiveEventParentFinder : WhsOrderAndReceiveEventParentFinder<WhsReceive>
	{
		internal WhsReceiveEventParentFinder(BusinessObjectFactory factory, WarehouseReceiveDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger) { }

		internal WhsReceiveEventParentFinder(BusinessObjectFactory factory, WarehouseBondedChangeOfInventoryDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger) { }

		protected override void AddSpecificReferences(WhsOrderAndReceiveReferences whsOrderReferences, IXmlEventValueObject eventValueObject)
		{
			whsOrderReferences.ExternalReference = eventValueObject.Context.ReceiveReference;
			whsOrderReferences.ExternalReferenceSplit = eventValueObject.Context.ReceiveReferenceSplit;
		}

		protected override WhsOrderAndReceiveLastResortMatcher<WhsReceive> GetMatcher(WhsOrderAndReceiveReferences referencesParent)
		{
			return new WhsReceiveMatcher(factory, referencesParent, logger);
		}

		protected override string DocketTypeCode
		{
			get { return DocketType.Codes.Receive; }
		}

		protected override CustomsDataSourceHelper<WhsReceive> GetNewCustomsHelper(UniversalEvent xmlEvent, IDataContextDataObject topLevelDataObject)
		{
			return new CustomsDataSourceHelperForReceive(xmlEvent, topLevelDataObject);
		}
	}
}