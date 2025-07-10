using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(OrgMatchApprovalForm))]
	public class OrgMatchApprovalFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyBusinessObject dummyParent = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);

			// in real life, the entity is saved and the form is shown for editing only
			Factory.Save();
			return new OrgMatchApprovalForm(matchApproval);
		}
	}

	public class OrgMatchApprovalFormTest : TestCaseWithDummyOrgMatchApproval
	{
		[RequiresSTA]
		public void TestOrgToBeMatchedDetailsGroupBoxText()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("The organization type should be shown on the group box", "Find a Match for This Organization - Org Type", form.OrgToBeMatchedDetailsGroupBox.Text);
			}
		}

		[RequiresSTA]
		public void TestCloseButtonIsFormCancelButton()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				AssertEquals("Close button must be the form cancel button so that escape closes the form", form.CancelButton, form.CloseBoundButton);
			}
		}

		[RequiresSTA]
		public void TestFirstSimilarMatchSelectedOnFormLoad()
		{
			CreateNewSimilarOrgAndSave();
			CreateNewSimilarOrgAndSave();

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("There should always be a selected similar org match in the grid after form load", form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.ListManager.List[0], form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.SelectedElements[0]);
				AssertEquals("The first (best) match should be selected on the form after form load", new DataGridCell(0, 0), form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.CurrentCell);
			}
		}

		public void TestSetPreviousNextControlParentAndPosition()
		{
			DummyOrgMatchApproval secondDummyMatchApprovalRequiredSoPreviousNextControlShows = (DummyOrgMatchApproval)Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			Factory.Save();

			using (IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
			using (OrgMatchApprovalForm form = module.ShowEditForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				PropertyInfo previousNextControlProperty = typeof(OrgMatchApprovalForm).GetProperty("PreviousNextControl", BindingFlags.NonPublic | BindingFlags.Instance);
				ZPreviousNextControl previousNextControl = (ZPreviousNextControl)previousNextControlProperty.GetValue(form, null);

				FieldInfo supervisorMatchBoundButtonField = typeof(OrgMatchApprovalForm).GetField("SupervisorMatchBoundButton", BindingFlags.NonPublic | BindingFlags.Instance);
				ZButton supervisorMatchBoundButton = (ZButton)supervisorMatchBoundButtonField.GetValue(form);

				AssertEquals("PreviousNextControl top should be aligned with the supervisor match button", supervisorMatchBoundButton.Top - 1, previousNextControl.Top, previousNextControl.Top * 0.1);
			}
		}

		public void TestNoMoreMatchesAvailableLabelHiddenUnderPreviousNextControl()
		{
			using (IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
			using (OrgMatchApprovalForm form = module.ShowEditForm(DummyMatchApproval))
			{
				PropertyInfo previousNextControlProperty = typeof(OrgMatchApprovalForm).GetProperty("PreviousNextControl", BindingFlags.NonPublic | BindingFlags.Instance);
				ZPreviousNextControl previousNextControl = (ZPreviousNextControl)previousNextControlProperty.GetValue(form, null);
				ZCurrentModules.Instance.SetCurrentModule((ZModule)module);

				form.Show();
				Application.DoEvents();

				ZLabel labelFound = null;
				foreach (Control control in previousNextControl.Parent.Controls)
				{
					if (control is ZLabel && previousNextControl.Bounds.Contains(control.Bounds))
					{
						labelFound = control as ZLabel;
						break;
					}
				}
				AssertNotNull("Should find a label completely behind the ZPreviousNextControl", labelFound);
			}
		}

		public void TestStatusBarChangesWhenBizObjPropertiesChange()
		{
			Factory.RefreshEnabled = true;

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();

				AssertEquals("Status bar should contain the relevant text", OrgMatchApproval.MatchStatus.Unmatched, form.MessageStatusBarPanel.Text);
				DummyMatchApproval.NotifyNoMatchFound();
				AssertEquals("Status bar should update automatically", OrgMatchApproval.MatchStatus.PartiallyMatchedByCurrentUser, form.MessageStatusBarPanel.Text);
				DummyMatchApproval.NotifyNoMatchFound("OP1");
				AssertEquals("Status bar should update automatically", OrgMatchApproval.MatchStatus.NoMatchFound, form.MessageStatusBarPanel.Text);
				DummyMatchApproval.P2_MatchUser1 = "";
				DummyMatchApproval.Match(organisation.PK);
				AssertEquals("Status bar should update automatically", OrgMatchApproval.MatchStatus.MatchMadeWithConflict, form.MessageStatusBarPanel.Text);
				DummyMatchApproval.P2_MatchUser2 = "";
				DummyMatchApproval.Match(organisation.PK, "OP1");
				AssertEquals("Status bar should update automatically", OrgMatchApproval.MatchStatus.MatchApproved, form.MessageStatusBarPanel.Text);

				Factory.Save();
				BusinessObjectFactory otherFactory = new BusinessObjectFactory();
				DummyOrgMatchApproval dummyMatchApprovalInOtherFactory = (DummyOrgMatchApproval)otherFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
				dummyMatchApprovalInOtherFactory.P2_MatchUser1 = "";
				dummyMatchApprovalInOtherFactory.P2_MatchUser2 = "";
				dummyMatchApprovalInOtherFactory.P2_OH_MatchOrg1 = ZGuid.Empty;
				dummyMatchApprovalInOtherFactory.P2_OH_MatchOrg2 = ZGuid.Empty;
				otherFactory.Save();
				AssertEquals("Status bar should update automatically", OrgMatchApproval.MatchStatus.Unmatched, form.MessageStatusBarPanel.Text);
			}
		}

		public void TestChangingDetailsAtTopResearchesSimilarOrgMatches()
		{
			Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled = false;

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("There should be 1 or more similar org matching the current details in the grid initially", true, form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.ListManager.List.Count >= 1);
				form.CompanyNameBoundTextBox.Focus();
				form.CompanyNameBoundTextBox.Text = "splaty";
				form.CloseBoundButton.Focus();
				AssertEquals("There should be no similar orgs matching the current details now that we're searching something silly", 0, form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.ListManager.List.Count);
			}
		}

		public void TestNewOrganisationCreatedOnFormLoadWhenNoSimilarOrgMatchesAndNotSupervisor()
		{
			DummyMatchApproval.SimilarOrgMatchesSortedByRank.RemoveAndDeleteAll();

			DummyMatchApproval.IsCurrentUserSupervisorOverride = false;
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				using (ZOrganisationsForm newOrgForm = (ZOrganisationsForm)form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm)
				{
					AssertEquals("New organisation form should be shown for user edit/save", true, newOrgForm.Visible);
				}
			}

			DummyMatchApproval.IsCurrentUserSupervisorOverride = true;
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Org form should not be automatically shown because we're the supervisor and I could just click the create new org button", null, form.LastCreatedOrgMatchApprovalCreateNewOrgController);
			}
		}

		[RequiresSTA]
		public void TestColour()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				Factory.Save();

				DummyMatchApproval.IsCurrentUserSupervisorOverride = false;
				SimilarOrgMatchForApproval similarOrgForApproval = DummyMatchApproval.SimilarOrgMatchesSortedByRank[0];
				AssertEquals("Should not be Orange as no user has made a match and the current user is not supervisor", Color.Empty, GetGridRowColour(form.SimilarOrgMatchesModuleButtonGrid.InnerGrid, similarOrgForApproval));

				DummyMatchApproval.IsCurrentUserSupervisorOverride = true;
				AssertEquals("Should not be Orange as no user has made a match yet", Color.Empty, GetGridRowColour(form.SimilarOrgMatchesModuleButtonGrid.InnerGrid, similarOrgForApproval));
				DummyMatchApproval.Match(SimilarOrg.PK);
				AssertEquals("Now it is matched by a user, the row should be Orange", Color.Orange, GetGridRowColour(form.SimilarOrgMatchesModuleButtonGrid.InnerGrid, similarOrgForApproval));
			}
		}

		Color GetGridRowColour(ZGrid grid, object objectAtRow)
		{
			EventHandler<ColourDecidingEventArgs> handler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
			ColourDecidingEventArgs e = new ColourDecidingEventArgs(objectAtRow);
			handler(null, e);
			return e.Colour;
		}

		#region TestSupervisorGetsSupervisorScreenLayout

		[RequiresSTA]
		public void TestSupervisorGetsSupervisorScreenLayout()
		{
			AssertSupervisorScreenLayoutVisibility(true);
		}

		public void TestNonSupervisorDoesntGetSupervisorScreenLayout()
		{
			AssertSupervisorScreenLayoutVisibility(false);
		}

		void AssertSupervisorScreenLayoutVisibility(bool forCurrentUserIsSupervisor)
		{
			DummyMatchApproval.IsCurrentUserSupervisorOverride = forCurrentUserIsSupervisor;

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				string notOrNot = forCurrentUserIsSupervisor ? "" : "NOT";
				AssertEquals("For when current user is " + notOrNot + " supervisor", forCurrentUserIsSupervisor, form.SupervisorMatchBoundButton.Enabled);
				AssertEquals("For when current user is " + notOrNot + " supervisor", forCurrentUserIsSupervisor, form.NewOrganisationBoundButton.Enabled);
				AssertEquals("For when current user is " + notOrNot + " supervisor", forCurrentUserIsSupervisor, form.ActiveMatchesGroupBox.Visible);

				if (!forCurrentUserIsSupervisor)
				{
					AssertEquals(
						"The user matches group box should be hidden for non-supervisors so they aren't influenced by other matches",
						form.OrgToBeMatchedDetailsGroupBox.Top, form.ActiveMatchesGroupBox.Top);
				}

				if (form.LastCreatedOrgMatchApprovalCreateNewOrgController != null)
				{
					form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm.Dispose();
				}
			}
		}

		#endregion

		#region Form Caption

		[RequiresSTA]
		public void TestFormCaption()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Organization Match Approval", form.Text);
			}
		}

		#endregion

		#region Supervisor Only Button Clicks

		public void TestOnSupervisorApprove_Click()
		{
			DummyMatchApproval.IsCurrentUserSupervisorOverride = true;

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				form.SupervisorMatchBoundButton.PerformClick();
				AssertEquals("Organisation should be committed to the air cargo record", "match committed", DummyParent.Z0_Description);
				AssertEquals("Changes should be committed to the db on the click", false, DummyMatchApproval.HasChanges);
			}
		}

		public void TestOnSupervisorApprove_Click_WhenNoItemSelected()
		{
			DummyMatchApproval.SimilarOrgMatchesSortedByRank.RemoveAndDeleteAll();

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				form.SupervisorMatchBoundButton.PerformClick();
				AssertEquals("An error should have been shown to the user due to no grid rows selected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		[RequiresSTA]
		public void TestOnSupervisorApprove_Click_WhenMatchAlreadyApproved_ShowsErrorToUser()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				DummyMatchApproval.Match(organisation.PK, "OP1");
				DummyMatchApproval.Match(organisation.PK, "OP2");
				Factory.Save();

				AssertEquals("The user should not be notified of anything before the test", false, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.SupervisorMatchBoundButton.PerformClick();
				AssertEquals("The user should be notified of an error as an unconflicting match has already occurred", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		[RequiresSTA]
		public void TestOnNewOrganisation_Click()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				form.NewOrganisationBoundButton.PerformClick();
				using (ZOrganisationsForm newOrgForm = (ZOrganisationsForm)form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm)
				{
					AssertEquals("New organisation form should be shown for user edit/save", true, newOrgForm.Visible);
					AssertEquals("New organisation form should be editing a new organisation", false, ((OrgHeader)newOrgForm.BusinessEntity).IsInDatabase);
					AssertEquals("The 'new organisation' form should be modal to the match approval form", form, ZFormModaliser.GetParentFormForModalForm((KForm)form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm));
				}
			}
		}

		public void TestOnNewOrganisation_Click_WhenMatchAlreadyApproved()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				DummyMatchApproval.ApproveMatchBySupervisor(organisation);

				AssertEquals("The user should not be notified of anything before the test", false, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.NewOrganisationBoundButton.PerformClick();
				AssertEquals("The user should be notified of an error as the match has already been approved", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Match Operators Button Clicks

		public void TestOnMatch_Click()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
				{
					DummyMatchApproval.MatchAndSaveAtomically(SimilarOrg.PK);
				}
				AssertEquals("Organisation should not be committed until the second user approves for this test", "Default", DummyParent.Z0_Description);

				form.MatchBoundButton.PerformClick();
				AssertEquals("Organisation should be committed to the air cargo record", "match committed", DummyParent.Z0_Description);
				AssertEquals("Changes should be committed to the db on the click", false, DummyMatchApproval.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestOnMatch_Click_ResettingFilterToUnmatchedByCurrentUser()
		{
			using (DummyOrgMatchApprovalModule module = new DummyOrgMatchApprovalModule())
			{
				using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
				{
					ZCurrentModules.Instance.SetCurrentModule(module);
					form.Show();
					Application.DoEvents();

					using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
					{
						DummyMatchApproval.MatchAndSaveAtomically(SimilarOrg.PK);
					}
					AssertEquals("Organisation should not be committed until the second user approves for this test", "Default", DummyParent.Z0_Description);

					module.NextRevertFilterToUnmatchedByCurrentUserAfterWarningUserResult = false;
					form.MatchBoundButton.PerformClick();
					AssertEquals("Organisation should not be committed because the user chose not to change the module filter", "Default", DummyParent.Z0_Description);

					module.NextRevertFilterToUnmatchedByCurrentUserAfterWarningUserResult = true;
					form.MatchBoundButton.PerformClick();
					AssertEquals("Organisation should be committed because the user was happy to change the module filter", "match committed", DummyParent.Z0_Description);
				}
			}
		}

		public void TestOnMatch_Click_WhenRecordAlreadyMatchedShowErrorToUser()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
				{
					DummyMatchApproval.MatchAndSaveAtomically(SimilarOrg.PK);
				}
				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP2"))
				{
					DummyMatchApproval.MatchAndSaveAtomically(SimilarOrg.PK);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchBoundButton.PerformClick();
				AssertEquals("An error message should be shown to the user indicated they can't match", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestOnMatch_Click_MovesToNextRecord()
		{
			CreateNewDummyMatchApprovalAndSave();

			using (IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
			{
				module.ShowEditForm(DummyMatchApproval);
				Application.DoEvents();

				ZCurrentModules.Instance.SetCurrentModule((ZModule)module);
				module.RevertFilterToUnmatchedByCurrentUser();

				ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber = 1;
				Application.DoEvents(); // process the messages that switch to the new form
				AssertEquals("There should be 2 in the queue to start with for the test", 2, ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.PKList.Count);
				AssertEquals("Should start on the first record", 1, ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber);
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				AssertEquals("Should have moved to the next record", 2, ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber);

				CreateNewDummyMatchApprovalAndSave();
				CreateNewDummyMatchApprovalAndSave();

				PressMatchButtonOnActiveOrgMatchApprovalForm();
				Application.DoEvents(); // process the messages that switch to the new form

				PropertyInfo previousNextControlProperty = typeof(OrgMatchApprovalForm).GetProperty("PreviousNextControl", BindingFlags.NonPublic | BindingFlags.Instance);
				ZPreviousNextControl previousNextControl = (ZPreviousNextControl)previousNextControlProperty.GetValue(ActiveOrgMatchApprovalForm, null);
				AssertEquals(
					"ZPreviousNextControl should still be visible so the user can continue matching any next recently added records",
					true, previousNextControl.Visible);
				AssertEquals("The user should be able to click the next button now", true, previousNextControl.NextButtonForTesting.Enabled);

				ActiveOrgMatchApprovalForm.Dispose();
			}
		}

		public void TestOnMatch_Click_RePerformsFilterWhenFormRunsOutOfRecords()
		{
			CreateNewDummyMatchApprovalAndSave();
			CreateNewSimilarOrgAndSave();

			using (IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
			{
				OrgMatchApprovalForm form = module.ShowEditForm(DummyMatchApproval);
				module.RevertFilterToUnmatchedByCurrentUser();
				DummyMatchApproval.Match(SimilarOrg.PK, "XXX");
				AssertEquals("Organisation should not be committed until the second user approves for this test", "Default", DummyParent.Z0_Description);

				ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber = 1;
				Application.DoEvents(); // process the messages that switch to the new form
				AssertEquals("Should start on the first record", 1, ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber);
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				Application.DoEvents(); // process the messages that switch to the new form
				AssertEquals("Should have moved to the next record", 2, ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject.CurrentRecordNumber);

				// expect no exception due to running out of records to move next to
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();
				PressMatchButtonOnActiveOrgMatchApprovalForm();

				ActiveOrgMatchApprovalForm.Dispose();
			}
		}

		public void TestOnMatch_Click_WhenNoItemSelected()
		{
			DummyMatchApproval.SimilarOrgMatchesSortedByRank.RemoveAndDeleteAll();

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				form.MatchBoundButton.PerformClick();
				AssertEquals("An error should have been shown to the user due to no grid rows selected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestOnNoMatchFound_Click()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("No match information should be recorded as yet for the test", true, DummyMatchApproval.P2_OH_MatchOrg1.IsEmpty);
				AssertEquals("No match information should be recorded as yet for the test", true, DummyMatchApproval.P2_MatchUser1.IsEmpty);
				form.NoMatchFoundBoundButton.PerformClick();
				AssertEquals("No organisation has been selected for the match", true, DummyMatchApproval.P2_OH_MatchOrg1.IsEmpty);
				AssertEquals("The user that notified of no match found should be recorded", false, DummyMatchApproval.P2_MatchUser1.IsEmpty);
				AssertEquals("Changes should be committed to the db on the click", false, DummyMatchApproval.HasChanges);

				if (form.LastCreatedOrgMatchApprovalCreateNewOrgController != null)
				{
					form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm.Dispose();
				}
			}
		}

		[RequiresSTA]
		public void TestOnNoMatchFound_Click_WhenRecordAlreadyMatchedShowErrorToUser()
		{
			using (DummyOrgMatchApprovalModule module = new DummyOrgMatchApprovalModule())
			{
				using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
				{
					ZCurrentModules.Instance.SetCurrentModule(module);
					form.Show();
					Application.DoEvents();

					DummyMatchApproval.NotifyNoMatchFound("OP1");
					DummyMatchApproval.NotifyNoMatchFound("OP2");
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.NoMatchFoundBoundButton.PerformClick();
					AssertEquals("An error message should be shown to the user indicated they can't match", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		public void TestOnNoMatchFound_Click_ShowCreateNewOrgFormWhenBothUsersAgreeOnNoMatch()
		{
			DummyMatchApproval.IsCurrentUserSupervisorOverride = false;
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
			{
				DummyMatchApproval.NotifyNoMatchFoundAndSaveAtomically();
			}

			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();

				form.NoMatchFoundBoundButton.PerformClick();
				using (ZOrganisationsForm newOrgForm = (ZOrganisationsForm)form.LastCreatedOrgMatchApprovalCreateNewOrgController.LastShownForm)
				{
					AssertEquals("New organisation form should be shown for user edit/save", true, newOrgForm.Visible);
				}
			}
		}

		#endregion

		#region Skipping over Already Matched Records

		[RequiresSTA]
		public void TestSkippingOverAlreadyMatchedRecordsOnMoveNext()
		{
			Create3SavedDummyMatchApprovals();

			using (IOrgMatchApprovalModule module = (IOrgMatchApprovalModule)ZModuleFactory.Instance.Create(ModuleIDs.OrgMatchApproval))
			{
				ZCurrentModules.Instance.SetCurrentModule((ZModule)module);
				module.PerformSearch();

				using (OrgMatchApprovalForm form = module.ShowEditForm(module.GridCollection[0]))
				{
					form.Show();
					Application.DoEvents();

					DummyOrgMatchApproval alreadyMatchedMatchApproval = (DummyOrgMatchApproval)module.GridCollection[1];
					DummyOrgMatchApproval nextUnmatchedMatchApproval = (DummyOrgMatchApproval)module.GridCollection[2];
					alreadyMatchedMatchApproval.SetIsApprovedByOtherUsers(true);
					alreadyMatchedMatchApproval.Factory.Save();

					PreviousNextControl.FireNextButtonForTesting();
					Application.DoEvents();
					AssertEquals("The record that was already approved by other users should have been skipped", nextUnmatchedMatchApproval.PK, ActiveOrgMatchApprovalForm.BusinessEntity.PK);

					// expect no exceptions when navigating off the end of the list
					SetItemsApprovedByOtherUsers(module.GridCollection);
					while (PreviousNextControl.NextButtonForTesting.Enabled)
					{
						PreviousNextControl.FireNextButtonForTesting();
					}
				}
			}
			ActiveOrgMatchApprovalForm.Dispose();
		}

		ZPreviousNextControl PreviousNextControl
		{
			get
			{
				PropertyInfo previousNextControlProperty = typeof(OrgMatchApprovalForm).GetProperty("PreviousNextControl", BindingFlags.NonPublic | BindingFlags.Instance);
				ZPreviousNextControl result = (ZPreviousNextControl)previousNextControlProperty.GetValue(ActiveOrgMatchApprovalForm, null);
				return result;
			}
		}

		void SetItemsApprovedByOtherUsers(OrgMatchApprovalCollection collection)
		{
			foreach (DummyOrgMatchApproval matchApproval in collection)
			{
				matchApproval.SetIsApprovedByOtherUsers(true);
			}
			collection.Factory.Save();
		}

		void Create3SavedDummyMatchApprovals()
		{
			DummyBusinessObjectAutoLogged dummyParent1 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval1 = (DummyOrgMatchApproval)Loader.LoadOrCreate(dummyParent1.PK, OrgMatchApprovalType.DummyType);
			DummyBusinessObjectAutoLogged dummyParent2 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval2 = (DummyOrgMatchApproval)Loader.LoadOrCreate(dummyParent2.PK, OrgMatchApprovalType.DummyType2);
			DummyBusinessObjectAutoLogged dummyParent3 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval3 = (DummyOrgMatchApproval)Loader.LoadOrCreate(dummyParent3.PK, OrgMatchApprovalType.DummyType);

			Factory.Save();
		}

		#endregion

		[ExpectNoExceptions]
		public void TestNoExceptionWhenMessageStatusBarPanelIsNull()
		{
			using (DummyOrgMatchApprovalForm form = new DummyOrgMatchApprovalForm(DummyMatchApproval))
			{
				form.Show();
				Application.DoEvents();
				form.UpdateStatusBar1_Exposed();
				form.UpdateStatusBar2_Exposed();

				form.NullMessageStatusBarPanel();
				form.UpdateStatusBar1_Exposed();
				form.UpdateStatusBar2_Exposed();
			}
		}

		#region Implementation

		OrgHeader CreateNewSimilarOrgAndSave()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.SetDefaultValuesForTemporaryOrganisation();
			result.OH_FullName = "Organisation FullName";
			result.MainAddress.OA_Address1 = "Street";
			result.MainAddress.OA_City = "City";
			result.MainAddress.OA_PostCode = "PostCode";
			Factory.Save();
			return result;
		}

		DummyOrgMatchApproval CreateNewDummyMatchApprovalAndSave()
		{
			DummyBusinessObjectAutoLogged newDummyParent = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval result = (DummyOrgMatchApproval)Loader.LoadOrCreate(newDummyParent.PK, OrgMatchApprovalType.DummyType);
			Factory.Save();
			return result;
		}

		OrgMatchApprovalForm ActiveOrgMatchApprovalForm
		{
			get
			{
				OrgMatchApprovalForm result = null;
				OrgMatchApproval[] matchApprovals = (OrgMatchApproval[])Factory.Load(typeof(OrgMatchApproval), new ZQuery());
				foreach (OrgMatchApproval matchApproval in matchApprovals)
				{
					result = OpenedFormCache.GetInstance().GetForm(matchApproval.PK.ToGuid(), ModuleIDs.OrgMatchApproval.ToString()) as OrgMatchApprovalForm;
					if (result != null)
					{
						break;
					}
				}
				return result;
			}
		}

		ModuleResultsBusinessObject ActiveOrgMatchApprovalForm_ModuleResultsBusinessObject
		{
			get
			{
				FieldInfo moduleResultsBusinessObjectField = typeof(ZForm).GetField("ModuleResultsBusinessObject", BindingFlags.NonPublic | BindingFlags.Instance);
				ModuleResultsBusinessObject result = (ModuleResultsBusinessObject)moduleResultsBusinessObjectField.GetValue(ActiveOrgMatchApprovalForm);
				return result;
			}
		}

		void PressMatchButtonOnActiveOrgMatchApprovalForm()
		{
			FieldInfo matchBoundButtonField = typeof(OrgMatchApprovalForm).GetField("MatchBoundButton", BindingFlags.NonPublic | BindingFlags.Instance);
			ZButton matchBoundButton = (ZButton)matchBoundButtonField.GetValue(ActiveOrgMatchApprovalForm);
			matchBoundButton.PerformClick();
		}

		#endregion

		#region Setup and Test Classes

		OrgHeader SimilarOrg;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(OrgMatchApproval.Schema.TableName);

			SimilarOrg = CreateNewSimilarOrgAndSave();
			Factory.Save();
		}

		class DummyOrgMatchApprovalModule : ZFilterGridModule, IOrgMatchApprovalModule
		{
			public bool NextRevertFilterToUnmatchedByCurrentUserAfterWarningUserResult;
			public bool RevertFilterToUnmatchedByCurrentUserAfterWarningUser()
			{
				return NextRevertFilterToUnmatchedByCurrentUserAfterWarningUserResult;
			}

			public OrgMatchApprovalForm ShowEditForm(OrgMatchApproval selectedBusinessObject)
			{
				throw new NotSupportedException();
			}

			public bool IsUnmatchForCurrentUserOnly
			{
				get { throw new NotSupportedException(); }
			}

			public void RevertFilterToUnmatchedByCurrentUser()
			{
				throw new NotSupportedException();
			}

			public void PerformSearch()
			{
				throw new NotSupportedException();
			}

			public new OrgMatchApprovalCollection GridCollection
			{
				get { return (OrgMatchApprovalCollection)base.GridCollection; }
			}

			#region ZFilterGridModule

			public override ModuleIdentifier ID
			{
				get { return ModuleIDs.OrgMatchApproval; }
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				throw new NotSupportedException();
			}

			protected override FilterBusinessObject GetNewFilterBusinessObject()
			{
				throw new NotSupportedException();
			}

			protected override IFilterControl GetNewFilterControl()
			{
				throw new NotSupportedException();
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				throw new NotSupportedException();
			}

			protected override LicenceCheckpoint LicenceCheckPointCore
			{
				get { throw new NotSupportedException(); }
			}

			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { throw new NotSupportedException(); }
			}

			#endregion
		}

		class DummyOrgMatchApprovalForm : OrgMatchApprovalForm
		{
			public DummyOrgMatchApprovalForm(OrgMatchApproval businessEntity)
				: base(businessEntity)
			{
			}

			public new void InitialiseForm()
			{
				base.InitialiseForm();
			}

			public void NullMessageStatusBarPanel()
			{
				base.MessageStatusBarPanel = null;
			}

			#region Buttons

			public new ZButton SupervisorMatchBoundButton
			{
				get { return base.SupervisorMatchBoundButton; }
			}

			public new ZButton NewOrganisationBoundButton
			{
				get { return base.NewOrganisationBoundButton; }
			}

			public new ZButton MatchBoundButton
			{
				get { return base.MatchBoundButton; }
			}

			public new ZButton NoMatchFoundBoundButton
			{
				get { return base.NoMatchFoundBoundButton; }
			}

			public new ZButton CloseBoundButton
			{
				get { return base.CloseBoundButton; }
			}

			#endregion

			#region Exposed Fields

			public new ZTextBox CompanyNameBoundTextBox
			{
				get { return base.CompanyNameBoundTextBox; }
			}

			public new ZModuleButtonGrid SimilarOrgMatchesModuleButtonGrid
			{
				get { return base.SimilarOrgMatchesModuleButtonGrid; }
			}

			public new ZGroupBox PossibleMatchingOrgsGroupBox
			{
				get { return base.PossibleMatchingOrgsGroupBox; }
			}

			public new ZGroupBox CandidateOrganisationGroupBox
			{
				get { return base.CandidateOrganisationGroupBox; }
			}

			public new ZGroupBox ActiveMatchesGroupBox
			{
				get { return base.ActiveMatchesGroupBox; }
			}

			public new ZGroupBox OrgToBeMatchedDetailsGroupBox
			{
				get { return base.OrgToBeMatchedDetailsGroupBox; }
			}

			public new ModuleResultsBusinessObject ModuleResultsBusinessObject
			{
				get { return base.ModuleResultsBusinessObject; }
			}

			public new ZStatusBarPanel MessageStatusBarPanel
			{
				get { return base.MessageStatusBarPanel; }
			}

			public new ZPreviousNextControl PreviousNextControl
			{
				get { return base.PreviousNextControl; }
			}

			#endregion

			#region Exposed Methods

			public void UpdateStatusBar1_Exposed()
			{
				UpdateStatusBar(null, new EventArgs());
			}

			public void UpdateStatusBar2_Exposed()
			{
				UpdateStatusBar("", null);
			}

			#endregion

			public ZController LastCreatedOrgMatchApprovalCreateNewOrgController;
			protected override IOrgMatchApprovalCreateNewOrgController NewOrgMatchApprovalCreateNewOrgController()
			{
				LastCreatedOrgMatchApprovalCreateNewOrgController = (ZController)base.NewOrgMatchApprovalCreateNewOrgController();
				return (IOrgMatchApprovalCreateNewOrgController)LastCreatedOrgMatchApprovalCreateNewOrgController;
			}
		}

		#endregion
	}
}
