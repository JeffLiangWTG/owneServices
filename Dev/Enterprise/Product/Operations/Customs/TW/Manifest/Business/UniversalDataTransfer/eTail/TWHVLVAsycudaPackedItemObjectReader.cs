using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer
{
	sealed class TWHVLVAsycudaPackedItemObjectReader : AsycudaPackedItemObjectReader
	{
		public TWHVLVAsycudaPackedItemObjectReader(PackingLine dataObject, List<AddInfo> packedItemAddInfoCollection, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaPack pack, AsycudaManifestDataObjectReaderHelper helper, Shipment hvlvShipment)
			: base(dataObject, packedItemAddInfoCollection, logger, factory, pack, helper)
		{
			this.hvlvShipment = hvlvShipment;
		}

		readonly Shipment hvlvShipment;

		protected override void PopulatePackedItemForSpecificRules(AsycudaPackedItem packedItem)
		{
			var packedItemDataObject = dataObject.PackedItemCollection?.FirstOrDefault();
			if (packedItemDataObject != null && hvlvShipment != null)
			{
				var packedItemDataRow = GetColumnIndexer(packedItem);
				var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(hvlvShipment);

				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GoodsDescription, string.IsNullOrEmpty(packedItemDataObject.Description) ? hvlvShipment.GoodsDescription : packedItemDataObject.Description);
			}
		}
	}
}
