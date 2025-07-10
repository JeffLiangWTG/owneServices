using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SendPasswordResetEmailForm))]
	sealed class SendPasswordResetEmailFormTest : ZFormBasherTest
	{
		public void TestBlankEmailAddress()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://test.com/Tracking/");
			var emailManager = Env.OutgoingMailManager;
			var passwordResetEmailSender = new ContactSendEmailSetResetPassword();
			var org = GlbCompany.CurrentCompany.OrgProxy;
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Receiver test";
			orgContact.OC_Email = "testreceiver@wtg.ccc";

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_LoginName = "tst";
			user.GS_FullName = "Test User";
			user.GS_EmailAddress = "";
			user.GS_IsDeveloper = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SendPasswordResetEmailForm.UseCurrentUserEmailOverriddenForTest = true;
				passwordResetEmailSender.SendEmailToContact(orgContact);
			}
			AssertEquals("Email count should be 1", emailManager.EmailsCreated.Count, 1);
			AssertEquals($"Sender email address should be default email address {Env.Registry.SMTPDefaultReturnEmailAddress}", Env.Registry.SMTPDefaultReturnEmailAddress, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromAddress);
			AssertEquals($"Sender name should be current user name {user.GS_FullName}", user.GS_FullName, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromDisplayName);
			AssertEquals($"Receiver email address should be {orgContact.OC_Email}", orgContact.OC_Email, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].Recipients[0]);
		}

		public void TestIfCorrectSenderEmailAddressSelected()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://test.com/Tracking/");
			var emailManager = Env.OutgoingMailManager;
			var passwordResetEmailSender = new ContactSendEmailSetResetPassword();
			var org = GlbCompany.CurrentCompany.OrgProxy;
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Receiver test";
			orgContact.OC_Email = "testreceiver@wtg.ccc";

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_LoginName = "tst";
			user.GS_FullName = "Test User";
			user.GS_EmailAddress = "testsender@wtg.ccc";
			user.GS_IsDeveloper = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SendPasswordResetEmailForm.UseCurrentUserEmailOverriddenForTest = true;
				passwordResetEmailSender.SendEmailToContact(orgContact);
			}
			AssertEquals("Email count should be 1", emailManager.EmailsCreated.Count, 1);
			AssertEquals($"Sender email address should be current user address {user.GS_EmailAddress}", user.GS_EmailAddress, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromAddress);
			AssertEquals($"Sender name should be current user name {user.GS_FullName}", user.GS_FullName, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromDisplayName);
			AssertEquals($"Receiver email address should be {orgContact.OC_Email}", orgContact.OC_Email, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].Recipients[0]);
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SendPasswordResetEmailForm.UseCurrentUserEmailOverriddenForTest = false;
				passwordResetEmailSender.SendEmailToContact(orgContact);
			}
			AssertEquals("Email count should be 2", emailManager.EmailsCreated.Count, 2);
			AssertNotEquals($"Sender email address should not be current user address {user.GS_EmailAddress}", user.GS_EmailAddress, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromAddress);
			AssertEquals($"Sender name should be current user name {user.GS_FullName}", user.GS_FullName, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromDisplayName);
			AssertEquals($"Sender email address should be default DoNotReply {Env.Registry.SMTPDefaultDoNotReplyEmailAddress}", Env.Registry.SMTPDefaultDoNotReplyEmailAddress, Env.OutgoingMailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].FromAddress);
			AssertEquals($"Receiver email address should be {orgContact.OC_Email}", orgContact.OC_Email, emailManager.EmailsCreated[emailManager.EmailsCreated.Count - 1].Recipients[0]);
		}

		public void TestSenderEmailLabel()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			using (var sendPasswordResetEmailForm = new SendPasswordResetEmailForm())
			{
				sendPasswordResetEmailForm.useCurrentEmailCheckBox.Checked = true; //To trigger event handler as default value is false
				sendPasswordResetEmailForm.useCurrentEmailCheckBox.Checked = false;
				ZFormModaliser.ShowDialogWithoutDispose(sendPasswordResetEmailForm);
				AssertEquals("Label should show correct email", sendPasswordResetEmailForm.fromEmailLabel.Text, $"Email will be sent from: {Env.Registry.SMTPDefaultDoNotReplyEmailAddress}");
			}

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_LoginName = "tst";
			user.GS_FullName = "Test User";
			user.GS_EmailAddress = "testsender@wtg.ccc";
			user.GS_IsDeveloper = false;
			Factory.Save();
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				using (var sendPasswordResetEmailForm = new SendPasswordResetEmailForm())
				{
					sendPasswordResetEmailForm.useCurrentEmailCheckBox.Checked = true;
					ZFormModaliser.ShowDialogWithoutDispose(sendPasswordResetEmailForm);
					AssertEquals("Label should show correct email", sendPasswordResetEmailForm.fromEmailLabel.Text, $"Email will be sent from: {user.GS_EmailAddress}");
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new SendPasswordResetEmailForm();
		}
	}
}
