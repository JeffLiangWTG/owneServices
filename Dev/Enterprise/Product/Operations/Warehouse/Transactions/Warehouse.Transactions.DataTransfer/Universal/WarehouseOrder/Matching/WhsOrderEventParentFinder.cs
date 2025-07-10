using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderEventParentFinder : WhsOrderAndReceiveEventParentFinder<WhsOrder>
	{
		public WhsOrderEventParentFinder(BusinessObjectFactory factory, WarehouseOrderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		public WhsOrderEventParentFinder(BusinessObjectFactory factory, WarehouseBondedChangeOfInventoryDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override void AddSpecificReferences(WhsOrderAndReceiveReferences whsOrderReferences, IXmlEventValueObject eventValueObject)
		{
			whsOrderReferences.ExternalReference = eventValueObject.Context.OrderNumber;
			whsOrderReferences.ExternalReferenceSplit = eventValueObject.Context.OrderNumberSplit;
		}

		protected override WhsOrderAndReceiveLastResortMatcher<WhsOrder> GetMatcher(WhsOrderAndReceiveReferences referencesParent)
		{
			return new WhsOrderMatcher(factory, referencesParent, logger);
		}

		protected override string DocketTypeCode
		{
			get { return DocketType.Codes.Order; }
		}

		protected override CustomsDataSourceHelper<WhsOrder> GetNewCustomsHelper(UniversalEvent xmlEvent, IDataContextDataObject topLevelDataObject)
		{
			return new CustomsDataSourceHelperForOrder(xmlEvent, topLevelDataObject);
		}
	}
}