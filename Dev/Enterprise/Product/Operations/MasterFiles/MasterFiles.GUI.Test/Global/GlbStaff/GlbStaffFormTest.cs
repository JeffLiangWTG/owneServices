using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Common.Enumeration;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffForm))]
	sealed class GlbStaffFormTest : ZFormBasherTest
	{
		public void TestDeviceOnlyRelatedOptionsReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var testForm = NewGlbStaffForm(staff))
			{
				staff.GS_IsController = true;
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = true;
				staff.IsReadOnlyDBUser = true;

				testForm.Show();
				testForm.IsDeviceOnlyCheckBox.Checked = true;
				testForm.IsDeviceOnlyCheckBox_Click(this, EventArgs.Empty);

				Assert("IsBackupOperatorBoundCheck option should be readonly.", testForm.IsBackupOperatorBoundCheck.ReadOnly);
				Assert("IsReadOnlyDBUserBoundCheck option should be readonly.", testForm.IsReadOnlyDBUserBoundCheck.ReadOnly);
				Assert("IsDatabaseDeveloperBoundCheck option should be readonly.", testForm.IsDatabaseDeveloperBoundCheck.ReadOnly);
				Assert("GS_IsControllerBoundCheck option should be readonly.", testForm.GS_IsControllerBoundCheck.ReadOnly);

				Assert(staff.GS_IsController == false);
				Assert(staff.IsDatabaseDeveloper == false);
				Assert(staff.IsBackupOperator == false);
				Assert(staff.IsReadOnlyDBUser == false);

				testForm.IsDeviceOnlyCheckBox.Checked = true;
				testForm.IsDeviceOnlyCheckBox_Click(this, EventArgs.Empty);
				Assert("IsBackupOperatorBoundCheck option should not be readonly.", testForm.IsBackupOperatorBoundCheck.ReadOnly);
				Assert("IsReadOnlyDBUserBoundCheck option should not be readonly.", testForm.IsReadOnlyDBUserBoundCheck.ReadOnly);
				Assert("IsDatabaseDeveloperBoundCheck option should not be readonly.", testForm.IsDatabaseDeveloperBoundCheck.ReadOnly);
				Assert("GS_IsControllerBoundCheck option should be not readonly.", testForm.GS_IsControllerBoundCheck.ReadOnly);
			}
		}

		public void TestGroupButtonsReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ExternalId = "";

			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals(true, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				AssertEquals(true, testForm.MembersModuleButtonGrid.DetachButton.Enabled);
			}

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ExternalId = "123";

			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.GlbStaffTabControl.SelectedTab = testForm.GroupsTabPage;
				testForm.Show();
				AssertEquals(false, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				AssertEquals(false, testForm.MembersModuleButtonGrid.DetachButton.Enabled);
			}
		}

		bool IsVisible(Control parent, string controlName)
		{
			var control = parent.Controls
				.Find(controlName, true)
				.SingleOrDefault();

			return control != null && ZEnumerable.Iterate(control, c => c.Parent, null).All(c => c.Visible);
		}

		protected override Form GetFormToBashCore()
		{
			var staff = StaffForForm;
			Factory.Save();
			return NewGlbStaffForm(staff);
		}

		GlbStaffForm NewGlbStaffForm(GlbStaff staff)
		{
			GlbStaffForm fctval = new GlbStaffForm(staff);
			return fctval;
		}

		#region Detached Logs

		public void TestDetachedLogs_DetachStaffFromGroupForm_ShouldNotHaveDuplicateLogs()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSF";
			staff.GS_FullName = "Test Staff";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "TGP";
			group.GG_Desc = "Test Group";

			var link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;

			Factory.Save();

			using (var staffForm = new GlbStaffForm(staff))
			{
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.GroupsTabPage;
				staffForm.Show();

				staffForm.MembersModuleButtonGrid.InnerGrid.SelectSingleElementByPK(group.PK);
				staffForm.MembersModuleButtonGrid.InnerGrid.PerformMouseDownForTest(0, 2);
				Application.DoEvents();

				using (var groupForm = Application.OpenForms.OfType<GlbGroupForm>().FirstOrDefault())
				{
					groupForm.Show();

					groupForm.MembersModuleButtonGrid.InnerGrid.SelectSingleElementByPK(staff.PK);
					KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					Application.DoEvents();

					groupForm.FireSaveButton();
				}
				Factory.Save();

				var groupDetachedLogsCount = group.Logs.Find(log => log.SL_Reference == "Detached - (TSF) Test Staff").Count();
				AssertEquals("Group should only have 1 detached log", 1, groupDetachedLogsCount);

				var staffDetachedLogsCount = staff.Logs.Find(log => log.SL_Reference == "Detached - (TGP) Test Group").Count();
				AssertEquals("Staff should only have 1 detached log", 1, staffDetachedLogsCount);
			}
		}

		#endregion

		public void TestDeActiveStaffShouldShowNotification()
		{
			var staff = Factory.NewWithValidTestData<GlbStaffForTest>();
			using (var form = NewGlbStaffForm(staff))
			{
				staff.GS_IsActive = true;
				form.Show();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				staff.GS_IsActive = false;
				AssertEquals("This user is currently logged in and their login will be immediately terminated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region DeduplicationTests

		public void TestFindDuplicatesMenuItemIsEnabledOrNot()
		{
			AssertFindDuplicatesMenuItemIsEnabledOrNot(true, true);
			AssertFindDuplicatesMenuItemIsEnabledOrNot(false, true);
			AssertFindDuplicatesMenuItemIsEnabledOrNot(true, false);
		}

		void AssertFindDuplicatesMenuItemIsEnabledOrNot(bool personsEnableDuplicateDetection, bool isStaffActive)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = isStaffActive;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, personsEnableDuplicateDetection))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				staffForm.Show();
				var actionsMenuItemCollection = staffForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var duplicatesMenuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");

				if (personsEnableDuplicateDetection && isStaffActive)
				{
					AssertEquals(true, duplicatesMenuItem.Enabled);
				}
				else
				{
					AssertEquals(false, duplicatesMenuItem.Enabled);
				}
			}
		}

		public void TestFindDuplicatesMenuItemExist()
		{
			var staff = Factory.New<GlbStaff>();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				staffForm.Show();
				var actionsMenuItemCollection = staffForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull(actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates"));
			}
		}

		public void TestFindDuplicatesFunctionality()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var staff = CreateStaffWithValidTestData();
			staff.Validation.ValidateAll();

			var isFindingDuplicates = false;
			staff.Person.DeduplicationStarted += delegate
			{
				isFindingDuplicates = true;
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				var actionsMenuItemCollection = staffForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertEquals("Precondition", false, isFindingDuplicates);
				actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates").PerformClick();
				AssertEquals(true, isFindingDuplicates);
			}
		}

		public void TestPersonNotNullWhenStaffFormNotSaved()
		{
			var staff = Factory.New<GlbStaff>();
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				AssertNotNull(staff.Person);
			}
		}

		public void TestDuplicationEventsHookedAfterShowingForm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var isSearchingForDuplicates = false;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				staff.Person.DeduplicationStarted += delegate
				{
					isSearchingForDuplicates = true;
				};

				AssertEquals("Precondition:", false, isSearchingForDuplicates);
				staff.Person.PropagateDeduplicationStarted();
				AssertEquals("After Form is shown, DuplicationEvents are hooked", true, isSearchingForDuplicates);
			}
		}

		public void TestDuplicationEventsHookedAfterChangingPerson()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var isOldPersonSearchingForDuplicates = false;
			var isNewPersonSearchingForDuplicates = false;
			staff.Person.DeduplicationStarted += delegate
			{
				isOldPersonSearchingForDuplicates = true;
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				var eventArgs = new DuplicationEventArgs(null);
				eventArgs.InvokedAction = DeduplicationAction.Merge;
				staffForm.DeduplicationActionOccurred(this, eventArgs);
				AssertEquals("Precondition1:", false, isOldPersonSearchingForDuplicates);

				staffForm.Person.PropagateDeduplicationStarted();
				AssertEquals("DuplicationEventsForOldPerson are hooked", true, isOldPersonSearchingForDuplicates);
				isOldPersonSearchingForDuplicates = false;

				staff.GS_PER = person.PK;
				staff.Person.DeduplicationStarted += delegate
				{
					isNewPersonSearchingForDuplicates = true;
				};
				staffForm.DeduplicationActionOccurred(this, eventArgs);
				AssertEquals("Precondition2: ", false, isOldPersonSearchingForDuplicates);
				AssertEquals("Precondition3: ", false, isNewPersonSearchingForDuplicates);

				staff.Person.PropagateDeduplicationStarted();
				AssertEquals("DuplicationEventsForOldPerson are unhooked", false, isOldPersonSearchingForDuplicates);
				AssertEquals("DuplicationEventsForNewPerson are hooked", true, isNewPersonSearchingForDuplicates);
			}
		}

		public void TestDeDupSpinnerNotShownAtStart()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();
				AssertEquals("Spinner should not be visible when form is created", false, form.DeduplicationStatusIconVisible);
			}
		}

		public void TestShowNoDuplicatesFoundDisplaysCorrectly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var args = new DuplicationEventArgs(null, null, null, null, DuplicationStatus.OK, new DeduplicationExclusionManager<GlbPerson>());

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.ShowDeduplicationStatus();
				Assert(form.DeduplicationStatusIconVisible);
				Assert(form.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(form.DeduplicationStatusLabelColorIs(Color.Black));

				form.ShowNoDuplicatesFound(args);
				AssertEquals(false, form.DeduplicationStatusIconVisible);

				var originalRegValueExcludingInactive = SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.Value;

				try
				{
					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					form.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", form.DeduplicationStatusText);

					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					form.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", form.DeduplicationStatusText);
				}
				finally
				{
					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegValueExcludingInactive);
				}
				Assert(form.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));
			}
		}

		public void TestDeduplicationActionOccurred()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				var eventArgs = new DuplicationEventArgs(null);

				eventArgs.InvokedAction = DeduplicationAction.Merge;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Ignore;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Link;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.NotMatched;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenMaster;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenTarget;
				form.ShowDuplicatesFound(eventArgs);
				Assert(form.DeduplicationStatusVisible);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(!form.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.None;
				form.ShowDuplicatesFound(eventArgs);
				form.DeduplicationActionOccurred(this, eventArgs);
				Assert(form.DeduplicationStatusVisible);
			}
		}

		public void TestShowDuplicationMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.ShowDeduplicationStatus();
				CombineAssertions("ShowDeduplicationStatus", () =>
				{
					Assert("DeduplicationStatusIcon visibility", form.DeduplicationStatusIconVisible);
					Assert("DeduplicationStatusLabel content", form.DeduplicationStatusLabelReads("Detecting duplicates"));
					Assert("DeduplicationStatusLabel color", form.DeduplicationStatusLabelColorIs(Color.Black));
				});

				var args = new DuplicationEventArgs(null, null, null, null);
				form.ShowDuplicatesFound(args);
				CombineAssertions("ShowDuplicatesFound", () =>
				{
					Assert("DeduplicationStatusIcon visibility", !form.DeduplicationStatusIconVisible);
					Assert("DeduplicationStatusLabel content", form.DeduplicationStatusLabelReads("Duplicates found"));
					Assert("DeduplicationStatusLabel color", form.DeduplicationStatusLabelColorIs(Color.Blue));
					AssertEquals("DuplicationEventArgs", args, form.CurrentDuplicationEventArgs);
				});

				form.ShowDeduplicationTimeoutMessage();
				CombineAssertions("ShowDeduplicationTimeoutMessage", () =>
				{
					Assert("DeduplicationStatusIcon visibility", !form.DeduplicationStatusIconVisible);
					Assert("DeduplicationStatusLabel content", form.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
					Assert("DeduplicationStatusLabel color", form.DeduplicationStatusLabelColorIs(Color.OrangeRed));
				});

				form.ShowNotEnoughInformation();
				CombineAssertions("ShowNotEnoughInformation", () =>
				{
					Assert("DeduplicationStatusIcon visibility", !form.DeduplicationStatusIconVisible);
					Assert("DeduplicationStatusLabel content", form.DeduplicationStatusLabelReads("Not enough information to detect duplicates"));
					Assert("DeduplicationStatusLabel color", form.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));
				});

				form.ShowExcludedDuplicationMessage();
				CombineAssertions("ShowExcludedDuplicationMessage", () =>
				{
					Assert("DeduplicationStatusIcon visibility", !form.DeduplicationStatusIconVisible);
					Assert("DeduplicationStatusLabel content", form.DeduplicationStatusLabelReads("Excluded from De-duplication"));
					Assert("DeduplicationStatusLabel color", form.DeduplicationStatusLabelColorIs(Color.OrangeRed));
				});
			}
		}

		public void TestDeduplicationHyperlinkInvokesDeduplicationSearch()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var staff = CreateStaffWithValidTestData();
			staff.Validation.ValidateAll();

			bool isSearchingForDuplicates;
			staff.Person.DeduplicationStarted += delegate
			{
				isSearchingForDuplicates = true;
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new GlbStaffFormForTesting(staff))
			{
				staff.GS_EmailAddress = "qqq@111.com";
				AssertEquals(false, staff.HasErrors);

				isSearchingForDuplicates = false;
				form.DuplicateDetectionHyperlinkClicked();
				AssertEquals(true, isSearchingForDuplicates);
			}
		}

		public void TestDeduplicationHyperlinkNotInvokesDeduplicationSearchWhenContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var staff = CreateStaffWithValidTestData();
			staff.Validation.ValidateAll();

			bool isSearchingForDuplicates;
			staff.Person.DeduplicationStarted += delegate
			{
				isSearchingForDuplicates = true;
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new GlbStaffFormForTesting(staff))
			{
				staff.GS_EmailAddress = "123";
				AssertEquals(true, staff.HasErrors);

				isSearchingForDuplicates = false;
				form.DuplicateDetectionHyperlinkClicked();
				AssertEquals(false, isSearchingForDuplicates);
			}
		}

		GlbStaff CreateStaffWithValidTestData()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "ABC";
			staff.GS_UserAddress1 = "DEF";
			staff.GS_City = "GHI";
			Factory.Save();

			return staff;
		}

		public void TestDeduplicationAlert()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = staff.Person.PK.ToGuid(),
				MasterType = typeof(GlbPerson),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(GlbPerson)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();
				form.DeduplicationHelper.ShowDuplicateAlert(form, args);
				AssertNotNull(form.DeduplicationHelper.ExistingAlertControl);
			}
		}

		public void TestDuplicationEndedWhenDuplicatesFound()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>() { new ScoringResult() }, null, ZGuid.Empty));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("Duplicates found", form.DeduplicationStatusText);
			}
		}

		public void TestDuplicationEndedWhenExcludedFromDuplication()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("Excluded from De-duplication", form.DeduplicationStatusText);
			}
		}

		public void TestDuplicationEndedWithNoScoringResultsAndMinimumRequirementsNotMet()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), null, ZGuid.Empty));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("Not enough information to detect duplicates", form.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithNoScoringResultsAndMinimumRequirementsMet()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), new List<PatternMatchingResultModel>(), ZGuid.Empty));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("No duplicates found.", form.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithTimeout()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.Timeout, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("This process has stopped due to timeout", form.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithErrorOccurred()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.ErrorOccurred, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(form.DeduplicationStatusVisible);
				AssertEquals("An error occurred while detecting duplicates", form.DeduplicationStatusText);
			}
		}

		#endregion

		public void TestClickGridElements_StaffForm_ShouldOpenCapabilitiesForm()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";

			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				const int HumanResourcesTabIndex = 5;
				const int CapabilitiesTabIndex = 4;

				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find(nameof(GlbStaffForm.GlbStaffTabControl), true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = HumanResourcesTabIndex;
				Application.DoEvents();

				var subTab = tab.Controls.Find("HRTabControl", true)[0] as ZTemplateTabControl;
				subTab.SelectedIndex = CapabilitiesTabIndex;
				Application.DoEvents();

				var grid = subTab.Controls.Find("CapabilityGrid", true)[0] as ZModuleButtonGrid;
				grid.InnerGrid.PerformMouseDoubleClickForTest(0);

				using (var capabilitiesTab = Application.OpenForms.OfType<GlbCapabilityForm>().SingleOrDefault())
				{
					var capabilityBizo = (GlbCapability)capabilitiesTab.CurrentDataItem;
					AssertNotNull("Double clicking a grid element should open the corresponding capability form!", capabilitiesTab);
					CombineAssertions("GlbCapability has no .Equals override method for comparing two staff bizos, so we do this", () =>
					{
						AssertEquals("Code is equal", capability.G4_Code, capabilityBizo.G4_Code);
						AssertEquals("Description is equal", capability.G4_Description, capabilityBizo.G4_Description);
					});
				}
			}
		}

		public void TestSavePersonalDataToActiveDirectoryCheckboxVisibility()
		{
			var adRegistry = ObjectFactory.Get<IADRegistry>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			//AD Not Enabled
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("AD Not Enable, CW1 Master", false, testForm.SavePersonalDataToActiveDirectoryCheckbox.Visible);
			}

			//AD Enabled, CW1 master
			adRegistry.IsIntegrationEnabled = true;
			adRegistry.SyncMode = SyncMode.EnterpriseIsMaster;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("AD Enable, CW1 Master", true, testForm.SavePersonalDataToActiveDirectoryCheckbox.Visible);
			}

			//AD Enabled, AD master, 1way Sync
			adRegistry.SyncMode = SyncMode.ADIsMaster;
			adRegistry.SyncDirection = SyncDirection.OneWay;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("AD Enable, AD Master, 1Way sync", false, testForm.SavePersonalDataToActiveDirectoryCheckbox.Visible);
			}

			//AD Enabled, AD master, 2way Sync
			adRegistry.SyncDirection = SyncDirection.TwoWay;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("AD Enable, AD Master, 2Way sync", true, testForm.SavePersonalDataToActiveDirectoryCheckbox.Visible);
			}
		}

		public void TestPrivacyGroupBoxNoOverlappingControlsWhenSavePersonalDataToActiveDirectoryCheckboxIsVisible()
		{
			var adRegistry = ObjectFactory.Get<IADRegistry>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			adRegistry.IsIntegrationEnabled = true;
			adRegistry.SyncMode = SyncMode.EnterpriseIsMaster;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("SavePersonalDataToActiveDirectoryCheckbox should be visible.", true, testForm.SavePersonalDataToActiveDirectoryCheckbox.Visible);
				AssertEquals("SavePersonalDataToActiveDirectoryCheckbox and ExcelModifyingPasswordTextBox should not overlap.", true, testForm.SavePersonalDataToActiveDirectoryCheckbox.Top > testForm.ExcelModifyingPasswordTextBox.Bottom);
			}
		}

		public void TestActivityLogTabPageReadOnly()
		{
			AssertActivityLogTabPageReadOnly("AAA", true);
			AssertActivityLogTabPageReadOnly(User.SupportUserName, false);

			void AssertActivityLogTabPageReadOnly(string loginName, bool expectedResult)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = loginName;

				using (var testForm = NewGlbStaffForm(staff))
				{
					testForm.Show();
					AssertEquals($"ShouldBeReadOnlyInViewMode should be {expectedResult} for user: {loginName}", expectedResult, testForm.ActivityLogTabPage.ShouldBeReadOnlyInViewMode);
				}
			}
		}

		public void TestActivityLogTabFilterDateReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = User.SupportUserName;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.Show();
				var control = testForm.ActivityLogTabPage.FindSingle<ZDateEdit>("ActivityLogFilterDateFromDateEdit");
				AssertEquals("ActivityLogFilterDateFromDateEdit.Readonly should be false", false, control.GetReadOnly());
			}
		}

		public void TestDomainPanelVisibility()
		{
			var adRegistry = ObjectFactory.Get<IADRegistry>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			//AD Enabled, no domains
			adRegistry.IsIntegrationEnabled = true;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should not show when Domain Credentials registry is not set even if AD Integration is enabled", false, testForm.DomainPanel.Visible);
			}

			//AD Enabled, one domain
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should show when there is one or more Domain Credentials and AD Integration is enabled", true, testForm.DomainPanel.Visible);
			}

			EnvProxy.SetHostedLocationForTest("SYD");

			//AD Enabled, one domain, is hosted, login as non-support
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var testForm = NewGlbStaffForm(staff))
				{
					testForm.Show();
					AssertEquals("Is Hosted", true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("Not support user", false, Env.CurrentUser.IsSupportUser);
					AssertEquals("Should not show when it is hosted", false, testForm.DomainPanel.Visible);
				}
			}

			//AD Enabled, one domain, is hosted, login as CW1 Support
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				using (var testForm = NewGlbStaffForm(staff))
				{
					testForm.Show();
					AssertEquals("Is Hosted", true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("Not support user", true, Env.CurrentUser.IsSupportUser);
					AssertEquals("Should show when CWSupport login even it is hosted", true, testForm.DomainPanel.Visible);
				}
			}

			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("Not IsHosted", false, EnvProxy.IsHostedWithCargowise);

			//AD Enabled, two domains
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should show when there is one or more Domain Credentials and AD Integration is enabled", true, testForm.DomainPanel.Visible);
			}

			//AD Disabled, two domains
			adRegistry.IsIntegrationEnabled = false;
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should show when there is one or more Domain Credentials even if AD Integration is disabled", true, testForm.DomainPanel.Visible);
			}

			//AD Disabled, one domain
			adRegistry.IsIntegrationEnabled = false;
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should show when there is one or more Domain Credentials even if AD Integration is disabled", true, testForm.DomainPanel.Visible);
			}

			//AD Disabled, no domains
			adRegistry.IsIntegrationEnabled = false;
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>();
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				AssertEquals("Should not show when Domain Credentials registry is not set, regardless of AD Integration status", false, testForm.DomainPanel.Visible);
			}
		}

		public void TestActivationWithValidationError_ShouldNotAllow()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_HomePhone = "blah"; // some invalid data

			using (var glbStaffForm = new GlbStaffFormForTesting(staff))
			{
				staff.Validation.ValidateAll();
				AssertEquals("Staff should have some validation errors", true, staff.HasErrors());

				glbStaffForm.DisplayMode = ODisplayMode.Delete;
				glbStaffForm.HandleApplyPostingButtonClickUnsafe(true);
				AssertEquals("Should have shown message to fix entity before Activation", "Please fix errors on this entity before Activating", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeactivationWithValidationError_ShouldAllow()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = false;
			staff.GS_HomePhone = "blah"; // some invalid data

			using (var glbStaffForm = new GlbStaffFormForTesting(staff))
			{
				staff.Validation.ValidateAll();
				AssertEquals("Staff should have some validation errors", true, staff.HasErrors());

				glbStaffForm.DisplayMode = ODisplayMode.Delete;
				glbStaffForm.HandleApplyPostingButtonClickUnsafe(true);
				AssertEquals("Should not have shown message to fix entity before Deactivation", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeactivationWithGS_IsActiveValidationError_ShouldNotAllow()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = false;
			staff.Groups.Add(Factory.NewWithValidTestData<GlbGroup>());

			using (var glbStaffForm = new GlbStaffFormForTesting(staff))
			{
				staff.Validation.ValidateAll();
				AssertEquals("Staff should have some validation errors", true, staff.HasErrors());

				glbStaffForm.DisplayMode = ODisplayMode.Delete;
				glbStaffForm.HandleApplyPostingButtonClickUnsafe(true);
				AssertEquals("Should show validation errors on deactivation", $"Please fix errors on {staff.HumanReadableName} before deactivating.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreviousNext()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var testForm = new GlbStaffFormForTesting(staff))
			{
				testForm.Show();
				AssertEquals("Form has Previous and Next Buttons", true, testForm.AutoAddPreviousNextButtons);
				AssertEquals("Display Mode:", testForm.DisplayMode, ODisplayMode.Edit);
			}
		}

		[ExpectNoExceptions]
		public void TestAddressValidationHasNoExceptionsIfDetailsTabPageIsNull()
		{
			bool originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;
			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				using (var testForm = new GlbStaffFormForTesting(staff))
				{
					testForm.ValidationJustForced = true;
					testForm.DetailsTabPage = null;
					testForm.ValidateAddress().Wait();
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
			}
		}

		public void TestActivityLogTabPage_ControlsRightBoundShouldBeCloseToTabsWidthWithProperAnchors()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var scale = 20f; //Keeping it 20, which is same as Logs->ActivityLog

			using (GlbStaffForm form = NewGlbStaffForm(staff))
			{
				form.Show();
				form.TopLevelTabControl.SelectedTab = form.ActivityLogTabPage;

				//ActivityLogFilterGroupBox
				var activityLogFilterGroupBox = form.ActivityLogFilterGroupBox;
				AssertEquals("ActivityLogFilterGroupBox should have anchor style: Top, Left, Right.", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, activityLogFilterGroupBox.Anchor);
				AssertLessThanOrEqualTo("ActivityLogFilterGroupBox right bound should be closer tab's width.", form.ActivityLogTabPage.Width - (activityLogFilterGroupBox.Left + activityLogFilterGroupBox.Width), scale);

				//ActivityLogGrid
				var activityLogGrid = form.ActivityLogGrid;
				AssertEquals("ActivityLogGrid should have anchor style: Top, Bottom, Left, Right.", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, activityLogGrid.Anchor);
				AssertLessThanOrEqualTo("ActivityLogGrid right bound should be closer tab's width.", form.ActivityLogTabPage.Width - (activityLogGrid.Left + activityLogGrid.Width), scale);

				//ActivityLogDetailsGroupBox
				var activityLogDetailsGroupBox = form.ActivityLogDetailsGroupBox;
				AssertEquals("ActivityLogDetailsGroupBox should have anchor style: Bottom, Left, Right.", AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, activityLogDetailsGroupBox.Anchor);
				AssertLessThanOrEqualTo("ActivityLogDetailsGroupBox right bound should be closer tab's width.", form.ActivityLogTabPage.Width - (activityLogDetailsGroupBox.Left + activityLogDetailsGroupBox.Width), scale);

				//ActivityLogTotalsGroupBox
				var activityLogTotalsGroupBox = form.ActivityLogTotalsGroupBox;
				AssertEquals("ActivityLogTotalsGroupBox should have anchor style: Bottom, Left, Right.", AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, activityLogTotalsGroupBox.Anchor);
				AssertLessThanOrEqualTo("ActivityLogTotalsGroupBox right bound should be closer tab's width.", form.ActivityLogTabPage.Width - (activityLogTotalsGroupBox.Left + activityLogTotalsGroupBox.Width), scale);
			}
		}
		public void TestActivityLogs()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader org = newFactory.NewWithValidTestData<OrgHeader>();
			newFactory.Save();

			GlbStaff staff = newFactory.New<GlbStaff>();
			staff.GS_Code = "ILR";

			StmActivityLog enterpriseSavedObjectLog = newFactory.New<StmActivityLog>();
			enterpriseSavedObjectLog.S7_ControllerID = ControllerIDs.Organisation.Name;
			enterpriseSavedObjectLog.S7_EnterpriseActivity = true;
			enterpriseSavedObjectLog.S7_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			enterpriseSavedObjectLog.S7_ParentID = org.PK;
			enterpriseSavedObjectLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 1);

			StmActivityLog enterpriseSavedDeletedObjectLog = newFactory.New<StmActivityLog>();
			enterpriseSavedDeletedObjectLog.S7_ControllerID = ControllerIDs.Organisation.Name;
			enterpriseSavedDeletedObjectLog.S7_EnterpriseActivity = true;
			enterpriseSavedDeletedObjectLog.S7_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			enterpriseSavedDeletedObjectLog.S7_ParentID = ZGuid.NewZGuid();
			enterpriseSavedDeletedObjectLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 2);

			StmActivityLog enterpriseNonSavedObjectLog = newFactory.New<StmActivityLog>();
			enterpriseNonSavedObjectLog.S7_EnterpriseActivity = true;
			enterpriseNonSavedObjectLog.S7_ControllerID = ControllerIDs.Organisation.Name;
			enterpriseNonSavedObjectLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 5);

			StmActivityLog externalLog = newFactory.New<StmActivityLog>();
			externalLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 20);

			StmActivityLog enterpriseSavedObjectOldControllerLog = newFactory.New<StmActivityLog>();
			enterpriseSavedObjectOldControllerLog.S7_ControllerID = "SomeCrap";
			enterpriseSavedObjectOldControllerLog.S7_EnterpriseActivity = true;
			enterpriseSavedObjectOldControllerLog.S7_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			enterpriseSavedObjectOldControllerLog.S7_ParentID = org.PK;
			enterpriseSavedObjectOldControllerLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 25);

			StmActivityLog enterpriseMissingControllerIdLog = newFactory.New<StmActivityLog>();
			enterpriseMissingControllerIdLog.S7_EnterpriseActivity = true;
			enterpriseMissingControllerIdLog.S7_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			enterpriseMissingControllerIdLog.S7_ParentID = ZGuid.NewZGuid();
			enterpriseMissingControllerIdLog.S7_OpenDateTimeUtc = new ZDateTime(2006, 1, 26);

			staff.ActivityLogsForUser.Add(enterpriseSavedObjectLog);
			staff.ActivityLogsForUser.Add(enterpriseSavedDeletedObjectLog);
			staff.ActivityLogsForUser.Add(enterpriseNonSavedObjectLog);
			staff.ActivityLogsForUser.Add(externalLog);
			staff.ActivityLogsForUser.Add(enterpriseSavedObjectOldControllerLog);
			staff.ActivityLogsForUser.Add(enterpriseMissingControllerIdLog);

			using (GlbStaffForm form = NewGlbStaffForm(staff))
			{
				form.Show();
				form.TopLevelTabControl.SelectedTab = form.ActivityLogTabPage;

				staff.ActivityLogsForUser.Sort(StmActivityLogSchema.S7_OpenDateTimeUtc.Name, System.ComponentModel.ListSortDirection.Ascending);

				form.ActivityLogGrid.Select(0);
				form.OpenRelatedObjectButton.PerformClick();
				AssertEquals(typeof(ZOrganisationsForm), ZFormModaliser.ActiveForm.GetType());
				AssertEquals(org.PK, ((BusinessObject)((ZForm)ZFormModaliser.ActiveForm).BusinessEntity).PK);
				((ZForm)ZFormModaliser.ActiveForm).Dispose();
				AssertEquals("Open Organization Record", form.OpenRelatedObjectButton.Text);

				form.ActivityLogGrid.UnSelect(0);
				form.ActivityLogGrid.Select(1);
				form.ActivityLogGrid.ListManager.Position = 1;
				form.OpenRelatedObjectButton.PerformClick();
				AssertEquals("This activity was performed on a record that no longer exists.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.ActiveForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Open Organization Record", form.OpenRelatedObjectButton.Text);

				form.ActivityLogGrid.UnSelect(1);
				form.ActivityLogGrid.Select(2);
				form.ActivityLogGrid.ListManager.Position = 2;
				form.OpenRelatedObjectButton.PerformClick();
				AssertEquals("No further details are available to be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.ActiveForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Open Record", form.OpenRelatedObjectButton.Text);

				form.ActivityLogGrid.UnSelect(2);
				form.ActivityLogGrid.Select(3);
				form.ActivityLogGrid.ListManager.Position = 3;
				form.OpenRelatedObjectButton.PerformClick();
				AssertEquals($"This activity is for an external non-{Core.Constants.ProductName} activity and so cannot be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.ActiveForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Open Record", form.OpenRelatedObjectButton.Text);

				form.ActivityLogGrid.UnSelect(4);
				form.ActivityLogGrid.Select(5);
				form.ActivityLogGrid.ListManager.Position = 5;
				form.OpenRelatedObjectButton.PerformClick();
				AssertEquals("No further details are available to be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.ActiveForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Open Record", form.OpenRelatedObjectButton.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestNoException_OpeningSecurityRightsTabPage_ForNewStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			using (GlbStaffForm testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.GlbStaffTabControl.SelectTab("SecurityRightsTabPage");
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestNoException_OpeningSecurityRightsTabPage_ForNewStaffWithAttachedGroup()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "AGI";
			Factory.Save();

			GlbStaff staff = Factory.New<GlbStaff>();
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staff.ActiveGroups.AddFromDatabase(group.PK);
				staffForm.GlbStaffTabControl.SelectTab("SecurityRightsTabPage");
				staffForm.SecurityRightsTabPage.NotifyBindingOrShowing();
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));
			}
		}

		public void TestEmailControlsReadOnlyWhenSecurityRightDenied()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.StaffOwnEmail.IsAllowed = false;

				using (var staffForm = NewGlbStaffForm(staff))
				{
					staffForm.Show();

					Assert("The Email grid should be read only", staffForm.EmailAddressesGrid.ReadOnly);
					Assert("The check box of publish email should be read only", staffForm.GS_PublishEmailCheckBox.ReadOnly);
				}

				Env.Security.StaffOwnEmail.IsAllowed = true;

				using (var staffForm = NewGlbStaffForm(staff))
				{
					staffForm.Show();

					Assert("The Email grid should not be read only", !staffForm.EmailAddressesGrid.ReadOnly);
					Assert("The check box of publish email should not be read only", !staffForm.GS_PublishEmailCheckBox.ReadOnly);
				}
			}
		}

		public void TestEnablingSearchControls()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.StaffEdit.IsAllowed = false;

				using (var staffForm = NewGlbStaffForm(staff))
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;

					Assert("The Department find box should be accessible", staffForm.SecDepFindBox.Enabled);
					Assert("The Branch find box should be accessible", staffForm.SecBranchFindBox.Enabled);
					Assert("The ZTree should be accessible", staffForm.SecurityPermissionsTreeView.Enabled);
				}
			}
		}

		public void TestDispose()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			string prevErrorMessage;
			using (GlbStaffForm testForm = NewGlbStaffForm(staff))
			{
				testForm.DisableNewAction();

				// Simulate the behaviour of StartupShowStaffForm.ShowStaffForm
				// which shows the form modally and disposes it AFTER the window handle has been destroyed.
				testForm.Load += (sender, e) =>
				{
					testForm.BeginInvoke(new MethodInvoker(delegate
					{
						testForm.GlbStaffTabControl.SelectTab("SecurityRightsTabPage");
					}));
					testForm.BeginInvoke(new MethodInvoker(delegate
					{
						testForm.ButtonsUserControl.CloseButton.PerformClick();
					}));
				};
				testForm.ShowDialog();
				prevErrorMessage = ErrorReporter.LastMessageReported;
			}
			AssertEquals("No error reported during dispose", prevErrorMessage, ErrorReporter.LastMessageReported);
		}

		/// <summary>
		/// This test reproduces a bug as per Issue 00028029 with a GDI+ External Exception occurring.
		/// 
		/// The problem was caused by:
		/// a) having an image loaded in memory
		/// b) trying to load an image which had a file-lock on it.
		/// 
		/// </summary>
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadingImageWithLockDoesntThrowGDIError()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var sig1 = resourceRetriever.SaveResourceToFile("sample_signature.PNG");
				var sig2 = resourceRetriever.SaveResourceToFile("sample_signature2.PNG");

				Assert("PRE: Image files are unique", sig1 != sig2);

				using (GlbStaffForm testForm = NewGlbStaffForm(Factory.NewWithValidTestData<GlbStaff>()))
				using (Bitmap sig1Image = new Bitmap(sig1))
				{
					testForm.Staff.SignatureImage = sig1Image;

					testForm.Show();
					testForm.TopLevelTabControl.SelectedTab = testForm.PasswordTabPage;
					AssertNotNull("PRE: The image should have been loaded", testForm.Staff.SignatureImage);

					using (FileStream f = new FileStream(sig2, FileMode.Open, FileAccess.Read))
					{
						int a = f.ReadByte(); // lock Signature 2

						try
						{
							Image.FromStream(f);
						}
						catch (OutOfMemoryException e)
						{
							object x = e; // ignore exception - we expect this because due to a .NET bug
						}

						AssertNotNull("The image should NOT be null", testForm.Staff.SignatureImage);
						using (MemoryStream stream = new MemoryStream())
						{
							testForm.Staff.SignatureImage.Save(stream, ImageFormat.Bmp);
						}
					}
				}
			}
		}

		public void TestShowPasswordDialogs()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff staff = Factory.New<GlbStaff>();
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				staffForm.ChangePasswordButton_Click(null, EventArgs.Empty);
				AssertEquals(typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestShowPasswordControlsWhenCurrentUserIsController()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = true;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = GetFormToBash())
			{
				testForm.Show();

				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", true, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", true, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Password and Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		public void TestHidePasswordControlsWhenOIDCIsEnabled()
		{
			var hrStaff = Factory.NewWithValidTestData<GlbStaff>();
			hrStaff.GS_IsController = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = true;

			Factory.Save();

			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOIDCConfig()))
			using (Env.SetTemporaryUserContext(new UserContext(hrStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = NewGlbStaffForm(staff))
			{
				testForm.Show();
				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", false, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", false, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		public void TestShowPasswordControlsWhenOIDCIsDisabled()
		{
			var hrStaff = Factory.NewWithValidTestData<GlbStaff>();
			hrStaff.GS_IsController = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = true;

			Factory.Save();

			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOIDCConfig(false)))
			using (Env.SetTemporaryUserContext(new UserContext(hrStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = new GlbStaffFormForTesting(staff))
			{
				testForm.Show();
				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", true, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", true, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Password and Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		OIDCConfig GetOIDCConfig(bool isOIDCEnabled = true)
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = isOIDCEnabled,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = AuthorityUrl,
				ClientIdentifier = ClientId,
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;
			return oidcConfig;
		}

		public void TestAUCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Australia);
		}

		public void TestUSCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.UnitedStates);
		}

		public void TestPRCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.PuertoRico);
		}

		public void TestSGCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Singapore);
		}

		public void TestITCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Italy);
		}

		public void TestTWCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Taiwan);
		}

		public void TestBRCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Brazil);
		}

		public void TestPLCredentialsVisibility()
		{
			AssertCredentialsVisibility(Core.Constants.CountryCodes.Poland);
		}

		public void TestShowErrorMessageWhenActivityLogForUserFilterProviderHasError()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ILR";
			staff.ActivityLogForUserFilterProvider.ActivityLogFilterDateFrom = ZDateTime.Invalid;
			using (var staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.FindButton_Click(null, EventArgs.Empty);
				AssertEquals("There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPhoneNumberControls()
		{
			var staff = Factory.New<GlbStaff>();
			using (var staffForm = new GlbStaffFormForTesting(staff))
			{
				Assert(staffForm.HomePhoneNumberControl.ShowToolTip);
				Assert(staffForm.HomePhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.HomePhoneNumberControl.ShowLocalNumberLabel);
				Assert(staffForm.HomePhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.HomePhoneNumberControl.EnableValidStateColor);

				Assert(staffForm.WorkPhoneNumberControl.ShowToolTip);
				Assert(staffForm.WorkPhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.WorkPhoneNumberControl.ShowLocalNumberLabel);
				Assert(staffForm.WorkPhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.WorkPhoneNumberControl.EnableValidStateColor);

				Assert(!staffForm.WorkExtensionNumberControl.ShowToolTip);
				Assert(!staffForm.WorkExtensionNumberControl.ShowDiallerControl);
				Assert(!staffForm.WorkExtensionNumberControl.ShowLocalNumberLabel);
				Assert(staffForm.WorkExtensionNumberControl.ShowPublishedCheckBox);
				Assert(!staffForm.WorkExtensionNumberControl.EnableValidStateColor);

				Assert(staffForm.MobilePhoneNumberControl.ShowToolTip);
				Assert(staffForm.MobilePhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.MobilePhoneNumberControl.ShowLocalNumberLabel);
				Assert(staffForm.MobilePhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.MobilePhoneNumberControl.EnableValidStateColor);

				Assert(staffForm.FaxNumberControl.ShowToolTip);
				Assert(!staffForm.FaxNumberControl.ShowDiallerControl);
				Assert(!staffForm.FaxNumberControl.ShowLocalNumberLabel);
				Assert(staffForm.FaxNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.FaxNumberControl.EnableValidStateColor);

				Assert(staffForm.NextOfKinHomePhoneNumberControl.ShowToolTip);
				Assert(staffForm.NextOfKinHomePhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.NextOfKinHomePhoneNumberControl.ShowLocalNumberLabel);
				Assert(!staffForm.NextOfKinHomePhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.NextOfKinHomePhoneNumberControl.EnableValidStateColor);

				Assert(staffForm.NextOfKinWorkPhoneNumberControl.ShowToolTip);
				Assert(staffForm.NextOfKinWorkPhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.NextOfKinWorkPhoneNumberControl.ShowLocalNumberLabel);
				Assert(!staffForm.NextOfKinWorkPhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.NextOfKinWorkPhoneNumberControl.EnableValidStateColor);

				Assert(staffForm.EmergencyHomePhoneNumberControl.ShowToolTip);
				Assert(staffForm.EmergencyHomePhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.EmergencyHomePhoneNumberControl.ShowLocalNumberLabel);
				Assert(!staffForm.EmergencyHomePhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.EmergencyHomePhoneNumberControl.EnableValidStateColor);

				Assert(staffForm.EmergencyWorkPhoneNumberControl.ShowToolTip);
				Assert(staffForm.EmergencyWorkPhoneNumberControl.ShowDiallerControl);
				Assert(!staffForm.EmergencyWorkPhoneNumberControl.ShowLocalNumberLabel);
				Assert(!staffForm.EmergencyWorkPhoneNumberControl.ShowPublishedCheckBox);
				Assert(staffForm.EmergencyWorkPhoneNumberControl.EnableValidStateColor);
			}
		}

		[TestDate(2016, 9, 14, 5, 0, 0)]
		public void TestGSLockoutDateTimeLocal()
		{
			var plus10TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			plus10TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 600;
			var plus10TimeZoneUnloco = Factory.NewWithValidTestData<RefUNLOCO>();
			plus10TimeZoneUnloco.RL_R3 = plus10TimeZoneSet.PK;
			plus10TimeZoneUnloco.RL_Code = "TESTX";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "TESTX";
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			staff.GS_PER = person.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMonths(1); // to ensure it is locked out today

				using (GlbStaffForm testForm = NewGlbStaffForm(staff))
				{
					testForm.Show();
					Application.DoEvents();
					testForm.GlbStaffTabControl.SelectTab("PasswordTabPage");
					AssertEquals("Should return lock out time in Local time", new ZDateTime(2016, 10, 14, 15, 0, 0), staff.GS_LockoutDateTimeLocal);
					AssertEquals("Value of GS_LockoutDateTimeBoundDate should be equal with GS_LockoutDateTimeLocal", testForm.GS_LockoutDateTimeBoundDate.DateTimeValue, staff.GS_LockoutDateTimeLocal);
				}
			}
		}

		public void TestTabPageGroupsWithGroupsFromDifferentDomain_ShowsWarningUponLoadingForm()
		{
			var domainCredentials1 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials1.DomainName = "domain1";
			domainCredentials1.IsDefaultDomain = true;
			var domainCredentials2 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials2.DomainName = "domain2";
			domainCredentials2.IsDefaultDomain = false;

			var adRegistry = ObjectFactory.Get<IADRegistry>();
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domainCredentials1, domainCredentials2 };
			adRegistry.IsIntegrationEnabled = true;
			adRegistry.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_DomainName = "domain1";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_DomainName = "domain2";

			var link = Factory.New<GlbGroupLink>();
			link.GK_GS = staff.PK;
			link.GK_GG = group.PK;

			using (var form = NewGlbStaffForm(staff))
			{
				form.GlbStaffTabControl.SelectedTab = form.GroupsTabPage;
				form.Show();
				var tabPages = form.Controls.Find("GroupsTabPage", true);
				var grid = ((ZModuleButtonGrid)tabPages[0].Controls.Find("MembersModuleButtonGrid", true)[0]).InnerGrid;
				var groupFromGrid = grid.List.Cast<GlbGroup>().First(g => g.PK == group.PK);

				Assert(groupFromGrid != null);
				Assert("Group should show warning in list", groupFromGrid.HasRowWarnings);
				AssertEquals("Message should be correct", "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.", groupFromGrid.RowWarnings.First().Message);

				form.Close();
			}
		}

		#region Designer bugs

		public void TestTabPageOrder()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (var form = NewGlbStaffForm(staff))
			{
				form.Show();
				var staffCredentialsPlugIn = form.GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffCredentialsPlugIn);
				AssertEquals($"{form.DetailsTabPage.CaptionResourceString.Caption}, {form.GroupsTabPage.CaptionResourceString.Caption}, {staffCredentialsPlugIn.TabPage.CaptionResourceString.Caption}, {form.SecurityRightsTabPage.CaptionResourceString.Caption}, {form.PasswordTabPage.CaptionResourceString.Caption}, {form.HRTabPage.CaptionResourceString.Caption}, {form.WorkflowTabPage.CaptionResourceString.Caption}, Doc Data, eDocs, Notes, Logs", new ZStringBuilder(form.GlbStaffTabControl.TabPages.OfType<ZTabPage>().Select(x => x.CaptionResourceString.Caption)).ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		public void TestStaffNumberRangesPlugIn()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (var form = NewGlbStaffForm(staff))
			{
				form.Show();
				var staffCredentialsPlugIn = form.GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffNumberRangesPlugIn);
				AssertNotNull(staffCredentialsPlugIn);
				AssertEquals(false, staffCredentialsPlugIn.Enabled);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			using (var form = NewGlbStaffForm(staff))
			{
				form.Show();
				var staffCredentialsPlugIn = form.GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffNumberRangesPlugIn);
				AssertNotNull(staffCredentialsPlugIn);
				AssertEquals(true, staffCredentialsPlugIn.Enabled);
			}
		}

		public void TestVanishingControlsFromDesignerBug()
		{
			using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				AssertNotNull("User Signature Image Selection Control should not be null.", staffForm.SignatureImageSelectionControl);
				AssertNotNull("User Signature Image Selection Control should be bound.", staffForm.SignatureImageSelectionControl.BindingContext);

				AssertNotNull("Security Tree View should not be null.", staffForm.SecurityPermissionsTreeView);
				AssertNotNull("Security Tree View should be bound.", staffForm.SecurityPermissionsTreeView.BindingContext);
				AssertEquals("Security Tree View should be anchored", (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
					| System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right),
					staffForm.SecurityPermissionsTreeView.Anchor);
			}
		}

		#endregion

		#region Security Permissions

		public void TestStaffDetailsPopupButtonEnabled()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			Env.Security.StaffDetails.IsAllowed = true;
			Factory.Save();
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				AssertEquals("Default setting should be set to true", true, Env.Security.StaffDetails.IsAllowed);
				AssertEquals("NationalityCodeFindBox should be enabled", true, staffForm.NationalityCodeFindBox.PopupButton.Enabled);
				AssertEquals("GS_CountryFindBox should be enabled", true, staffForm.GS_CountryFindBox.PopupButton.Enabled);
				AssertEquals("HomeBranchGuidFindBox should be enabled", true, staffForm.HomeBranchGuidFindBox.PopupButton.Enabled);
				AssertEquals("HomeDepartmentFindBox should be enabled", true, staffForm.HomeDepartmentFindBox.PopupButton.Enabled);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.StaffDetails.IsAllowed = false;
				Factory.Save();
				using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
				{
					staffForm.Show();
					AssertEquals("Default setting should not be set to false", false, Env.Security.StaffDetails.IsAllowed);
					AssertEquals("NationalityCodeFindBox should not be enabled", false, staffForm.NationalityCodeFindBox.PopupButton.Enabled);
					AssertEquals("GS_CountryFindBox should not be enabled", false, staffForm.GS_CountryFindBox.PopupButton.Enabled);
					AssertEquals("HomeBranchGuidFindBox should not be enabled", false, staffForm.HomeBranchGuidFindBox.PopupButton.Enabled);
					AssertEquals("HomeDepartmentFindBox should not be enabled", false, staffForm.HomeDepartmentFindBox.PopupButton.Enabled);
				}
			}
		}

		public void TestLoginFieldAndAttributeIsDisabled()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.StaffAllLogin.IsAllowed = false;
			Env.Security.StaffLoginAttributesAll.IsAllowed = false;
			Factory.Save();
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				AssertEquals("Default setting should be set to false", false, Env.Security.StaffAllLogin.IsAllowed);
				AssertEquals("Security Right didn't disable login textbox", false, staffForm.GS_LoginNameBoundText.ReadOnly);
				AssertEquals("Security Right didn't disable active checkbox", false, staffForm.GS_IsActiveBoundCheck.ReadOnly);
				AssertEquals("Security Right didn't disable IsCLU checkbox", false, staffForm.IsCLUserCheckBox.ReadOnly);
				AssertEquals("Security Right didn't disable device checkbox", false, staffForm.IsDeviceOnlyCheckBox.ReadOnly);
			}

			Env.Security.StaffAllLogin.IsAllowed = true;
			Env.Security.StaffLoginAttributesAll.IsAllowed = true;
			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				AssertEquals("Security Right should allow login textbox to be editable", false, staffForm.GS_LoginNameBoundText.ReadOnly);
				AssertEquals("Security Right didn't disable active checkbox", false, staffForm.GS_IsActiveBoundCheck.ReadOnly);
				AssertEquals("Security Right didn't disable IsCLU checkbox", false, staffForm.IsCLUserCheckBox.ReadOnly);
				AssertEquals("Security Right didn't disable device checkbox", false, staffForm.IsDeviceOnlyCheckBox.ReadOnly);
			}

			Env.Security.StaffOwnLogin.IsAllowed = true;
			Env.Security.StaffLoginAttributesOwn.IsAllowed = true;
			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				AssertEquals("Security Right should allow login textbox to be editable", false, staffForm.GS_LoginNameBoundText.ReadOnly);
				AssertEquals("Security Right didn't disable active checkbox", false, staffForm.GS_IsActiveBoundCheck.ReadOnly);
				AssertEquals("Security Right didn't disable IsCLU checkbox", false, staffForm.IsCLUserCheckBox.ReadOnly);
				AssertEquals("Security Right didn't disable device checkbox", false, staffForm.IsDeviceOnlyCheckBox.ReadOnly);
			}

			Env.Security.StaffOwnLogin.IsAllowed = false;
			Env.Security.StaffLoginAttributesOwn.IsAllowed = false;
			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				AssertEquals("Security Right should allow login textbox to be editable", false, staffForm.GS_LoginNameBoundText.ReadOnly);
				AssertEquals("Security Right didn't disable active checkbox", false, staffForm.GS_IsActiveBoundCheck.ReadOnly);
				AssertEquals("Security Right didn't disable IsCLU checkbox", false, staffForm.IsCLUserCheckBox.ReadOnly);
				AssertEquals("Security Right didn't disable device checkbox", false, staffForm.IsDeviceOnlyCheckBox.ReadOnly);
			}

			try
			{
				staff.Delete();
				Factory.Save();
			}
			catch { }
		}

		public void TestAddAndRemovesAssociatedGroupPermissions()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			GlbGroup groupAddedAfter = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup groupAddedBefore = Factory.NewWithValidTestData<GlbGroup>();

			BusinessObject[] operationsCodeCollection1 = groupAddedAfter.SecurityPermissions.Find(new ZQuery(GlbSecuritySchema.GU_SecurityRight, Env.Security.Operations.Code));
			AssertEquals("Group Added After should have one security permission for operations", 1, operationsCodeCollection1.Length);
			GlbSecurity securityOnGroupAddedAfter = (GlbSecurity)operationsCodeCollection1[0];

			BusinessObject[] operationsCodeCollection2 = groupAddedBefore.SecurityPermissions.Find(new ZQuery(GlbSecuritySchema.GU_SecurityRight, Env.Security.Operations.Code));
			AssertEquals("Group Added Before should have one security permission for operations", 1, operationsCodeCollection1.Length);
			GlbSecurity securityOnGroupAddedBefore = (GlbSecurity)operationsCodeCollection2[0];

			staff.Groups.Add(groupAddedBefore);

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staff.Groups.Add(groupAddedAfter);
				Factory.Save();

				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));

				Assert("Staff form should display security from group added Before", staff.GroupSecurityPermissionsCollectionForBinding.Contains(securityOnGroupAddedBefore.PK));
				Assert("Staff form should display security from group added After", staff.GroupSecurityPermissionsCollectionForBinding.Contains(securityOnGroupAddedAfter.PK));
			}
		}

		public void TestLazyBillingStuffIsInTheTree()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));

				// Testing that lazy loaded security items appear in the security tree.
				AssertEquals(true, staffForm.SecurityPermissionsTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.SelectRecursive(t => t.Nodes.Cast<TreeNode>())).Any(n => n.Text == "View Global Charge Details"));
				AssertEquals(true, staffForm.SecurityPermissionsTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.SelectRecursive(t => t.Nodes.Cast<TreeNode>())).Any(n => n.Text == "Reverse WIP/ACR Transaction"));
				AssertEquals(true, staffForm.SecurityPermissionsTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.SelectRecursive(t => t.Nodes.Cast<TreeNode>())).Any(n => n.Text == "Update Compliance Sub Type and/or Number"));
			}
		}

		public void TestWorkflowAndPAVEStuffIsInTheTree()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));

				// Testing that lazy loaded security items appear in the security tree.
				AssertEquals(true, staffForm.SecurityPermissionsTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.SelectRecursive(t => t.Nodes.Cast<TreeNode>())).Any(n => n.Text == "Mark Resources as Constrained"));
				AssertEquals(true, staffForm.SecurityPermissionsTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.SelectRecursive(t => t.Nodes.Cast<TreeNode>())).Any(n => n.Text == "Edit Actual Date / Duration"));
			}
		}

		public void TestSecurityIsSetForStaffSecurityPermissionsCollection()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			Factory.Save();

			using (GlbStaffForm form = NewGlbStaffForm(staff))
			{
				form.Show();
				form.GlbStaffTabControl.SelectedTab = form.SecurityRightsTabPage;
				AssertEquals("Security should be set on staff.StaffSecurityPermissionsCollection.", form.fStaffSecurity, staff.StaffSecurityPermissionsCollection.Security);
				bool foundKeyWithItemGuid = false;
				foreach (CheckpointLookupKey key in staff.StaffSecurityPermissionsCollection.Security.CheckPointLookUpTable_ForTest.Keys)
				{
					if (key.ItemGuid != Guid.Empty)
					{
						foundKeyWithItemGuid = true;
						break;
					}
				}
				AssertEquals("Security should be populated.", true, foundKeyWithItemGuid);
			}
		}

		[RequiresSTA]
		public void TestSecurityPermissionsTreeView_AfterSelect()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				GlbSecurity securityRecordParent = Factory.New<GlbSecurity>();
				securityRecordParent.GU_SecurityItemIsAllowed = true;
				securityRecordParent.GU_SecurityRight = staffForm.SecurityPermissionsTreeView.Nodes[0].Text;
				securityRecordParent.GU_GS = staff.PK;
				staff.StaffSecurityPermissionsCollection.Add(securityRecordParent);

				var schedulesSecurityNode = staffForm.SecurityPermissionsTreeView
					.Nodes.Cast<TreeNode>().First(node => node.Text == "Operate").Nodes.Cast<TreeNode>().First(node => node.Text == "Schedules");
				AssertNotNull("We should be able to find the 'Schedules' Security Node within the 'Operate' Node, but instead we could not! Bad!", schedulesSecurityNode);

				GlbSecurity securityRecordChild = Factory.New<GlbSecurity>();
				securityRecordChild.GU_SecurityItemIsAllowed = true;
				securityRecordChild.GU_SecurityRight = schedulesSecurityNode.Text;
				securityRecordChild.GU_GS = staff.PK;
				staff.StaffSecurityPermissionsCollection.Add(securityRecordChild);

				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.SecurityPermissionsTreeView.HideSelection = false;
				staffForm.SecurityPermissionsTreeView.SelectedNode = staffForm.SecurityPermissionsTreeView.Nodes[0];
				FireAfterSelect(staffForm.SecurityPermissionsTreeView.Nodes[0]);
				AssertEquals("First node should be selected", staffForm.SecurityPermissionsTreeView.Nodes[0], staffForm.SecurityPermissionsTreeView.SelectedNode);
				AssertEquals("Security function name should be the same as selected security node", staffForm.EffSecRightsBox.Text, staffForm.EffectiveSecurityRightsMessage + staffForm.SecurityPermissionsTreeView.SelectedNode.Text);

				staffForm.SecurityPermissionsTreeView.SelectedNode = staffForm.SecurityPermissionsTreeView.SelectedNode.Nodes[0].Nodes[0];
				FireAfterSelect(schedulesSecurityNode);
				string securityPathName = staffForm.EffectiveSecurityRightsMessage + staffForm.SecurityPermissionsTreeView.SelectedNode.Parent.Text + "|" + staffForm.SecurityPermissionsTreeView.SelectedNode.Text;
				AssertEquals("Security function name should be", securityPathName, staffForm.EffSecRightsBox.Text);

				AssertEquals("One relevant security permission should be shown", 1, staffForm.Staff.StaffSecurityPermissionsView.Count);

				GlbSecurity securityRecordSecondChild = Factory.New<GlbSecurity>();
				securityRecordSecondChild.GU_SecurityItemIsAllowed = true;
				securityRecordSecondChild.GU_SecurityRight = schedulesSecurityNode.Text;
				securityRecordSecondChild.GU_GS = staff.PK;
				securityRecordSecondChild.GU_GB = Env.CurrentBranch.PK;
				staff.StaffSecurityPermissionsCollection.Add(securityRecordSecondChild);

				FireAfterSelect(schedulesSecurityNode);
				AssertEquals("Two relevant security permissions should be shown", 2, staffForm.Staff.StaffSecurityPermissionsView.Count);
			}
		}

		void FireAfterSelect(TreeNode treeNode)
		{
			TreeView treeView = treeNode.TreeView;
			treeView.GetType().GetMethod("OnAfterSelect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				.Invoke(treeView, new object[] { new TreeViewEventArgs(treeNode, TreeViewAction.ByMouse) });
		}

		#region Refresh Allowed Organisations And Warehouses Control

		public void TestRefreshAllowedOrgsAndWarehousesSecurityGrid()
		{
			var staff = Factory.New<GlbStaff>();

			var security1 = CreateSecurityItem(staff, Factory.NewWithValidTestData<OrgHeader>(), GlbSecurity.AllowedPrincipalsSecurityRightName);
			var security2 = CreateSecurityItem(staff, Factory.NewWithValidTestData<OrgHeader>(), GlbSecurity.AllowedClientsSecurityRightName);
			var security3 = CreateSecurityItem(staff, Factory.NewWithValidTestData<OrgHeader>(), GlbSecurity.AllowedWarehousesSecurityRightName); //bind to OrgHeader because no access to WhsWarehouse in there

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;

				// Could create correct Item (WhsWarehouse) only there
				staffForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedWarehouses));
				security3.GU_ItemGUID = (Factory.New(staff.SecurityAllowedOrgsAndWarehousesView.TypeOfElements)).PK;

				staffForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.AgencyPrincipalAccess));
				AssertEquals("Collection of allowed principals should contain only one security item", 1, staff.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed principals should contain the correct security item", security1, staff.SecurityAllowedOrgsAndWarehousesView);

				staffForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedClients));
				AssertEquals("Collection of allowed clients should contain only one security item", 1, staff.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed clients should contain the correct security item", security2, staff.SecurityAllowedOrgsAndWarehousesView);

				staffForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedWarehouses));
				AssertEquals("Collection of allowed warehouses should contain only one security item", 1, staff.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed warehouses should contain the correct security item", security3, staff.SecurityAllowedOrgsAndWarehousesView);
			}
		}

		ZSecurityPointNode GetSecurityPointNode(SecurityCheckpoint securityCheckPoint)
		{
			return new ZSecurityPointNode(securityCheckPoint.DisplayText, securityCheckPoint);
		}

		GlbSecurity CreateSecurityItem(GlbStaff staff, BusinessObject bizO, ZString securityRight)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_ItemGUID = bizO.PK;
			security.GU_GS = staff.PK;
			security.GU_SecurityRight = securityRight;
			security.GU_SecurityItemIsAllowed = true;
			return security;
		}

		#endregion

		#endregion

		#region Sales Commission

		public void TestSalesCommissionTab()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsSalesRep = ZBool.False;
			using (GlbStaffForm form = NewGlbStaffForm(staff))
			{
				AssertSalesCommissionTabPageAvailability(form, false);

				staff.GS_IsSalesRep = ZBool.True;
				AssertSalesCommissionTabPageAvailability(form, true);

				staff.GS_IsSalesRep = ZBool.False;
				AssertSalesCommissionTabPageAvailability(form, false);
			}
		}

		public void TestSalesCommissionTab_ViewSecurityRights()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsSalesRep = true;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				Env.Security.StaffViewSales.IsAllowed = false;
				Env.Security.StaffViewOwnSales.IsAllowed = false;
				using (var form = NewGlbStaffForm(staff))
				{
					AssertSalesCommissionTabPageAvailability(form, false);
				}

				Env.Security.StaffViewOwnSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(staff))
				{
					AssertSalesCommissionTabPageAvailability(form, true);
				}

				var otherStaff = Factory.New<GlbStaff>();
				otherStaff.GS_IsSalesRep = true;
				using (var form = NewGlbStaffForm(otherStaff))
				{
					AssertSalesCommissionTabPageAvailability(form, false);
				}

				Env.Security.StaffViewSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(otherStaff))
				{
					AssertSalesCommissionTabPageAvailability(form, true);
				}

				Env.Security.StaffViewSales.IsAllowed = false;
				Env.Security.StaffViewOwnSales.IsAllowed = false;

				Env.Security.StaffOwnSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(staff))
				{
					AssertSalesCommissionTabPageAvailability(form, true);
				}

				using (var form = NewGlbStaffForm(otherStaff))
				{
					AssertSalesCommissionTabPageAvailability(form, false);
				}

				Env.Security.StaffSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(otherStaff))
				{
					AssertSalesCommissionTabPageAvailability(form, true);
				}
			}
		}

		public void TestSalesCommisisionTab_EditSecurityRights()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsSalesRep = true;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				Env.Security.StaffViewSales.IsAllowed = true;
				Env.Security.StaffViewOwnSales.IsAllowed = true;

				Env.Security.StaffSales.IsAllowed = false;
				Env.Security.StaffOwnSales.IsAllowed = false;
				using (var form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertSalesCommissionTabPageReadOnly(form, true);
				}

				Env.Security.StaffOwnSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertSalesCommissionTabPageReadOnly(form, false);
				}

				var otherStaff = Factory.New<GlbStaff>();
				otherStaff.GS_IsSalesRep = true;
				using (var form = NewGlbStaffForm(otherStaff))
				{
					form.Show();
					AssertSalesCommissionTabPageReadOnly(form, true);
				}

				Env.Security.StaffSales.IsAllowed = true;
				using (var form = NewGlbStaffForm(otherStaff))
				{
					form.Show();
					AssertSalesCommissionTabPageReadOnly(form, false);
				}

				Env.Security.StaffOwnSales.IsAllowed = false;
				using (var form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertSalesCommissionTabPageReadOnly(form, false);
				}
			}
		}

		void AssertSalesCommissionTabPageAvailability(GlbStaffForm form, bool expectedResult)
		{
			AssertEquals("Sales Tab Page Availability", expectedResult, form.GlbStaffTabControl.Controls.Contains(form.SalesTabPage));
		}

		void AssertSalesCommissionTabPageReadOnly(GlbStaffForm form, bool expectedResult)
		{
			form.GlbStaffTabControl.SelectTab(form.SalesTabPage);
			AssertEquals("Sales Tab Page ReadOnly", expectedResult, form.SalesHintLabel.Enabled);
		}

		#endregion

		#region Duplicate Security Checkpoints test

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[RequiresSTA]
		public void TestDuplicateSecurityCheckpointsDoNotExist()
		{
			string result = "";
			foreach (string countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				Dictionary<CheckpointLookupKey, ZSecurityPointNode> uniqueSecurityCheckpoints = new Dictionary<CheckpointLookupKey, ZSecurityPointNode>();
				GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, Env.CurrentUser.PK));
				using (GlbStaffForm form = NewGlbStaffForm(staff))
				{
					Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
					foreach (TreeNode node in form.SecurityPermissionsTreeView.Nodes)
					{
						result = result + CheckDuplicatedNodes(node, uniqueSecurityCheckpoints);
					}
				}
			}

			AssertEquals("Duplicate SecurityCheckpoints", "", result);
		}

		string CheckIfNodeIsDuplicated(TreeNode node, Dictionary<CheckpointLookupKey, ZSecurityPointNode> uniqueSecurityCheckpoints)
		{
			string result = "";

			ZSecurityPointNode securityNode = node as ZSecurityPointNode;
			if (securityNode != null)
			{
				if (securityNode.Checkpoint != null)
				{
					if (uniqueSecurityCheckpoints.ContainsKey(securityNode.Checkpoint.LookupKey))
					{
						ZSecurityPointNode anotherNode = uniqueSecurityCheckpoints[securityNode.Checkpoint.LookupKey];
						Assert("Test logic error: comparing a node with itself", !ReferenceEquals(anotherNode, node));
						result = "Country : " + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + ", Security Check Point: '" + securityNode.Checkpoint.ToString() + "' is used in " +
							"'" + securityNode.FullPath + "' and '" + anotherNode.FullPath + "'" +
							System.Environment.NewLine;
					}
					else
					{
						uniqueSecurityCheckpoints.Add(securityNode.Checkpoint.LookupKey, securityNode);
					}
				}
				else
				{
					Fail("The SecurityCheckpoint of this node is Null: " + securityNode.FullPath);
				}
			}
			return result;
		}

		string CheckDuplicatedNodes(TreeNode tNode, Dictionary<CheckpointLookupKey, ZSecurityPointNode> uniqueSecurityCheckpoints)
		{
			string result = "", newResult = "";

			if (tNode != null)
			{
				try
				{
					newResult = CheckIfNodeIsDuplicated(tNode, uniqueSecurityCheckpoints);
				}
				catch (NullReferenceException e)
				{
					e.Source = "Null Reference. Current Node : " + tNode.Text +
						", Parent: " + ((tNode.Parent == null) ? "Null" :
						tNode.Parent.Text + ", CheckPoint : " +
						(tNode as ZSecurityPointNode).Checkpoint == null ? "Null" : "Ok");
					throw;
				}

				if (!string.IsNullOrEmpty(newResult))
				{
					result = result + newResult; //When Parent is duplicated, all children will be duplicated as well. So there is no need to check them
				}
				else
				{
					foreach (TreeNode child in tNode.Nodes)
					{
						result = result + CheckDuplicatedNodes(child, uniqueSecurityCheckpoints);
					}
				}
			}
			return result;
		}

		#endregion

		#region Form Setup / Loading

		public void TestSetEmploymentBasisHint()
		{
			using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.HRTabPage;
				staffForm.HRTabControl.SelectedTab = staffForm.EmploymentTabPage;
				ZString pathToRegistryItem = SystemDataRegistry.Instance.StaffEmploymentTypes.Category + "/" + SystemDataRegistry.Instance.StaffEmploymentTypes.Caption;
				AssertEquals("Tool tip should be", GlbStaffForm.EmploymentBasisRegistryHint + pathToRegistryItem, staffForm.FormToolTip.GetToolTip(staffForm.EmploymentBasisDropEdit));
			}
		}

		public void TestFormCaption()
		{
			using (var staffForm = (GlbStaffForm)GetFormToBashCore())
			{
				AssertEquals("Form caption is Staff", "Staff", staffForm.FormCaption);
			}

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "testUser";

			using (var staffForm = NewGlbStaffForm(staff))
			{
				AssertEquals("Form caption is Staff with fullName", "Staff " + staff.GS_FullName, staffForm.FormCaption);
			}
		}

		public void TestCreateStaffForm()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				Assert("SecurityPermissionsTreeView should be populated", staffForm.SecurityPermissionsTreeView.Nodes.Count != 0);
				AssertEquals("Staff member should be used for staff form", staff, staffForm.Staff);

				AssertEquals("Security Branch PK should be current branch", Env.CurrentBranch.PK, staffForm.fStaffSecurity.BranchPK);
				AssertEquals("Security Department PK should be current branch", Env.CurrentDepartment.PK, staffForm.fStaffSecurity.DepartmentPK);
				AssertEquals("Security Company PK should be current branch", Env.CurrentCompany.PK, staffForm.fStaffSecurity.CompanyPK);

				AssertEquals("Security Branch PK should be current branch", Env.CurrentBranch.PK, staffForm.fStaffGroupSecurity.BranchPK);
				AssertEquals("Security Department PK should be current branch", Env.CurrentDepartment.PK, staffForm.fStaffGroupSecurity.DepartmentPK);
				AssertEquals("Security Company PK should be current branch", Env.CurrentCompany.PK, staffForm.fStaffGroupSecurity.CompanyPK);
			}
		}

		public void TestStaffFormLoad()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Ben";
			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				Application.DoEvents();
				Assert("Staff should be a previously saved object", !staffForm.PasswordPanel.Visible);
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				staffForm.GlbStaffForm_Load(null, EventArgs.Empty);
				Assert("Staff should be a previously saved object", staffForm.ChangePasswordButton.Enabled);
				AssertEquals("Selected tab should be DetailsTabPage", staffForm.DetailsTabPage, staffForm.GlbStaffTabControl.SelectedTab);
				Assert(!staffForm.GroupRightsCollapsedPanel.Visible);
				Assert(!staffForm.SecurityGridsSplitter.Visible);
				Assert(!staffForm.GroupRightsPanel.Visible);
			}
		}

		public void TestEffectiveSecurityRightsGroupBoxTextIsTrimmedOnExcessLength()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (GlbStaffForm form = NewGlbStaffForm(staff))
			{
				form.Show();
				ZSecurityPointNode node = new ZSecurityPointNode("some not very long text", Env.Security.WhsDiagnostic);
				ZSecurityPointNode nodeWithLongText = new ZSecurityPointNode("some very long text aaaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaa aaaaaaa aaaaaaa aaaaaaa aaaaaaa aaaaaaa aaaaaaa aaaaaaaa aaaaaaa aaaaaa aaaaa aaaaaaa aaaaaa aaaaaaaa aaaaaaa aaaaaaa aaaaaaa aaaaaaa", Env.Security.WhsDiagnostic);

				form.GlbStaffTabControl.SelectedTab = form.SecurityRightsTabPage;

				form.SecurityPermissionsTreeView.Nodes.Add(node);
				form.SecurityPermissionsTreeView.SelectedNode = node;
				Assert(node.IsSelected);
				AssertEquals("Short string should not be truncated", "Effective Security Right for: some not very long text", form.EffSecRightsBox.Text);

				form.SecurityPermissionsTreeView.Nodes.Add(nodeWithLongText);
				form.SecurityPermissionsTreeView.SelectedNode = nodeWithLongText;
				Assert(nodeWithLongText.IsSelected);
				var assertionMessage = "Long string should be truncated to " + (form.EffSecRightsBox.Width * 2 - 200).ToString() + " pixels";
				bool assertionResult = TextRenderer.MeasureText(form.EffSecRightsBox.Text, form.EffSecRightsBox.Font).Width <= form.EffSecRightsBox.Width * 2 - 200;
				Assert(assertionMessage, assertionResult);
				AssertEquals("truncated long string should end with '...'", "...", form.EffSecRightsBox.Text.Substring(form.EffSecRightsBox.Text.Length - 3, 3));
			}
		}

		#endregion

		#region GS_Code readonly immediately after saving
		public void TestGS_CodeReadonlyAfterSavingButNotClosingForm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "test.user";
			staff.GS_Code = "TST";
			staff.StaffPlainTextPassword = "abc123";
			staff.StaffConfirmPassword = "abc123";
			staff.GS_FullName = "Test User";
			staff.GS_UserAddress1 = "123 Test Street";
			staff.GS_City = "Testville";

			using (var form = new GlbStaffForm(staff))
			{
				form.Show();

				Assert("Staff shouldn't be in database", !staff.IsInDatabase);
				Assert("Staff code should be editable", !form.GS_InitialsBoundText.ReadOnly);

				form.FireSaveButton();

				Assert("Form should not have closed after save", form.Visible);
				Assert("Staff should have been saved to database", staff.IsInDatabase);
				Assert("Staff code should be readonly", form.GS_InitialsBoundText.ReadOnly);
			}
		}
		#endregion

		#region Events

		public void TestSaveNewStaffMember()
		{
			using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				staffForm.SuccessfulSaveNewStaffMember(null, EventArgs.Empty);
				Assert("Password panel should not be visible", !staffForm.PasswordPanel.Visible);
				Assert("ChangePassword button should be enabled", staffForm.ChangePasswordButton.Enabled);
				Assert("Enable Two Factor Authentication check box should be enabled", staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
			}
		}

		public void TestUnlock()
		{
			var staff = Factory.New<GlbStaff>();
			var person = Factory.New<GlbPerson>();
			staff.GS_PER = person.PK;
			person.PER_LoginDisabledUntilUtc = ZDateTime.Now;
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.UnlockButton_Click(null, EventArgs.Empty);
				AssertEquals("Lock out should no longer be set", ZDateTime.Empty, person.PER_LoginDisabledUntilUtc);
			}
		}

		[ExpectException(typeof(CannotDeleteException))]
		[RequiresSTA]
		public void TestAttemptDeleteFromAllUsers()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staff.Groups.RemoveAll();
				AssertEquals("All users group should not have been removed", 1, staff.Groups.Count);
				AssertEquals("Cannot remove staff member from the ALL Users group", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestAttemptDeleteFromDatabaseAccessGroups()
		{
			var databaseDeveloperGroup = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);
			var databaseReaderGroup = Factory.Load<GlbGroup>(GlbGroup.DbReaderGroupPK);
			var backupOperatorGroup = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);
			var staff = Factory.New<GlbStaff>();
			staff.Groups.Add(databaseDeveloperGroup);
			staff.Groups.Add(databaseReaderGroup);
			staff.Groups.Add(backupOperatorGroup);

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staff.Groups.RemoveAll();
				AssertEquals("Database access groups should not have been removed", 5, staff.Groups.Count);
				AssertEquals("Cannot remove staff member from the ALL Users group", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectIndexChange()
		{
			using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.SecBranchFindBox.CurrentCode = "";
				staffForm.SecDepFindBox.CurrentCode = "";

				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);
				AssertEquals("Selected tab should be SecurityTabPage", staffForm.SecurityRightsTabPage, staffForm.GlbStaffTabControl.SelectedTab);
				AssertEquals("Branch code to filter by is current branch", Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK, staffForm.Staff.SecurityBranch);
				AssertEquals("Department code to filter by is current department", Enterprise.MasterFiles.Business.GlbDepartment.CurrentDepartment.PK, staffForm.Staff.SecurityDepartment);
			}
		}

		public void TestRefreshStaffSecurityLabel()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				GlbSecurity securityRecordWithBranch = Factory.New<GlbSecurity>();
				securityRecordWithBranch.GU_SecurityItemIsAllowed = false;
				securityRecordWithBranch.GU_SecurityRight = ((SecurityCheckpoint)staffForm.SecurityPermissionsTreeView.Nodes[0].Tag).Code;
				securityRecordWithBranch.GU_GS = staff.PK;
				securityRecordWithBranch.GU_GB = Env.CurrentBranch.PK;
				staff.StaffSecurityPermissionsCollection.Add(securityRecordWithBranch);

				GlbSecurity securityRecordWithoutBranch = Factory.New<GlbSecurity>();
				securityRecordWithoutBranch.GU_SecurityItemIsAllowed = true;
				securityRecordWithoutBranch.GU_SecurityRight = ((SecurityCheckpoint)staffForm.SecurityPermissionsTreeView.Nodes[0].Tag).Code;
				securityRecordWithBranch.GU_GS = staff.PK;
				staff.StaffSecurityPermissionsCollection.Add(securityRecordWithoutBranch);

				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));

				AssertEquals("First node should be selected", staffForm.SecurityPermissionsTreeView.Nodes[0], staffForm.SecurityPermissionsTreeView.SelectedNode);
				AssertEquals("Filtering by branch and department, permission should be denied", "No", staffForm.ActualSecurityRightLabel.Text);

				securityRecordWithBranch.GU_SecurityItemIsAllowed = true;
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));
				AssertEquals("Filtering by branch and department, permission should be allowed", "Yes", staffForm.ActualSecurityRightLabel.Text);

				securityRecordWithBranch.GU_SecurityItemIsAllowed = false;
				staffForm.SecurityPermissionsTreeView.SelectedNode = staffForm.SecurityPermissionsTreeView.SelectedNode.Nodes[0];
				staffForm.GlbSecurityBoundGrid_CurrentCellChanged(null, EventArgs.Empty);
				AssertEquals("Filtering by branch and department, permission should be denied", "No", staffForm.ActualSecurityRightLabel.Text);

				staffForm.Staff.SecurityBranch = ZGuid.Empty;
				staffForm.SecurityPermissionsTreeView_AfterSelect(null, new TreeViewEventArgs(staffForm.SecurityPermissionsTreeView.Nodes[0], TreeViewAction.ByMouse));
				AssertEquals("No branch specified, permissions not found", "N/A", staffForm.ActualSecurityRightLabel.Text);
			}
		}

		void AssertChangePasswordDialogOldPasswordTextBoxEnabled(GlbStaff staff, string message, bool isResetPasswordAction, bool isADEnabled)
		{
			message = (isADEnabled ? "AD Enabled, " : "AD Disabled, ") + message;
			using (var form = new GlbStaffForm(staff))
			{
				form.Show();
				var changePasswordButtonCaption = isResetPasswordAction ? "Reset Password" : "Change Password";
				AssertEquals("Unexpected ChangePasswordButton.Caption when " + message, changePasswordButtonCaption, form.ChangePasswordButton.CaptionResourceString.Caption);
				var changePasswordButtonEnabled = (isADEnabled && staff.IsADLinked) || (!isADEnabled);
				AssertEquals("Unexpected ChangePasswordButton.Enabled when " + message, changePasswordButtonEnabled, form.ChangePasswordButton.Enabled);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var changePasswordDialog = (ChangePasswordDialog)dialog;
					var expectedOldPasswordTextBoxEnabled = !isResetPasswordAction;
					AssertEquals("Unexpected OldPasswordTextBox.Enabled when " + message, expectedOldPasswordTextBoxEnabled, changePasswordDialog.OldPasswordTextBox.Enabled);
					AssertEquals("Unexpected OldPasswordTextBox.Text.IsNullOrEmpty " + message, true, changePasswordDialog.OldPasswordTextBox.Text.IsNullOrEmpty());
				});

				try
				{
					form.ChangePasswordButton_Click(null, EventArgs.Empty);
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			}
		}

		[RequiresSTA]
		public void TestChangePassword_OldPasswordRequirement()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "something";
			Factory.Save();

			//AD NOT Enabled
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;

			//not current user
			AssertChangePasswordDialogOldPasswordTextBoxEnabled(staff, "Non-current user", true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

			//current user
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertChangePasswordDialogOldPasswordTextBoxEnabled(staff, "Current user", false, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);
			}

			//AD Enabled
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			//not current user
			AssertChangePasswordDialogOldPasswordTextBoxEnabled(staff, "Non-current user", true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

			//current user
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertChangePasswordDialogOldPasswordTextBoxEnabled(staff, "Current user", false, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);
			}
		}

		#region Password controls visibility and readonly

		void AssertPasswordControlsVisibility(GlbStaff staff, bool hasPwdSecurity, bool expectedChangePasswordButtonEnabled, bool expectedChangePasswordButtonVisible, bool expectedChangePasswordAtNextLoginReadOnly, bool expectedPasswordNeverChangesReadOnly, GlbStaff loginUser = null)
		{
			loginUser = loginUser ?? Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var message = hasPwdSecurity ? "With" : "Without";
			message += " password security, ";
			if (staff.IsADIntegrationEnabled)
			{
				message += "AD Enabled, ";
				message += ObjectFactory.Get<IADRegistry>().SyncMode.ToString() + " & ";
				message += ObjectFactory.Get<IADRegistry>().SyncDirection.ToString();
				message += "sync ";
			}

			using (Env.SetTemporaryUserContext(loginUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.StaffEdit.IsAllowed = true;
				Env.Security.StaffModifyAll.IsAllowed = true;
				Env.Security.StaffPasswordAndSignature.IsAllowed = hasPwdSecurity;

				using (var form = new GlbStaffFormForTesting(staff))
				{
					form.Show();
					form.GlbStaffTabControl.SelectedTab = form.PasswordTabPage;
					form.PasswordTabPage.Show();

					AssertEquals(message + "ChangePasswordButton.Enabled", expectedChangePasswordButtonEnabled, form.ChangePasswordButton.Enabled);
					AssertEquals(message + "ChangePasswordButton.Visible", expectedChangePasswordButtonVisible, form.ChangePasswordButton.Visible);
					AssertEquals(message + "GS_ChangePasswordAtNextLoginBoundCheck.ReadOnly", expectedChangePasswordAtNextLoginReadOnly, form.GS_ChangePasswordAtNextLoginBoundCheck.ReadOnly);
					AssertEquals(message + "GS_PasswordNeverChangesBoundCheck.ReadOnly", expectedPasswordNeverChangesReadOnly, form.GS_PasswordNeverChangesBoundCheck.ReadOnly);
				}
			}
		}

		public void TestPasswordControlsVisibility_NonAD_Allowed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			AssertPasswordControlsVisibility(staff, true, true, true, false, false);
		}

		public void TestPasswordControlsVisibility_NonAD_NotAllowed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			AssertPasswordControlsVisibility(staff, false, false, true, true, true);
		}

		public void TestPasswordControlsVisibility_AD()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.TwoWay;

			var loginUser = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			ObjectFactory.Substitute(adEntityProvider.Object);

			//AD primary & 2Way sync - without password security
			AssertPasswordControlsVisibility(staff, false, false, true, true, true, loginUser);

			//AD primary & 2Way sync - with password security
			AssertPasswordControlsVisibility(staff, true, true, true, false, false, loginUser);

			//AD primary & 1Way sync
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;
			AssertPasswordControlsVisibility(staff, true, false, true, true, true, loginUser);

			//AD primary & 1Way sync - current user can change password
			AssertPasswordControlsVisibility(staff, true, true, true, true, true, staff);

			//DisableADPasswordChange
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			AssertPasswordControlsVisibility(staff, true, false, true, true, true, staff);
		}

		#endregion

		#region Locked out controls

		void AssertLockedOutControlsVisibility(GlbStaff staff, bool isLockedOut, bool isADEnabled = false, bool unlockButtonEnabled = true)
		{
			AssertEquals(isLockedOut, staff.IsLockedOut);
			AssertEquals(isADEnabled, staff.IsADIntegrationEnabled);

			var message = (isADEnabled ? "AD Enabled" : "AD Disabled");
			message += ", ";
			message += (isLockedOut ? "Locked out" : "Unlocked");

			using (var form = NewGlbStaffForm(staff))
			{
				form.Show();
				form.GlbStaffTabControl.SelectedTab = form.PasswordTabPage;

				AssertEquals("Unexpected UnlockButton.Visible when " + message, isLockedOut, form.UnlockButton.Visible);
				AssertEquals("Unexpected UnlockButton.Enabled when " + message, unlockButtonEnabled, form.UnlockButton.Enabled);
				AssertEquals("Unexpected IsADLockedOut.Visible when " + message, isADEnabled & isLockedOut, form.IsADLockedOut.Visible);
				AssertEquals("Unexpected GS_LockoutDateTimeBoundDate.Visible when " + message, !isADEnabled & isLockedOut, form.GS_LockoutDateTimeBoundDate.Visible);
			}
		}

		public void TestLockedOutControlsVisibility_NonAD()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			// Unlocked
			AssertEquals("Unlocked", false, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, false);

			// Locked out
			var person = Factory.New<GlbPerson>();
			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(10);
			staff.GS_PER = person.PK;
			AssertEquals("Locked out", true, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, true);

			// Unlocked
			person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(-10);
			AssertEquals("Unlocked", false, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, false);
		}

		public void TestLockedOutControlsVisibility_AD()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().SyncMode = SyncMode.ADIsMaster;
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.TwoWay;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
			ObjectFactory.Substitute(adEntityProvider.Object);

			//Unlocked - AD primary & 2Way sync
			adUser.Setup(m => m.LockedOut).Returns(false);
			AssertEquals("AD unlocked", false, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, false, true);

			//Locked out - AD primary & 2Way sync
			adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(m => m.PasswordLastSet).Returns(DateTime.MinValue);
			adUser.Setup(m => m.PasswordDoesntExpire).Returns(true);
			adUser.Setup(m => m.LockedOut).Returns(true);
			AssertEquals("AD Locked out", true, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, true, true);

			//Locked out - AD primary & 1Way sync
			ObjectFactory.Get<IADRegistry>().SyncDirection = SyncDirection.OneWay;
			AssertEquals("AD Locked out", true, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, true, true, false);

			//Unlocked - AD primary & 1Way sync
			adUser.Setup(m => m.LockedOut).Returns(false);
			AssertEquals("AD unlocked", false, staff.IsLockedOut);
			AssertLockedOutControlsVisibility(staff, false, true, false);
		}

		#endregion

		public void TestSystemAccountIsReadonly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsSystemAccount = true;
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				Assert("System account global staff form is readonly", form.DisplayMode == ODisplayMode.ReadOnly);
			}
		}

		public void TestEdiSupportAccountIsReadonlyOnHostedSystem()
		{
			var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			keyForTest.HostedLocationForTest = "SYD";
			keyForTest.EnterpriseCodeForTest = "ZZZ";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "EDS";
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				AssertEquals("EDS account staff form is readonly", ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		public void TestEdiSupportAccountIsReadonly_WhenClientDLLIsEDI()
		{
			var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			keyForTest.HostedLocationForTest = Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			keyForTest.EnterpriseCodeForTest = "AAA";
			var testHook = new TestClientHook()
			{
				ClientOverride = Clients.EDI
			};
			using (ClientHookLoader.Instance.OverrideClientHookForTest(testHook))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "EDS";
				Factory.Save();

				using (var form = new GlbStaffForm(staff))
				{
					AssertEquals("EDS account staff form is readonly", ODisplayMode.ReadOnly, form.DisplayMode);
				}
			}
		}

		public void TestEdiSupportAccountIsEditableOnNonHostedSystem()
		{
			var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			keyForTest.HostedLocationForTest = Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			keyForTest.EnterpriseCodeForTest = "ZZZ";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "EDS";
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				AssertNotEquals("EDS account staff form is not readonly for non-hosted systems", ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				form.Delete();
			}
		}

		#endregion

		#region Tab pages Enable / Disable

		void AssertAllTabPagesSecurity(GlbStaffForm form, bool allowed, bool viewAllowed)
		{
			AssertAllTabPagesSecurity(form, allowed, allowed, viewAllowed);
		}

		void AssertAllTabPagesSecurity(GlbStaffForm form, bool allowed, bool securityRightsTabPageAllowed, bool viewAllowed)
		{
			AssertAllTabPagesSecurity(form, allowed, securityRightsTabPageAllowed, !allowed, viewAllowed);
		}

		void AssertAllTabPagesSecurity(GlbStaffForm form, bool allowed, bool securityRightsTabPageAllowed, bool warningVisible, bool hrViewAllowed)
		{
			AssertCertificatesTabPageSecurity(form, allowed, warningVisible, hrViewAllowed);
			AssertDetailsTabPageSecurity(form, allowed, warningVisible);
			AssertGroupTabPageSecurity(form, allowed, warningVisible);
			AssertLanguagesTabPageSecurity(form, allowed, warningVisible, hrViewAllowed);
			AssertLeaveTabPageSecurity(form, allowed, warningVisible, hrViewAllowed);
			AssertPasswordAndSignatureTabPageSecurity(form, allowed, warningVisible);
			AssertSecurityRightsTabPageSecurity(form, securityRightsTabPageAllowed, warningVisible || !securityRightsTabPageAllowed);
			AssertTimeAllocationTabPageSecurity(form, allowed, warningVisible, hrViewAllowed);
			AssertWorkingHoursTabPageSecurity(form, allowed, warningVisible, hrViewAllowed);
		}

		void AssertCertificatesTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible, bool hrViewAllowed)
		{
			form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
			form.HRTabControl.SelectedTab = form.CertificatesTabPage;
			AssertEquals("CertificatesHintLabel.Visible", warningVisible, form.CertificatesHintLabel.Visible);
			AssertEquals("GlbCertificatesBoundGrid.Enabled", true, form.CertificatesUserControl.Enabled);
			AssertEquals("GlbCertificatesBoundGrid.ReadOnly", !allowed, form.CertificatesUserControl.CertificatesGrid.ReadOnly);

			if (!hrViewAllowed)
			{
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("CoveringLabel", false)[0].Visible);
			}
		}

		void AssertDetailsTabPageSecurity(GlbStaffForm form, bool allowed)
		{
			AssertDetailsTabPageSecurity(form, allowed, !allowed);
		}

		void AssertDetailsTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible)
		{
			form.GlbStaffTabControl.SelectedTab = form.DetailsTabPage;
			AssertEquals("DetailsHintLabel.Visible", warningVisible, form.DetailsHintLabel.Visible);
			foreach (Control control in form.EmployeeStatusGroupBox.Controls)
			{
				if (control != form.ActivityTrackingStatusDropEdit)
				{
					AssertEquals(control.Name + ".Enabled", allowed, control.Enabled);
				}
			}

			AssertEquals("NextOfKinGroupBox.Enabled", allowed, form.NextOfKinGroupBox.Enabled);
			AssertEquals("ContactNumbersGroupBox.Enabled", allowed, form.ContactNumbersGroupBox.Enabled);
			AssertEquals("EmergencyContactGroupBox.Enabled", allowed, form.EmergencyContactGroupBox.Enabled);
			AssertEquals("WagesGroupBox.Enabled", allowed, form.WagesGroupBox.Enabled);
		}

		void AssertGroupTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible)
		{
			form.GlbStaffTabControl.SelectedTab = form.GroupsTabPage;
			AssertEquals("GroupsHintLabel.Visible", warningVisible, form.GroupsHintLabel.Visible);
			AssertEquals("MembersModuleButtonGrid.Enabled", true, form.MembersModuleButtonGrid.Enabled);
			AssertEquals("MembersModuleButtonGrid.ReadOnly", !allowed, form.MembersModuleButtonGrid.ReadOnly);
		}

		void AssertLanguagesTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible, bool hrViewAllowed)
		{
			form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
			form.HRTabControl.SelectedTab = form.LanguageTabPage;
			AssertEquals("LanguagesHintLabel.Visible", warningVisible, form.LanguagesHintLabel.Visible);
			AssertEquals("LanguagesGrid.Enabled", true, form.LanguagesGrid.Enabled);
			AssertEquals("LanguagesGrid.ReadOnly", !allowed, form.LanguagesGrid.ReadOnly);

			if (!hrViewAllowed)
			{
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("CoveringLabel", false)[0].Visible);
			}
		}

		void AssertLeaveTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible, bool hrViewAllowed)
		{
			form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
			form.HRTabControl.SelectedTab = form.LeaveTabPage;
			AssertEquals("LeaveHintLabel.Visible", warningVisible, form.LeaveHintLabel.Visible);
			AssertEquals("zGrid1.Enabled", true, form.HolidaysGrid.Enabled);
			AssertEquals("zGrid1.ReadOnly", !allowed, form.HolidaysGrid.ReadOnly);

			if (!hrViewAllowed)
			{
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("CoveringLabel", false)[0].Visible);
			}
		}

		void AssertPasswordAndSignatureTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible)
		{
			form.GlbStaffTabControl.SelectedTab = form.PasswordTabPage;
			AssertEquals("PasswordAndSignatureHintLabel.Visible", warningVisible, form.PasswordAndSignatureHintLabel.Visible);
			AssertEquals("PasswordOptionsGroupBox.Enabled", allowed, form.PasswordOptionsGroupBox.Enabled);
			AssertEquals("SignatureGroupBox.Enabled", allowed, form.SignatureGroupBox.Enabled);
			AssertEquals("ChangePasswordButton.Enabled", allowed && form.Staff.IsInDatabase, form.ChangePasswordButton.Enabled);
			AssertEquals("IsTwoFactorAuthenticationEnabledCheckBox.Enabled", form.ShowTwoFactorAuthenticationCheckBox, form.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
		}

		void AssertSecurityRightsTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible)
		{
			form.GlbStaffTabControl.SelectedTab = form.SecurityRightsTabPage;
			AssertEquals("SecurityRightsHintLabel.Visible", warningVisible, form.SecurityRightsHintLabel.Visible);
			AssertEquals("ActualSecurityRightPanel.Enabled", true, form.EffSecRightsBox.Enabled);
			AssertEquals("SecurityPermissionsTreeView.Enabled", true, form.SecurityPermissionsTreeView.Enabled);
			AssertEquals("SecurityPermissionsTreeView.Enabled", !allowed, form.SecurityPermissionsTreeView.ReadOnly);
		}

		void AssertTimeAllocationTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible, bool hrViewAllowed)
		{
			form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
			form.HRTabControl.SelectedTab = form.TimeAllocationTabPage;
			AssertEquals("TimeAllocationHintLabel.Visible", warningVisible, form.TimeAllocationHintLabel.Visible);
			AssertEquals("TimeAllocationGrid.Enabled", true, form.TimeAllocationGrid.Enabled);
			AssertEquals("TimeAllocationGrid.ReadOnly", !allowed, form.TimeAllocationGrid.ReadOnly);
			AssertEquals("TimeAllocationFilterGroupBox.Enabled", allowed, form.TimeAllocationFilterGroupBox.Enabled);

			if (!hrViewAllowed)
			{
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("CoveringLabel", false)[0].Visible);
			}
		}

		void AssertWorkingHoursTabPageSecurity(GlbStaffForm form, bool allowed, bool warningVisible, bool hrViewAllowed)
		{
			form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
			form.HRTabControl.SelectedTab = form.EmploymentTabPage;
			AssertEquals("WorkingHoursHintLabel.Visible", warningVisible, form.WorkingHoursHintLabel.Visible);
			AssertEquals("glbWorkTimeControl.Enabled", allowed, form.glbWorkTimeControl.Enabled);

			if (!hrViewAllowed)
			{
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("WorkingHoursGroupBox", true)[0].Controls.Find("CoveringLabel", false)[0].Visible);
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("ReportingManagementGroupBox", true)[0].Controls.Find("CoveringLabel", false)[0].Visible);
				AssertEquals("CoveringLabel", true, form.HRTabControl.SelectedTab.Controls.Find("glbStaffDirectReportsControl", true)[0].Controls.Find("CoveringLabel", false)[0].Visible);
				AssertEquals("CoveringLabel", true, !IsVisible(form, "glbEmploymentPositionControl") || form.HRTabControl.SelectedTab.Controls.Find("glbEmploymentPositionControl", true)[0].Controls.Find("CoveringLabel", false)[0].Visible);
			}
		}

		public void TestTabPageSecurity_RightsForBackupOperator()
		{
			GlbStaff staff = GetStaffWhoCanModifyOnly();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertEquals(false, form.IsBackupOperatorBoundCheck.Enabled);
					form.GlbStaffTabControl.SelectedTab = form.DetailsTabPage;
					AssertEquals(false, form.IsBackupOperatorBoundCheck.Enabled);
					form.GlbStaffTabControl.SelectedTab = form.GroupsTabPage;
					AssertEquals(false, form.IsBackupOperatorBoundCheck.Enabled);
					form.GlbStaffTabControl.SelectedTab = form.DetailsTabPage;
					AssertEquals(false, form.IsBackupOperatorBoundCheck.Enabled);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_StaffWithoutPermissions()
		{
			GlbStaff staff = GetStaffWithoutPermissions();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertAllTabPagesSecurity(form, false, false);
					AssertEquals("Label should be shown in blue color", Color.Red, form.DetailsHintLabel.ForeColor);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_StaffWithPermissions()
		{
			GlbStaff staff = GetStaffWithPermissions();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertAllTabPagesSecurity(form, true, true);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_GridsRemainReadOnly()
		{
			GlbStaff staff = GetStaffWithPermissions();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					form.CertificatesUserControl.CertificatesGrid.ReadOnly = true; //Simulating a grid that is readonly by default, and so shouldn't be made not readonly by security rights to a tab.
					form.GlbStaffTabControl.SelectedTab = form.HRTabPage;
					form.HRTabControl.SelectedTab = form.CertificatesTabPage;
					AssertEquals("CertificatesHintLabel.Visible", false, form.CertificatesHintLabel.Visible);
					AssertEquals("GlbCertificatesBoundGrid.ReadOnly", true, form.CertificatesUserControl.CertificatesGrid.ReadOnly);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_StaffWhoCannotModifyOthers()
		{
			GlbStaff staff = GetStaffWhoCannotModifyOthers();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				// This Staff member should be allowed access to their own record.
				using (GlbStaffForm form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertDetailsTabPageSecurity(form, true);
				}

				// This Staff member should not be allowed access to others records.
				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertDetailsTabPageSecurity(form, false);
					AssertEquals("Label should be shown in blue color", Color.Red, form.DetailsHintLabel.ForeColor);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_StaffWithLocalAdminRightsForGroup()
		{
			GlbStaff staff = GetStaffWithLocalAdminRightsForGroup();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertDetailsTabPageSecurity(form, true, true);
					AssertAllTabPagesSecurity(form, true, false, true, true);
					AssertEquals("Label should be shown in blue color", Color.Blue, form.DetailsHintLabel.ForeColor);
					AssertEquals("You have rights to view/edit this user because you (and/or a group you are member of) are its Local Administrator.", form.DetailsHintLabel.Text);
				}

				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertDetailsTabPageSecurity(form, true, true);
					AssertAllTabPagesSecurity(form, true, true, true, true);
					AssertEquals("Label should be shown in blue color", Color.Blue, form.DetailsHintLabel.ForeColor);
					AssertEquals("You have rights to view/edit this user because you (and/or a group you are member of) are its Local Administrator.", form.DetailsHintLabel.Text);
				}
			}
		}

		[StressTest]
		public void TestTabPageSecurity_StaffWithLocalAdminRightsForStaff()
		{
			GlbStaff staff = GetStaffWithLocalAdminRightsForStaff();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				using (GlbStaffForm form = NewGlbStaffForm(staff))
				{
					form.Show();
					AssertAllTabPagesSecurity(form, false, true);
					AssertEquals("Label should be shown in red color", Color.Red, form.DetailsHintLabel.ForeColor);
				}

				using (GlbStaffForm form = GetFormToBash())
				{
					form.Show();
					AssertAllTabPagesSecurity(form, true, true, true, true);
					AssertEquals("Label should be shown in blue color", Color.Blue, form.DetailsHintLabel.ForeColor);
					AssertEquals("You have rights to view/edit this user because you (and/or a group you are member of) are its Local Administrator.", form.DetailsHintLabel.Text);
				}
			}
		}

		#region TestInterfaceLanguageSecurity

		[StressTest]
		public void TestInterfaceLanguageSecurity()
		{
			GlbStaff staff1 = GetStaffWhoCannotModifyOthers();
			GlbStaff staff2 = GetStaffWithLocalAdminRightsForStaff();
			GlbStaff staff3 = GetStaffWhoCannotChangeInterfaceLanguage();

			Factory.Save();

			AssertInterfaceLanguageSecurity(staff1, staff1, false, false);
			AssertInterfaceLanguageSecurity(staff1, StaffForForm, true, false);

			AssertInterfaceLanguageSecurity(staff2, staff2, true, false);
			AssertInterfaceLanguageSecurity(staff2, StaffForForm, false, false);

			AssertInterfaceLanguageSecurity(staff3, staff3, true, true);
			AssertInterfaceLanguageSecurity(staff3, StaffForForm, true, true);
		}

		void AssertInterfaceLanguageSecurity(GlbStaff editor, GlbStaff resource, bool shouldBeReadonly, bool shouldHaveValidation)
		{
			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(editor.GS_LoginName))
			using (GlbStaffForm form = NewGlbStaffForm(resource))
			{
				form.Show();

				AssertEquals(shouldBeReadonly, form.WorkingLanguageDropEdit.ReadOnly);
				AssertEquals(shouldHaveValidation, resource.GS_WorkingLanguageInfo.HasNotifications());
			}
		}

		GlbStaff GetStaffWhoCannotChangeInterfaceLanguage()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;

			foreach (SecurityCheckpoint checkpoint in Env.Security.AllLoadedCheckPoints)
			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = checkpoint.Code;
				securityRecord.GU_ItemGUID = checkpoint.ItemGuid;
				securityRecord.GU_SecurityItemIsAllowed = checkpoint.Code == Env.Security.StaffDetails.Code || checkpoint.Code == Env.Security.StaffOwnDetails.Code;
			}

			return result;
		}

		#endregion

		#region TestActivityLogsTabPageVisibility

		public void TestActivityLogsTabPageVisibility()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActivityTrackingStatus = ActivityTrackingStatus.BasedOnCompany;
			AssertActivityLogTabPageVisibility(true, true, staff, true);
			AssertActivityLogTabPageVisibility(true, false, staff, true);
			AssertActivityLogTabPageVisibility(false, true, staff, true);
			AssertActivityLogTabPageVisibility(false, false, staff, false);

			Env.Instance.Security.StaffActivityLog.IsAllowed = false;
			AssertActivityLogTabPageVisibility(true, true, staff, false);
			AssertActivityLogTabPageVisibility(true, false, staff, false);
			AssertActivityLogTabPageVisibility(false, true, staff, false);
			AssertActivityLogTabPageVisibility(false, false, staff, false);

			Env.Instance.Security.StaffActivityLog.IsAllowed = true;
			staff.GS_ActivityTrackingStatus = ActivityTrackingStatus.Yes;
			AssertActivityLogTabPageVisibility(true, true, staff, true);
			AssertActivityLogTabPageVisibility(true, false, staff, true);
			AssertActivityLogTabPageVisibility(false, true, staff, true);
			AssertActivityLogTabPageVisibility(false, false, staff, true);

			staff.GS_ActivityTrackingStatus = ActivityTrackingStatus.No;
			AssertActivityLogTabPageVisibility(true, true, staff, false);
			AssertActivityLogTabPageVisibility(true, false, staff, false);
			AssertActivityLogTabPageVisibility(false, true, staff, false);
			AssertActivityLogTabPageVisibility(false, false, staff, false);
		}

		void AssertActivityLogTabPageVisibility(bool log1, bool log2, GlbStaff staff, bool expectedVisibility)
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = log1;
			EnvProxy.Instance.Registry.UserEventTrackingExternal = log2;
			using (GlbStaffForm staffForm = new GlbStaffForm(staff))
			{
				AssertEquals(expectedVisibility, staffForm.ActivityLogTabPage.TabVisible);
			}
		}

		#endregion

		public void TestActivityTrackingStatusDropEditEnabled()
		{
			var staff = GetStaffWithoutPermissions();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.StaffModifyOwn.IsAllowed = false;

				using (GlbStaffForm form = NewGlbStaffForm(staff))
				{
					form.Show();
					Assert("Activity tracking status drop edit should be editable", form.ActivityTrackingStatusDropEdit.Enabled);
				}
			}
		}

		public void TestChangePasswordButton()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (var staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.DisplayMode = ODisplayMode.ReadOnly;
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				AssertEquals(false, staffForm.ChangePasswordButton.Enabled);
			}
		}

		public void TestSignatureProfileImageSelectionControl()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.SignatureImage = new Bitmap(1, 1);
			staff.ProfileImage = new Bitmap(1, 1);
			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);

				AssertEquals(false, staff.HasChanges);
				(staffForm.ProfileImageSelectionControl.Controls["ClearImageButton"] as IButtonControl).PerformClick();
				AssertEquals(true, staff.HasChanges);
			}

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);

				AssertEquals(false, staff.HasChanges);
				(staffForm.SignatureImageSelectionControl.Controls["ClearImageButton"] as IButtonControl).PerformClick();
				AssertEquals(true, staff.HasChanges);
			}
		}

		#endregion

		#region Setup for Controller access

		public void TestSetupForControllerSecurityRightsTabPage()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_IsController = true;

			Factory.Save();

			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);

				Assert("Group Rights Panel should not be visible", !staffForm.GroupRightsPanel.Visible);
				Assert("Sys Admin label should be visible", staffForm.SystemAdministratorsLabel.Visible);
				Assert("Security grid should not be visible", !staffForm.GlbSecurityBoundGrid.Visible);
				Assert("Actual security rights panel should not be visible", !staffForm.EffSecRightsBox.Visible);
				Assert("Staff rights label should not be visible", !staffForm.StaffRightsLabel.Visible);
				Assert("Group Rights collapsed panel should not be visible", !staffForm.GroupRightsCollapsedPanel.Visible);
			}

			staff.GS_IsController = false;
			using (GlbStaffForm staffForm = NewGlbStaffForm(staff))
			{
				staffForm.Show();
				staffForm.GlbStaffTabControl.SelectedTab = staffForm.SecurityRightsTabPage;
				staffForm.GlbStaffTabControl_SelectedIndexChanged(null, EventArgs.Empty);

				Assert("Group Rights Panel should not be visible", !staffForm.GroupRightsPanel.Visible);
				Assert("Sys Admin label should not be visible", !staffForm.SystemAdministratorsLabel.Visible);
				Assert("Security grid should be visible", staffForm.GlbSecurityBoundGrid.Visible);
				Assert("Actual security rights panel should be visible", staffForm.EffSecRightsBox.Visible);
				Assert("Staff rights label should be visible", staffForm.StaffRightsLabel.Visible);
				Assert("Group Rights collapsed panel should be visible", staffForm.GroupRightsCollapsedPanel.Visible);
			}
		}

		public void TestInitialiseControllerOnlyAccessDetailsTabPage(bool isOperational)
		{
			EnvProxy.SetHostedLocationForTest(string.Empty);
			using (Env.Security.SecurityCachingDisabler)
			{
				GlbStaff staffController = GetStaffWithPermissions();
				GlbStaff staffNotController = GetStaffWithLocalAdminRightsForStaff();
				GlbStaff staffNonOperational = GetStaffWithLocalAdminRightsForStaff();
				staffController.GS_IsSystemAccount = false;
				staffNotController.GS_IsSystemAccount = false;
				staffNonOperational.GS_IsSystemAccount = false;
				staffNonOperational.GS_IsOperational = isOperational;
				Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(staffNotController.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					Env.Security.StaffSales.IsAllowed = false;
					AssertEquals("Precondition: Current user has changed", staffNotController.PK, Env.CurrentUser.PK);
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					Assert("Sys Admin checkbox should not be enabled", !staffForm.GS_IsControllerBoundCheck.Enabled);
					Assert("Is Operational checkbox should not be enabled", !staffForm.GS_IsOperationalBoundCheck.Enabled);
					Assert("Is Sales Rep checkbox should not be enabled", !staffForm.SalesRepCheckBox.Enabled);
					Assert("Is Driver checkbox should not be enabled", !staffForm.DriverCheckBox.Enabled);
					Assert("Is Robot checkbox should not be enabled", !staffForm.RobotCheckbox.Enabled);
					Assert("Is database developer checkbox should not be enabled", !staffForm.IsDatabaseDeveloperBoundCheck.Enabled);
					Assert("Is database reader checkbox should not be enabled", !staffForm.IsReadOnlyDBUserBoundCheck.Enabled);
					Assert("Is backup operator checkbox should not be enabled", !staffForm.IsBackupOperatorBoundCheck.Enabled);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffController.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					Assert("Sys Admin checkbox should be enabled", staffForm.GS_IsControllerBoundCheck.Enabled);
					Assert("Is Operational checkbox checkbox should be enabled", staffForm.GS_IsOperationalBoundCheck.Enabled);
					Assert("Is Sales Rep checkbox checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
					Assert("Is Driver checkbox checkbox should be enabled", staffForm.DriverCheckBox.Enabled);
					Assert("Is Robot checkbox checkbox should be enabled", staffForm.RobotCheckbox.Enabled);
					Assert("Is database developer checkbox should be enabled", staffForm.IsDatabaseDeveloperBoundCheck.Enabled);
					Assert("Is database reader checkbox should be enabled", staffForm.IsReadOnlyDBUserBoundCheck.Enabled);
					Assert("Is backup operator checkbox should be enabled", staffForm.IsBackupOperatorBoundCheck.Enabled);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffNonOperational.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					Env.Security.StaffSales.IsAllowed = false;
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					Assert("Sys Admin checkbox should not be enabled", !staffForm.GS_IsControllerBoundCheck.Enabled);
					Assert("Is Operational checkbox checkbox should be enabled", staffForm.GS_IsOperationalBoundCheck.Enabled);
					Assert("Is Sales Rep checkbox checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
					Assert("Is Driver checkbox checkbox should be enabled", staffForm.DriverCheckBox.Enabled);
					Assert($"Is Robot checkbox checkbox should be enabled", staffForm.RobotCheckbox.Enabled);
					Assert("Is database developer checkbox should be enabled", staffForm.IsDatabaseDeveloperBoundCheck.Enabled);
					Assert("Is database reader checkbox should be enabled", staffForm.IsReadOnlyDBUserBoundCheck.Enabled);
					Assert("Is backup operator checkbox should be enabled", staffForm.IsBackupOperatorBoundCheck.Enabled);
				}

				using (Env.SetTemporaryUserContext(new UserContext("sysadmin", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					Env.Security.StaffSales.IsAllowed = false;
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					Assert("Sys Admin checkbox should be enabled", staffForm.GS_IsControllerBoundCheck.Enabled);
					Assert("Is Operational checkbox checkbox should be enabled", staffForm.GS_IsOperationalBoundCheck.Enabled);
					Assert("Is Sales Rep checkbox checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
					Assert("Is Driver checkbox checkbox should be enabled", staffForm.DriverCheckBox.Enabled);
					Assert($"Is Robot checkbox checkbox should be enabled", staffForm.RobotCheckbox.Enabled);
					Assert("Is database developer checkbox should be enabled", staffForm.IsDatabaseDeveloperBoundCheck.Enabled);
					Assert("Is database reader checkbox should be enabled", staffForm.IsReadOnlyDBUserBoundCheck.Enabled);
					Assert("Is backup operator checkbox should be enabled", staffForm.IsBackupOperatorBoundCheck.Enabled);
				}
			}
		}

		public void TestInitialiseControllerOnlyAccessDetailsTabPage_WithHostedClient_BackupOperatorDisabled()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var staffController = GetStaffWithPermissions();
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffController.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					Env.Security.StaffSales.IsAllowed = false;
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					AssertEquals("Is backup operator checkbox should not be enabled for hosted clients.", false, staffForm.IsBackupOperatorBoundCheck.Enabled);
				}
			}
		}

		[StressTest()]
		public void TestTwoFactorAuthenticationCheckboxSecurity()
		{
			using (Env.Security.SecurityCachingDisabler)
			{
				var regularStaff = Factory.NewWithValidTestData<GlbStaff>();
				regularStaff.GS_IsController = false;
				Factory.Save();

				Env.Security.StaffModifyAll.IsAllowed = true;
				Env.Security.StaffModifyOwn.IsAllowed = true;

				Env.Security.StaffPasswordAndSignature.IsAllowed = true;
				Env.Security.StaffTwoFactorAuthentication.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be enabled", true, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
				}

				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffTwoFactorAuthentication.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be enabled", true, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
				}

				Env.Security.StaffOwnPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffOwnTwoFactorAuthentication.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be enabled", true, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
				}

				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffOwnPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffTwoFactorAuthentication.IsAllowed = false;
				Env.Security.StaffOwnTwoFactorAuthentication.IsAllowed = false;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should not be enabled", false, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
				}

				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffOwnPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffTwoFactorAuthentication.IsAllowed = true;
				Env.Security.StaffOwnTwoFactorAuthentication.IsAllowed = false;
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be enabled for other users", true, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
					AssertEquals("Change Password button should be disabled", false, staffForm.ChangePasswordButton.Enabled);
				}

				Env.Security.StaffPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffOwnPasswordAndSignature.IsAllowed = false;
				Env.Security.StaffTwoFactorAuthentication.IsAllowed = false;
				Env.Security.StaffOwnTwoFactorAuthentication.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be enabled for own", true, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
					AssertEquals("Change Password button should be disabled", false, staffForm.ChangePasswordButton.Enabled);
				}
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = new GlbStaffForm(StaffForForm))
				{
					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.PasswordTabPage;
					AssertEquals("Is2FAcheckbox should be disabled for other user", false, staffForm.IsTwoFactorAuthenticationEnabledCheckBox.Enabled);
					AssertEquals("Change Password button should be disabled", false, staffForm.ChangePasswordButton.Enabled);
				}
			}
		}

		[StressTest()]
		public void TestSalesRepCheckboxSecurity()
		{
			using (Env.Security.SecurityCachingDisabler)
			{
				GlbStaff regularStaff = Factory.NewWithValidTestData<GlbStaff>();
				regularStaff.GS_IsController = false;
				Factory.Save();

				Env.Security.StaffModifyAll.IsAllowed = true;
				Env.Security.StaffModifyOwn.IsAllowed = true;

				Env.Security.StaffDetails.IsAllowed = true;
				Env.Security.StaffSetSalesRep.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					Assert("Is Sales Rep checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
				}

				Env.Security.StaffDetails.IsAllowed = false;
				Env.Security.StaffSetSalesRep.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					Assert("Is Sales Rep checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
				}

				Env.Security.StaffOwnDetails.IsAllowed = false;
				Env.Security.StaffOwnSetSalesRep.IsAllowed = true;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					Assert("Is Sales Rep checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
				}

				Env.Security.StaffDetails.IsAllowed = false;
				Env.Security.StaffOwnDetails.IsAllowed = false;
				Env.Security.StaffSetSalesRep.IsAllowed = false;
				Env.Security.StaffOwnSetSalesRep.IsAllowed = false;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					Assert("Is Sales Rep checkbox should not be enabled", !staffForm.SalesRepCheckBox.Enabled);
				}

				Env.Security.StaffDetails.IsAllowed = false;
				Env.Security.StaffOwnDetails.IsAllowed = false;
				Env.Security.StaffSetSalesRep.IsAllowed = true;
				Env.Security.StaffOwnSetSalesRep.IsAllowed = false;
				using (Env.SetTemporaryUserContext(new UserContext(StaffForForm.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = (GlbStaffForm)GetFormToBashCore())
				{
					staffForm.Show();
					Assert("Is Sales Rep checkbox should be enabled", staffForm.SalesRepCheckBox.Enabled);
				}
			}
		}

		[StressTest()]
		public void TestShowTabPagesForLocalAdmin()
		{
			using (Env.Security.SecurityCachingDisabler)
			{
				GlbStaff localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
				GlbStaff regularStaff = GetStaffWithoutPermissions();
				GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
				regularStaff.Groups.Add(group);
				localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);

				Factory.Save();

				var newStaff = Factory.NewWithValidTestData<GlbStaff>();

				using (Env.SetTemporaryUserContext(new UserContext(regularStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (GlbStaffForm staffForm = new GlbStaffForm(newStaff))
				{
					AssertEquals("Precondition: Current user has changed", regularStaff.PK, Env.CurrentUser.PK);

					staffForm.Show();
					staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
					Assert("Details tab page should not be available", !staffForm.ShowDetailsTabPage);
					Assert("Groups tab page should not be available", !staffForm.ShowGroupsTabPage);
					Assert("Security rights tab page should not be available", !staffForm.ShowSecurityRightsTabPage);

					Assert("Password tab page should not be available", !staffForm.ShowPasswordTabPage);
					Assert("Credentials tab page should not be available", !staffForm.ShowCredentialsTabPage);
					Assert("Certificates tab page should not be available", !staffForm.ShowCertificatesTabPage);

					Assert("Leave tab page should not be available", !staffForm.ShowLeaveTabPage);

					Assert("Working hours tab page should not be available", !staffForm.ShowWorkingHoursControl);
					Assert("Language tab page should not be available", !staffForm.ShowLanguagesTabPage);
					Assert("Time allocation tab page should not be available", !staffForm.ShowTimeAllocationTabPage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(localAdminForGroup.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					using (GlbStaffForm staffForm = new GlbStaffForm(newStaff))
					{
						Env.Security.StaffModifyAll.IsAllowed = false;
						Env.Security.StaffDetails.IsAllowed = false;
						Env.Security.StaffViewOtherStaffDetails.IsAllowed = false;

						AssertEquals("Precondition: Current user has changed", localAdminForGroup.PK, Env.CurrentUser.PK);

						staffForm.Show();
						staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
						Assert("Details tab page should be available for local admin editing non-saved staff", staffForm.ShowDetailsTabPage);
						Assert("Groups tab page should be available for local admin editing non-saved staff", staffForm.ShowGroupsTabPage);
						Assert("Security rights tab page should be available for local admin editing non-saved staff", staffForm.ShowSecurityRightsTabPage);

						Assert("Password tab page should be available for local admin editing non-saved staff", staffForm.ShowPasswordTabPage);
						Assert("Credentials tab page should be available for local admin editing non-saved staff", staffForm.ShowCredentialsTabPage);
						Assert("Certificates tab page should be available for local admin editing non-saved staff", staffForm.ShowCertificatesTabPage);

						Assert("Leave tab page should be available for local admin editing non-saved staff", staffForm.ShowLeaveTabPage);

						Assert("Working hours tab page should be available for local admin editing non-saved staff", staffForm.ShowWorkingHoursControl);
						Assert("Language tab page should be available for local admin editing non-saved staff", staffForm.ShowLanguagesTabPage);
						Assert("Time allocation tab page should be available for local admin editing non-saved staff", staffForm.ShowTimeAllocationTabPage);
					}

					Factory.Save();

					AssertEquals("Precondition: Current user is localAdminForGroup", localAdminForGroup.PK, Env.CurrentUser.PK);
					Assert("Precondition: Current user is not LocalAdmin for newStaff", !newStaff.IsCurrentUserLocalAdminForThisStaff);
					using (GlbStaffForm staffForm = new GlbStaffForm(newStaff))
					{
						staffForm.Show();
						staffForm.GlbStaffTabControl.SelectedTab = staffForm.DetailsTabPage;
						Assert("Details tab page should NOT be available for local admin editing already existing staff ", !staffForm.ShowDetailsTabPage);
						Assert("Groups tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowGroupsTabPage);
						Assert("Security rights tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowSecurityRightsTabPage);

						Assert("Password tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowPasswordTabPage);
						Assert("Credentials tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowCredentialsTabPage);
						Assert("Certificates tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowCertificatesTabPage);

						Assert("Leave tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowLeaveTabPage);

						Assert("Working hours tab page NOT should be available for local admin editing already existing staff", !staffForm.ShowWorkingHoursControl);
						Assert("Language tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowLanguagesTabPage);
						Assert("Time allocation tab page should NOT be available for local admin editing already existing staff", !staffForm.ShowTimeAllocationTabPage);
					}
				}
			}
		}

		public void TestShouldSetMandatoryReportingRolesOnSave()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources Manager", true, true, false }
				};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals("Precondition - Should be added by FillWithValidTestData", 1, staff.Managers.Count);
			staff.Managers[0].Delete();
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
			Factory.Save();

			staff.Validation.ValidateAll();
			AssertHasRowError(staff, "The Human Resources Manager role is mandatory. It must be added for this staff member.");
			using (var staffForm = new GlbStaffForm(staff))
			{
				staffForm.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				staffForm.FireSaveButton();
				var managerForm = (GlbStaffManagerForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(managerForm);
			}
		}

		public void TestShouldShowErrorIfMissingMandatoryReportingRolesAndNoSecurityRightOnSave()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources Manager", true, true, false },
					{ "PRM", (NoResString)"Payroll Manager", true, true, false }
				};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = SetupStaffWithNoErrors();
			staff.GS_IsController = false;
			var securityRecord = staff.StaffSecurityPermissionsCollection.AddNew();
			securityRecord.GU_SecurityRight = "Maintain";
			securityRecord.GU_SecurityItemIsAllowed = true;

			var securityRecord2 = staff.StaffSecurityPermissionsCollection.AddNew();
			securityRecord2.GU_SecurityRight = "StaffReportingManagerRolesAdd";
			securityRecord2.GU_SecurityItemIsAllowed = false;

			AssertEquals("Precondition - Should be added by FillWithValidTestData", 2, staff.Managers.Count);
			staff.Managers.ForEach(x => x.Delete());
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
			Factory.Save();

			staff.Validation.ValidateAll();
			AssertHasRowError(staff, "The Human Resources Manager role is mandatory. It must be added for this staff member.");
			AssertHasRowError(staff, "The Payroll Manager role is mandatory. It must be added for this staff member.");
			AssertEquals("Precondition - Should only have 2 notifications (the row errors)", 2, staff.Notifications.Count());

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			using (var staffForm = new GlbStaffForm(staff))
			{
				staffForm.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Should not continue with save", ContinueWithSave.No, staffForm.FireSaveButton());
				AssertEquals("Should show error", "The form cannot be saved as the staff member is missing the mandatory reporting role(s): Human Resources Manager, Payroll Manager. Please ask your administrator to add this role for the staff member.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		GlbStaff SetupStaffWithNoErrors()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Alexandria";
			staff.GS_UserAddress1 = "1 Alexandria Road";
			staff.GS_FullName = "Alexander";
			staff.StaffPlainTextPassword = "Alexander";
			staff.StaffConfirmPassword = "Alexander";
			return staff;
		}

		public void TestShouldShowErrorIfMissingMandatoryDirectManagerAndNoSecurityRightOnSave()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, true, true, false }
				};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = SetupStaffWithNoErrors();
			staff.GS_IsController = false;
			var securityRecord = staff.StaffSecurityPermissionsCollection.AddNew();
			securityRecord.GU_SecurityRight = "Maintain";
			securityRecord.GU_SecurityItemIsAllowed = true;

			var securityRecord2 = staff.StaffSecurityPermissionsCollection.AddNew();
			securityRecord2.GU_SecurityRight = "StaffEmploymentHistoryNew";
			securityRecord2.GU_SecurityItemIsAllowed = false;

			AssertEquals("Precondition - Should be added by FillWithValidTestData", 1, staff.Managers.Count);
			staff.Managers[0].Delete();
			AssertEquals("Should have deleted manager", 0, staff.Managers.Count);
			Factory.Save();

			staff.Validation.ValidateAll();
			AssertHasRowError(staff, FormattableString.Invariant($"The {DefaultStaffReportingRoles.Descriptions.DirectManager} role is mandatory. It must be added for this staff member."));
			AssertEquals("Precondition - Should only have 1 notification (the row error)", 1, staff.Notifications.Count());

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			using (var staffForm = new GlbStaffForm(staff))
			{
				staffForm.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Should not continue with save since no position was added", ContinueWithSave.No, staffForm.FireSaveButton());
				AssertEquals("Should show error", FormattableString.Invariant($"The form cannot be saved as the staff member is missing the mandatory reporting role(s): {DefaultStaffReportingRoles.Descriptions.DirectManager}. Please ask your administrator to add this role for the staff member."), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithManageeTypeRemoved()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources", true, true, false }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				staff.GS_IsActive = false;

				AssertNoExceptionThrown(() => form.FireSaveButton());
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate.Date);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithMultipleManageeTypeRemoved()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources", true, true, false },
					{ "TRM", (NoResString)"Test Resources", true, true, false },
					{ "RRM", (NoResString)"Robot Resources", true, true, false }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport1 = Factory.NewWithValidTestData<GlbStaff>();
			directReport1.GS_FullName = "report1";
			directReport1.GS_Code = "RE1";
			var directReport2 = Factory.NewWithValidTestData<GlbStaff>();
			directReport2.GS_FullName = "report2";
			directReport2.GS_Code = "RE2";
			var directReport3 = Factory.NewWithValidTestData<GlbStaff>();
			directReport3.GS_FullName = "report3";
			directReport3.GS_Code = "RE3";

			var managerRecord1 = StaffManagerTestHelper.AddManager(directReport1, staff, "HRM", new ZDateTime(2019, 01, 01));
			var managerRecord2 = StaffManagerTestHelper.AddManager(directReport2, staff, "TRM", new ZDateTime(2019, 01, 01));
			var managerRecord3 = StaffManagerTestHelper.AddManager(directReport3, staff, "RRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				staff.GS_IsActive = false;

				AssertNoExceptionThrown(() => form.FireSaveButton());
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord1.GSM_EndDate.Date);
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord2.GSM_EndDate.Date);
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord3.GSM_EndDate.Date);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithManagerTypeRemoved()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources", true, true, false }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_FullName = "report";
			manager.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				staff.GS_IsActive = false;

				AssertNoExceptionThrown(() => form.FireSaveButton());
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate.Date);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithMultipleManagerTypeRemoved()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources", true, true, false },
					{ "TRM", (NoResString)"Test Resources", true, true, false },
					{ "RRM", (NoResString)"Robot Resources", true, true, false }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_FullName = "report1";
			manager1.GS_Code = "RE1";
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager2.GS_FullName = "report2";
			manager2.GS_Code = "RE2";
			var manager3 = Factory.NewWithValidTestData<GlbStaff>();
			manager3.GS_FullName = "report3";
			manager3.GS_Code = "RE3";

			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "TRM", new ZDateTime(2019, 01, 01));
			var managerRecord3 = StaffManagerTestHelper.AddManager(staff, manager3, "RRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				staff.GS_IsActive = false;

				AssertNoExceptionThrown(() => form.FireSaveButton());
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord1.GSM_EndDate.Date);
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord2.GSM_EndDate.Date);
				AssertEquals(new ZDateTime(2019, 04, 04), managerRecord3.GSM_EndDate.Date);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithDirectReportsInMandatoryRoleShouldReplaceManager()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources", true, true, false }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				staff.GS_IsActive = false;
				form.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();

				var today = new ZDateTime(2019, 04, 04);

				AssertEquals("End Date should have been set.", today, managerRecord.GSM_EndDate);
				AssertEquals("Manager should have been replaced", 1, replacementStaff.DirectReports.Count);

				var newReport = replacementStaff.DirectReports.FirstOrDefault(x => !x.SelfManaged);
				AssertEquals("Manager type should match existing record", "HRM", newReport.GSM_ManagerType);
				AssertEquals("Effective date should be tomorrow", new ZDateTime(2019, 04, 05), newReport.GSM_EffectiveDate);
				AssertEquals("Direct report should remain the same", directReport.PK, newReport.GSM_GS_Staff);
				AssertEquals("No end date should be set", ZDateTime.Empty, newReport.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestValidateAndSaveDeactivateStaffWithDirectReportsShouldRefreshTrees()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources Manager", true, true, true }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var directReportRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			var managerRecord = StaffManagerTestHelper.AddManager(staff, replacementStaff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();

				form.StaffManagementControlExposed.Tree.Model = new GlbStaffManagementTreeModelView(staff.ManagementTreeModel);
				form.DirectReportsControlExposed.Tree.Model = new GlbStaffManagementTreeModelView(staff.DirectReportsTreeModel);
				form.StaffManagementControlExposed.ModelView.BuildTree();
				form.DirectReportsControlExposed.ModelView.BuildTree();
				AssertEquals("First node is RoleWrapper, other two are ManagerWrappers (mandatory + managerRecord)", 3, form.StaffManagementControlExposed.Tree.AllNodes.Count());
				AssertEquals("First node is RoleWrapper, other two are ManagerWrappers (directReportRecord)", 2, form.DirectReportsControlExposed.Tree.AllNodes.Count());

				staff.GS_IsActive = false;
				form.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();

				AssertEquals("Tree should be rebuilt with no managers", 0, form.StaffManagementControlExposed.Tree.AllNodes.Count());
				AssertEquals("Tree should be rebuilt with no direct reports", 0, form.DirectReportsControlExposed.Tree.AllNodes.Count());
			}
		}

		public void TestDoesntResetHasChangesOnDelete()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var displayModes = Enum.GetValues(typeof(ODisplayMode)).Cast<ODisplayMode>();
			foreach (var displayMode in displayModes)
			{
				using (var form = NewGlbStaffForm(staff))
				{
					form.DisplayMode = displayMode;
					form.Show();
					AssertEquals(displayMode == ODisplayMode.Delete, staff.HasChanges);
					staff.HasChanges = true;
				}
			}
		}

		#endregion

		#region Detaching Groups From Staff Tests

		public void TestDetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			DetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast(allowed: true, "Do you still want to remove the groups from this staff member?", MessageBoxButtons.YesNo);
		}

		public void TestDetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast_AndOperationIsNotAllowed()
		{
			DetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast(allowed: false, "You do not have the relevant permission to make this change.", System.Windows.Forms.MessageBoxButtons.OK);
		}

		public void DetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast(bool allowed, string message, MessageBoxButtons buttons)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			// see corresponding test in MasterFiles (TestGetGRPCapabilitiesWithLastStaffPerGroup)
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "C3";
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Capabilities.Add(capability3);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbGroup[] { group1, group2 }));

				AssertEquals(@"- This staff member is the last member of group G1 with capability(s) C2, C3.
- This staff member is the last member of group G2 with capability(s) C1, C3.

Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. " + message, staffForm.MembersModuleButtonGrid.DetachMessage.Caption);

				AssertEquals(staffForm.MessageBoxButtons, buttons);
			}
		}

		public void TestDetachingGroups_ShouldShowDefaultMessage_WhenStaffIsNotLast()
		{
			DetachingGroups_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: true);
		}

		public void TestDetachingGroups_ShouldShowDefaultMessage_WhenStaffIsNotLast_AndOperationIsNotAllowed()
		{
			DetachingGroups_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: false);
		}

		public void DetachingGroups_ShouldShowDefaultMessage_WhenStaffIsNotLast(bool allowed)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_Code = "C1";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbGroup[] { group1 }));

				AssertEquals("Are you sure you want to detach the selected groups?", staffForm.MembersModuleButtonGrid.DetachMessage.Caption);
				AssertEquals(staffForm.MessageBoxButtons, MessageBoxButtons.YesNo);
			}
		}

		public void TestDetachingGroups_ShouldShowDefaultMessage_WhenStaffIsLast_ButGroupIsSecurityEnabled()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbGroup[] { group1 }));

				AssertEquals("Are you sure you want to detach the selected groups?", staffForm.MembersModuleButtonGrid.DetachMessage.Caption);
			}
		}

		public void TestDetachingGroups_ShouldShowWarningMessage_WhenStaffIsLast_AndAllGroupsAreConsidered()
		{
			WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbGroup[] { group1 }));

				AssertEquals(@"- This staff member is the last member of group G1 with capability(s) C1.

Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the groups from this staff member?", staffForm.MembersModuleButtonGrid.DetachMessage.Caption);
			}
		}

		public void TestDeleteKey_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.TopLevelTabControl.SelectedTab = staffForm.GroupsTabPage;
				staffForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements(x => ((GlbGroup)x).GG_Code != "ALL");

				KeySender.PostKeyDown(staffForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				Application.DoEvents();

				AssertEquals(@"- This staff member is the last member of group G1 with capability(s) C1.
- This staff member is the last member of group G2 with capability(s) C1.

Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the groups from this staff member?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteKey_ShouldNotRemoveGroup_WhenResponseIsNo()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.TopLevelTabControl.SelectedTab = staffForm.GroupsTabPage;
				staffForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements(x => ((GlbGroup)x).GG_Code != "ALL");

				KeySender.PostKeyDown(staffForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Application.DoEvents();

				AssertEquals("Groups should not be detached", 3, staff1.Groups.Count); // ALL + G1 + G2
			}
		}

		public void TestDeleteKey_ShouldRemoveGroup_WhenResponseIsYes()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.TopLevelTabControl.SelectedTab = staffForm.GroupsTabPage;
				staffForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements(x => ((GlbGroup)x).GG_Code != "ALL");

				KeySender.PostKeyDown(staffForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();

				AssertEquals("Groups should be detached", 1, staff1.Groups.Count); // ALL (which is unremovable)
			}
		}

		public void TestDeleteMenuItemClick_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var staffForm = NewGlbStaffForm(staff1))
			{
				staffForm.Show();
				staffForm.TopLevelTabControl.SelectedTab = staffForm.GroupsTabPage;
				staffForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements(x => ((GlbGroup)x).GG_Code != "ALL");
				staffForm.MembersModuleButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();

				AssertEquals(@"- This staff member is the last member of group G1 with capability(s) C1.

Removing the groups from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the groups from this staff member?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		GlbStaff staffForForm;

		GlbStaff StaffForForm
		{
			get { return staffForForm ?? (staffForForm = Factory.NewWithValidTestData<GlbStaff>()); }
		}

		// This staff has permission to modify own details, but not others.
		GlbStaff GetStaffWhoCannotModifyOthers()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;
			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = "Maintain";
				securityRecord.GU_SecurityItemIsAllowed = false;
			}

			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = "StaffModifyOwn";
				securityRecord.GU_SecurityItemIsAllowed = true;
			}

			return result;
		}

		// This staff has permission to modify details, but that's it.
		GlbStaff GetStaffWhoCanModifyOnly()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;
			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = "Maintain";
				securityRecord.GU_SecurityItemIsAllowed = false;
			}

			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = "StaffModifyOwn";
				securityRecord.GU_SecurityItemIsAllowed = true;
			}

			{
				GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
				securityRecord.GU_SecurityRight = "StaffModify";
				securityRecord.GU_SecurityItemIsAllowed = true;
			}

			return result;
		}

		GlbStaff GetStaffWithLocalAdminRightsForGroup()
		{
			GlbStaff result = GetStaffWithoutPermissions();
			result.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(StaffForForm.Groups[0].GG_Code); // All Users group.
			return result;
		}

		GlbStaff GetStaffWithLocalAdminRightsForStaff()
		{
			GlbStaff result = GetStaffWithoutPermissions();
			result.SecurityChangeOthersView.AddSecurityToChangeOtherStaff(StaffForForm.GS_Code);
			return result;
		}

		GlbStaff GetStaffWithoutPermissions()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;

			GlbSecurity securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
			securityRecord.GU_SecurityRight = "Maintain";
			securityRecord.GU_SecurityItemIsAllowed = false;

			return result;
		}

		GlbStaff GetStaffWithPermissions()
		{
			GlbStaff result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = true;
			return result;
		}

		[GuiTest]
		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				const int MinScreenWidthSupported = 1366;
				const int MinScreenHeightSupported = 811;
				const int TypicalTaskbarHeight = 43;

				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);

				Assert("Form min size too wide (" + testForm.MinimumSize.Width + ") for the screen. Should be less than or equal to " + maxSizeWidth, testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height + ") for the screen. Should be less than or equal to " + maxSizeHeight, testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		void AssertCredentialsVisibility(ZString countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var form = NewGlbStaffForm(StaffForForm))
			{
				form.Show();
				var staffCredentialsPlugIn = form.GlbStaffTabControl.PlugIns.GetPlugIn(ControllerIDs.StaffCredentialsPlugIn);
				form.GlbStaffTabControl.SelectedTab = staffCredentialsPlugIn.TabPage;
				AssertEquals(true, staffCredentialsPlugIn.UserControl.Visible);
			}
		}

		#endregion

		public void TestEmergencyContactSameAsNextOfKin()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_NextOfKinHomePhone = "3355";
			staff.GS_NextOfKinWorkPhone = "9911";

			using (var glbStaffForm = new GlbStaffFormForTesting(staff))
			{
				glbStaffForm.Show();
				staff.EmergencySameAsNextOfKin = true;

				var homePhoneNumberTextBox = glbStaffForm.EmergencyHomePhoneNumberControlForTest.Controls["NumberTextBox"] as PhoneNumberUserControl.PhoneNumberTextBox;
				if (homePhoneNumberTextBox != null)
				{
					AssertEquals("3355", homePhoneNumberTextBox.Text);
					Assert("home phone number should be read only when set emergency to be same as NextOfKin.", homePhoneNumberTextBox.ReadOnly);
				}

				var workPhoneNumberTextBox = glbStaffForm.EmergencyWorkPhoneNumberControlForTest.Controls["NumberTextBox"] as PhoneNumberUserControl.PhoneNumberTextBox;
				if (workPhoneNumberTextBox != null)
				{
					AssertEquals("9911", workPhoneNumberTextBox.Text);
					Assert("work phone number should be read only when set emergency to be same as NextOfKin.", workPhoneNumberTextBox.ReadOnly);
				}
			}
		}
		public void TestPersonRecordSecurity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.PersonIntelligenceView.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithPermissions.PK;
			staffWithPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.PersonIntelligenceView.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff))
				{
					form.Show();

					form.EditPersonButton_Click(this, EventArgs.Empty);

					AssertNotContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff))
				{
					form.Show();

					form.EditPersonButton_Click(this, EventArgs.Empty);

					AssertContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
			}
		}
		public void TestViewAdditionalEmailAddressesOfOthersIsDisabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = Env.Security.StaffViewOtherEmailAddresses.Code;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "a@test.com";

			var secondEmail = staff2.EmailAddresses.AddNew();
			secondEmail.GSE_EmailAddress = "b@test.com";
			secondEmail.EmailType = "TST";
			secondEmail.GSE_IsVisible = false;

			var thirdEmail = staff2.EmailAddresses.AddNew();
			thirdEmail.GSE_EmailAddress = "c@test.com";
			thirdEmail.EmailType = "WAH";
			thirdEmail.GSE_IsVisible = false;

			Factory.Save();

			var staff2WithNewFactory = new BusinessObjectFactory().Load<GlbStaff>(staff2.PK);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff2WithNewFactory))
				{
					form.Show();

					var details = form.DetailsTabPage;
					var emailAddressGrid = details.Controls.Find("EmailAddressesGrid", true)[0] as ZGrid;

					AssertEquals(1, emailAddressGrid.VisibleRowCount);
				}
			}
		}

		[RequiresSTA]
		public void TestViewAdditionalEmailAddressesOfOthersIsEnabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = Env.Security.StaffViewOtherEmailAddresses.Code;
			securityRecord.GU_SecurityItemIsAllowed = true;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "a@test.com";

			var secondEmail = staff2.EmailAddresses.AddNew();
			secondEmail.GSE_EmailAddress = "b@test.com";
			secondEmail.EmailType = "TST";
			secondEmail.GSE_IsVisible = true;

			var thirdEmail = staff2.EmailAddresses.AddNew();
			thirdEmail.GSE_EmailAddress = "c@test.com";
			thirdEmail.EmailType = "WAH";
			thirdEmail.GSE_IsVisible = true;

			Factory.Save();

			var staff2WithNewFactory = new BusinessObjectFactory().Load<GlbStaff>(staff2.PK);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff2WithNewFactory))
				{
					form.Show();

					var details = form.DetailsTabPage;
					var emailAddressGrid = details.Controls.Find("EmailAddressesGrid", true)[0] as ZGrid;

					AssertEquals(3, emailAddressGrid.VisibleRowCount);
				}
			}
		}

		public void TestViewNationalityOfOthersIsDisabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = Env.Security.StaffViewOtherNationality.Code;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_RN_NKNationalityCode = "AU";

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff2))
				{
					form.Show();

					AssertEquals(true, staff2.GS_RN_NKNationalityCodeInfo.ReadOnly);
					AssertEquals("** View Denied due to Security Access **", staff2.GS_RN_NKNationalityCode);
				}
			}
		}

		public void TestViewNationalityOfOthersIsEabled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = Env.Security.StaffViewOtherNationality.Code;
			securityRecord.GU_SecurityItemIsAllowed = true;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_RN_NKNationalityCode = "AU";

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new GlbStaffFormForTesting(staff2))
				{
					form.Show();

					AssertEquals(false, staff2.GS_RN_NKNationalityCodeInfo.ReadOnly);
					AssertEquals("AU", staff2.GS_RN_NKNationalityCode);
				}
			}
		}

		public void TestEditPersonButtonAvailability()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();
				AssertEquals(false, form.EditPersonButton_Exposed.Available);
			}

			Factory.Save();
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();
				AssertEquals(true, form.EditPersonButton_Exposed.Available);
			}

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new GlbStaffFormForTesting(staff))
			{
				form.Show();
				AssertEquals(false, form.EditPersonButton_Exposed.Available);
			}
		}

		public void TestValidateAddressButton()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var staff = Factory.New<GlbStaffForTest>();
			staff.GS_UserAddress1 = "xxxx";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_State = "AST";
			staff.GS_City = "Sydney";
			using (var form = new GlbStaffForm(staff))
			{
				form.Show();

				form.ValidateAddressButton.PerformClick();
				var suggestionControl = form.FindSingleOrDefault<AddressSuggestionControl>("AddressSuggestionControl");
				AssertNotNull("AddressSuggestionControl", suggestionControl);
				var infoLabel = suggestionControl.FindSingleOrDefault<ZLabel>("InfoLabel");
				AssertNotNull("InfoLabel", infoLabel);
				AssertEquals("No suggestions have been found for the address that you entered. Please confirm as original or amend the address to receive suggestions.", infoLabel.Text);
			}
		}

		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var staff = Factory.New<GlbStaffForTest>();
				staff.GS_UserAddress1 = "xxxx";
				staff.GS_RN_NKCountryCode = "AU";
				staff.GS_State = "AST";
				staff.GS_City = "Sydney";
				staff.ValidationStatus = AddressValidationStatus.ToBeVerified;
				staff.Address1 = "Test Address1";
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new GlbStaffFormForTesting(staff))
					{
						form.Show();
						Assert("Change should not happen on the staff when load", !staff.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, staff.ValidationStatus);
					}
				});
			}
		}

		class GlbStaffForTest : GlbStaff, ISupportWebAddressValidation
		{
			public GlbStaffForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				ValidationStatus = AddressValidationStatus.Invalid;
				return Task.FromResult(new WebAddressValidationResult());
			}

			public override bool UserIsLoggedIn()
			{
				return true;
			}
		}

		public void TestValidateAddressButton_HasClickEvent()
		{
			using (var form = new GlbStaffForm())
			{
				form.Show();
				var validateButton = (ZButton)form.Controls.Find("ValidateAddressButton", true).First();
				Assert("ValidateAddressButton has click event", validateButton.ClickHasBeenHooked);
			}
		}

		public void TestLoginNameChangeConfirmation()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "spiderman";
			Factory.Save();

			using (var form = new GlbStaffForm(staff))
			{
				form.Show();
				staff.GS_LoginName = "spiderham";
				form.FireSaveButton();
				var warningMessage = "You have made changes to the Login Name. Please re-enter the new Login Name to confirm your changes.";
				AssertEquals("Confirmation Message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(warningMessage));
				AssertEquals("Confirmation String", staff.GS_LoginName, UnitTestUserNotification.Instance.LastConfirmationStringShown);
			}
		}

		class GlbStaffFormForTesting : GlbStaffForm
		{
			public GlbStaffFormForTesting(GlbStaff staff)
				: base(staff)
			{
			}

			public bool DeduplicationStatusIconVisible => DuplicateDetectionStatusLabel.Visible && DuplicateDetectionStatusIcon.Visible;

			public bool DeduplicationStatusLabelReads(string text) => DuplicateDetectionStatusLabel.Text.Equals(text);

			public bool DeduplicationStatusLabelColorIs(Color color) => DuplicateDetectionStatusLabel.ForeColor.Equals(color);

			public IDuplicationEventArgs CurrentDuplicationEventArgs
			{
				get { return currentDuplicationEventArgs; }
			}

			public void DuplicateDetectionHyperlinkClicked()
			{
				DuplicateDetectionStatusLabelOnClick(this, EventArgs.Empty);
			}

			public PhoneNumberUserControl EmergencyHomePhoneNumberControlForTest
			{
				get
				{
					return EmergencyHomePhoneNumberControl;
				}
			}

			public PhoneNumberUserControl EmergencyWorkPhoneNumberControlForTest
			{
				get
				{
					return EmergencyWorkPhoneNumberControl;
				}
			}

			public GlbStaffManagementControl StaffManagementControlExposed => glbStaffManagementControl;

			public GlbStaffDirectReportsControl DirectReportsControlExposed => glbStaffDirectReportsControl;

			public GlbStaffPopupModuleHelperForTest StaffPopupModuleHelperExposed => (GlbStaffPopupModuleHelperForTest)StaffPopupModuleHelper;

			protected override GlbStaffPopupModuleHelper StaffPopupModuleHelper
			{
				get
				{
					return staffPopupModuleHelper ?? (staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest());
				}
			}

			public new void EditPersonButton_Click(object sender, EventArgs e)
			{
				base.EditPersonButton_Click(sender, e);
			}

			public new void IsDeviceOnlyCheckBox_Click(object sender, EventArgs e)
			{
				base.IsDeviceOnlyCheckBox_Click(sender, e);
			}

			public ZToolStripButton EditPersonButton_Exposed => (Controls.Find("EditToolStrip", true).Single() as ZToolStrip).Items.Find("EditPersonButton", true).Single() as ZToolStripButton;
			public ZToolStripButton AddEditEmploymentPositionButton_Exposed => (Controls.Find("AddEditEmploymentPositionToolStrip", true).Single() as ZToolStrip).Items.Find("AddEditEmploymentPositionButton", true).Single() as ZToolStripButton;
		}

		new GlbStaffForm GetFormToBash() => (GlbStaffForm)base.GetFormToBash();

		const string AuthorityUrl = "https://www.example.com";

		const string ClientId = "46546646-e627-46fb-afe4-e5ea9928740e";
	}
}
