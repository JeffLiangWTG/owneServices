using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalDataObjectReaderHelper = Enterprise.Customs.US.DataTransfer.Universal.UniversalDataObjectReaderHelper;

namespace Enterprise.Customs.US.DataTransfer
{
	public class AddInfoDataObjectReaderForNMFS : AddInfoDataObjectReader<NMFSHarvestingDetail>
	{
		public AddInfoDataObjectReaderForNMFS(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USNMFSHarvestingDetailAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			var orgAddresses = organizationAddresContainer.OrganizationAddressCollection;
			if (orgAddresses != null)
			{
				var nmfsHarvestingDetail = (NMFSHarvestingDetail)addInfoManager;
				SetAddressPK(setValue, nmfsHarvestingDetail.US_OA_ContactPartyInfo, organizationAddresContainer, Universal.Constants.AddressType.ContactParty, MasterFiles.Integration.OrganisationTypes.None);
			}
		}
	}
}
