using NUnit.Framework;
using static CargoWise.EventReference.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVACASStatusListTest : TestCase
	{
		public void TestACASStatusList()
		{
			var list = new HVLVACASStatusList();
			CombineAssertions(() =>
			{
				AssertEquals(ACASActions.Desc.SecurityFilingAssessmentInProgress, list.GetDescriptionFromCode(ACASActions.Code.SecurityFilingAssessmentInProgress));
				AssertEquals(ACASActions.Desc.SecurityFilingAssessmentComplete, list.GetDescriptionFromCode(ACASActions.Code.SecurityFilingAssessmentComplete));
				AssertEquals(ACASActions.Desc.DoNotLoadHold, list.GetDescriptionFromCode(ACASActions.Code.DoNotLoadHold));
				AssertEquals(ACASActions.Desc.SelecteeDataIssueHold, list.GetDescriptionFromCode(ACASActions.Code.SelecteeDataIssueHold));
				AssertEquals(ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHold, list.GetDescriptionFromCode(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold));
				AssertEquals(ACASActions.Desc.DoNotLoadHoldCurrentlyInPlace, list.GetDescriptionFromCode(ACASActions.Code.DoNotLoadHoldCurrentlyInPlace));
				AssertEquals(ACASActions.Desc.SelecteeDataIssueHoldCurrentlyInPlace, list.GetDescriptionFromCode(ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace));
				AssertEquals(ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace, list.GetDescriptionFromCode(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace));
				AssertEquals(ACASActions.Desc.DoNotLoadHoldRemoved, list.GetDescriptionFromCode(ACASActions.Code.DoNotLoadHoldRemoved));
				AssertEquals(ACASActions.Desc.SelecteeDataIssueHoldRemoved, list.GetDescriptionFromCode(ACASActions.Code.SelecteeDataIssueHoldRemoved));
				AssertEquals(ACASActions.Desc.SelecteeScreeningOrVerificationRequiredHoldRemoved, list.GetDescriptionFromCode(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldRemoved));
			});
		}
	}
}
