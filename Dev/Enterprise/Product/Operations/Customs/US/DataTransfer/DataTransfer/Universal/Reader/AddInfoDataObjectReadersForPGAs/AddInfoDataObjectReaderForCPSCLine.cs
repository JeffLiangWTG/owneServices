using System;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UniversalDataObjectReaderHelper = Enterprise.Customs.US.DataTransfer.Universal.UniversalDataObjectReaderHelper;

namespace Enterprise.Customs.US.DataTransfer
{
	public class AddInfoDataObjectReaderForCPSCLine : AddInfoDataObjectReader<CPSCRule>
	{
		public AddInfoDataObjectReaderForCPSCLine(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, USCPSCRuleAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var cpscRule = (CPSCRule)addInfoManager;
				SetAddressPK(setValue, cpscRule.US_OA_SafetyTestLocationAddressInfo, organizationAddresContainer, Constants.AddressType.Laboratory, OrganisationTypes.None);
			}
		}
	}
}
