using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(EmailMultipleContactsForm))]
	sealed class EmailMultipleContactsForm_Test : ZFormBasherTest
	{
		public void TestCurrent()
		{
			DummyEmailToContactBusinessObject emailContactObject1 = new DummyEmailToContactBusinessObject(BizO);
			DummyEmailToContactBusinessObject emailContactObject2 = new DummyEmailToContactBusinessObject(BizO);
			EmailsToContacts.Add(emailContactObject1);
			EmailsToContacts.Add(emailContactObject2);

			using (EmailMultipleContactsForm form = new EmailMultipleContactsForm(Sender))
			{
				form.Show();
				AssertEquals("There should be two EmailContacts in the list", 2, form.ContactListGrid.ListManager.List.Count);

				int emailContact1Index = form.ContactListGrid.ListManager.List.IndexOf(emailContactObject1);
				int emailContact2Index = form.ContactListGrid.ListManager.List.IndexOf(emailContactObject2);

				form.ContactListGrid.CurrentRowIndex = emailContact1Index;
				AssertEquals("Current object should be EmailContactObject1", emailContactObject1, ((IEmailAttachmentForm)form).Current);

				form.ContactListGrid.CurrentRowIndex = emailContact2Index;
				AssertEquals("Current object should be EmailContactObject2", emailContactObject2, ((IEmailAttachmentForm)form).Current);
			}
		}

		public void TestClose()
		{
			using (DummyEmailMultipleContactsForm form = new DummyEmailMultipleContactsForm(Sender))
			{
				form.Show();
				Assert("Form should not be closed yet.", !form.IsOnClosingCalled);
				form.CloseButton.PerformClick();
				Assert("Form should be closed.", form.IsOnClosingCalled);
			}
		}

		public void TestSendEmail()
		{
			DummyEmailToContactBusinessObject emailContactObject1 = new DummyEmailToContactBusinessObject(BizO);
			DummyEmailToContactBusinessObject emailContactObject2 = new DummyEmailToContactBusinessObject(BizO);
			EmailsToContacts.Add(emailContactObject1);
			EmailsToContacts.Add(emailContactObject2);

			using (DummyEmailMultipleContactsForm form = new DummyEmailMultipleContactsForm(Sender))
			{
				form.Show();
				Assert("Precondition: EmailContactObject1's SendEmail method should not have been called yet.", !emailContactObject1.IsSendEmailCalled);
				Assert("Precondition: EmailContactObject2's SendEmail method should not have been called yet.", !emailContactObject2.IsSendEmailCalled);
				Assert("Precondition: Form should not be closed yet.", !form.IsOnClosingCalled);

				emailContactObject1.Cc = "cc@cc.com";
				emailContactObject1.FromEmailAddress = "Default@edi.com.au";
				emailContactObject1.Priority = "MED";
				emailContactObject1.ToEmailAddress = "testreceiver@cargowise.com";

				form.SendAllButton.PerformClick();
				Assert("Errors on EmailContactObject2: EmailContactObject1's SendEmail method should not have been called yet.", !emailContactObject1.IsSendEmailCalled);
				Assert("Errors on EmailContactObject2: EmailContactObject2's SendEmail method should not have been called yet.", !emailContactObject2.IsSendEmailCalled);
				Assert("Errors on EmailContactObject2: Form should not be closed yet.", !form.IsOnClosingCalled);

				AssertEquals("An error should have been shown", "Please correct all errors first before sending this Email.", UnitTestUserNotification.Instance.LastMessage.Text);

				emailContactObject2.Cc = "cc@cc.com";
				emailContactObject2.FromEmailAddress = "Default@edi.com.au";
				emailContactObject2.Priority = "MED";
				emailContactObject2.ToEmailAddress = "testreceiver@cargowise.com";

				form.SendAllButton.PerformClick();
				Assert("EmailContactObject1's SendEmail method should have been called", emailContactObject1.IsSendEmailCalled);
				Assert("EmailContactObject2's SendEmail method should have been called", emailContactObject2.IsSendEmailCalled);
				Assert("Form should be closed.", form.IsOnClosingCalled);
			}
		}

		[RequiresSTA]
		public void TestPreviewButton()
		{
			DummyEmailToContactBusinessObject emailContactObject1 = new DummyEmailToContactBusinessObject(BizO);
			DummyEmailToContactBusinessObject emailContactObject2 = new DummyEmailToContactBusinessObject(BizO);
			EmailsToContacts.Add(emailContactObject1);
			EmailsToContacts.Add(emailContactObject2);

			using (DummyEmailMultipleContactsForm form = new DummyEmailMultipleContactsForm(Sender))
			{
				form.Show();
				form.PreviewButton.PerformClick();
				AssertEquals("Preview form should be shown", typeof(RichTextEmailDisplayZForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#region Implementation

		EmailToContactBusinessObjectCollection EmailsToContacts;
		MultipleEmailToContactSender Sender;
		DummyBusinessObject BizO;

		protected override void SetUp()
		{
			base.SetUp();
			BizO = Factory.New<DummyBusinessObject>();
			EmailsToContacts = new EmailToContactBusinessObjectCollection(BizO);
			Sender = new MultipleEmailToContactSender(EmailsToContacts);
		}

		protected override Form GetFormToBashCore()
		{
			return new EmailMultipleContactsForm(Sender);
		}

		#region class DummyEmailMultipleContactsForm

		class DummyEmailMultipleContactsForm : EmailMultipleContactsForm
		{
			public DummyEmailMultipleContactsForm(MultipleEmailToContactSender sender) : base(sender)
			{
			}

			protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
			{
				IsOnClosingCalled = true;
				base.OnClosing(e);
			}

			public bool IsOnClosingCalled;
		}

		#endregion

		#region class DummyEmailToContactBusinessObject

		class DummyEmailToContactBusinessObject : EmailToContactBusinessObject
		{
			public DummyEmailToContactBusinessObject(BusinessObject @object) : base(@object)
			{
			}

			protected override void SendEmailCore(bool systemCommunication = false)
			{
				IsSendEmailCalled = true;
			}

			public bool IsSendEmailCalled;
		}

		#endregion

		#endregion
	}
}
