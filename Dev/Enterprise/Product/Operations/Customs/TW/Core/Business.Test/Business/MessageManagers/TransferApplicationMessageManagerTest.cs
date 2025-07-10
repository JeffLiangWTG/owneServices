using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransferApplicationMessageManagerTest : TestCaseWithFactory
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
			AssertEquals("MessageFriendlyName", "Send Transhipment Application", messageManager.MessageFriendlyName);
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
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals(false, message.EM_IsTestMessage);
		}

		public void TestUpdateStatusAfterSendingMessage()
		{
			messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals(TWMessageStatusCodeList.Codes.AwaitingResponse, messageSender.Header.BH_MessageStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusHead1 = Factory.NewWithValidTestData<CusInBondHeader>();
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "TRS";
			cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			messageSender = new N5301MessageSendingObject(cusHead1);
			messageManager = new TransferApplicationMessageManager(messageSender);
		}

		TranshipmentMessageSendingObject messageSender;
		TransferApplicationMessageManager messageManager;
	}
}
