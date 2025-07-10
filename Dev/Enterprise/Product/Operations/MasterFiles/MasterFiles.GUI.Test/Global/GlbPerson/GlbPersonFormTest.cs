using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbPersonForm))]
	public class GlbPersonFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "some name";

			using (var form = new GlbPersonForm(person))
			{
				AssertEquals(person.HumanReadableName, form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "@#$_Basher_Test ";
			Factory.Save();
			var form = new GlbPersonForm(person);
			return form;
		}

		protected void TestPlugInsAdded()
		{
			using (var form = (GlbPersonForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		protected void TestWorkflowTabpage()
		{
			using (var form = (GlbPersonForm)GetFormToBash())
			{
				AssertEquals(form.ControllerID, ControllerIDs.GlbPerson);
			}
		}

		[RequiresSTA]
		public void TestActiveAssociationsGroupBoxShouldExtendToEdgeOfMainTabPage()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "Chancellor Bennett";
			Factory.Save();

			using (var form = new GlbPersonFormForTest(person))
			{
				var mainTabPage = form.MainTabPageExposed();

				var controlCollection = mainTabPage.Controls.Find("ActiveAssociationsGroupBox", false);
				AssertEquals("Should only be 1 groupbox", 1, controlCollection.Length);
				var activeAssociationsGroupBox = (ZGroupBox)controlCollection.First();

				controlCollection = mainTabPage.Controls.Find("PersonalInformationGroupBox", false);
				AssertEquals("Should only be 1 groupbox", 1, controlCollection.Length);
				var personalInformationGroupBox = (ZGroupBox)controlCollection.First();

				controlCollection = mainTabPage.Controls.Find("HomeAddressGroupBox", false);
				AssertEquals("Should only be 1 groupbox", 1, controlCollection.Length);
				var homeAddressGroupBox = (ZGroupBox)controlCollection.First();

				AssertEquals("ActiveAssociations and PersonalInformation GroupBoxes should cover most of the width of the page",
					mainTabPage.Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(14),
					activeAssociationsGroupBox.Width + personalInformationGroupBox.Width);
				AssertEquals("ActiveAssociations and HomeAddress GroupBoxes should cover most of the height of the page",
					mainTabPage.Height - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(16),
					activeAssociationsGroupBox.Height + homeAddressGroupBox.Height);
			}
		}

		public void TestActionsMenuIsSaved()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			using (var form = new GlbPersonFormForTest(person))
			{
				form.Show();
				var accreditationUpdaterItem = form.ActionsMenuItemExposed().MenuItems.FindByText("Create Accreditation Attempt for Existing Exam Attempts", true);
				accreditationUpdaterItem.PerformClick();

				AssertEquals("Should show error since person is unsaved.", FormattableString.Invariant($"Cannot create Accreditation Attempts until {person.HumanReadableName} is saved."), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public class GlbPersonFormForTest : GlbPersonForm
		{
			public GlbPersonFormForTest(GlbPerson person) : base(person)
			{
			}

			public ZTabPage MainTabPageExposed()
			{
				return MainTabPage;
			}

			public ZTemplateTabControl MainTabControlExposed()
			{
				return MainTabControl;
			}

			public MenuItem ActionsMenuItemExposed()
			{
				return ActionsMenuItem;
			}

			public new void UnlockButton_Click(object sender, EventArgs e) => base.UnlockButton_Click(sender, e);

			public ZButton UnlockButtonExposed => UnlockButton;

			public ZDateEdit LockoutDateTimeBoundDateExposed => LockoutDateTimeBoundDate;
		}

		public void TestRegenerateProgressFormShown()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.PER_MobilePhone = "0455555555";
			person.PER_HomeAddress1 = "Bourke Street3";
			person.PER_EmailAddress = "XXXXXX@wisetech.com";
			Factory.Save();

			using (var testForm = new GlbPersonForm(person))
			{
				testForm.Show();
				Application.DoEvents();
				person.PatternMatchingRecalculator.Regenerate(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationGlbPerson(person));
				Assert("Regenerate progressForm form is shown.", testForm.isrecalculateprogressFormShownForTest);
			}
		}

		[RequiresSTA]
		public void TestViewPersonalChangedDetailsFromLogAndNoteTab()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.PER_MobilePhone = "0455555555";
			person.PER_HomeAddress1 = "Bourke Street3";
			person.PER_EmailAddress = "XXXXXX@wisetech.com";
			Factory.Save();

			Env.Security.PersonIntelligenceViewNotesText.IsAllowed = false;
			Env.Security.PersonIntelligenceViewLogsReference.IsAllowed = false;
			AssertVisibility(false);

			Env.Security.PersonIntelligenceViewNotesText.IsAllowed = true;
			Env.Security.PersonIntelligenceViewLogsReference.IsAllowed = true;
			AssertVisibility(true);

			void AssertVisibility(bool visible)
			{
				using (var testForm = new GlbPersonFormForTest(person))
				{
					testForm.Show();
					testForm.MainTabControlExposed().SelectedIndex = 6;
					Application.DoEvents();

					var tabPages = testForm.MainTabControlExposed().TabPages;
					var noteTabPage = tabPages.OfType<ZTabPage>().FirstOrDefault(x => x.Name == "NotesTabPage");
					AssertNotNull(noteTabPage);

					var noteRichTextBox = noteTabPage.Controls.Find("NoteRichTextBox", true).FirstOrDefault();
					AssertNotNull(noteRichTextBox);
					AssertEquals($"Should {(visible ? "" : "not")} display the Note Rich TextBox", visible, noteRichTextBox.Visible);

					var noteGrid = (ZGrid)noteTabPage.Controls.Find("NoteGrid", true).FirstOrDefault();
					AssertNotNull(noteGrid);
					AssertEquals($"Should {(visible ? "" : "not")} display the Note Text column", visible, noteGrid.Columns.Contains("ST_NoteDataAsTextConcatenatedAndTrimmed"));

					testForm.MainTabControlExposed().SelectedIndex = 7;
					Application.DoEvents();

					var logsTabPage = tabPages.OfType<ZTabPage>().FirstOrDefault(x => x.Name == "LogsTabPage");
					AssertNotNull(logsTabPage);
					var logsGrid = (ZGrid)logsTabPage.Controls.Find("FilteredGrid", true).FirstOrDefault();
					AssertNotNull(logsGrid);
					AssertEquals($"Should {(visible ? "" : "not")} display the log reference column", visible, logsGrid.Columns.Contains("SL_ReferenceForBinding"));
					AssertEquals($"Should {(visible ? "" : "not")} display the log event detail column", visible, logsGrid.Columns.Contains("DisplayEventReference"));
				}
			}
		}

		public void TestTabPageSecurity()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";
			person.PER_MobilePhone = "0455555555";
			person.PER_HomeAddress1 = "Bourke Street3";
			person.PER_EmailAddress = "XXXXXX@wisetech.com";
			Factory.Save();
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;

			using (var testForm = new GlbPersonFormForTest(person))
			{
				testForm.Show();
				var tabPages = testForm.MainTabControlExposed().TabPages;
				AssertEquals(8, tabPages.Count);
				var accreditationsTabPage = tabPages.ToList<ZTabPage>().FirstOrDefault(x => x.Name == "AccreditationsTabPage");
				AssertNotNull(accreditationsTabPage);
				AssertEquals(accreditationsTabPage.Text, 0, accreditationsTabPage.Controls.Find("coveringLabel", false).Length);
				AssertNotNull(testForm.ActionsMenuItemExposed().MenuItems.ToList<ZMenuItem>().FirstOrDefault(x => x.Caption == "Create Accreditation Attempt for Existing Exam Attempts"));
			}

			Env.Security.GlbAccreditationAttemptView.IsAllowed = false;

			using (var testForm = new GlbPersonFormForTest(person))
			{
				testForm.Show();
				var tabPages = testForm.MainTabControlExposed().TabPages;
				AssertEquals(8, tabPages.Count);
				var accreditationsTabPage = tabPages.ToList<ZTabPage>().FirstOrDefault(x => x.Name == "AccreditationsTabPage");
				AssertNotNull(accreditationsTabPage);
				AssertEquals(accreditationsTabPage.Text, 1, accreditationsTabPage.Controls.Find("coveringLabel", false).Length);
				AssertNull(testForm.ActionsMenuItemExposed().MenuItems.ToList<ZMenuItem>().FirstOrDefault(x => x.Caption == "Create Accreditation Attempt for Existing Exam Attempts"));
			}
		}

		[RequiresSTA]
		public void TestActionsMenu()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertActionsMenu(person, true);

			Env.Security.PersonIntelligenceEdit.IsAllowed = false;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertActionsMenu(person, false);

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = false;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertActionsMenu(person, false);

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = false;
			person.ReadOnly = false;
			AssertActionsMenu(person, false);

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = true;
			AssertActionsMenu(person, false);

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptView.IsAllowed = true;
			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			person.ReadOnly = false;
			AssertActionsMenu(person, true);
		}

		void AssertActionsMenu(GlbPerson person, bool isEditable)
		{
			using (var testForm = new GlbPersonFormForTest(person))
			{
				testForm.Show();
				var menus = testForm.ActionsMenuItemExposed().MenuItems.ToList<ZMenuItem>();
				AssertEquals(isEditable, menus.Any(x => x.Caption == "Create Accreditation Attempt for Existing Exam Attempts"));
				AssertEquals(isEditable, menus.Any(x => x.Caption == "Delete Existing Accreditation Attempts for this Person"));
				AssertEquals(isEditable, menus.Any(x => x.Caption == "Delete Existing Accreditation Attempts and their Related Certificates for this Person"));
			}
		}

		public void TestUnlockButton_Click()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var lockoutTime = ZDateTime.UtcNow.AddDays(1);
			person.LockOutUntil(lockoutTime);
			Factory.Save();
			AssertEquals("Precondition", true, person.IsLockedOut);

			using (var testForm = new GlbPersonFormForTest(person))
			{
				testForm.Show();
				testForm.MainTabControlExposed().SelectedIndex = 2;
				Application.DoEvents();

				testForm.UnlockButton_Click(null, EventArgs.Empty);

				AssertEquals(false, person.IsLockedOut);
			}
		}

		[RequiresSTA]
		public void TestUnlockButton_Visibility()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			AssertEquals("Precondition", false, person.IsLockedOut);

			AssertUnlockButton(false);

			var lockoutTime = ZDateTime.UtcNow.AddDays(1);
			person.LockOutUntil(lockoutTime);

			AssertUnlockButton(true);

			void AssertUnlockButton(bool isLockedOut)
			{
				using (var testForm = new GlbPersonFormForTest(person))
				{
					testForm.Show();
					testForm.MainTabControlExposed().SelectedIndex = 2;
					Application.DoEvents();

					AssertEquals(isLockedOut, testForm.UnlockButtonExposed.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestLockoutDateTimeBoundDate()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			AssertEquals("Precondition", false, person.IsLockedOut);

			AssertLockoutDateTimeBoundDate(false);

			var lockoutTime = ZDateTime.UtcNow.AddDays(1);
			person.LockOutUntil(lockoutTime);

			AssertLockoutDateTimeBoundDate(true);

			void AssertLockoutDateTimeBoundDate(bool isLocked)
			{
				using (var testForm = new GlbPersonFormForTest(person))
				{
					testForm.Show();
					testForm.MainTabControlExposed().SelectedIndex = 2;
					Application.DoEvents();

					AssertEquals(isLocked, testForm.LockoutDateTimeBoundDateExposed.Visible);
					AssertEquals(false, testForm.LockoutDateTimeBoundDateExposed.Enabled);
					AssertEquals(person.LockoutDateTimeLocal, testForm.LockoutDateTimeBoundDateExposed.DateTimeValue);
				}
			}
		}
	}
}
