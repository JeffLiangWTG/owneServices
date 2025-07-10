using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UniversalDataObjectReaderHelper = Enterprise.Customs.US.DataTransfer.Universal.UniversalDataObjectReaderHelper;

namespace Enterprise.Customs.US.DataTransfer
{
	public class AddInfoDataObjectReaderForOMCHeader : AddInfoDataObjectReader<OMCHeader>
	{
		public AddInfoDataObjectReaderForOMCHeader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USOMCAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var omcHeader = (OMCHeader)addInfoManager;
				SetAddressPK(setValue, omcHeader.US_OA_ExporterInfo, organizationAddresContainer, Constants.AddressType.Exporter, MasterFiles.Integration.OrganisationTypes.Consignor);
				SetAddressPK(setValue, omcHeader.US_OA_ResponsibleGovernmentOfficialInfo, organizationAddresContainer, Constants.AddressType.ResponsibleGovernmentOfficial, MasterFiles.Integration.OrganisationTypes.None);
				SetAddressPK(setValue, omcHeader.US_OA_AquacultureFacilityInfo, organizationAddresContainer, Constants.AddressType.AquacultureFacility, MasterFiles.Integration.OrganisationTypes.None);
			}
		}
	}
}
