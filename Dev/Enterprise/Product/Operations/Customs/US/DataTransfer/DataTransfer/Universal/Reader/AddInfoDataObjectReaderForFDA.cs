using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForFDA : AddInfoDataObjectReader<FDA>, IOrganisationDataObjectReaderSupporter
	{
		public AddInfoDataObjectReaderForFDA(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USFDAAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var fda = (FDA)addInfoManager;
				SetAddressPK(setValue, fda.US_OA_FDAFEIInfo, organizationAddresContainer, Constants.AddressType.FDAEstablishmentIdentifier, OrganisationTypes.None);
				SetAddressPK(setValue, fda.US_FDAManufacturerAddressInfo, organizationAddresContainer, nameof(DocAddressType.Manufacturer), OrganisationTypes.Consignor);
				SetAddressPK(setValue, fda.US_FDAShipperAddressInfo, organizationAddresContainer, Constants.AddressType.FDAShipper, OrganisationTypes.Consignor);
			}
		}

		#region IOrganisationDataObjectReaderSupporter Members

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, helper.Factory);
		}

		#endregion
	}
}
