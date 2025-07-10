using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class FreeDayExclusionFindBoxTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGetNewPopupForm()
		{
			var exclusion = Factory.New<ContainerPenaltyDayExclusion>();
			var detention = Factory.New<OrgContainerDetention>();
			detention.PD_CEX_FreeDayExclusion = exclusion.PK;

			using (var findBox = new FreeDayExclusionFindBox_ForTest())
			{
				findBox.CurrentItem = detention;

				var popupForm = findBox.GetNewPopupForm_ForTest();

				AssertEquals("GetNewPopupForm should return ContainerPenaltyExclusionFindBoxPopup", popupForm.GetType(), typeof(ContainerPenaltyExclusionFindBoxPopup));
				AssertEquals("Exclusion is correctly inherited as DataSource of the popup", (popupForm as ContainerPenaltyExclusionFindBoxPopup).DataSource, exclusion);
			}
		}
	}

	public class FreeDayExclusionFindBox_ForTest : FreeDayExclusionFindBox
	{
		public IFindBoxPopup GetNewPopupForm_ForTest() => GetNewPopupForm();
	}
}
