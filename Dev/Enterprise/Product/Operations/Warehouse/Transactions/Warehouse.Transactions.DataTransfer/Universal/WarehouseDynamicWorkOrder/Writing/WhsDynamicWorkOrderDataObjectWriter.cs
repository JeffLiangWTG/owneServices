using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDynamicWorkOrderDataObjectWriter : WhsComponentOrderDataObjectWriter<WhsDynamicWorkOrder>
	{
		internal WhsDynamicWorkOrderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(WhsDynamicWorkOrder whsDocketBO, Shipment shipmentDataObject)
		{
			base.PopulateDataObject(whsDocketBO, shipmentDataObject);

			shipmentDataObject.OuterPacks = ToZIntSafely(whsDocketBO.WD_TotalUnits, nameof(whsDocketBO.WD_TotalUnits));

			var packTypeCode = Constants.PkgUnit.Piece;
			shipmentDataObject.OuterPacksPackageType = new PackageType { Code = packTypeCode, Description = Constants.PkgUnit.GetDescription(packTypeCode) };

			var orderDataObject = shipmentDataObject.Order;
			orderDataObject.SetOrderLineCollection(() => ProcessCollection(whsDocketBO.Lines, new WhsDynamicWorkOrderLineDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.WarehouseDynamicWorkOrder;
	}
}
