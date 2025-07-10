using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test
{
	public class DeduplicationResultsViewerDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestHotKeyFireFilter()
		{
			var testModel = new DeduplicationOrganisationResultDetailForTest();
			using (var form = new ZForm())
			{
				var control = new DeduplicationResultsViewerDetailsUserControlForTest();
				control.SetupDataContext(testModel, true);
				form.Controls.Add(control);
				form.Show();

				var oldCount = testModel.FilterCallNumber;

				control.SimulateKeyPress(Keys.Control | Keys.Enter);

				AssertEquals("Filter method should have been called once", oldCount + 1, testModel.FilterCallNumber);
			}
		}

		public void TestSetViewBusy()
		{
			var resultDetail = new DeduplicationOrganisationResultDetail();
			using (var form = new ZForm())
			{
				var control = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail, isAdminPanel: false);

				AssertEquals(expected: false, resultDetail.IsLoadingData);

				control.SetViewBusy(true);
				AssertEquals(expected: true, resultDetail.IsLoadingData);

				control.SetViewBusy(false);
				AssertEquals(expected: false, resultDetail.IsLoadingData);
			}
		}

		public void TestChildControlDisposed_AfterReloadResultDetail()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var masterOrg = new DeduplicationOrgHeader(testOrg);
			var deduplicationOrg1 = new DeduplicationOrgHeader(testOrg1);
			var deduplicationOrg2 = new DeduplicationOrgHeader(testOrg2);
			var presentModel = new DeduplicationPresenterModel
			{
				Confidence = ConfidenceRating.Undefined,
				MainScore = 0.99
			};

			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "OrganisationNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterValue = "Dummy Master Org",
					ChildTargetValue = "Dummy Candidate Org",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.1
				}
			};

			var candidate1 = new DuplicationOrganisationCandidate(presentModel, deduplicationOrg1);
			candidate1.TargetPK = candidate1.PK.ToGuid();
			candidate1.DeduplicationPresenterModels = models;
			var candidate2 = new DuplicationOrganisationCandidate(presentModel, deduplicationOrg2);
			candidate2.TargetPK = candidate2.PK.ToGuid();
			candidate2.DeduplicationPresenterModels = models;

			Factory.Save();

			var resultDetail = new DeduplicationOrganisationResultDetailForTest(masterOrg);
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			resultDetail.SelectedCandidatePK = candidate2.PK;

			using var form = new ZForm();
			using var control = new DeduplicationResultsViewerDetailsUserControl();
			form.Controls.Add(control);
			form.Show();
			control.SetupDataContext(resultDetail, false);

			form.Close();
			AssertEquals(true, form.IsDisposed);
			var masterInformationControls = control.FindAll<MasterCandidateInformationControl>();
			AssertAllControlsOfMasterInformationControlsDisposed(masterInformationControls);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();
			AssertAllControlsOfMasterInformationControlsDisposed(masterInformationControls);
		}

		void AssertAllControlsOfMasterInformationControlsDisposed(IEnumerable<MasterCandidateInformationControl> masterInformationControls)
		{
			var undisposedControlNum = 0;
			foreach (var informationControl in masterInformationControls)
			{
				var contentPanel = informationControl.FindAll<KTableLayoutPanel>(c => c.Name == "ContentPanel").FirstOrDefault();
				foreach (Control childControl in contentPanel.Controls)
				{
					if (!childControl.IsDisposed)
					{
						undisposedControlNum++;
					}
				}
			}
			AssertEquals(0, undisposedControlNum);
		}

		public void TestEmptyOrganisationResultDetail()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var masterOrg = new DeduplicationOrgHeader(testOrg);
			var deduplicationOrg1 = new DeduplicationOrgHeader(testOrg1);
			var presentModel = new DeduplicationPresenterModel
			{
				Confidence = ConfidenceRating.Undefined,
				MainScore = 0.99
			};

			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "OrganisationNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterValue = "Dummy Master Org",
					ChildTargetValue = "Dummy Candidate Org",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.1
				}
			};

			var candidate1 = new DuplicationOrganisationCandidate(presentModel, deduplicationOrg1);
			candidate1.TargetPK = candidate1.PK.ToGuid();
			candidate1.DeduplicationPresenterModels = models;

			Factory.Save();

			var emptyResultDetail = new DeduplicationOrganisationResultDetail();
			var resultDetail = new DeduplicationOrganisationResultDetailForTest(masterOrg);
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1 };
			resultDetail.SelectedCandidatePK = candidate1.PK;

			using (var form = new ZForm())
			{
				var control = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail, isAdminPanel: true);

				var informationControls = control.FindAll<MasterCandidateInformationControl>();
				AssertEquals(2, informationControls.Count());

				var contentPanels = new List<KTableLayoutPanel>();
				informationControls.ForEach(control => contentPanels.Add(control.FindSingleOrDefault<KTableLayoutPanel>(c => c.Name == "ContentPanel")));
				contentPanels.ForEach(panel => AssertGreaterThan(panel.Controls.Count, 1));

				control.SetupDataContext(emptyResultDetail, isAdminPanel: true);
				contentPanels.ForEach(panel => AssertEquals(1, panel.Controls.Count));
			}
		}

		public void TestEmptyPersonResultDetail()
		{
			var presentModel = new DeduplicationPresenterModel
			{
				Confidence = ConfidenceRating.Undefined,
				MainScore = 0.99
			};
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "PersonNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterValue = "Dummy Master Person",
					ChildTargetValue = "Dummy Candidate Person",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.1
				}
			};
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_HomeAddress1 = "Some text";
			person.PER_City = "Some City";
			person.PER_FullName = "Test Person";
			person.PER_MobilePhone = "0492052686";

			var candidate1PK = Guid.NewGuid();
			var candidate1 = new DuplicationPersonCandidate(presentModel, new DeduplicationGlbPerson(person));
			candidate1.TargetPK = candidate1PK;
			candidate1.DeduplicationPresenterModels = models;

			var emptyResultDetail = new DeduplicationPersonResultDetail();
			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1 };
			resultDetail.SelectedCandidatePK = candidate1PK;

			using (var form = new ZForm())
			{
				var control = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail, isAdminPanel: true);

				var informationControls = control.FindAll<MasterCandidateInformationControl>();
				AssertEquals(2, informationControls.Count());

				var contentPanels = new List<KTableLayoutPanel>();
				informationControls.ForEach(control => contentPanels.Add(control.FindSingleOrDefault<KTableLayoutPanel>(c => c.Name == "ContentPanel")));
				contentPanels.ForEach(panel => AssertGreaterThan(panel.Controls.Count, 1));

				control.SetupDataContext(emptyResultDetail, isAdminPanel: true);
				contentPanels.ForEach(panel => AssertEquals(1, panel.Controls.Count));
			}
		}

		class DeduplicationResultsViewerDetailsUserControlForTest : DeduplicationResultsViewerDetailsUserControl
		{
			public void SimulateKeyPress(Keys keys)
			{
				var message = new Message();
				ProcessCmdKey(ref message, keys);
			}
		}
	}
}
