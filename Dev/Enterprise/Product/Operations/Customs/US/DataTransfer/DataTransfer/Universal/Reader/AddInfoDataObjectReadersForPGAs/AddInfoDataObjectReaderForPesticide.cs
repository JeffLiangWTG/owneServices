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
	public class AddInfoDataObjectReaderForPesticide : AddInfoDataObjectReader<Pesticide>
	{
		public AddInfoDataObjectReaderForPesticide(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USPSTAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var header = (Pesticide)addInfoManager;
				SetAddressPK(setValue, header.US_OA_ExaminationLocationInfo, organizationAddresContainer, nameof(DocAddressType.Location), OrganisationTypes.None);
			}
		}
	}
}
