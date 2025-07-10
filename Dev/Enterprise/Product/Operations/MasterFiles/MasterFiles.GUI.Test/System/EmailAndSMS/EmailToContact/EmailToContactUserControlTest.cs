using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.Testing.EmailContactFormTest;
using static Enterprise.MasterFiles.GUI.Testing.OutlookEmailSenderTest;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EmailToContactUserControlTest : TestCaseWithFactory
	{
		public void TestAddAndRemoveAttachments()
		{
			using (var tempFile = TempFile.New())
			{
				EmailToContactBusinessObject emailContactObject = new EmailToContactBusinessObject(Factory.New<DummyBusinessObject>());

				using (DummyFormForEmailToContactUserControl form = new DummyFormForEmailToContactUserControl(emailContactObject))
				{
					var emailUserControl = new EmailToContactUserControl();
					form.Controls.Add(emailUserControl);
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					emailUserControl.AddAttachmentButton.PerformClick();
					AssertEquals("EmailContactObject.AttachmentList.Count", 1, emailContactObject.AttachmentList.Count);
					AssertEquals("EmailContactObject.AttachmentList[0].Description", tempFile.Filename + " [Size: 0KB]", emailContactObject.AttachmentList[0].Description);

					AssertNotNull("openFileDialog should be not be disposed otherwise file created by using ForceLocalFile() may be deleted", emailUserControl.openFileDialog);
					AssertNotNull("openFileDialog should be not be disposed otherwise file created by using ForceLocalFile() may be deleted", DisposableLeakListener.Instance.IsRegistered(emailUserControl.openFileDialog));

					AssertEquals("Precondition: There should not be any error messages yet.", null, UnitTestUserNotification.Instance.LastMessage.Text);

					emailUserControl.AddAttachmentButton.PerformClick();
					AssertEquals("An error message should be shown.", "An attachment with the same filename has already been added. You cannot add multiple attachments with the same filename.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("EmailContactObject.AttachmentList.Count", 1, emailContactObject.AttachmentList.Count);
					AssertEquals("EmailContactObject.AttachmentList[0].Description", tempFile.Filename + " [Size: 0KB]", emailContactObject.AttachmentList[0].Description);

					emailUserControl.RemoveAttachmentButton.PerformClick();
					AssertEquals("EmailContactObject.AttachmentList.Count", 0, emailContactObject.AttachmentList.Count);

					emailUserControl.RemoveAttachmentButton.PerformClick();
					AssertEquals("EmailContactObject.AttachmentList.Count", 0, emailContactObject.AttachmentList.Count);
					AssertEquals("An error message should be shown.", "There are no attachments to remove.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					emailUserControl.AddAttachmentButton.PerformClick();
					AssertEquals("EmailContactObject.AttachmentList.Count", 0, emailContactObject.AttachmentList.Count);
					AssertEquals("No error messages should be shown.", null, UnitTestUserNotification.Instance.LastMessage.Text);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					emailUserControl.AddAttachmentButton.PerformClick();
					AssertEquals("EmailContactObject.AttachmentList.Count", 1, emailContactObject.AttachmentList.Count);
					AssertEquals("EmailContactObject.AttachmentList[0].Description", tempFile.Filename + " [Size: 0KB]", emailContactObject.AttachmentList[0].Description);
					AssertEquals("No error messages should be shown.", null, UnitTestUserNotification.Instance.LastMessage.Text);

					form.ReturnNullCurrent = true;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					emailUserControl.AddAttachmentButton.PerformClick();
					Assert("No contacts exist, should have shown error", UnitTestUserNotification.Instance.LastMessage.WasError);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					emailUserControl.RemoveAttachmentButton.PerformClick();
					Assert("No contacts exist, should have shown error", UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		[RequiresSTA]
		public void TestToAndCcButtonEnabled()
		{
			var emailToContactObject = new EmailToContactBusinessObject(Factory.NewWithValidTestData<DummyBusinessObject>());
			using (var form = new DummyEmailContactForm(emailToContactObject))
			{
				form.Show();
				Assert(form.EmailToContactUserControl.ToTextBox.Enabled);
				Assert(form.EmailToContactUserControl.CcTextBox.Enabled);
			}

			emailToContactObject = new EmailToContactBusinessObject(new MockContactSource(Factory));
			using (var form = new DummyEmailContactForm(emailToContactObject))
			{
				form.Show();
				Assert(!form.EmailToContactUserControl.ToTextBox.Enabled);
				Assert(!form.EmailToContactUserControl.CcTextBox.Enabled);
			}
		}

		#region Implementation

		#region class DummyFormForEmailToContactUserControl

		class DummyFormForEmailToContactUserControl : ZForm, IEmailAttachmentForm
		{
			public DummyFormForEmailToContactUserControl(EmailToContactBusinessObject emailToContact) : base(emailToContact)
			{
			}

			public bool ReturnNullCurrent;

			#region IEmailAttachmentForm Members

			public EmailToContactBusinessObject Current
			{
				get { return ReturnNullCurrent ? null : (EmailToContactBusinessObject)BusinessEntity; }
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
