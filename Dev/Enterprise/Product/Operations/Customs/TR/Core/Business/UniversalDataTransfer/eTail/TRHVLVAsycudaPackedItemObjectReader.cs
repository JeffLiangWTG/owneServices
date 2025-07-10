using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer
{
	public class TRHVLVAsycudaPackedItemObjectReader : AsycudaPackedItemObjectReader
	{
		public TRHVLVAsycudaPackedItemObjectReader(PackingLine dataObject, PackedItem packedItemDataObject, List<AddInfo> packedItemAddInfoCollection, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaPack pack, AsycudaManifestDataObjectReaderHelper helper, Shipment shipment)
			: base(dataObject, packedItemAddInfoCollection, logger, factory, pack, helper)
		{
			hvlvShipment = shipment;
			this.packedItemDataObject = packedItemDataObject;
		}

		readonly Shipment hvlvShipment;
		readonly PackedItem packedItemDataObject;

		protected override void PopulatePackedItemForSpecificRules(AsycudaPackedItem packedItem)
		{
			if (packedItemDataObject != null && hvlvShipment != null)
			{
				var packedItemDataRow = GetColumnIndexer(packedItem);

				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_CustomsUQ, "PP");
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GoodsValue, packedItemDataObject.CIFValue);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GoodsDescription, packedItemDataObject.Description);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_CustomsQty, packedItemDataObject.PackedQuantity);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GrossWeight, packedItemDataObject.GrossWeight);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GrossWeightUQ, packedItemDataObject.GrossWeightUnit);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_NetWeight, packedItemDataObject.NetWeight);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_NetWeightUQ, packedItemDataObject.NetWeightUnit);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_CustomsValue, packedItemDataObject.GoodsValue);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_RX_NKGoodsValueCurrency, hvlvShipment.GoodsValueCurrency.Code);

				var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(hvlvShipment);
				if (commercialInvoiceLine != null)
				{
					SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
				}
			}
		}
	}
}
