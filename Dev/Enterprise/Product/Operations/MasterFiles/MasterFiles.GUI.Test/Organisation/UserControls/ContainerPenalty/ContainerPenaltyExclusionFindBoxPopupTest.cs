using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ContainerPenaltyExclusionFindBoxPopup))]
	sealed class ContractAllocationFindBoxPopupTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var exclusion = Factory.New<ContainerPenaltyDayExclusion>();
			return new ContainerPenaltyExclusionFindBoxPopup_ForTest(exclusion, Point.Empty);
		}

		public void TestContainerPenaltyExclusionPopupIsShown()
		{
			using (var parentForm = new Form())
			using (var findBoxPopup = GetFormToBashCore() as ContainerPenaltyExclusionFindBoxPopup_ForTest)
			{
				findBoxPopup.ShowModal(new DummyFindBox(), parentForm);
				var penaltyExclusionPopup = findBoxPopup.GetCurrentPopup_ForTest();

				AssertNotNull("ContainerPenaltyExclusionPopup should be created", penaltyExclusionPopup);
				Assert("ContainerPenaltyExclusionPopup should be visible", penaltyExclusionPopup.Visible);
			}
		}
	}

	public class ContainerPenaltyExclusionFindBoxPopup_ForTest(ContainerPenaltyDayExclusion exclusion, Point initialPopupLocation) : ContainerPenaltyExclusionFindBoxPopup(exclusion, initialPopupLocation)
	{
		public Form GetCurrentPopup_ForTest() => popup;
	}
}
