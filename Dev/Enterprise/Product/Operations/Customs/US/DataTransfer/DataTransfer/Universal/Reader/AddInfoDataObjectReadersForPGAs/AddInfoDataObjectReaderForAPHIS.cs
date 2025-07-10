using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForAPHIS : AddInfoDataObjectReader<APHISHeader>
	{
		public AddInfoDataObjectReaderForAPHIS(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USAPHISHeaderAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var aphisHeader = (APHISHeader)addInfoManager;
				SetAddressPK(setValue, aphisHeader.US_OA_ApplicantAddressInfo, organizationAddresContainer, Constants.AddressType.Applicant, OrganisationTypes.None);
				SetAddressPK(setValue, aphisHeader.US_OA_CropGrowerAddressInfo, organizationAddresContainer, Constants.AddressType.Grower, OrganisationTypes.None);
				SetAddressPK(setValue, aphisHeader.US_OA_ShipperAddressInfo, organizationAddresContainer, Constants.AddressType.Shipper, OrganisationTypes.None);
				SetAddressPK(setValue, aphisHeader.US_OA_PermittedAddressInfo, organizationAddresContainer, Constants.AddressType.Permitted, OrganisationTypes.None);
			}
		}
	}
}
