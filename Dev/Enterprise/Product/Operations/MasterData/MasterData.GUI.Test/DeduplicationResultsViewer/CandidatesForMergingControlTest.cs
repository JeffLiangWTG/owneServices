using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test.DeduplicationResultsViewer
{
	public class CandidatesForMergingControlTest : TestCaseWithFactory
	{
		public void TestButtonsVisible()
		{
			var candidate = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader("Test"));
			var adminResultDetail = new DeduplicationOrganisationResultDetailForTest(false, false, false);
			adminResultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			adminResultDetail.DuplicationCandidatesForTest = new[] { candidate };

			using (var form = new ZForm())
			{
				var control = new PotentialDuplicatesUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(adminResultDetail, isAdminPanel: true);
				var splitButton = form.GetControl<ZPanel>("SplitButtonBorderPanel");
				AssertEquals(true, splitButton.Visible);
				var mergeButton = form.GetControl<ZButton>("MergeButton");
				AssertEquals(false, mergeButton.Visible);
				var selectButton = form.GetControl<ZButton>("SelectButton");
				AssertEquals(false, selectButton.Visible);
			}
		}
	}
}
