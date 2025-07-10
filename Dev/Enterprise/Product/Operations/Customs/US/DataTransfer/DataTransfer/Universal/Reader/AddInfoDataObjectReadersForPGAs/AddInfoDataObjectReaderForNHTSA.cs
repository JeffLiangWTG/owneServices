using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForNHTSA : AddInfoDataObjectReader<NHTSAHeader>
	{
		public AddInfoDataObjectReaderForNHTSA(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USNHTSAAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var header = (NHTSAHeader)addInfoManager;
				SetAddressPK(setValue, header.US_OA_NHTOwnerInfo, organizationAddresContainer, Constants.AddressType.Owner, OrganisationTypes.None);
				SetAddressPK(setValue, header.US_OA_NHTRetailerInfo, organizationAddresContainer, nameof(DocAddressType.DistributionCentreAddress), OrganisationTypes.None);
				SetAddressPK(setValue, header.US_NHTFabricatingMFRAddressInfo, organizationAddresContainer, nameof(DocAddressType.Manufacturer), OrganisationTypes.None);
				SetAddressPK(setValue, header.US_NHTOriginalMFRAddressInfo, organizationAddresContainer, Constants.AddressType.OriginalVehicleManufacturerAddress, OrganisationTypes.None);
			}
		}

		protected override void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, ValueSetter> delaySetters)
		{
			var header = (NHTSAHeader)addInfoManager;
			if (header != null)
			{
				header.IsImportingData = true;
				base.ReadCore(addInfoManager, row, addInfoCollection, organizationAddresContainer, infoMappings, delaySetters);
			}
		}
	}
}
