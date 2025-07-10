using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVACASStatusList : CodeDescriptionPairList
	{
		public HVLVACASStatusList()
		{
			AddPair(ACASActions.Code.SecurityFilingAssessmentInProgress, ResString.GetMultilingualString("1fc48fce-b90d-4105-81f9-33622a491163", ACASActions.Desc.SecurityFilingAssessmentInProgress));
			AddPair(ACASActions.Code.SecurityFilingAssessmentComplete, ResString.GetMultilingualString("7d010d73-51a2-47fd-8864-35e89fc6e98e", ACASActions.Desc.SecurityFilingAssessmentComplete));
			AddPair(ACASActions.Code.DoNotLoadHold, ResString.GetMultilingualString("b854be39-8e58-49f8-b045-490289d8a11f", ACASActions.Desc.DoNotLoadHold));
			AddPair(ACASActions.Code.SelecteeDataIssueHold, ResString.GetMultilingualString("b28a31e9-d7c8-42ee-bfb5-6660585305d8", ACASActions.Desc.SelecteeDataIssueHold));
			AddPair(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold, ResString.GetMultilingualString("0f47bbdd-d074-4122-8cc4-6d7407666659", ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHold));
			AddPair(ACASActions.Code.DoNotLoadHoldCurrentlyInPlace, ResString.GetMultilingualString("d2fc6687-477b-47e7-af09-7d1460bac1a0", ACASActions.Desc.DoNotLoadHoldCurrentlyInPlace));
			AddPair(ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace, ResString.GetMultilingualString("f5b41bd3-3868-46aa-930a-bbeffab701e3", ACASActions.Desc.SelecteeDataIssueHoldCurrentlyInPlace));
			AddPair(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace, ResString.GetMultilingualString("00e27e67-d5e3-4a22-a004-8c1dbaaaf22e", ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace));
			AddPair(ACASActions.Code.DoNotLoadHoldRemoved, ResString.GetMultilingualString("a396cd10-f0ac-457b-ad4d-6b012ba13f13", ACASActions.Desc.DoNotLoadHoldRemoved));
			AddPair(ACASActions.Code.SelecteeDataIssueHoldRemoved, ResString.GetMultilingualString("1bf31327-21cb-4a00-8805-044dfafc0e66", ACASActions.Desc.SelecteeDataIssueHoldRemoved));
			AddPair(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldRemoved, ResString.GetMultilingualString("235cfb81-4206-4175-82b5-08094d9cfae1", ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHoldRemoved));
		}
	}
}
