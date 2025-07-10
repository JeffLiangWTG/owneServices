using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TR.ETrade.Business
{
	sealed class TRETradeHVLVAsycudaPackDataObjectReader : AsycudaPackDataObjectReader
	{
		public TRETradeHVLVAsycudaPackDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled, Shipment hvlvShipmentDataObject)
			: base(dataObject, logger, factory, bill, helper, isUpdateEnabled)
		{
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly Shipment hvlvShipmentDataObject;

		protected override void FillPackedItem(ASYCUDA.Business.AsycudaPack packingLine)
		{
			var packedItemCollection = dataObject.PackedItemCollection;
			if (packedItemCollection != null)
			{
				foreach (var packedItemDataObject in packedItemCollection)
				{
					new TRETradeHVLVAsycudaPackedItemDataObjectReader(dataObject, packedItemDataObject, logger, factory, packingLine, helper, hvlvShipmentDataObject).ReadIntoBusinessObject();
				}
			}
		}
	}
}
