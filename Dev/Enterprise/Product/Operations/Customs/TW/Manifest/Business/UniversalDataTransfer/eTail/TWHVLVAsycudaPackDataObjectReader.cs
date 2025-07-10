using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer
{
	sealed class TWHVLVAsycudaPackDataObjectReader : AsycudaPackDataObjectReader
	{
		public TWHVLVAsycudaPackDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled, Shipment hvlvShipmentDataObject)
			: base(dataObject, logger, factory, bill, helper, isUpdateEnabled)
		{
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly Shipment hvlvShipmentDataObject;

		protected override void FillPackedItem(ASYCUDA.Business.AsycudaPack pack)
		{
			new TWHVLVAsycudaPackedItemObjectReader(dataObject, null, logger, factory, pack, helper, hvlvShipmentDataObject).ReadIntoBusinessObject();
		}

		protected override UNDGDataObjectReader GetUNDGDataObjectReader(UNDG undgData, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new TWUNDGDataObjectReader(undgData, logger, factory);
		}
	}
}
