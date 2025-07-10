using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer
{
	public class TRHVLVAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public TRHVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override AsycudaPackDataObjectReader GetAsycudaPackDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new TRHVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, factory, bill, helper, isUpdateEnabled, dataObject);
		}
	}
}
