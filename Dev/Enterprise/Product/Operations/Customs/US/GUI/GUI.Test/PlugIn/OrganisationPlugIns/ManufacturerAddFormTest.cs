using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ManufacturerAddForm))]
	sealed class ManufacturerAddFormTest : ZFormBasherTest
	{
		public void TestSendAddButton_Click()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(organisation));
			using (var form = new ManufacturerAddForm(messageData))
			{
				form.Visible = true;
				messageData.US_MID = "123";
				Assert("PreCondition: should have an error", messageData.HasErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendAddButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);
				messageData.US_MID = "";
				messageData.US_FirmName = "";
				Assert("PreCondition: should not have errors", !messageData.HasErrors);
				Assert("PreCondition: should have a message error", messageData.HasMessageErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendAddButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
#if !WINZOR
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
#endif
				AssertEquals("Add message will be sent", ManufacturerAddFormResult.SendAddMessage, form.Result);
			}
		}

		public void TestSendUpdateButton_Click()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(organisation));
			using (var form = new ManufacturerAddForm(messageData))
			{
				form.Visible = true;
				messageData.US_MID = "";
				Assert("Send Update disabled", !form.SendUpdateButton.Enabled);
				messageData.US_MID = "US1";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendUpdateButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);
				messageData.US_MID = "XX123456";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendUpdateButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
#if !WINZOR
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
#endif
				AssertEquals("Update message will be sent", ManufacturerAddFormResult.SendUpdateMessage, form.Result);
			}
		}

		public void TestSendQueryButton_Click()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(organisation));
			using (ManufacturerAddForm form = new ManufacturerAddForm(messageData))
			{
				form.Visible = true;
				messageData.US_MID = "";
				Assert("Send Query disabled", !form.SendQueryButton.Enabled);
				messageData.US_MID = "US1";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendQueryButton.PerformClick();
				Assert("Errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Please fix the following error to continue.\r\n"));
				Assert("Users have clicked Send but stopped at notifications", form.Visible);
				messageData.US_MID = "XX123456";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendQueryButton.PerformClick();
				Assert("Message errors should have been informed", UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors.\r\n\r\n"));
#if !WINZOR
				Assert("Users have clicked Send but ignored notifications", !form.Visible);
#endif
				AssertEquals("Query message will be sent", ManufacturerAddFormResult.SendQueryMessage, form.Result);
			}
		}

		public void TestCancelButton_Click()
		{
			using (ManufacturerAddForm form = (ManufacturerAddForm)GetFormToBash())
			{
				form.Visible = true;
				form.CancelButton2.PerformClick();
				AssertEquals("Users have clicked Cancel and should not send messages", ManufacturerAddFormResult.Cancel, form.Result);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(organisation));
			return new ManufacturerAddForm(messageData);
		}
	}
}
