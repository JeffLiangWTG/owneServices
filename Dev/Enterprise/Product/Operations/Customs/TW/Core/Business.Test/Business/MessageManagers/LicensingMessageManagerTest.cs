using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LicensingMessageManagerTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(messageSender, messageManager.BusinessObject);
		}

		public void TestCanSendOriginal()
		{
			AssertEquals(true, messageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			AssertEquals(true, messageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			AssertEquals(false, messageManager.IsWaitingForResponse);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Send Licensing Message", messageManager.MessageFriendlyName);
		}

		public void TestGenerateMessage()
		{
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			messageSender.ShouldSend = true;
			var messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			var message = messages[0];
			AssertEquals(true, message.EM_IsTestMessage);
			messageSender.Header.Messages.RemoveAndDeleteAll();
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			messages = messageManager.GenerateOriginalMessages(messageSender);
			CombineAssertions(() =>
			{
				AssertEquals("Messages count", 1, messages.Length);
				message = messages[0];
				AssertEquals("EM_IsTestMessage", false, message.EM_IsTestMessage);
				AssertEquals("TW1_MessageStatus", TWMessageStatusCodeList.Codes.AwaitingResponse, controllingMessageHeader.TW1_MessageStatus);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageSender = new NX101MessageSendingObject(controllingMessageHeader);
			messageManager = new LicensingMessageManager(messageSender);
		}

		CusTWControllingMessageHeader controllingMessageHeader;
		LicensingMessageSendingObject messageSender;
		LicensingMessageManager messageManager;
	}
}
