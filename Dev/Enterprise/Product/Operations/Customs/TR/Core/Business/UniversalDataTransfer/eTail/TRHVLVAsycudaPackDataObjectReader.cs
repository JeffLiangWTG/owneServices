using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer
{
	public class TRHVLVAsycudaPackDataObjectReader : AsycudaPackDataObjectReader
	{
		public TRHVLVAsycudaPackDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled, Shipment hvlvShipmentDataObject)
			: base(dataObject, logger, factory, bill, helper, isUpdateEnabled)
		{
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly Shipment hvlvShipmentDataObject;

		protected override void FillPackedItem(AsycudaPack pack)
		{
			var packedItemCollection = dataObject.PackedItemCollection;
			if (packedItemCollection != null)
			{
				foreach (var packedItemDataObject in packedItemCollection)
				{
					new TRHVLVAsycudaPackedItemObjectReader(dataObject, packedItemDataObject, null, logger, factory, pack, helper, hvlvShipmentDataObject).ReadIntoBusinessObject();
				}
			}
		}
	}
}
