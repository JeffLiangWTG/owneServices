using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForVehicle : AddInfoDataObjectReader<Business.Vehicle>
	{
		public AddInfoDataObjectReaderForVehicle(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USVehicleAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var header = (Business.Vehicle)addInfoManager;
				SetAddressPK(setValue, header.US_OA_OwnerInfo, organizationAddresContainer, Constants.AddressType.Owner, OrganisationTypes.None);
				SetAddressPK(setValue, header.US_OA_StorageLocationInfo, organizationAddresContainer, nameof(DocAddressType.Location), OrganisationTypes.None);
			}
		}
	}
}
