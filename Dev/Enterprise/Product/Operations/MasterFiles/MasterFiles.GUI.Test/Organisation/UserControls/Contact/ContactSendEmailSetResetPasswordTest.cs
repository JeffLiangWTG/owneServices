using System;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ContactSendEmailSetResetPasswordTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
		}

		[RequiresSTA]
		public void TestSendPasswordInstructions()
		{
			var contact1DisabledEmailValid = Org.Contacts.AddNew();
			var contact1EnabledEmailValid = Org.Contacts.AddNew();
			var contactEmailInvalid = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { contact1EnabledEmailValid }, new OrgContact[] { contact1DisabledEmailValid, contactEmailInvalid });
			contactEmailInvalid.OC_Email = "";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contact1DisabledEmailValid, true);
			var expectedMsgDisabledContact = $"Instructions can only be sent to users with web access enabled.";
			AssertEquals("Msg shown about web access not enabled", expectedMsgDisabledContact, UnitTestUserNotification.Instance.LastMessage.Text);

			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contactEmailInvalid, true);
			var expectedMsgInvalidEmail = $"Please specify a valid email address for this contact before sending the password instruction.";
			AssertEquals("Msg shown about invalid email address", expectedMsgInvalidEmail, UnitTestUserNotification.Instance.LastMessage.Text);

			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contact1EnabledEmailValid, true);
			AssertEquals("Msg shown about password being successfully sent", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestSendEmailToContact()
		{
			var contactEnabledEmailValid = Org.Contacts.AddNew();
			var contactEmailInvalid = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { contactEnabledEmailValid }, new OrgContact[] { contactEmailInvalid });
			contactEmailInvalid.OC_Email = "";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ContactSendEmailSetResetPasswordInstance.SendEmailToContact(contactEmailInvalid, true);
			var expectedMsgInvalidEmail = $"An error was encountered while sending the password instruction email.";
			AssertEquals("Msg shown about invalid email address", expectedMsgInvalidEmail, UnitTestUserNotification.Instance.LastMessage.Text);

			ContactSendEmailSetResetPasswordInstance.SendEmailToContact(contactEnabledEmailValid, true);
			var expectedSuccessMsg = $"An email was sent to this contact containing the password Instruction and URL.";
			AssertEquals("Msg shown about successfully sending email", expectedSuccessMsg, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestSendEmailToContact_EmailDestinationOverride()
		{
			var contactEnabledEmailValid = Org.Contacts.AddNew();
			SetRequiredOrgContactValues(new OrgContact[] { contactEnabledEmailValid }, Array.Empty<OrgContact>());
			Factory.Save();

			var relatedContact = Factory.NewWithValidTestData<OrgContact>();
			relatedContact.OC_PER = contactEnabledEmailValid.OC_PER;
			Factory.Save();

			RawDataRegistry.Instance.EmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "sinkhole@dummy.com");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contactEnabledEmailValid, true);
			AssertEquals("Email should be sent", true, EmailSentTest());

			AssertEquals("Registry setting was overriden", 0, Db.Connection.ExecuteScalar(string.Format(
				"Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = '{0}'", "sinkhole@dummy.com")));
			AssertEquals("Registry setting was overriden", 1, Db.Connection.ExecuteScalar(string.Format(
				"Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = '{0}'", contactEnabledEmailValid.Email.ToString())));
		}

		[RequiresSTA]
		public void TestDialogResult()
		{
			var contactEnabledEmailValid = Org.Contacts.AddNew();

			SetRequiredOrgContactValues(new OrgContact[] { contactEnabledEmailValid }, Array.Empty<OrgContact>());

			Factory.Save();

			var relatedContact = Factory.NewWithValidTestData<OrgContact>();
			relatedContact.OC_PER = contactEnabledEmailValid.OC_PER;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contactEnabledEmailValid, true);
			AssertEquals("No email should be sent", true, NoEmailSentTest());
			Env.OutgoingMailManager.EmailsCreated.Clear();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contactEnabledEmailValid, true);
			AssertEquals("Email should be sent", true, EmailSentTest());
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestCreatePasswordResetInfo()
		{
			var contactEnabledEmailValid = Org.Contacts.AddNew();
			SetRequiredOrgContactValues(new OrgContact[] { contactEnabledEmailValid }, Array.Empty<OrgContact>());

			var passwordResetInfo1 = new ContactSendEmailSetResetPasswordForTest().CreatePasswordResetInfo_Exposed(PasswordInstructionType.Set, contactEnabledEmailValid);
			AssertEquals(passwordResetInfo1.ContactEmail, contactEnabledEmailValid.Email);
			AssertEquals(passwordResetInfo1.OrgCode, contactEnabledEmailValid.OrgCode);
			AssertEquals(passwordResetInfo1.EmailTemplateCompanyPk, GlbCompany.CurrentCompany.PK.ToString());

			var passwordResetInfo2 = new ContactSendEmailSetResetPasswordForTest().CreatePasswordResetInfo_Exposed(PasswordInstructionType.Reset, contactEnabledEmailValid);
			AssertEquals(passwordResetInfo2.ContactEmail, contactEnabledEmailValid.Email);
			AssertEquals(passwordResetInfo2.OrgCode, contactEnabledEmailValid.OrgCode);
			AssertEquals(passwordResetInfo2.EmailTemplateCompanyPk, GlbCompany.CurrentCompany.PK.ToString());
		}

		[RequiresSTA]
		public void TestSendEmailToContact_CompanySpecific()
		{
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });

			var contactEnabledEmailValid = Org.Contacts.AddNew();
			contactEnabledEmailValid.OC_WebAccessEnabled = true;
			contactEnabledEmailValid.SetHashedPassword("pass123");
			contactEnabledEmailValid.OC_WebAccessEnabled = true;
			contactEnabledEmailValid.OC_ContactName = "contact1";
			contactEnabledEmailValid.OC_Email = "email1@testing.com";

			SetRequiredOrgContactValues(new OrgContact[] { contactEnabledEmailValid }, Array.Empty<OrgContact>());
			Factory.Save();

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contactEnabledEmailValid.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ContactSendEmailSetResetPasswordInstance.SendPasswordInstructions(contactEnabledEmailValid, true);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Subject Company", sentEmail.Subject);
			Assert(sentEmail.Body.Contains("Test Email Body Template Company"));
		}

		bool NoEmailSentTest()
		{
			return Env.OutgoingMailManager.EmailsCreated.Count == 0;
		}

		bool EmailSentTest()
		{
			if (Env.OutgoingMailManager.EmailsCreated.Count == 1)
			{
				var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
				if (Env.CurrentCompany.Name + " Password Set" == sentEmail.Subject && sentEmail.Body.Contains("http://localhost/webtracker/Admin/SetPassword.aspx?SetKey="))
				{
					return true;
				}
			}
			return false;
		}

		static int count;
		void SetRequiredOrgContactValues(OrgContact[] enabledContacts, OrgContact[] disabledContacts)
		{
			foreach (var enabledContact in enabledContacts)
			{
				enabledContact.OC_WebAccessEnabled = true;
				enabledContact.OC_ContactName = "contact" + count;
				enabledContact.OC_Email = "email" + count + "@testing.com";
				count++;
			}
			foreach (var disabledContact in disabledContacts)
			{
				disabledContact.OC_WebAccessEnabled = false;
				disabledContact.OC_ContactName = "contact" + count;
				disabledContact.OC_Email = "email" + count + "@testing.com";
				count++;
			}
		}

		OrgHeader Org;
		ContactSendEmailSetResetPassword ContactSendEmailSetResetPassword;
		ContactSendEmailSetResetPassword ContactSendEmailSetResetPasswordInstance => ContactSendEmailSetResetPassword ?? (ContactSendEmailSetResetPassword = new ContactSendEmailSetResetPassword());

		class ContactSendEmailSetResetPasswordForTest : ContactSendEmailSetResetPassword
		{
			public PasswordResetInfo CreatePasswordResetInfo_Exposed(PasswordInstructionType passwordInstructionType, OrgContact contact)
			{
				return CreatePasswordResetInfo(passwordInstructionType, contact);
			}
		}
	}
}
