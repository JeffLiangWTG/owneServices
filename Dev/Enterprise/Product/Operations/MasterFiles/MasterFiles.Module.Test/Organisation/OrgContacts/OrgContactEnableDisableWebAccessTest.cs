using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgContactEnableDisableWebAccessTest : TestCaseWithFactory
	{
		enum TestCase { allEnabled, allDisabled, mix }

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
		}

		public void TestCheckpointDisallowed()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var enabler = new OrgContactEnableDisableWebAccess();
			var bizObj1 = Factory.New<OrgContact>();

			var checkpoint = new SecurityCheckpoint("XXX", (NoResString)"ZZZ", null, security);
			checkpoint.IsAllowed = false;
			enabler.EnableDisableWebAccess(true, new BusinessObject[] { bizObj1 }, checkpoint);

			AssertEquals("You are not allowed to Enable/Disable Web Access in this module. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Helper functions

		static int count;

		void SetRequiredOrgContactValues(OrgContact[] enabledContacts, OrgContact[] disabledContacts)
		{
			foreach (OrgContact enabledContact in enabledContacts)
			{
				enabledContact.OC_WebAccessEnabled = true;
				enabledContact.OC_ContactName = "contact" + count.ToString();
				enabledContact.OC_Email = "email" + count.ToString() + "@testing.com";
				count++;
			}
			foreach (OrgContact disabledContact in disabledContacts)
			{
				disabledContact.OC_WebAccessEnabled = false;
				disabledContact.OC_ContactName = "contact" + count.ToString();
				disabledContact.OC_Email = "email" + count.ToString() + "@testing.com";
				count++;
			}
		}

		void EnableWebAccessTest(OrgContact[] selectedObjects, TestCase testCase)
		{
			bool enable = true;
			if (testCase == TestCase.allEnabled)
			{
				Helper(selectedObjects, TestCase.allEnabled);
				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);
				Helper(selectedObjects, TestCase.allEnabled);
			}
			if (testCase == TestCase.allDisabled)
			{
				Helper(selectedObjects, TestCase.allDisabled);
				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);
				Helper(selectedObjects, TestCase.allEnabled);
			}
			if (testCase == TestCase.mix)
			{
				Assert("Precondition", selectedObjects[0].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[1].OC_WebAccessEnabled);
				Assert("Precondition", selectedObjects[2].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[3].OC_WebAccessEnabled);

				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);

				Assert("Precondition", selectedObjects[0].OC_WebAccessEnabled);
				Assert("Precondition", selectedObjects[1].OC_WebAccessEnabled);
				Assert("Precondition", selectedObjects[2].OC_WebAccessEnabled);
				Assert("Precondition", selectedObjects[3].OC_WebAccessEnabled);
			}
		}

		void DisableWebAccessTest(OrgContact[] selectedObjects, TestCase testCase)
		{
			bool enable = false;
			if (testCase == TestCase.allEnabled)
			{
				Helper(selectedObjects, TestCase.allEnabled);
				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);
				Helper(selectedObjects, TestCase.allDisabled);
			}
			if (testCase == TestCase.allDisabled)
			{
				Helper(selectedObjects, TestCase.allDisabled);
				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);
				Helper(selectedObjects, TestCase.allDisabled);
			}
			if (testCase == TestCase.mix)
			{
				Assert("Precondition", selectedObjects[0].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[1].OC_WebAccessEnabled);
				Assert("Precondition", selectedObjects[2].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[3].OC_WebAccessEnabled);

				orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, selectedObjects, EnvProxy.Instance.Security.None);

				Assert("Precondition", !selectedObjects[0].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[1].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[2].OC_WebAccessEnabled);
				Assert("Precondition", !selectedObjects[3].OC_WebAccessEnabled);
			}
		}

		void Helper(OrgContact[] selectedObjects, TestCase testCase)
		{
			if (testCase == TestCase.allEnabled)
			{
				foreach (OrgContact orgContact in selectedObjects)
				{
					Assert("Org contact should have Web Access enabled", orgContact.OC_WebAccessEnabled);
				}
			}
			else
			{
				foreach (OrgContact orgContact in selectedObjects)
				{
					Assert("Org contact should have Web Access disabled", !orgContact.OC_WebAccessEnabled);
				}
			}
		}

		#endregion

		#region Test Enable Disable Web Access OC_WebAccessValue

		public void TestEnableDisableWebAccessBase()
		{
			var enabled1 = Org.Contacts.AddNew();
			var enabled2 = Org.Contacts.AddNew();
			var enabled3 = Org.Contacts.AddNew();
			var enabled4 = Org.Contacts.AddNew();
			var enabled5 = Org.Contacts.AddNew();
			var enabled6 = Org.Contacts.AddNew();
			var disabled1 = Org.Contacts.AddNew();
			var disabled2 = Org.Contacts.AddNew();
			var disabled3 = Org.Contacts.AddNew();
			var disabled4 = Org.Contacts.AddNew();
			var disabled5 = Org.Contacts.AddNew();
			var disabled6 = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { enabled1, enabled2, enabled3, enabled4, enabled5, enabled6 }, new OrgContact[] { disabled1, disabled2, disabled3, disabled4, disabled5, disabled6 });

			Factory.Save();

			var collectionAllEnabled = new OrgContact[] { enabled2, enabled3, enabled4 };
			var collectionAllDisabled = new OrgContact[] { disabled2, disabled3, disabled4 };
			var collectionMix = new OrgContact[] { enabled5, disabled5, enabled6, disabled6 };

			EnableWebAccessTest(new OrgContact[] { enabled1 }, TestCase.allEnabled);
			EnableWebAccessTest(collectionAllEnabled, TestCase.allEnabled);
			EnableWebAccessTest(new OrgContact[] { disabled1 }, TestCase.allDisabled);
			EnableWebAccessTest(collectionAllDisabled, TestCase.allDisabled);
			EnableWebAccessTest(collectionMix, TestCase.mix);

			SetRequiredOrgContactValues(new OrgContact[] { enabled1, enabled2, enabled3, enabled4, enabled5, enabled6 }, new OrgContact[] { disabled1, disabled2, disabled3, disabled4, disabled5, disabled6 });

			DisableWebAccessTest(new OrgContact[] { enabled1 }, TestCase.allEnabled);
			DisableWebAccessTest(collectionAllEnabled, TestCase.allEnabled);
			DisableWebAccessTest(new OrgContact[] { disabled1 }, TestCase.allDisabled);
			DisableWebAccessTest(collectionAllDisabled, TestCase.allDisabled);
			DisableWebAccessTest(collectionMix, TestCase.mix);
		}

		public void TestEnableDisableWebAccessOC_EmailError()
		{
			var disabled1NoEmail = Org.Contacts.AddNew();
			var disabled2WithEmail = Org.Contacts.AddNew();
			var disabled3NoEmail = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(Array.Empty<OrgContact>(), new OrgContact[] { disabled1NoEmail, disabled2WithEmail, disabled3NoEmail });

			disabled1NoEmail.OC_Email = "";
			disabled3NoEmail.OC_Email = "";

			Factory.Save();

			var enable = true;
			var disable = false;

			AssertEquals("Precondition: contact must have OC_WebAccess disabled to enable it", disabled1NoEmail.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { disabled1NoEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Cannot enable Web Access if org contact has invalid/no email", disabled1NoEmail.OC_WebAccessEnabled, false);

			AssertEquals("Precondition", disabled2WithEmail.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", disabled3NoEmail.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { disabled2WithEmail, disabled3NoEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Can enable Web Access of disabled org contact with valid email", disabled2WithEmail.OC_WebAccessEnabled, true);
			AssertEquals("Cannot enable Web Access if org contact has invalid/no email", disabled3NoEmail.OC_WebAccessEnabled, false);

			var enabled1NoEmail = disabled1NoEmail;
			enabled1NoEmail.OC_WebAccessEnabled = true;
			var enabled2WithEmail = disabled2WithEmail;
			var enabled3NoEmail = disabled3NoEmail;
			enabled3NoEmail.OC_WebAccessEnabled = true;

			AssertEquals("Precondition: contact must have OC_WebAccess enabled to disable it", enabled1NoEmail.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, new OrgContact[] { enabled1NoEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Test OC_Email has no affect on disabling Web Access", disabled1NoEmail.OC_WebAccessEnabled, false);

			AssertEquals("Precondition", enabled3NoEmail.OC_WebAccessEnabled, true);
			AssertEquals("Precondition", enabled2WithEmail.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, new OrgContact[] { enabled2WithEmail, enabled3NoEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Test OC_Email has no affect on disabling Web Access", enabled2WithEmail.OC_WebAccessEnabled, false);
			AssertEquals("Test OC_Email has no affect on disabling Web Access", enabled3NoEmail.OC_WebAccessEnabled, false);
		}

		public void TestEnableDisableWebAccessOC_EmailConstraintTest()
		{
			var contact1SameEmail = Org.Contacts.AddNew();
			var contact2SameEmail = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(Array.Empty<OrgContact>(), new OrgContact[] { contact1SameEmail, contact2SameEmail });
			var contact2OldEmail = contact2SameEmail.OC_Email;
			contact2SameEmail.OC_Email = contact1SameEmail.OC_Email;

			Factory.Save();

			var enable = true;

			AssertEquals("Test that duplicate values of OC_Email has no impact on enabling Web Access unless one of these contacts already has Web Access enabled", contact1SameEmail.OC_Email, contact2SameEmail.OC_Email);
			AssertEquals("Precondition", contact1SameEmail.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", contact2SameEmail.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { contact1SameEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Postcondition", contact1SameEmail.OC_WebAccessEnabled, true);

			AssertEquals("Precondition: OC_Email is the same between contacts and one of these contacts has Web Access enabled", contact1SameEmail.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { contact2SameEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Contact2 should not have Web Access enabled due to the OC_Email constraint, which is now violated", contact2SameEmail.OC_WebAccessEnabled, false);

			contact2SameEmail.OC_Email = contact2OldEmail;
			AssertNotEquals("Precondition that two contacts have different emails", contact2SameEmail.OC_Email, contact1SameEmail.OC_Email);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { contact2SameEmail }, EnvProxy.Instance.Security.None);
			AssertEquals("Expect contact2 to have Web Access enabled as OC_Email constraint no longer violated", contact2SameEmail.OC_WebAccessEnabled, true);
		}

		public void TestDisplayedPromptWhenTryingToEnableWebAccessForNonActiveUsers()
		{
			var contact1 = Org.Contacts.AddNew();
			var contact2 = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(Array.Empty<OrgContact>(), new OrgContact[] { contact1, contact2 });
			contact2.OC_IsActive = false;
			contact2.OC_Email = "email2@testing.com";

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(true, new OrgContact[] { contact1, contact2 }, EnvProxy.Instance.Security.None);
			AssertEquals(true, contact2.OC_IsActive);
			AssertEquals("There are some contacts marked as inactive. Would you like to continue to activate and enable web access?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			contact2.OC_IsActive = false;
			Factory.Save();

			orgContactEnableDisableWebAccess.EnableDisableWebAccess(false, new OrgContact[] { contact1, contact2 }, EnvProxy.Instance.Security.None);
			AssertNotEquals("There are some contacts marked as inactive. Would you like to continue to activate and enable web access?", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(true, new OrgContact[] { contact1, contact2 }, EnvProxy.Instance.Security.None);
			AssertEquals(false, contact2.OC_IsActive);
			AssertEquals("There are some contacts marked as inactive. Would you like to continue to activate and enable web access?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, contact2.OC_WebAccessEnabled);
			AssertEquals(false, contact1.OC_WebAccessEnabled);
		}

		#endregion

		#region Test Displayed Messages

		public void TestDisplayedMsgsOC_WebAccessCanBeChanged()
		{
			var enabled1 = Org.Contacts.AddNew();
			var enabled2 = Org.Contacts.AddNew();
			var enabled3 = Org.Contacts.AddNew();
			var enabled4 = Org.Contacts.AddNew();
			var disabled1 = Org.Contacts.AddNew();
			var disabled2 = Org.Contacts.AddNew();
			var disabled3 = Org.Contacts.AddNew();
			var disabled4 = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { enabled1, enabled2, enabled3, enabled4 }, new OrgContact[] { disabled1, disabled2, disabled3, disabled4 });

			Factory.Save();

			var collectionAllEnabled = new OrgContact[] { enabled2, enabled3, enabled4 };
			var collectionAllDisabled = new OrgContact[] { disabled2, disabled3, disabled4 };

			var enable = true;
			var disable = false;

			AssertEquals("Precondition : to enable the contact, it must have Web Access disabled", disabled1.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { disabled1 }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[2].Text, "You are about to enable Web Access for 1 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to send emails containing instructions to set/reset passwords to the selected contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "1 contacts have had Web Access enabled.");
			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals("Precondition", disabled2.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", disabled3.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", disabled4.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, collectionAllDisabled, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[2].Text, "You are about to enable Web Access for 3 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to send emails containing instructions to set/reset passwords to the selected contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "3 contacts have had Web Access enabled.");
			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals("Precondition : to disable the contact, it must have Web Access enabled", enabled1.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, new OrgContact[] { enabled1 }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to disable Web Access for 1 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "1 contacts have had Web Access disabled.");
			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals("Precondition", enabled2.OC_WebAccessEnabled, true);
			AssertEquals("Precondition", enabled3.OC_WebAccessEnabled, true);
			AssertEquals("Precondition", enabled4.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, collectionAllEnabled, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to disable Web Access for 3 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "3 contacts have had Web Access disabled.");
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestDisplayedMsgsOC_WebAccessNoChanges()
		{
			var enabled1 = Org.Contacts.AddNew();
			var enabled2 = Org.Contacts.AddNew();
			var enabled3 = Org.Contacts.AddNew();
			var enabled4 = Org.Contacts.AddNew();
			var disabled1 = Org.Contacts.AddNew();
			var disabled2 = Org.Contacts.AddNew();
			var disabled3 = Org.Contacts.AddNew();
			var disabled4 = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { enabled1, enabled2, enabled3, enabled4 }, new OrgContact[] { disabled1, disabled2, disabled3, disabled4 });

			Factory.Save();

			var collectionAllEnabled = new OrgContact[] { enabled2, enabled3, enabled4 };
			var collectionAllDisabled = new OrgContact[] { disabled2, disabled3, disabled4 };

			var enable = true;
			var disable = false;

			AssertEquals("Precondition : make sure there is no change to OC_WebAccess", disabled1.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, new OrgContact[] { disabled1 }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[0].Text, System.Environment.NewLine + "The selected contacts are already disabled. No changes were made.");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals("Precondition", disabled2.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", disabled3.OC_WebAccessEnabled, false);
			AssertEquals("Precondition", disabled4.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, collectionAllDisabled, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "The selected contacts are already disabled. No changes were made.");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals("Precondition ", enabled1.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { enabled1 }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "The selected contacts are already enabled. No changes were made.");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals("Precondition", enabled2.OC_WebAccessEnabled, true);
			AssertEquals("Precondition", enabled3.OC_WebAccessEnabled, true);
			AssertEquals("Precondition", enabled4.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, collectionAllEnabled, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "The selected contacts are already enabled. No changes were made.");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDisplayedMsgsOC_WebAccessMix()
		{
			var enabled1 = Org.Contacts.AddNew();
			var enabled2 = Org.Contacts.AddNew();
			var disabled1 = Org.Contacts.AddNew();
			var disabled2 = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { enabled1, enabled2 }, new OrgContact[] { disabled1, disabled2 });

			Factory.Save();

			var enable = true;
			var disable = false;

			var collectionMix = new OrgContact[] { enabled1, enabled2, disabled1, disabled2 };
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, collectionMix, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to disable Web Access for 4 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "2 contacts have had Web Access disabled.");
			UnitTestUserNotification.Instance.ClearMessages();

			enabled1.OC_WebAccessEnabled = true;
			enabled2.OC_WebAccessEnabled = true;

			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, collectionMix, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[2].Text, "You are about to enable Web Access for 4 contacts.");
			AssertEquals(UnitTestUserNotification.Instance.PreviousMessages[1].Text, "You are about to send emails containing instructions to set/reset passwords to the selected contacts.");
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "2 contacts have had Web Access enabled.");
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestDisplayedMsgsOC_WebAccessError()
		{
			var contactNoEmail = Org.Contacts.AddNew();
			var contact1SameEmail = Org.Contacts.AddNew();
			var contact2SameEmail = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(Array.Empty<OrgContact>(), new OrgContact[] { contactNoEmail, contact1SameEmail, contact2SameEmail });
			contactNoEmail.OC_Email = "";
			contact1SameEmail.OC_Email = contact2SameEmail.OC_Email;
			contact1SameEmail.OC_WebAccessEnabled = true;
			contactNoEmail.OC_ContactName = "contactABC";
			contact2SameEmail.OC_ContactName = "contactDEF";

			Factory.Save();

			var enable = true;

			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { contactNoEmail }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "The following contacts could not be enabled as they have an invalid email address:" + System.Environment.NewLine + "contactABC" + System.Environment.NewLine);
			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals("Precondition for OC_Email constraint to be violated", contact1SameEmail.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new OrgContact[] { contact2SameEmail }, EnvProxy.Instance.Security.None);
			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, System.Environment.NewLine + "The following contacts could not be enabled as Web Access can only be enabled once per email:" + System.Environment.NewLine + "contactDEF" + System.Environment.NewLine);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		public void TestEmailSetResetPassword()
		{
			var enabledContact = Org.Contacts.AddNew();
			var disabledContact1 = Org.Contacts.AddNew();
			var disabledContact2 = Org.Contacts.AddNew();
			SetRequiredOrgContactValues(new OrgContact[] { enabledContact }, new OrgContact[] { disabledContact1, disabledContact2 });

			Factory.Save();

			var enable = true;
			var disable = false;

			AssertEquals("Precondition", enabledContact.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new BusinessObject[] { enabledContact }, EnvProxy.Instance.Security.None);
			NoEmailSentTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("Precondition", enabledContact.OC_WebAccessEnabled, true);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(disable, new BusinessObject[] { enabledContact }, EnvProxy.Instance.Security.None);
			NoEmailSentTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("Precondition", disabledContact1.OC_WebAccessEnabled, false);
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new BusinessObject[] { disabledContact1 }, EnvProxy.Instance.Security.None);
			AssertEquals("Postcondition", disabledContact1.OC_WebAccessEnabled, true);
			SetEmailSentTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emailSender = new ContactSendEmailSetResetPassword();
			disabledContact2.OC_WebAccessEnabled = true;
			AssertEquals("Precondition that we are able to send an email containing set password instructions to contact", true, emailSender.SendPasswordInstructions(disabledContact2, false));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			disabledContact2.OC_WebAccessEnabled = false;
			AssertEquals("Precondition that contact does not have Web Access enabled", disabledContact2.OC_WebAccessEnabled, false);
			disabledContact2.Logs.AddNew(AutoEvents.WebAccessPasswordChanged);
			Factory.Save();
			orgContactEnableDisableWebAccess.EnableDisableWebAccess(enable, new BusinessObject[] { disabledContact2 }, EnvProxy.Instance.Security.None);
			AssertEquals("Postcondition", disabledContact2.OC_WebAccessEnabled, true);
			ResetEmailSentTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		void SetEmailSentTest()
		{
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
			AssertContains("http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey=", sentEmail.Body);
		}

		void ResetEmailSentTest()
		{
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(Env.CurrentCompany.Name + " Password Reset", sentEmail.Subject);
			AssertContains("http://localhost/webtracker/Admin/ResetMasterPassword.aspx?ResetKey=", sentEmail.Body);
		}

		void NoEmailSentTest()
		{
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		OrgContactEnableDisableWebAccess orgContactEnableDisableWebAccess => organisationContactEnableDisableWebAccess ?? (organisationContactEnableDisableWebAccess = new OrgContactEnableDisableWebAccess());
		OrgContactEnableDisableWebAccess organisationContactEnableDisableWebAccess;
		OrgHeader Org;
	}
}
