using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.Business
{
	sealed class TRETradeHVLVAsycudaPackedItemDataObjectReader : AsycudaPackedItemObjectReader
	{
		public TRETradeHVLVAsycudaPackedItemDataObjectReader(PackingLine dataObject, PackedItem packedItemDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaPack pack, AsycudaManifestDataObjectReaderHelper helper, Shipment hvlvShipmentDataObject)
			: base(dataObject, null, logger, factory, pack, helper)
		{
			this.packedItemDataObject = packedItemDataObject;
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly PackedItem packedItemDataObject;
		readonly Shipment hvlvShipmentDataObject;

		protected override void PopulatePackedItemForSpecificRules(ASYCUDA.Business.AsycudaPackedItem packedItem)
		{
			var packedItemRow = GetColumnIndexer(packedItem);

			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsUQ2, TRUOMCodeList.Codes.KGM);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsValue, packedItemDataObject.CIFValue);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsDescription, packedItemDataObject.Description);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsQty, packedItemDataObject.PackedQuantity);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GrossWeight, packedItemDataObject.GrossWeight);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GrossWeightUQ, packedItemDataObject.GrossWeightUnit);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_NetWeight, packedItemDataObject.NetWeight);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_NetWeightUQ, packedItemDataObject.NetWeightUnit);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsValue, packedItemDataObject.GoodsValue);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_RX_NKGoodsValueCurrency, hvlvShipmentDataObject.GoodsValueCurrency?.Code);

			var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(hvlvShipmentDataObject);
			if (commercialInvoiceLine != null)
			{
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, commercialInvoiceLine.CountryOfOrigin);
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
			}

			if (packedItemDataObject.GrossWeight.HasValue)
			{
				var grossWeight = packedItemDataObject.GrossWeight.Value;
				if (packedItemDataObject.GrossWeightUnit.Code.HasValue && packedItemDataObject.GrossWeightUnit.Code.Value != Weight.Kilograms)
				{
					grossWeight = Weight.Convert(grossWeight, packedItemDataObject.GrossWeightUnit.Code.Value, Weight.Kilograms);
				}

				SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsQty2, grossWeight);
			}
		}
	}
}
