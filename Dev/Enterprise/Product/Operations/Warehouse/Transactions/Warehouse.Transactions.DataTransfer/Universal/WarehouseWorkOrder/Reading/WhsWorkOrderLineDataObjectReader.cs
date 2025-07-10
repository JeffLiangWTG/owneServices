using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsWorkOrderLineDataObjectReader : WhsPickableDocketLineDataObjectReader<WhsWorkOrder, WhsWorkOrderLine>
	{
		internal WhsWorkOrderLineDataObjectReader(
			OrderLine orderLineDataObject,
			IXmlImportLogger logger,
			UniversalObjectFactory factory,
			WhsWorkOrder parent,
			IEnumerable<WhsWorkOrderLine> matchedLines)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
		}

		protected override string NotAllowedToChangeRestrictedFieldsMessage => Res.GetString("3eadfd66-f70e-4379-b936-24e388a3d63f", "Cannot change product on an allocated Work Order.");

		protected override string DocketLineType => WhsWorkOrderDataObjectReader.WorkOrderDocketType;

		protected override bool CanCreateNewProducts => false;

		protected override CustomsDataSourceHelper<WhsWorkOrder> GetNewCustomsDataSourceHelper(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext) => null;

		protected override bool ShouldPopulateCustomsData => false;

		protected override void SetBondedEntryKey(WhsWorkOrderLine line)
		{
		}
	}
}
