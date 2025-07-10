using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalDataObjectReaderHelper = Enterprise.Customs.US.DataTransfer.Universal.UniversalDataObjectReaderHelper;

namespace Enterprise.Customs.US.DataTransfer
{
	public class AddInfoDataObjectReaderForCPSCHeader : AddInfoDataObjectReader<CPSCHeader>
	{
		public AddInfoDataObjectReaderForCPSCHeader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USCPSCAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var cpscHeader = (CPSCHeader)addInfoManager;
				SetAddressPK(setValue, cpscHeader.US_OA_ManufacturerAddressInfo, organizationAddresContainer, nameof(DocAddressType.Manufacturer), OrganisationTypes.None);
			}
		}
	}
}
