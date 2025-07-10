using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(PersonMergeSummaryForm))]
	public class PersonMergeSummaryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			return new PersonMergeSummaryForm(retainedPerson, dissolvePerson);
		}

		public void TestPersonMergeBusinessObjectPropertiesAreCorrectAfterSuccessfulMergeSelected()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "RP";
			var dissolvePersonList = new List<GlbPerson>();
			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "DP1";
			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "DP2";

			PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);
			PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			var dissolvedPersonStaff1 = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson1);
			var dissolvedPersonStaff2 = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson2);

			Factory.Save();

			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			dissolvePersonList.Add(dissolvedPerson1);
			dissolvePersonList.Add(dissolvedPerson2);

			Factory.Save();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePersonList))
			{
				pmsForm.SetExceptions = false;
				pmsForm.Show();

				var retainedGrid = pmsForm.RetainedControlsForTest.RetainedBoundGrid;
				var dissolvedGrid = pmsForm.CandidatesControlsForTest.CandidatesBoundGrid;
				var retainedFullName = GetValueAsString(0, "FullName", retainedGrid);
				var retainedActiveAssociations = GetValueAsString(0, "ActiveAssociations", retainedGrid);
				var dissolved1FullName = GetValueAsString(0, "FullName", dissolvedGrid);
				var dissolved1ActiveAssociations = GetValueAsString(0, "ActiveAssociations", dissolvedGrid);
				var dissolved1MergeStatus = GetValueAsString(0, "MergeStatus", dissolvedGrid);
				var dissolved2FullName = GetValueAsString(1, "FullName", dissolvedGrid);
				var dissolved2ActiveAssociations = GetValueAsString(1, "ActiveAssociations", dissolvedGrid);
				var dissolved2MergeStatus = GetValueAsString(1, "MergeStatus", dissolvedGrid);

				CombineAssertions("Before merge all", () =>
				{
					AssertEquals("Retained person's full name is RP", "RP", retainedFullName);
					AssertEquals("Retained person's active association is Staff (1)", "Staff (1)", retainedActiveAssociations);

					AssertEquals("First dissolved person's full name is DP1", "DP1", dissolved1FullName);
					AssertEquals("First dissolved person's active associations are Contact (3), Staff (1), Applicant (1)", "Contact (3), Staff (1), Applicant (1)", dissolved1ActiveAssociations);
					AssertEquals("First dissolved person's merge status is Queued", ParticipantStatus.Queued, dissolved1MergeStatus);

					AssertEquals("Second dissolved person's full name is DP2", "DP2", dissolved2FullName);
					AssertEquals("Second dissolved person's active association is Staff (1)", "Staff (1)", dissolved2ActiveAssociations);
					AssertEquals("Second dissolved person's merge status is Queued", ParticipantStatus.Queued, dissolved2MergeStatus);
				});

				(pmsForm.Controls.Find("MergeButton", true)[0] as ZButton).PerformClick();

				retainedFullName = GetValueAsString(0, "FullName", retainedGrid);
				retainedActiveAssociations = GetValueAsString(0, "ActiveAssociations", retainedGrid);
				dissolved1FullName = GetValueAsString(0, "FullName", dissolvedGrid);
				dissolved1ActiveAssociations = GetValueAsString(0, "ActiveAssociations", dissolvedGrid);
				dissolved1MergeStatus = GetValueAsString(0, "MergeStatus", dissolvedGrid);
				dissolved2FullName = GetValueAsString(1, "FullName", dissolvedGrid);
				dissolved2ActiveAssociations = GetValueAsString(1, "ActiveAssociations", dissolvedGrid);
				dissolved2MergeStatus = GetValueAsString(1, "MergeStatus", dissolvedGrid);

				CombineAssertions("After merge all succeeds", () =>
				{
					AssertEquals("Retained person's full name is RP", "RP", retainedFullName);
					AssertEquals("Retained person's active associations are Contact (3), Staff (3), Applicant (1)", "Contact (3), Staff (3), Applicant (1)", retainedActiveAssociations);

					AssertEquals("First dissolved person's full name is DP1", "DP1", dissolved1FullName);
					AssertEquals("First dissolved person's active associations remain unchanged", "Contact (3), Staff (1), Applicant (1)", dissolved1ActiveAssociations);
					AssertEquals("First dissolved person's merge status is Completed", ParticipantStatus.Completed, dissolved1MergeStatus);

					AssertEquals("Second dissolved person's full name is DP2", "DP2", dissolved2FullName);
					AssertEquals("Second dissolved person's active associations remain unchanged", "Staff (1)", dissolved2ActiveAssociations);
					AssertEquals("Second dissolved person's merge status is Completed", ParticipantStatus.Completed, dissolved2MergeStatus);
				});
			}
		}

		public void TestPersonMergeBusinessObjectPropertiesAreCorrectAfterFailedMergeSelected()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "RP";
			var dissolvePersonList = new List<GlbPerson>();
			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "DP1";
			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "DP2";

			PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);
			PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			var dissolvedPersonStaff1 = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson1);
			var dissolvedPersonStaff2 = PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson2);

			Factory.Save();

			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			dissolvePersonList.Add(dissolvedPerson1);
			dissolvePersonList.Add(dissolvedPerson2);

			Factory.Save();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePersonList))
			{
				pmsForm.SetExceptions = true;
				pmsForm.Show();

				var retainedGrid = pmsForm.RetainedControlsForTest.RetainedBoundGrid;
				var dissolvedGrid = pmsForm.CandidatesControlsForTest.CandidatesBoundGrid;
				var retainedFullName = GetValueAsString(0, "FullName", retainedGrid);
				var retainedActiveAssociations = GetValueAsString(0, "ActiveAssociations", retainedGrid);
				var dissolved1FullName = GetValueAsString(0, "FullName", dissolvedGrid);
				var dissolved1ActiveAssociations = GetValueAsString(0, "ActiveAssociations", dissolvedGrid);
				var dissolved1MergeStatus = GetValueAsString(0, "MergeStatus", dissolvedGrid);
				var dissolved2FullName = GetValueAsString(1, "FullName", dissolvedGrid);
				var dissolved2ActiveAssociations = GetValueAsString(1, "ActiveAssociations", dissolvedGrid);
				var dissolved2MergeStatus = GetValueAsString(1, "MergeStatus", dissolvedGrid);

				CombineAssertions("Before merge all", () =>
				{
					AssertEquals("Retained person's full name is RP", "RP", retainedFullName);
					AssertEquals("Retained person's active association is Staff (1)", "Staff (1)", retainedActiveAssociations);

					AssertEquals("First dissolved person's full name is DP1", "DP1", dissolved1FullName);
					AssertEquals("First dissolved person's active associations are Contact (3), Staff (1), Applicant (1)", "Contact (3), Staff (1), Applicant (1)", dissolved1ActiveAssociations);
					AssertEquals("First dissolved person's merge status is Queued", ParticipantStatus.Queued, dissolved1MergeStatus);

					AssertEquals("Second dissolved person's full name is DP2", "DP2", dissolved2FullName);
					AssertEquals("Second dissolved person's active association is Staff (1)", "Staff (1)", dissolved2ActiveAssociations);
					AssertEquals("Second dissolved person's merge status is Queued", ParticipantStatus.Queued, dissolved2MergeStatus);
				});

				(pmsForm.Controls.Find("MergeButton", true)[0] as ZButton).PerformClick();

				retainedFullName = GetValueAsString(0, "FullName", retainedGrid);
				retainedActiveAssociations = GetValueAsString(0, "ActiveAssociations", retainedGrid);
				dissolved1FullName = GetValueAsString(0, "FullName", dissolvedGrid);
				dissolved1ActiveAssociations = GetValueAsString(0, "ActiveAssociations", dissolvedGrid);
				dissolved1MergeStatus = GetValueAsString(0, "MergeStatus", dissolvedGrid);
				dissolved2FullName = GetValueAsString(1, "FullName", dissolvedGrid);
				dissolved2ActiveAssociations = GetValueAsString(1, "ActiveAssociations", dissolvedGrid);
				dissolved2MergeStatus = GetValueAsString(1, "MergeStatus", dissolvedGrid);

				CombineAssertions("After merge all fails", () =>
				{
					AssertEquals("Retained person's full name is RP", "RP", retainedFullName);
					AssertEquals("Retained person's active associations remain unchanged", "Staff (1)", retainedActiveAssociations);

					AssertEquals("First dissolved person's full name is DP1", "DP1", dissolved1FullName);
					AssertEquals("First dissolved person's active associations remain unchanged", "Contact (3), Staff (1), Applicant (1)", dissolved1ActiveAssociations);
					AssertEquals("First dissolved person's merge status is FailedWithCriticalError", ParticipantStatus.FailedWithCriticalError, dissolved1MergeStatus);

					AssertEquals("Second dissolved person's full name is DP2", "DP2", dissolved2FullName);
					AssertEquals("Second dissolved person's active associations remain unchanged", "Staff (1)", dissolved2ActiveAssociations);
					AssertEquals("Second dissolved person's merge status is FailedWithCriticalError", ParticipantStatus.FailedWithCriticalError, dissolved2MergeStatus);
				});
			}
		}

		public void TestMergingWarningMessageDisplayedBeforeMerging()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPerson))
			{
				pmsForm.Show();
				AssertEquals("Before merging, MergingWarningMessage should display.", true, pmsForm.MergeWarningLabel.Visible);
				AssertEquals("MergingWarningMessage should be default.", pmsForm.MergingWarningMessageForTest, pmsForm.MergeWarningLabel.Text);
			}
		}

		public void TestMergingWarningMessageDisappearedIfMergingCompleted()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = new List<GlbPerson>();
			dissolvedPerson.Add(Factory.NewWithValidTestData<GlbPerson>());
			Factory.Save();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPerson))
			{
				pmsForm.Show();
				(pmsForm.Controls.Find("MergeButton", true)[0] as ZButton).PerformClick();

				var dissolvedGrid = pmsForm.CandidatesControlsForTest.CandidatesBoundGrid;
				AssertEquals("MergeStatus should be Completed.", "Completed", (dissolvedGrid.Columns["MergeStatus"].ColumnStyle as ZGridColumnStyle).GetValueAsString(dissolvedGrid.ListManager, 0));
				AssertEquals("After merging is completed, MergingWarningMessage should not display.", false, pmsForm.MergeWarningLabel.Visible);
			}
		}

		public void TestMergingWarningMessageDisplayedIfMergingFailed()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = new List<GlbPerson>();
			dissolvedPerson.Add(Factory.NewWithValidTestData<GlbPerson>());

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPerson))
			{
				pmsForm.Show();
				(pmsForm.Controls.Find("MergeButton", true)[0] as ZButton).PerformClick();

				var dissolvedGrid = pmsForm.CandidatesControlsForTest.CandidatesBoundGrid;
				CombineAssertions("If merging failed", () =>
				{
					AssertNotEquals("MergeStatus should not be Completed.", "Completed", (dissolvedGrid.Columns["MergeStatus"].ColumnStyle as ZGridColumnStyle).GetValueAsString(dissolvedGrid.ListManager, 0));
					AssertEquals("MergingWarningMessage should display.", true, pmsForm.MergeWarningLabel.Visible);
					AssertEquals("MergingWarningMessage should be default", pmsForm.MergingWarningMessageForTest, pmsForm.MergeWarningLabel.Text);
				});
			}
		}

		public void TestMandatoryColumns()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();
				CombineAssertions("Mandatory columns should be set: ", () =>
				{
					AssertEquals("Retained Full Name:", true, pmsForm.RetainedControlsForTest.RetainedBoundGrid.Columns["FullName"].IsMandatory);
					AssertEquals("Candidates Full Name:", true, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.Columns["FullName"].IsMandatory);
					AssertEquals("Candidates Merge Status:", true, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.Columns["MergeStatus"].IsMandatory);
				});
			}
		}

		public void TestCloseButtonFocusedAfterFormShow()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();
				Application.DoEvents();
				AssertEquals("The CloseButton should be focused by default", true, (pmsForm.Controls.Find("CloseButton", true)[0] as ZButton).Focused);
			}
		}

		public void TestRetainedGrid_AllowSortingShouldBeDisabled()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePerson))
			{
				AssertEquals("Retained Grid sorting should be disabled", false, pmsForm.RetainedControlsForTest.RetainedBoundGrid.AllowSorting);
			}
		}

		public void TestCandidatesdGrid_AllowSortingShouldBeEnabledOnOpen()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();
				AssertEquals("Candidates Grid sorting should be enabled on opening the form", true, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.AllowSorting);
			}
		}

		public void TestDefaultButtons()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;

				AssertEquals("Confirm Merge", mergeButton.CaptionResourceString.Caption);
				AssertEquals("Cancel", closeButton.CaptionResourceString.Caption);
			}
		}

		public void TestClickConfirmMerge()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;

				CombineAssertions("Precondition: Before Confirm Merge button was clicked", () =>
				{
					AssertEquals(true, mergeButton.Visible);
					AssertEquals(true, mergeButton.Enabled);
					AssertEquals(true, closeButton.Visible);
					AssertEquals(true, closeButton.Enabled);
				});

				mergeButton.PerformClick();

				CombineAssertions("After Confirm Merge button was clicked", () =>
				{
					AssertEquals(false, mergeButton.Visible);
					AssertEquals(true, mergeButton.Enabled);
					AssertEquals(true, closeButton.Visible);
					AssertEquals(true, closeButton.Enabled);
				});
			}
		}

		public void TestClickConfirmMerge_DissolvedGridDisabledDuringMerge()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>()
			{
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>()
			};

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();

				// Use this to make sure sorting is actually flipped
				var enabledPreMerge = pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.Enabled;
				AssertEquals("Pre-condition: Candidates grid: Enabled should be true before merging", true, enabledPreMerge);

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				mergeButton.PerformClick();

				CombineAssertions("After Confirm Merge button was clicked", () =>
				{
					AssertEquals("Candidates grid: Enabled should be false during merging", true, pmsForm.EnabledStatusWhileMerging.All(x => !x));
					AssertEquals("Candidates grid: Enabled should be flipped during merging", true, pmsForm.EnabledStatusWhileMerging.All(x => x == !enabledPreMerge));
					AssertEquals("Candidates grid: Enabled should be true after merging", true, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.Enabled);
				});
			}
		}

		public void TestClickConfirmMerge_ScrollDownRowInDissolvedGridWhenMerging()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Jane Smith";

			var dissolvePersons = new List<GlbPerson>
			{
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>()
			};
			dissolvePersons[0].PER_FullName = "Smith Richard";
			dissolvePersons[1].PER_FullName = "John Smith";
			dissolvePersons[2].PER_FullName = "Bob Smith";

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePersons))
			{
				pmsForm.Show();

				AssertEquals("Precondition", 0, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.CurrentRowIndex);

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;

				mergeButton.PerformClick();

				AssertEquals(0, pmsForm.DissolvedGridRowIndex[0]);
				AssertEquals(1, pmsForm.DissolvedGridRowIndex[1]);
				AssertEquals(2, pmsForm.DissolvedGridRowIndex[2]);
				AssertEquals(2, pmsForm.CandidatesControlsForTest.CandidatesBoundGrid.CurrentRowIndex);
			}
		}

		public void TestClickConfirmMerge_DisplayMessageStatusBarNotification()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Ashton Smith";

			var dissolvePersons = new List<GlbPerson>
			{
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>(),
				Factory.NewWithValidTestData<GlbPerson>()
			};
			dissolvePersons[0].PER_FullName = "Smith Richard";
			dissolvePersons[1].PER_FullName = "John Smith";
			dissolvePersons[2].PER_FullName = "Bob Smith";

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvePersons))
			{
				pmsForm.Show();
				Application.DoEvents();
				AssertEquals("Precondition", "Cancel", pmsForm.MessageStatusBarPanel.Text);

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				mergeButton.PerformClick();

				AssertEquals("Merge (1 of 3) were processed ...", pmsForm.MergeStatusBarNotification[0]);
				AssertEquals("Merge (2 of 3) were processed ...", pmsForm.MergeStatusBarNotification[1]);
				AssertEquals("Merge (3 of 3) were processed ...", pmsForm.MergeStatusBarNotification[2]);
				AssertEquals("Merge (3 of 3) were processed ...", pmsForm.MessageStatusBarPanel.Text);
			}
		}

		public void TestClickClose()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				pmsForm.Show();

				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;
				closeButton.PerformClick();

				AssertEquals(true, pmsForm.IsDisposed);
			}
		}

		public void TestPersonMergeSummaryForm_IsShownModally()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var parentForm = new ZForm())
			{
				var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson);

				parentForm.Show();
				ZFormModaliser.Show(pmsForm, parentForm);

				AssertEquals(parentForm, ZFormModaliser.GetParentFormForModalForm(pmsForm));

				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;
				closeButton.PerformClick();

				AssertEquals(true, pmsForm.IsDisposed);
				AssertEquals(true, parentForm.Enabled);
				AssertNotNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(ZFormModaliser.LastFormShownForTest.GetType(), typeof(PersonMergeSummaryForm));
			}
		}

		public void TestGetForeColourOfMergeStatusFromCellText()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				AssertEquals(Color.Black, pmsForm.GetForColourFromCellText("ABC"));
				AssertEquals(Color.Orange, pmsForm.GetForColourFromCellText("Queued"));
				AssertEquals(Color.Blue, pmsForm.GetForColourFromCellText("Merging"));
				AssertEquals(Color.Red, pmsForm.GetForColourFromCellText("MergedWithErrors"));
				AssertEquals(Color.Red, pmsForm.GetForColourFromCellText("FailedWithCriticalError"));
				AssertEquals(Color.Green, pmsForm.GetForColourFromCellText("Completed"));
			}
		}

		public void TestCloseButtonChangesToOk()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var parentForm = new ZForm())
			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				ZFormModaliser.Show(pmsForm, parentForm);

				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;
				AssertEquals("Precondition: CloseButton is captioned 'Cancel'", "Cancel", closeButton.CaptionResourceString.Caption);

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				mergeButton.PerformClick();

				AssertEquals("CloseButton now shows 'OK' following merge", "OK", closeButton.Text);
			}
		}

		public void TestDialogResult_Closed()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvePerson = new List<GlbPerson>();

			using (var parentForm = new ZForm())
			using (var pmsForm = new PersonMergeSummaryForm(retainedPerson, dissolvePerson))
			{
				ZFormModaliser.Show(pmsForm, parentForm);

				var closeButton = pmsForm.Controls.Find("CloseButton", true)[0] as ZButton;
				closeButton.PerformClick();

				AssertEquals("The dialog result should be none if the form is simply closed", DialogResult.None, pmsForm.DialogResult);
			}
		}

		#region OpenPersonForm

		public void TestOpenPersonForm_ShownModallyInReadMode_WhenDoubleClickRetainedPerson()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var controller = pmsForm.Controls.Find("RetainedControls", true)[0] as PersonMergeSummaryRetainedUserControls;
				var grid = controller.RetainedBoundGrid;

				grid.CurrentRowIndex = 0;

				var selectedBizO = (PersonMergeBusinessObject)grid.ListManager.GetCurrent();
				var selectedPerson = selectedBizO.Person;
				AssertNotNull("Pre-condition: selected person", selectedPerson);
				AssertEquals("Pre-condition: selected person's name", "Ashton Smith", selectedPerson.PER_FullName);
				AssertEquals("Pre-condition: merging in progress", false, pmsForm.IsMergingInProgress);

				grid.PerformMouseDownForTest(grid.CurrentRowIndex, 2); //simulate double click

				using (var dialogForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull("Dialog form", dialogForm);
					AssertEquals("Form type shown modally", typeof(GlbPersonForm), dialogForm.GetType());

					var personForm = (GlbPersonForm)dialogForm;
					AssertEquals("Form caption", "Person - Ashton Smith", personForm.FormCaption);
					AssertEquals("Display mode", ODisplayMode.ReadOnly, personForm.DisplayMode);
				}
			}
		}

		public void TestOpenPersonForm_ShownModallyInReadMode_WhenDoubleClickDissolvedPerson()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var controller = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var grid = controller.CandidatesBoundGrid;

				grid.CurrentRowIndex = 1;

				var selectedBizO = (PersonMergeBusinessObject)grid.ListManager.GetCurrent();
				var selectedPerson = selectedBizO.Person;
				AssertNotNull("Pre-condition: selected person", selectedPerson);
				AssertEquals("Pre-condition: selected person's name", "John Smith", selectedPerson.PER_FullName);
				AssertEquals("Pre-condition: merging in progress", false, pmsForm.IsMergingInProgress);

				grid.PerformMouseDownForTest(grid.CurrentRowIndex, 2); //simulate double click

				using (var dialogForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull("Dialog form", dialogForm);
					AssertEquals("Form type shown modally", typeof(GlbPersonForm), dialogForm.GetType());

					var personForm = (GlbPersonForm)dialogForm;
					AssertEquals("Form caption", "Person - John Smith", personForm.FormCaption);
					AssertEquals("Display mode", ODisplayMode.ReadOnly, personForm.DisplayMode);
				}
			}
		}

		[RequiresSTA]
		public void TestOpenPersonForm_ShouldNotOpen_WhenDoubleClickRetainedPerson_WhileMergingInProgress()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var controller = pmsForm.Controls.Find("RetainedControls", true)[0] as PersonMergeSummaryRetainedUserControls;
				var grid = controller.RetainedBoundGrid;

				AssertEquals("Pre-condition: selected row", 0, grid.CurrentRowIndex);
				var selectedBizO = (PersonMergeBusinessObject)grid.ListManager.GetCurrent();
				var selectedPerson = selectedBizO.Person;

				pmsForm.SetMergingInProgress(true);

				AssertNotNull("Pre-condition: selected person", selectedPerson);
				AssertEquals("Pre-condition: merging in progress", true, pmsForm.IsMergingInProgress);

				UnitTestUserNotification.Instance.ClearMessages();
				grid.PerformMouseDownForTest(grid.CurrentRowIndex, 2); //simulate double click

				using (var dialogForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull("Dialog form", dialogForm);
					AssertContains("Can not show the person while the merging is in progress.", UnitTestUserNotification.Instance.LastMessage.Text, true);
				}
			}
		}

		public void TestOpenPersonForm_ShouldNotOpen_WhenDoubleClickDissolvedPerson_WhileMergingInProgress()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var controller = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var grid = controller.CandidatesBoundGrid;

				AssertEquals("Pre-condition: selected row", 0, grid.CurrentRowIndex);
				var selectedBizO = (PersonMergeBusinessObject)grid.ListManager.GetCurrent();
				var selectedPerson = selectedBizO.Person;

				pmsForm.SetMergingInProgress(true);

				AssertNotNull("Pre-condition: selected person", selectedPerson);
				AssertEquals("Pre-condition: merging in progress", true, pmsForm.IsMergingInProgress);

				UnitTestUserNotification.Instance.ClearMessages();
				grid.PerformMouseDownForTest(grid.CurrentRowIndex, 2); //simulate double click

				using (var dialogForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull("Dialog form", dialogForm);
					AssertContains("Can not show the person while the merging is in progress.", UnitTestUserNotification.Instance.LastMessage.Text, true);
				}
			}
		}

		public void TestOpenPersonForm_ShouldNotOpen_WhenDoubleClickDissolvedPersonThatHasBeenDeleted()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var controller = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var grid = controller.CandidatesBoundGrid;

				AssertEquals("Pre-condition: selected row", 0, grid.CurrentRowIndex);
				var selectedBizO = (PersonMergeBusinessObject)grid.ListManager.GetCurrent();
				var selectedPerson = selectedBizO.Person;
				AssertNotNull("Pre-condition: selected person before merge", selectedPerson);

				var mergeButton = pmsForm.Controls.Find("MergeButton", true)[0] as ZButton;
				mergeButton.PerformClick();

				var newFactory = new BusinessObjectFactory();
				var newSelectedPerson = newFactory.Load<GlbPerson>(selectedPerson.PK);
				AssertNull("Pre-condition: selected person has been deleted", newSelectedPerson);

				grid.CurrentRowIndex = 0; //re-select the 1st row

				UnitTestUserNotification.Instance.ClearMessages();
				grid.PerformMouseDownForTest(grid.CurrentRowIndex, 2); //simulate double click

				using (var dialogForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNull("Dialog form", dialogForm);
					AssertContains("Can not show the person that is being deleted or has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text, true);
				}
			}
		}

		#endregion

		#region SelectRow

		public void TestSelectRowInCandidatesGrid_WhenRowSelectedInRetainedGrid_ShouldHighlightRowInCandidatesGrid()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var retainedController = pmsForm.Controls.Find("RetainedControls", true)[0] as PersonMergeSummaryRetainedUserControls;
				var retainedGrid = retainedController.RetainedBoundGrid;

				var candidatesController = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var candidatesGrid = candidatesController.CandidatesBoundGrid;

				retainedGrid.Focus();
				retainedGrid.CurrentRowIndex = 0;
				retainedGrid.PerformMouseDownForTest(retainedGrid.CurrentRowIndex, 1);

				CombineAssertions("Pre-condition", () =>
				{
					AssertEquals("Is retained grid in focus?", true, retainedGrid.Focused);
					AssertEquals("Number of selected rows in retained grid", 1, retainedGrid.SelectedRowCount);

					AssertEquals("Is candidates grid in focus?", false, candidatesGrid.Focused);
					AssertEquals("Number of selected rows in candidates grid", 0, candidatesGrid.SelectedRowCount);
				});

				candidatesGrid.CurrentRowIndex = 2;
				candidatesGrid.PerformMouseDownForTest(candidatesGrid.CurrentRowIndex, 1);

				CombineAssertions(() =>
				{
					AssertEquals("Is retained grid in focus?", false, retainedGrid.Focused);
					AssertEquals("Number of selected rows in retained grid", 0, retainedGrid.SelectedRowCount);

					AssertEquals("Is candidates grid in focus?", true, candidatesGrid.Focused);
					AssertEquals("Number of selected rows in candidates grid", 1, candidatesGrid.SelectedRowCount);
					AssertEquals("3rd row is selected in candidates grid", true, candidatesGrid.IsSelected(2));
				});
			}
		}

		[RequiresSTA]
		public void TestSelectRowInRetainedGrid_WhenRowSelectedInCandidatesGrid_ShouldHighlightedRowInRetainedGrid()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var retainedController = pmsForm.Controls.Find("RetainedControls", true)[0] as PersonMergeSummaryRetainedUserControls;
				var retainedGrid = retainedController.RetainedBoundGrid;

				var candidatesController = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var candidatesGrid = candidatesController.CandidatesBoundGrid;

				candidatesGrid.Focus();
				candidatesGrid.CurrentRowIndex = 2;
				candidatesGrid.PerformMouseDownForTest(candidatesGrid.CurrentRowIndex, 1);

				CombineAssertions("Pre-condition", () =>
				{
					AssertEquals("Is candidates grid in focus?", true, candidatesGrid.Focused);
					AssertEquals("Number of selected rows in candidates grid", 1, candidatesGrid.SelectedRowCount);

					AssertEquals("Is retained grid in focus?", false, retainedGrid.Focused);
					AssertEquals("Number of selected rows in retained grid", 0, retainedGrid.SelectedRowCount);
				});

				retainedGrid.CurrentRowIndex = 0;
				retainedGrid.PerformMouseDownForTest(retainedGrid.CurrentRowIndex, 1);

				CombineAssertions(() =>
				{
					AssertEquals("Is candidates grid in focus?", false, candidatesGrid.Focused);
					AssertEquals("Number of selected rows in candidates grid", 0, candidatesGrid.SelectedRowCount);

					AssertEquals("Is retained grid in focus?", true, retainedGrid.Focused);
					AssertEquals("Number of selected rows in retained grid", 1, retainedGrid.SelectedRowCount);
					AssertEquals("1st row is selected in retained grid", true, retainedGrid.IsSelected(0));
				});
			}
		}

		public void TestAllowOnlyOneRowSelectedInCandidatesGrid()
		{
			CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons);

			using (var pmsForm = new PersonMergeSummaryFormForTest(retainedPerson, dissolvedPersons))
			{
				pmsForm.Show();

				var candidatesController = pmsForm.Controls.Find("CandidatesControls", true)[0] as PersonMergeSummaryCandidatesUserControls;
				var candidatesGrid = candidatesController.CandidatesBoundGrid;

				candidatesGrid.Focus();

				for (int row = 0; row < candidatesGrid.ListManager.Count; row++)
				{
					candidatesGrid.Select(row);
				}
				AssertEquals("PreCondition - Number of selected rows in candidates grid", 3, candidatesGrid.SelectedRowCount);

				candidatesGrid.CurrentRowIndex = 1;
				candidatesGrid.PerformMouseDownForTest(candidatesGrid.CurrentRowIndex, 1);

				CombineAssertions(() =>
				{
					AssertEquals("Is candidates grid in focus?", true, candidatesGrid.Focused);
					AssertEquals("Number of selected rows in candidates grid", 1, candidatesGrid.SelectedRowCount);
					AssertEquals("Last selected row in candidates grid", 1, candidatesGrid.CurrentRowIndex);
				});
			}
		}

		#endregion

		#region Implementation

		string GetValueAsString(int rowNum, string column, ZGrid grid)
		{
			return (grid.Columns[column].ColumnStyle as ZGridColumnStyle).GetValueAsString(grid.ListManager, rowNum);
		}

		void CreateRertainedAndDissolvedPersons(out GlbPerson retainedPerson, out List<GlbPerson> dissolvedPersons)
		{
			retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Ashton Smith";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Smith Richard";
			dissolvedPerson2.PER_FullName = "John Smith";
			dissolvedPerson3.PER_FullName = "Bob Smith";

			Factory.Save();

			dissolvedPersons = new List<GlbPerson>
			{
				dissolvedPerson1,
				dissolvedPerson2,
				dissolvedPerson3
			};
		}

		#endregion

		internal class PersonMergeSummaryFormForTest : PersonMergeSummaryForm
		{
			internal string MergingWarningMessageForTest => "This action is irreversible and cannot be undone.\r\nAre you sure you want to proceed?";
			public PersonMergeSummaryRetainedUserControls RetainedControlsForTest => RetainedControls;
			public PersonMergeSummaryCandidatesUserControls CandidatesControlsForTest => CandidatesControls;
			public List<int> DissolvedGridRowIndex = new List<int>();
			public List<string> MergeStatusBarNotification = new List<string>();
			public List<bool> EnabledStatusWhileMerging = new List<bool>();
			internal bool SetExceptions { get; set; }

			public PersonMergeSummaryFormForTest(GlbPerson personToRetained, List<GlbPerson> personToDissolved)
				: base(personToRetained, personToDissolved)
			{
				ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			}

			protected override MultiPersonMerger GetMultiPersonMerger(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection)
			{
				MultiPersonMerger multiPersonMergerResult;
				if (SetExceptions)
				{
					var exception = new ZSaveException(new ZDataException(null, null, null), new BusinessObjectFactory());
					var saverWithException = new PersonMergerTest.PersonMergeTransactionSaverForTest(exception, true);
					var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>
					{
						new Tuple<IPersonMergeTransactionSaver, Exception>(saverWithException, exception),
						new Tuple<IPersonMergeTransactionSaver, Exception>(saverWithException, exception)
					};
					multiPersonMergerResult = new MultiPersonMergerTest.MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);
				}
				else
				{
					multiPersonMergerResult = base.GetMultiPersonMerger(retainedCollection, dissolvedCollection);
				}

				return multiPersonMergerResult;
			}

			protected override void OnMergeProgressNotification(object sender, MergeProgressEventArgs e)
			{
				base.OnMergeProgressNotification(sender, e);

				DissolvedGridRowIndex.Add(CandidatesControls.CandidatesBoundGrid.CurrentRowIndex);

				MergeStatusBarNotification.Add(MessageStatusBarPanel.Text);

				EnabledStatusWhileMerging.Add(CandidatesControls.CandidatesBoundGrid.Enabled);
			}

			internal void SetMergingInProgress(bool inProgress)
			{
				IsMergingInProgress = inProgress;
			}
		}
	}

	public class PersonMergeSummaryFormTestNoTestFactory : TestCase
	{
		[UseSnapshotProtection]
		public void TestDialogResult_Merged()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var factory = new BusinessObjectFactory();

			var masterPerson = factory.New<GlbPerson>();
			masterPerson.PER_FullName = "Shoryu Das-Zaman";

			var target1 = factory.New<GlbPerson>();
			target1.PER_FullName = "Shoryu DZ";

			var target2 = factory.New<GlbPerson>();
			target2.PER_FullName = "Shoryu Zaman-Das";

			factory.Save();

			var personsToDissolved = new List<GlbPerson>()
			{
				factory.Load<GlbPerson>(target1.PK),
				factory.Load<GlbPerson>(target2.PK)
			};

			var personToRetained = factory.Load<GlbPerson>(masterPerson.PK);

			using (var parentForm = new ZForm())
			using (var pmsForm = new PersonMergeSummaryForm(personToRetained, personsToDissolved))
			{
				ZFormModaliser.Show(pmsForm, parentForm);

				pmsForm.MergeButton_Click(null, null);

				pmsForm.CloseButton_Click(null, null);

				AssertEquals("The dialog result should be Yes if the merge completes", DialogResult.Yes, pmsForm.DialogResult);
			}
		}
	}
}
