using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ManufacturerQueryForm))]
	sealed class ManufacturerQueryFormTest : ZFormBasherTest
	{
		public void TestSendButton_Click()
		{
			var messageData = new USMIDQuery(Factory);
			messageData.US_MID = "";
			AssertHasNotifications("PreCondition:Should have a message error", messageData.US_MIDInfo);
			using (ManufacturerQueryForm form = new ManufacturerQueryForm(messageData))
			{
				form.Visible = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", !form.IsOKToSendMessage);
				messageData.US_MID = "XX123456";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
				Assert("Users have clicked Send but ignored notifications", form.IsOKToSendMessage);
			}

			Env.Security.OrganisationNew.IsAllowed = false;
			messageData.US_AutoCreateOrganization = true;
			messageData.US_MID = "XX123456";
			AssertHasNotifications("PreCondition:Should have a message error", messageData.US_AutoCreateOrganizationInfo);
			using (ManufacturerQueryForm form = new ManufacturerQueryForm(messageData))
			{
				form.Visible = true;
				Env.Security.OrganisationNew.IsAllowed = false;
				messageData.US_AutoCreateOrganization = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", !form.IsOKToSendMessage);
				Env.Security.OrganisationNew.IsAllowed = true;
				messageData.US_AutoCreateOrganization = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
				Assert("Users have clicked Send but stopped at notifications", form.IsOKToSendMessage);
			}
		}

		public void TestGiveUpButton_Click()
		{
			using (ManufacturerQueryForm form = (ManufacturerQueryForm)GetFormToBash())
			{
				form.Visible = true;
				form.GiveUpButton.PerformClick();
				AssertEquals("users have clicked Cancel and should not send messages", false, form.IsOKToSendMessage);
			}
		}

		protected override Form GetFormToBashCore() => new ManufacturerQueryForm(new USMIDQuery(Factory));
	}
}
