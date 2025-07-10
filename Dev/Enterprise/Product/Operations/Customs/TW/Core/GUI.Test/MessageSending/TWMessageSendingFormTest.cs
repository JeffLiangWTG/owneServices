using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public abstract class TWMessageSendingFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		public void TestClickOKButton()
		{
			var nothingSelected = "There's nothing selected to be sent to Customs";
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				var messageSendingObject = Wrapper.SendingObjectsCollection.Cast<BaseMessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendButton.PerformClick();
				AssertEquals("has select select send message", nothingSelected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				messageSendingObject.ShouldSend = true;
				sendButton.PerformClick();
				AssertNotContains("No select send message", nothingSelected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public virtual void TestDialogInCheckIsOKToSend()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				var messageSendingObject = Wrapper.SendingObjectsCollection.Cast<BaseMessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				sendButton.PerformClick();
				AssertEquals("IsTestMode", TranshipmentMessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				sendButton.PerformClick();
				AssertNotEquals("IsTestMode", TWMessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public abstract void TestMessageMenuItem();
		protected abstract BaseMessageSendingObjectParent Wrapper
		{
			get;
		}
	}
}
