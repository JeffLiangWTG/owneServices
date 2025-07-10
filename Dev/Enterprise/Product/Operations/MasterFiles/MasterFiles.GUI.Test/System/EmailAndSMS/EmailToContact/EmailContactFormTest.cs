using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EmailContactForm))]
	sealed class EmailContactFormTest : ZFormBasherTest
	{
		public void TestCloseButtonNameOverride()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();
				AssertEquals("Close button name should be default", "Close", form.CloseButton_Exposed.Text);
			}

			using (var form = new DummyEmailContactFormWithButtonNameOverride(emailContactObject))
			{
				form.Show();
				AssertEquals("Close button name should be overridden", "Howdy", form.CloseButton_Exposed.Text);
			}
		}

		public void TestCurrent()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();
				AssertEquals("Email Contact Object should be current", emailContactObject, ((IEmailAttachmentForm)form).Current);
			}
		}

		public void TestSendEmail()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();
				AssertEquals("Precondition: EmailContactObject's SendEmail method should not have been called yet.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Precondition: Form should not be closed yet.", false, form.IsOnClosingCalled);

				form.SendButton.PerformClick();
				AssertEquals("EmailContactObject's SendEmail method should have been called.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Form should be closed.", true, form.IsOnClosingCalled);
				AssertEquals("Dialog result should be OK", DialogResult.OK, form.DialogResult);
			}
		}

		[RequiresSTA]
		public void TestClose()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();
				form.CloseButton_Exposed.PerformClick();
				Assert("Form should be closed.", form.IsOnClosingCalled);
				AssertEquals("Dialog result should be Cancel", DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestCannotSendEmailIfHasErrors()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();

				emailContactObject.FromEmailAddress = "";
				AssertEquals("Precondition: There should not be any error messages yet.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Precondition: EmailContactObject's SendEmail method should not have been called yet.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Precondition: Form should not be closed yet.", false, form.IsOnClosingCalled);

				form.SendButton.PerformClick();
				AssertEquals("An error message should be shown.", "Please correct all errors first before sending this Email.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("EmailContactObject's SendEmail method should not have been called.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Form should not be closed.", false, form.IsOnClosingCalled);
			}
		}

		public void TestSendEmailCatchesIOException()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			using (var attachment = TempFile.New())
			using (var lockedAttachmentStream = File.OpenWrite(attachment.Filename))
			{
				form.Show();
				emailContactObject.AddAttachment(attachment.Filename);
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.SendButton.PerformClick();
				AssertEquals("An error message should be shown.", string.Format(@"An error occurred while sending the email:

The process cannot access the file '{0}' because it is being used by another process.

Please check that all attachments are not currently being used by other applications.", attachment.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendEmailCatchesSaveException()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var dummy1 = Factory.New<DummyBizOWithRelatedNotes>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 1;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBizOWithRelatedNotes>(dummy1.PK);
			dummy2.Z0_Number = 10;
			factory2.Save();

			dummy1.Z0_Number = 5;

			var emailDummy = new EmailToContactBusinessObject(dummy1);
			emailDummy.FromEmailAddress = "Default@edi.com.au";
			emailDummy.Subject = "test email";
			emailDummy.ToEmailAddress = "testreceiver@cargowise.com";
			emailDummy.ShouldAddNoteAndEvent = false;

			using (var mainForm = new ZForm(dummy1))
			using (var emailForm = new DummyEmailContactForm(emailDummy))
			{
				emailForm.Show();
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				emailForm.SendButton.PerformClick();
				AssertStartsWith("The concurrency resolver dialog should be shown.", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Email has been sent nevertheless", 1, Enterprise.Environment.Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Email Contact form has been closed to show the form of the Dummy object", true, emailForm.IsDisposed);
				AssertEquals("The form of the Dummy object is left open", true, !mainForm.IsDisposed);
			}
		}

		public void TestPreview()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			{
				form.Show();
				form.PreviewButton.PerformClick();
				AssertEquals("Preview form should be shown", typeof(RichTextEmailDisplayZForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestPreviewEmailCatchesIOException()
		{
			var emailContactObject = GetPrepopulatedEmailToContactBusinessObject();

			using (var form = new DummyEmailContactForm(emailContactObject))
			using (var attachment = TempFile.New())
			using (var lockedAttachmentStream = File.OpenWrite(attachment.Filename))
			{
				form.Show();
				emailContactObject.AddAttachment(attachment.Filename);

				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PreviewButton.PerformClick();
				AssertEquals("An error message should be shown.", string.Format(@"An error occurred while previewing the email:

The process cannot access the file '{0}' because it is being used by another process.

Please check that all attachments are not currently being used by other applications.", attachment.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		EmailToContactBusinessObject GetPrepopulatedEmailToContactBusinessObject()
		{
			var bizo = new EmailToContactBusinessObject(Factory.New<DummyBizOWithRelatedNotes>());
			bizo.Cc = "cc@cc.com";
			bizo.FromEmailAddress = "Default@edi.com.au";
			bizo.Priority = "MED";
			bizo.ToEmailAddress = "to@to.com";

			return bizo;
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bizO = Factory.New<DummyBizOWithRelatedNotes>();
			return new EmailContactForm(new EmailToContactBusinessObject(bizO));
		}

		#region class DummyEmailContactForm

		class DummyEmailContactFormWithButtonNameOverride : DummyEmailContactForm
		{
			public DummyEmailContactFormWithButtonNameOverride(EmailToContactBusinessObject emailToContactBusinessObject) : base(emailToContactBusinessObject)
			{
			}

			protected override string CloseButtonText => "Howdy";
		}

		internal class DummyEmailContactForm : EmailContactForm
		{
			public DummyEmailContactForm(EmailToContactBusinessObject emailToContactBusinessObject)
				: base(emailToContactBusinessObject)
			{
			}

			public ZButton SendButton
			{
				get { return (ZButton)GetField("SendButton"); }
			}

			public ZButton CloseButton_Exposed
			{
				get { return base.CloseButton; }
			}

			public ZButton PreviewButton
			{
				get { return (ZButton)GetField("PreviewButton"); }
			}

			object GetField(string fieldName)
			{
				FieldInfo fieldInfo = typeof(EmailContactForm).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				return fieldInfo.GetValue(this);
			}

			internal EmailToContactUserControl EmailToContactUserControl => EmailToContactUserControl1;

			protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
			{
				IsOnClosingCalled = true;
				base.OnClosing(e);
			}

			public bool IsOnClosingCalled;
		}

		#endregion

		#endregion
	}
}
