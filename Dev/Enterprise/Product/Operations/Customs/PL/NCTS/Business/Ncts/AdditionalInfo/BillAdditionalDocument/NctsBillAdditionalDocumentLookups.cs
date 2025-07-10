using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsBillAdditionalDocumentLookups : EU.NCTS.Business.NctsBillAdditionalDocumentLookups
{
	public NctsBillAdditionalDocumentLookups(EU.NCTS.Business.NctsBillAdditionalDocument parent) : base(parent)
	{
	}

	protected override ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string subType, string dataGroupingCode)
		=> CusSupportingInfoHelper.GetTypeCodeList(Factory
			, subType
			, true
			, UniversalReferenceConstants.RefCusCodeListLevelTypes.House
			, dataGroupingCode
			, applyLevelAttributeToChild: false);
}
