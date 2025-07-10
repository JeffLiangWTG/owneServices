using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(GlobalBusinessIdentifierForm))]
	internal class GlobalBusinessIdentifierFormTest : ZFormBasherTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(organisation));
			return new GlobalBusinessIdentifierForm(messageData);
		}

		public void TestClickSendGBIAddButton()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(organisation));
			using (var form = new GlobalBusinessIdentifierForm(messageData))
			{
				form.Show();
				messageData.US_OA_AddressDetails = ZGuid.Empty;
				Assert("PreCondition: should have an error", messageData.HasErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendGBIAddButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);

				messageData.US_OA_AddressDetails = organisation.MainAddress.PK;
				Assert("PreCondition: should not have errors", !messageData.HasErrors);
				Assert("PreCondition: should have a message error", messageData.HasMessageErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendGBIAddButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
				AssertEquals("Original message will be sent", GlobalBusinessIdentifierMessageType.Original, form.MessageType);
			}
		}

		public void TestClickSendGBIUpdateButton()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(organisation));
			using (var form = new GlobalBusinessIdentifierForm(messageData))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.SendGBIUpdateButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("A GBI Add message has not yet been sent for this Organization/Address."));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);

				messageData.SubmissionStatus = GBISubmissionStatusList.Codes.AwaitingGBIUpdate;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendGBIUpdateButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
				AssertEquals("Update message will be sent", GlobalBusinessIdentifierMessageType.Update, form.MessageType);
			}
		}

		public void TestClickSendGBIDeleteButton()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(organisation));
			using (var form = new GlobalBusinessIdentifierForm(messageData))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.SendGBIDeleteButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("A GBI Add message has not yet been sent for this Organization/Address."));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);

				messageData.SubmissionStatus = GBISubmissionStatusList.Codes.AwaitingGBIDelete;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendGBIDeleteButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
				AssertEquals("Delete message will be sent", GlobalBusinessIdentifierMessageType.Delete, form.MessageType);
			}
		}
	}
}
