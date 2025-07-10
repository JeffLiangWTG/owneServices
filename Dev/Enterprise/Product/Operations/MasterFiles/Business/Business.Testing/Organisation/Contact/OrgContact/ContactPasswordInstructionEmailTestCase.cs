using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(ContactPasswordInstructionEmail), ExcludePrivate = true)]
	public abstract class ContactPasswordInstructionEmailTestCase<T> : HtmlFormatEmailToContactBusinessObjectTestCase<T> where T : ContactPasswordInstructionEmail
	{
		#region Default Email Address / Display Name

		protected override string DefaultFromEmailAddress => EnvProxy.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress;
		protected override string DefaultFromDisplayName => GlbStaff.CurrentUser.GS_FullName;

		#endregion Default Email Address / Display Name

		public void TestSendEmailWithResetPasswordUrl()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var source = GetNewBusinessObjectSendingEmail() as IPasswordInstructionEmailSource;
			Factory.Save();
			AssertNotNull(source);
			source.PasswordInstructionEmail.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/ResetPassword.aspx", PasswordInstructionType.Reset);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("http://webtracker/Admin/ResetPassword.aspx", sentEmail.Body);
			Assert((source as BusinessObject).GetLogs().HasLogWith((x) => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
		}

		public void TestSendEmailWithSetPasswordUrl()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var source = GetNewBusinessObjectSendingEmail() as IPasswordInstructionEmailSource;
			Factory.Save();
			AssertNotNull(source);
			source.PasswordInstructionEmail.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/SetPassword.aspx", PasswordInstructionType.Set);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("http://webtracker/Admin/SetPassword.aspx", sentEmail.Body);
			Assert((source as BusinessObject).GetLogs().HasLogWith((x) => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
		}

		public void TestSendEmailWithPasswordInstructionUrlShouldAddLog()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var source = GetNewBusinessObjectSendingEmail() as IPasswordInstructionEmailSource;
			Factory.Save();

			SetSenderForLogs(source);
			AssertNotNull(source);
			AssertNotNull(source.SenderForLogs);
			AssertNotEquals("Precondition: SenderForLogs should have been set to a different object", source.PK, source.SenderForLogs.PK);
			source.PasswordInstructionEmail.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/SetPassword.aspx", PasswordInstructionType.Set);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Precondition", "http://webtracker/Admin/SetPassword.aspx", sentEmail.Body);
			Assert("Log should be on the SenderForLogs", source.SenderForLogs.GetLogs().HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebAccessPasswordEmailSentCode));
		}

		protected abstract void SetSenderForLogs(IPasswordInstructionEmailSource source);

		public void TestEmailSubject_Reset()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			EmailContactObject.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/ResetPassword.aspx", PasswordInstructionType.Reset);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			var companyName = (EmailContactObject.BusinessObjectSendingEmail as OrgContact).BranchForLogin.CompanyName;
			AssertContains(companyName + " Password Reset", sentEmail.Subject);
		}

		public void TestEmailSet_CompanySpecific()
		{
			WebDataRegistry.Instance.PasswordSetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "nuts", EmailBody = "bolts" });
			const string companySpecificSubject = "buts";
			const string companySpecificBody = "volts";
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var contact = Factory.Load<OrgContact>(EmailContactObject.BusinessObjectSendingEmail.PK);
			WebDataRegistry.Instance.PasswordSetEmailTemplate.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });

			EmailContactObject.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/ResetPassword.aspx", PasswordInstructionType.Set);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains(companySpecificSubject, sentEmail.Subject);
			AssertContains(companySpecificBody, sentEmail.Body);
		}

		public void TestEmailReset_CompanySpecific()
		{
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "nuts", EmailBody = "bolts" });
			const string companySpecificSubject = "buts";
			const string companySpecificBody = "volts";
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var contact = Factory.Load<OrgContact>(EmailContactObject.BusinessObjectSendingEmail.PK);
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });

			EmailContactObject.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/ResetPassword.aspx", PasswordInstructionType.Reset);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains(companySpecificSubject, sentEmail.Subject);
			AssertContains(companySpecificBody, sentEmail.Body);
		}

		public void TestEmailReset_CompanySpecific_EmailTemplateCompanyPk()
		{
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "nuts", EmailBody = "bolts" });
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			var contact = Factory.Load<OrgContact>(EmailContactObject.BusinessObjectSendingEmail.PK);
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(contact.CompanyPKForLogin, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "subject1", EmailBody = "body1" });
			WebDataRegistry.Instance.PasswordResetEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject For Company", EmailBody = "Body For Company" });

			EmailContactObject.SendEmailWithPasswordInstructionUrl("http://webtracker/Admin/ResetPassword.aspx", PasswordInstructionType.Reset, companyPk);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Subject For Company", sentEmail.Subject);
			AssertContains("Body For Company", sentEmail.Body);
		}
	}
}
