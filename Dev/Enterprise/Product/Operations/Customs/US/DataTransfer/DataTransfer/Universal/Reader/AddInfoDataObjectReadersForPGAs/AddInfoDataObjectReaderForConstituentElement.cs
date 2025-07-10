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
	class AddInfoDataObjectReaderForConstituentElement : AddInfoDataObjectReader<ConstituentElement>
	{
		public AddInfoDataObjectReaderForConstituentElement(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USConstituentElementAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var constituentElement = (ConstituentElement)addInfoManager;
				SetAddressPK(setValue, constituentElement.US_OA_ProducerAddressInfo, organizationAddresContainer, Constants.AddressType.Producer, OrganisationTypes.None);
			}
		}
	}
}
