using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsComponentOrderDataObjectWriter<T> : WhsDocketDataObjectWriter<T>
		where T : WhsComponentOrder
	{
		protected WhsComponentOrderDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override void PopulateDataObject(T whsDocketBO, Shipment shipmentDataObject)
		{
			base.PopulateDataObject(whsDocketBO, shipmentDataObject);

			var orderDataObject = shipmentDataObject.Order;
			orderDataObject.OrderNumberSplit = whsDocketBO.WD_ExternalReferenceSplit;
			orderDataObject.TotalLineVolume = whsDocketBO.WD_TotalCubic;
			orderDataObject.TotalLineWeight = whsDocketBO.WD_TotalWeight;
			orderDataObject.TotalUnits = whsDocketBO.WD_TotalUnits;
			orderDataObject.PickPriority = whsDocketBO.WD_PickPriority;
			orderDataObject.Type = ListHelper.GetWithDescription<CodeDescriptionPair>(whsDocketBO.WD_DocketSubType, whsDocketBO.Lookups.SubTypes);
			orderDataObject.PickOption = ListHelper.GetWithDescription<CodeDescriptionPair>(whsDocketBO.WD_PickOption, whsDocketBO.Lookups.PickOptions);
			orderDataObject.AutoFinaliseBOMIntoInventory = whsDocketBO.WD_AutoFinaliseBOMIntoInventory;
			orderDataObject.IsInwardsProcessingJob = whsDocketBO.WD_IsInwardsProcessingJob;

			shipmentDataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy) { DeliveryRequiredBy = whsDocketBO.WD_RequiredDate.ToZDateTime() };
			shipmentDataObject.TotalVolume = whsDocketBO.WD_TotalCubic;
			shipmentDataObject.TotalWeight = whsDocketBO.WD_TotalWeight;
			shipmentDataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(whsDocketBO.WD_TotalCubicUnit, whsDocketBO.TotalCubicUnits);
			shipmentDataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(whsDocketBO.WD_TotalWeightUnit, whsDocketBO.TotalWeightUnits);
		}
	}
}
