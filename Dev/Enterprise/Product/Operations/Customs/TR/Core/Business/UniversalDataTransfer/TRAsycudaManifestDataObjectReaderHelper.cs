using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer
{
	public class TRAsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper
	{
		public TRAsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory) : base(countryCode, factory)
		{
		}

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			if (dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null)
			{
				return new TRHVLVAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
			}
			return base.GetBillDataObjectReaderCore(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}
	}
}
