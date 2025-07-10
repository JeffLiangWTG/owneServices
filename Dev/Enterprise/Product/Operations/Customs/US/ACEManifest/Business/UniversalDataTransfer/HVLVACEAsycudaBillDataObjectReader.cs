using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer
{
	public class HVLVACEAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public HVLVACEAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, ACEAsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		protected override ASYCUDA.Business.AsycudaBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return header.IsInDatabase ? base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules() : null;
		}
	}
}
