using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeDuplicationInitializerTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrgMenuOnNullParentFormArgument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "ABC";

			new DeduplicationInitializer().CreateOrgMenu(null, org);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrgMenuOnNullOrgHeaderArgument()
		{
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new DeduplicationInitializer().CreateOrgMenu(dummyForm, null);
			}
		}

		public void TestFindDuplicatesMenuItemIsEnabledOrNot()
		{
			AssertFindDuplicatesMenuItemIsEnabledOrNot(false, false, false, false, false);
			AssertFindDuplicatesMenuItemIsEnabledOrNot(true, true, false, false, false);
			AssertFindDuplicatesMenuItemIsEnabledOrNot(true, false, true, false, true);
			AssertFindDuplicatesMenuItemIsEnabledOrNot(false, true, true, true, false);
		}

		void AssertFindDuplicatesMenuItemIsEnabledOrNot(bool orgEnableDuplicateDetection, bool personsEnableDuplicateDetection, bool orgIsActive, bool expectedPersonMenuItemIsActive, bool expectedOrgMenuItemIsActive)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";
			org.OH_IsActive = orgIsActive;
			var contact = org.Contacts.AddNew();
			GlbPerson.CreateFromContact(Factory, contact);

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgEnableDuplicateDetection))
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, personsEnableDuplicateDetection))
			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();

				var actionsMenuItemCollection = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull(actionsMenuItemCollection);
				var duplicatesMenuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");
				AssertEquals(expectedOrgMenuItemIsActive, duplicatesMenuItem.Enabled);

				duplicatesMenuItem.Enabled = true;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				AssertEquals(expectedPersonMenuItemIsActive, duplicatesMenuItem.Enabled);

				duplicatesMenuItem.Enabled = true;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.AddressesTabPage;
				AssertEquals(expectedOrgMenuItemIsActive, duplicatesMenuItem.Enabled);

				duplicatesMenuItem.Enabled = true;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.DetailsTabPage;
				AssertEquals(expectedOrgMenuItemIsActive, duplicatesMenuItem.Enabled);

				duplicatesMenuItem.Enabled = true;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.StmNoteTabPage;
				AssertEquals(false, duplicatesMenuItem.Enabled);
			}
		}

		public void TestFindDedupDisabledOnSecurityRightsOrgEnableDedupNotAllowed()
		{
			OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Environment.Env.Security.OrgDuplicateDetection.IsAllowed = false;

			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();

				var actionsMenuItem = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var duplicatMenuItem = actionsMenuItem.MenuItems.FindByText("Find Duplicates");

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.DetailsTabPage;
				AssertEquals(false, duplicatMenuItem.Enabled);
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.AddressesTabPage;
				AssertEquals(false, duplicatMenuItem.Enabled);
			}
		}

		public void TestFindDedupDisabledOnSecurityPersonIntelligenceDedupNotAllowed()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Environment.Env.Security.PersonIntelligenceDuplicateDetection.IsAllowed = false;

			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();

				var actionsMenuItem = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var duplicatMenuItem = actionsMenuItem.MenuItems.FindByText("Find Duplicates");

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				AssertEquals(false, duplicatMenuItem.Enabled);
			}
		}

		public void TestCreateOrgMenu()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";

			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new DeduplicationInitializer().CreateOrgMenu(dummyForm, org);
				dummyForm.Show();
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull("Check duplicate menu item exists", actionsMenuItem.MenuItems.FindByText("Find Duplicates"));
			}
		}

		public void TestNoErrorMessageAppearsUponDeduplicationWhenThereAreErrorsOnTheForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";

			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new DeduplicationInitializer().CreateOrgMenu(dummyForm, org);
				dummyForm.Show();
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				((CargoWise.EntityFramework.BusinessObject)dummyForm.CurrentDataItem).AddRowError("error");

				actionsMenuItem.MenuItems.FindByText("Find Duplicates").PerformClick();

				AssertEquals(false, Globals.Message.IsShowingError);
			}
		}

		public void TestEnableAndDisableMenuInDifferentTabPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";
			var contact = org.Contacts.AddNew();
			GlbPerson.CreateFromContact(Factory, contact);

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();

				var actionsMenuItemCollection = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull(actionsMenuItemCollection);
				var menuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");
				AssertNotNull(menuItem);
				Assert(menuItem.Enabled);

				menuItem.Enabled = false;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				Assert(menuItem.Enabled);

				menuItem.Enabled = false;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.AddressesTabPage;
				Assert(menuItem.Enabled);

				menuItem.Enabled = false;
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.DetailsTabPage;
				Assert(menuItem.Enabled);

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.StmNoteTabPage;
				Assert(!menuItem.Enabled);
			}
		}

		public void TestRunDeduplicationForOrganization()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";
			((IDeduplicatable)org).ShouldRunDeduplication = true;

			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();

				var actionsMenuItemCollection = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull(actionsMenuItemCollection);
				var menuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");
				AssertNotNull(menuItem);
				Assert(menuItem.Enabled);
				AssertOrganizationStatusLabelVisibleAndReset(orgForm, false);

				menuItem.PerformClick();
				AssertOrganizationStatusLabelVisibleAndReset(orgForm, true);

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				menuItem.PerformClick();
				AssertOrganizationStatusLabelVisibleAndReset(orgForm, false);
			}
		}

		public void TestRunDeduplicationForPerson()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var orgForm = new BaseOrganisationsForm(org))
			{
				orgForm.Show();
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage; // Init Contact Tab Page
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.DetailsTabPage;

				var actionsMenuItemCollection = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull(actionsMenuItemCollection);
				var menuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");
				AssertNotNull(menuItem);
				Assert(menuItem.Enabled);
				AssertPersonStatusLabelVisibleAndReset(orgForm, false);

				menuItem.PerformClick();
				AssertPersonStatusLabelVisibleAndReset(orgForm, false);

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				menuItem.PerformClick();
				AssertPersonStatusLabelVisibleAndReset(orgForm, false);

				var contact = org.Contacts.AddNew();
				menuItem.PerformClick();
				AssertPersonStatusLabelVisibleAndReset(orgForm, false);

				var person = GlbPerson.CreateFromContact(Factory, contact);
				orgForm.ContactsControl.HookDuplicationDetectEvents();
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				menuItem.PerformClick();
				AssertPersonStatusLabelVisibleAndReset(orgForm, true);

				orgForm.OrganisationsTabControl.SelectedTab = orgForm.DetailsTabPage;
				menuItem.PerformClick();
				AssertPersonStatusLabelVisibleAndReset(orgForm, false);
			}
		}

		void AssertOrganizationStatusLabelVisibleAndReset(BaseOrganisationsForm orgForm, bool visible)
		{
			AssertEquals(visible, orgForm.DetailsControl.NameAndAddressDetailsControl.DuplicateDetectionStatusLabel.Visible);
			orgForm.DetailsControl.NameAndAddressDetailsControl.DuplicateDetectionStatusLabel.Visible = false;
		}

		void AssertPersonStatusLabelVisibleAndReset(BaseOrganisationsForm orgForm, bool visible)
		{
			AssertEquals(visible, orgForm.ContactsControl.DuplicateDetectionStatusLabel.Visible);
			orgForm.ContactsControl.DuplicateDetectionStatusLabel.Visible = false;
		}
	}
}
