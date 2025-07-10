using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class MailtoUrlEmailSenderTest : TestCaseWithFactory
	{
		public void TestMailClientNotSetupErrorShown()
		{
			TestMailtoUrlEmailSender sender = new TestMailtoUrlEmailSender(null, Factory.New<OrgHeader>());
			sender.FakeMailClientNotSetup = false;

			ISendEmailSource sup = Factory.New<OrgHeader>();
			sender.DisplayEmailAndSave(new EmailSenderConfiguration(sup));
			AssertEquals("Mail client set up ok", false, sender.FakeMailClientNotSetup);
			sender.FakeMailClientNotSetup = true;
			sender.DisplayEmailAndSave(new EmailSenderConfiguration(sup));
			AssertEquals("Mail client not set up ok should be an error", true, sender.FakeMailClientNotSetup);
		}

		public void TestMailClientRecipientsAndSubject()
		{
			var org = Factory.New<OrgHeader>();
			TestMailtoUrlEmailSender sender = new TestMailtoUrlEmailSender(null, org);
			sender.FakeMailClientNotSetup = false;

			ISendEmailSource sup = Factory.New<OrgHeader>();
			EmailSenderConfiguration selection = new EmailSenderConfiguration(sup);
			selection.HtmlEmail.Subject = "Hello!";
			selection.HtmlEmail.ToEmailAddress = "1@1.com;2@2.com";
			sender.DisplayEmailAndSave(selection);
			AssertEquals("mailto:1@1.com;2@2.com?subject=Hello!", sender.OpenedUrl);
		}

		class TestMailtoUrlEmailSender : MailtoUrlEmailSender
		{
			internal TestMailtoUrlEmailSender(ISynchronizeInvoke uIThreadSyncInvoke, ISendEmailSource contactSource)
				: base(uIThreadSyncInvoke, contactSource)
			{
			}

			public bool MailClientNotSetupErrorShown;
			public bool FakeMailClientNotSetup;

			public new void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration)
			{
				base.DisplayEmailAndSave(senderConfiguration);
			}

			protected override void ShowMailClientNotSetupError()
			{
				base.ShowMailClientNotSetupError();
				MailClientNotSetupErrorShown = true;
			}

			protected override void OpenUrl(string url)
			{
				OpenedUrl = url;
				if (FakeMailClientNotSetup)
				{
					throw new Win32Exception(2); // file not found win32 error code
				}
			}

			internal string OpenedUrl;
		}
	}
}
