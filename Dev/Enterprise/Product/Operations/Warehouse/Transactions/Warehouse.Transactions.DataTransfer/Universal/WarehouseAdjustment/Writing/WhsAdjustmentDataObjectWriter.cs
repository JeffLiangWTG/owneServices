using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentDataObjectWriter : WhsDocketDataObjectWriter<WhsAdjustment>
	{
		internal WhsAdjustmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(WhsAdjustment adjustment, UniversalShipment shipmentDataObject)
		{
			base.PopulateDataObject(adjustment, shipmentDataObject);

			shipmentDataObject.Order.SetOrderLineCollection(() => ProcessCollection(adjustment.Lines, new WhsAdjustmentLineDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WarehouseAdjustment;
		}
	}
}
