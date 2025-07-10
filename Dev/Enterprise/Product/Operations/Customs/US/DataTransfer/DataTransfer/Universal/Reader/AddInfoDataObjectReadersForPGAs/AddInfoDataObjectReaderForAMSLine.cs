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
	public class AddInfoDataObjectReaderForAMSLine : AddInfoDataObjectReader<AMSLine>
	{
		public AddInfoDataObjectReaderForAMSLine(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USAMSLineAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var amsLine = (AMSLine)addInfoManager;
				SetAddressPK(setValue, amsLine.US_OA_ApplicantInfo, organizationAddresContainer, Constants.AddressType.Applicant, OrganisationTypes.None);
				SetAddressPK(setValue, amsLine.US_OA_GoodsLocationInfo, organizationAddresContainer, nameof(DocAddressType.Location), OrganisationTypes.None);
			}
		}
	}
}
