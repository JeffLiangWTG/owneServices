using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsAdditionalInfoPhase5Lookups : EU.NCTS.Business.NctsAdditionalInfoPhase5Lookups
{
	public NctsAdditionalInfoPhase5Lookups(EU.NCTS.Business.NctsAdditionalInfo parent) : base(parent)
	{
	}

	protected override ZZRefCusCodeListCombinedCollection GetCodeList(bool includeParent, string dataGroupingCode, string codeListType, string level)
		=> CusSupportingInfoHelper.GetTypeCodeList(Factory
			, codeListType
			, true
			, level
			, dataGroupingCode
			, applyLevelAttributeToChild: false);
}
