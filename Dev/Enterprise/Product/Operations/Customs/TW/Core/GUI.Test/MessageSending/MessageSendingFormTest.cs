using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public sealed class MessageSendingFormTest : MessageSendingFormAbstractTest<MessageSendingForm, JobDeclarationMessageSendingObjectParent>
	{
		public void TestAdditionalWarningsTextBox()
		{
			Declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddWarning("MessageType Add Warning");
			}

			;
			using (var form = new MessageSendingFormForTest(DeclarationWrapper))
			{
				form.Show();
				var additionalWarningsTextBox = form.Controls.Find("AdditionalWarningsTextBox", true)[0] as ZTextBox;
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(additionalWarningsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("MessageType Add Warning", additionalWarningsTextBox.Text);
			}
		}

		public void TestRunPreSendValidation()
		{
			Declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("MessageType Add Message Error");
			}

			;
			using (var form = new MessageSendingFormForTest(DeclarationWrapper))
			{
				form.Show();
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
				messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
				{
					x.AddMessageError("Blue Message Error");
				}

				;
				messageSendingObject.ShouldSend = false;
				AssertEquals(0, form.RunPreSendValidationCore().Count);
				messageSendingObject.ShouldSend = true;
				AssertEquals(0, form.RunPreSendValidationCore().Count);
			}
		}

		public void TestSendButtonEnabled()
		{
			Declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("MessageType Add Message Error");
			}

			;
			using (var form = new MessageSendingFormForTest(DeclarationWrapper))
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				Assert(!sendButton.Enabled);
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				Assert(!sendButton.Enabled);
				DeclarationWrapper.AllowSendWithError = true;
				Assert(sendButton.Enabled);
				messageSendingObject.ShouldSend = false;
				Assert(!sendButton.Enabled);
			}
		}

		public void TestValidationErrorsTextBox()
		{
			Declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("MessageType Add Message Error");
			}

			;
			using (var form = new MessageSendingFormForTest(DeclarationWrapper))
			{
				form.Show();
				var validationErrorsTextBox = form.Controls.Find("ValidationErrorsTextBox", true)[0] as ZTextBox;
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObjectForTesting>().FirstOrDefault();
				messageSendingObject.MockValidationMessage = (ZPropertyInfo x) =>
				{
					x.AddMessageError("Blue Message Error");
				}

				;
				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(validationErrorsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("MessageType Add Message Error", validationErrorsTextBox.Text);
				AssertContains("Blue Message Error", validationErrorsTextBox.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MessageSendingForm(DeclarationWrapper);
		}

		protected override JobDeclarationMessageSendingObjectParent GetDeclarationWrapper()
		{
			return new JobDeclarationMessageSendingObjectParentForTest(Declaration, "ECD");
		}
	}
}
