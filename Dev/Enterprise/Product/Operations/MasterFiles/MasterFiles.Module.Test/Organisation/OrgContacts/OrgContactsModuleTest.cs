using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsModule))]
	sealed class OrgContactsModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (OrgContactsModule module = new OrgContactsModule())
			{
				AssertEquals(ModuleIDs.OrgContacts, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgContacts;
		}

		public void TestCheckpoints()
		{
			using (OrgContactsModule module = new OrgContactsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.OrgContact, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (OrgContactsModuleForTest module = new OrgContactsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgContactsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (OrgContactsModuleForTest module = new OrgContactsModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is OrgContactCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (OrgContactsModuleForTest module = new OrgContactsModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgContactsFilterBusinessObject);
			}
		}

		public void TestActivateDeactivateMenuItems()
		{
			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				AssertEquals("Precondition", module.AllowDefaultActivateDeactivate, true);
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var activateMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Activate");
				var deactivateMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate");

				AssertNotNull("Activate option in menu item under Actions should exist.", activateMenuItem);
				AssertNotNull("Deactivate option in menu item under Actions should exist", deactivateMenuItem);
			}
		}

		public void TestCheckpointActivateDeactivate()
		{
			Env.Security.OrgContactModify.IsAllowed = false;
			Factory.Save();
			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowActivateDeactivate(true);

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var activateMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Activate");
				var deactivateMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate");

				activateMenuItem.PerformClick();
				AssertEquals("You are not allowed to Activate/Deactivate in this module. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				deactivateMenuItem.PerformClick();
				AssertEquals("You are not allowed to Activate/Deactivate in this module. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowAdvancedDataAutomationWizard()
		{
			using (var module = new OrgContactsModuleForTest())
			using (GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				AssertNotNull("Precondition: ", module.FormActionMenu.SingleOrDefault(am => am.Text == "&Actions"));
				AssertEquals(true, module.AllowAdvancedDataAutomationWizardExposed);

				var dataTransferMenu = module.DataTransferMenuItem;
				dataTransferMenu.OnPopup(EventArgs.Empty);
				var glowIntegrationMenuItems = module.DataTransferMenuItem.MenuItems.Find("GlowIntegrationMenuItem", true);
				AssertNotNull("GlowIntegrationMenuItems exists", glowIntegrationMenuItems);

				var dataAutomationMenuItem = glowIntegrationMenuItems.SingleOrDefault(gm => gm.Text == "Advanced Data Automation Wizard");
				AssertNotNull("Contact module should have Advanced Data Automation Wizard enabled", dataAutomationMenuItem);
			}
		}

		#endregion

		#region WebAccess

		public void TestEnableDisableWebAccessMenuItems()
		{
			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Enable Disable Web Access Menu Items";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var enableWebAccessMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Enable Web Access");
				var disableWebAccessMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Disable Web Access");

				AssertNotNull("Enable Web Access option in menu item under Actions should exist.", enableWebAccessMenuItem);
				AssertNotNull("Disable Web Access option in menu item under Actions should exist.", disableWebAccessMenuItem);
			}

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(false);
				form.Text = "Test Enable Disable Web Access Menu Items";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var enableWebAccessMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Enable Web Access");
				var disableWebAccessMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Disable Web Access");

				AssertNull("Enable Web Access option in menu item under Actions should not exist.", enableWebAccessMenuItem);
				AssertNull("Disable Web Access option in menu item under Actions should not exist.", disableWebAccessMenuItem);
			}
		}

		#endregion

		#region DeactivateWithRedirection

		public void TestDeactivateWithRedirection()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 2";
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 1";
			contact3.OC_IsActive = true;
			contact3.OC_PER = contact1.OC_PER;
			contact3.OC_WebAccessEnabled = true;
			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "Mr 2";
			contact4.OC_IsActive = false;
			contact4.OC_PER = contact2.OC_PER;
			contact4.OC_WebAccessEnabled = true;
			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Deactivate and Supersede Web Access Menu Item";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var deactivateWithRedirectionMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate and Supersede Web Access");

				module.SetSelectedBusinessObjects(new[] { contact1, contact2 });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Should not be deactivated with redirection since no was selected", false, contact1.WebAccessSuperseded);
				AssertEquals("Should not be deactivated with redirection since no was selected", true, contact1.OC_IsActive);
				AssertEquals("Should not be deactivated with redirection since no was selected", false, contact2.WebAccessSuperseded);
				AssertEquals("Should not be deactivated with redirection since no was selected", true, contact2.OC_IsActive);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Should be deactivated with redirection since yes was selected", true, contact1.WebAccessSuperseded);
				AssertEquals("Should be deactivated with redirection since yes was selected", false, contact1.OC_IsActive);
				AssertEquals("Should be hard deactivated since its other web access contact was inactive", false, contact2.WebAccessSuperseded);
				AssertEquals("Should be hard deactivated since its other web access contact was inactive", false, contact2.OC_IsActive);
				AssertEquals("Should display message for contacts without other active web access contacts", FormattableString.Invariant($"The following contacts will be deactivated (without superseding their web access) because there are no other active web access contacts for the Person:\r\n{contact2.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeactivateWithRedirectionShouldSkipInactiveContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 3";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = false;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 4";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_ContactName = "Mr 1";
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = contact1.OC_PER;
			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Deactivate and Supersede Web Access Menu Item";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var deactivateWithRedirectionMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate and Supersede Web Access");

				module.SetSelectedBusinessObjects(new[] { contact1, contact2, contact3 });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Nothing should change since cancel was clicked", false, contact1.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", true, contact1.OC_IsActive);
				AssertEquals("Nothing should change since cancel was clicked", false, contact2.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", false, contact2.OC_IsActive);
				AssertEquals("Nothing should change since cancel was clicked", false, contact3.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", false, contact3.OC_IsActive);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Should be deactivated with redirection", true, contact1.WebAccessSuperseded);
				AssertEquals("Should be deactivated with redirection", false, contact1.OC_IsActive);
				AssertEquals("Should stay hard deactivated", false, contact2.WebAccessSuperseded);
				AssertEquals("Should stay hard deactivated", false, contact2.OC_IsActive);
				AssertEquals("Should stay hard deactivated", false, contact3.WebAccessSuperseded);
				AssertEquals("Should stay hard deactivated", false, contact3.OC_IsActive);

				AssertEquals("Should display message for skipped inactive contacts", FormattableString.Invariant($"The following contacts will be skipped because they are already inactive:\r\n{contact2.OC_ContactName}\r\n{contact3.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeactivateWithRedirectionShouldDeactivateContactsWithNoOtherWebAccessContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 2";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 3";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Deactivate and Supersede Web Access Menu Item";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var deactivateWithRedirectionMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate and Supersede Web Access");

				module.SetSelectedBusinessObjects(new[] { contact1, contact2, contact3 });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Nothing should change since cancel was clicked", false, contact1.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", true, contact1.OC_IsActive);
				AssertEquals("Nothing should change since cancel was clicked", false, contact2.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", true, contact2.OC_IsActive);
				AssertEquals("Nothing should change since cancel was clicked", false, contact3.WebAccessSuperseded);
				AssertEquals("Nothing should change since cancel was clicked", false, contact3.OC_IsActive);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Should be hard deactivated", false, contact1.WebAccessSuperseded);
				AssertEquals("Should be hard deactivated", false, contact1.OC_IsActive);
				AssertEquals("Should be hard deactivated", false, contact2.WebAccessSuperseded);
				AssertEquals("Should be hard deactivated", false, contact2.OC_IsActive);
				AssertEquals("Nothing should change since contact is inactive", false, contact3.WebAccessSuperseded);
				AssertEquals("Nothing should change since contact is inactive", false, contact3.OC_IsActive);

				AssertEquals("Should display message for contacts without other web access contacts", FormattableString.Invariant($"The following contacts will be deactivated (without superseding their web access) because there are no other active web access contacts for the Person:\r\n{contact1.OC_ContactName}\r\n{contact2.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeactivateShouldRemoveSupersededFlag()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 3";
			contact2.OC_WebAccessEnabled = true;
			contact2.SupersedeWebAccess();
			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Deactivate Menu Item";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var deactivateWithRedirectionMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Deactivate");

				module.SetSelectedBusinessObjects(new[] { contact1, contact2 });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				deactivateWithRedirectionMenuItem.PerformClick();

				AssertEquals("Should be deactivated", false, contact1.OC_IsActive);
				AssertEquals("Should hard deactivate", false, contact2.WebAccessSuperseded);
				AssertEquals("Should hard deactivate", false, contact2.OC_IsActive);
			}
		}

		#endregion

		#region Send Password Instructions

#if !WINZOR
		[TestDate(2023, 05, 18)]
		public void TestSendPasswordInstructions()
		{
			var contactNormal = Factory.NewWithValidTestData<OrgContact>();
			contactNormal.OC_ContactName = "contactNormal";
			contactNormal.OC_IsActive = true;
			contactNormal.OC_Email = "contactNormal@test.com";
			contactNormal.OC_WebAccessEnabled = true;

			var contactInactive = Factory.NewWithValidTestData<OrgContact>();
			contactInactive.OC_ContactName = "contactInactive";
			contactInactive.OC_IsActive = false;
			contactInactive.OC_Email = "contactInactive@test.com";
			contactInactive.OC_WebAccessEnabled = true;

			var contactNoEmail = Factory.NewWithValidTestData<OrgContact>();
			contactNoEmail.OC_ContactName = "contactNoEmail";
			contactNoEmail.OC_IsActive = true;
			contactNoEmail.OC_Email = "";
			contactNoEmail.OC_WebAccessEnabled = true;

			var contactNoWebAccess = Factory.NewWithValidTestData<OrgContact>();
			contactNoWebAccess.OC_ContactName = "contactNoWebAccess";
			contactNoWebAccess.OC_IsActive = true;
			contactNoWebAccess.OC_Email = "contactNoWebAccess@test.com";
			contactNoWebAccess.OC_WebAccessEnabled = false;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contactNormal.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

			using (var form = new ZChildForm())
			using (var module = new OrgContactsModuleForTest())
			{
				module.SetAllowEdit(true);
				form.Text = "Test Send Password Instructions Menu Item";
				form.Show();

				var menuItems = module.GetNewAdditionalMenuItemsExposed();
				var sendPasswordInstructionsMenuItem = menuItems.FindByText("&Actions").MenuItems.FindByText("&Send Password Instructions");
				AssertNotNull("Precondition: Send Password Instructions Menu Item should not be null", sendPasswordInstructionsMenuItem);

				module.SetSelectedBusinessObjects(new[] { contactNormal, contactInactive, contactNoEmail, contactNoWebAccess });
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendPasswordInstructionsMenuItem.PerformClick();

				AssertEquals("outgoing emails count should be 0", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("contactNormal Password Instruction Last Sent Time should be empty", ZDateTime.Empty, contactNormal.PasswordInstructionLastSentTime);
				AssertEquals("contactNormal Password Instruction Sent By should be empty", "", contactNormal.PasswordInstructionSentBy);
				AssertEquals("contactInactive Password Instruction Last Sent Time should be empty", ZDateTime.Empty, contactInactive.PasswordInstructionLastSentTime);
				AssertEquals("contactInactive Password Instruction Sent By should be empty", "", contactInactive.PasswordInstructionSentBy);
				AssertEquals("contactNoEmail Password Instruction Last Sent Time should be empty", ZDateTime.Empty, contactNoEmail.PasswordInstructionLastSentTime);
				AssertEquals("contactNoEmail Password Instruction Sent By should be empty", "", contactNoEmail.PasswordInstructionSentBy);
				AssertEquals("contactNoWebAccess Password Instruction Last Sent Time should be empty", ZDateTime.Empty, contactNoWebAccess.PasswordInstructionLastSentTime);
				AssertEquals("contactNoWebAccess Password Instruction Sent By should be empty", "", contactNoWebAccess.PasswordInstructionSentBy);

				ZString expectedText = @"Send Password Instructions failed. Selected contacts are marked inactive below.
contactInactive
Selected contacts with disabled Web Access below.
contactNoWebAccess
Selected contacts with blank email address below.
contactNoEmail
";
				AssertEquals("a message should be shown when contact validation fails.", UnitTestUserNotification.Instance.LastMessage.Text, expectedText);

				module.SetSelectedBusinessObjects(new[] { contactNormal });
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendPasswordInstructionsMenuItem.PerformClick();

				AssertEquals("outgoing emails count should be 1", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("contactNormal Password Instruction Last Sent Time should be 2023-05-18", new ZDateTime(2023, 05, 18).ToLocalBranchTime(), contactNormal.PasswordInstructionLastSentTime);
				AssertEquals("contactNormal Password Instruction Sent By should be CargoWise One Support", Env.CurrentUser.FullName, contactNormal.PasswordInstructionSentBy);
			}
		}
#endif

		#endregion

		#region OrgContactsModuleForTest

		public class OrgContactsModuleForTest : OrgContactsModule
		{
			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}

			public MenuItem[] GetNewAdditionalMenuItemsExposed()
			{
				return GetNewAdditionalMenuItems();
			}

			public void SetAllowActivateDeactivate(bool value)
			{
				fAllowActivateDeactivate = value;
			}

			bool fAllowActivateDeactivate = true;

			public override bool AllowDefaultActivateDeactivate
			{
				get { return fAllowActivateDeactivate; }
			}

			public SecurityCheckpoint GetModuleSecurityCheckpointForActivateDeactivate(BusinessObject[] selectedObjects)
			{
				return base.GetCheckpointForActivateDeactivate(selectedObjects);
			}

			public override BusinessObject[] GetSelectedBusinessObjects()
			{
				return selectedContacts == null ? new BusinessObject[] { Factory.New<OrgContact>() } : selectedContacts.Cast<BusinessObject>().ToArray();
			}

			public void SetSelectedBusinessObjects(OrgContact[] contacts)
			{
				selectedContacts = contacts;
			}

			OrgContact[] selectedContacts;

			public void SetAllowEdit(bool value)
			{
				fAllowEdit = value;
			}

			public override bool AllowEdit => fAllowEdit;

			bool fAllowEdit = true;

			public bool AllowAdvancedDataAutomationWizardExposed => AllowAdvancedDataAutomationWizard;
		}

		#endregion

	}
}
