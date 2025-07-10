using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test.DeduplicationResultsViewer
{
	public class PotentialDuplicatesUserControlTest : TestCaseWithFactory
	{
		public void TestCheckBoxVisible()
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
				var checkBoxLayoutPanel = form.GetControl<KTableLayoutPanel>("CheckBoxTableLayoutPanel");
				AssertEquals(true, checkBoxLayoutPanel.Visible);
			}
		}
	}
}
