using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(FTZMessageSendingForm))]
	sealed class FTZMessageSendingFormTest : ZFormBasherTest
	{
		public void TestVisibility()
		{
			var sendingObject = new FTZMessageSendingObject(Factory.New<JobDeclaration>(), UpdateActionCode.Replace);
			using (var form = new FTZMessageSendingForm(sendingObject))
			{
				form.Show();
				var remarksTextBox = form.FindSingle<ZArchitecture.ZTextBox>("RemarksTextBox");
				AssertEquals(false, remarksTextBox.Visible);
				sendingObject.US_OtherReason = true;
				AssertEquals(true, remarksTextBox.Visible);
			}
		}

		public void TestSendButton_Click()
		{
			var sendingObject = new FTZMessageSendingObject(Factory.New<JobDeclaration>(), UpdateActionCode.Replace);
			using (var form = new FTZMessageSendingForm(sendingObject))
			{
				form.Visible = true;
				var sendButton = form.FindSingle<ZButton>("OKButton");
				sendButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("A reason is required; please select a maximum of 8 reasons."));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendingObject.US_ChangeOrAddConveyance = true;
				sendingObject.US_DeleteConveyance = true;
				sendingObject.US_ChangeOrAddBillOfLading = true;
				sendingObject.US_DeleteBillOfLading = true;
				sendingObject.US_ChangeOrAddHTSLine = true;
				sendingObject.US_DeleteHTSLine = true;
				sendingObject.US_ChangeAdmittedQuantity = true;
				sendingObject.US_CancelOrAddPTT = true;
				sendingObject.US_OtherReason = true;
				sendingObject.US_FTZContactName = "";
				sendingObject.US_FTZContactPhone = "12345678";
				sendButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("A reason is required; please select a maximum of 8 reasons."));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendingObject.US_OtherReason = false;
				sendButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("There is a notification. Are you sure you wish to continue?"));
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors on this job and you don't have security rights to send with message errors."));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var sendingObject = new FTZMessageSendingObject(Factory.New<JobDeclaration>(), UpdateActionCode.Replace);
			sendingObject.US_FTZContactName = "Dan Brown";
			sendingObject.US_FTZContactPhone = "7382945000";
			sendingObject.US_OtherReason = true;
			sendingObject.US_Remarks = "Random Reason";
			Factory.Save();
			return new FTZMessageSendingForm(sendingObject);
		}
	}
}
