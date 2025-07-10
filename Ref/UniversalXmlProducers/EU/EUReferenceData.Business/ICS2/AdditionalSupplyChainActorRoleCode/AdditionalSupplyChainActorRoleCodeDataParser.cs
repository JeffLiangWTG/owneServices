using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCode.Business
{
	public class AdditionalSupplyChainActorRoleCodeDataParser : CommonDataParser, IAdditionalTranslationSupporter
	{
		public AdditionalSupplyChainActorRoleCodeDataParser()
			: base(Constants.AdditionalSupplyChainActorRoleCode.RDEntityAttributeValue, Constants.AdditionalSupplyChainActorRoleCode.RoleAttributeValue)
		{
		}

		public string DataParserKey => "AdditionalSupplyChainActorRoleCodeDataParser";
	}
}
