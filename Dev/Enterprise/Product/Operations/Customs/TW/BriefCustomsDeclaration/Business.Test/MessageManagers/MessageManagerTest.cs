using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class MessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageManager()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var messageSender = new N5205MessageSendingObject(header);
			var messageManager = new MessageManager(messageSender);
			CombineAssertions(() =>
			{
				AssertEquals("MessageFriendlyName", "Send To Customs Message", messageManager.MessageFriendlyName);
				AssertEquals("CanSendOriginal", true, messageManager.CanSendOriginal);
				AssertEquals("CanSendWithdrawal", true, messageManager.CanSendWithdrawal);
				AssertEquals("IsWaitingForResponse", false, messageManager.IsWaitingForResponse);
			});
		}

		public void TestGenerateMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var n5205MessageSendingObject = new N5205MessageSendingObject(header);
			var messageManager = new MessageManager(n5205MessageSendingObject);
			var messages = messageManager.GenerateOriginalMessages(n5205MessageSendingObject);
			var message = messages[0];
			var xml = new N5205MessageBuilder().PopulateXml(n5205MessageSendingObject, "1");

			CombineAssertions("N5205 Message", () =>
			{
				AssertEquals("Messages Count", 1, messages.Length);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.EBC, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", header.AMA_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("EM_LinkUniqueID", header.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", header.TableName, message.EM_LinkTable);
				AssertEquals("EM_MessageText", xml, message.EM_MessageText);
				AssertEquals("EM_IsTestMessage", TWCustomsDataRegistry.IsTestMode, message.EM_IsTestMessage);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
			});

			var n5135MessageSendingObject = new N5135MessageSendingObject(header);
			messageManager = new MessageManager(n5135MessageSendingObject);
			messages = messageManager.GenerateOriginalMessages(n5135MessageSendingObject);
			message = messages[0];
			xml = new N5135MessageBuilder().PopulateXml(n5135MessageSendingObject, "1");

			CombineAssertions("N5135 Message", () =>
			{
				AssertEquals("Messages Count", 1, messages.Length);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.IBC, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", header.AMA_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("EM_LinkUniqueID", header.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", header.TableName, message.EM_LinkTable);
				AssertEquals("EM_MessageText", xml, message.EM_MessageText);
				AssertEquals("EM_IsTestMessage", TWCustomsDataRegistry.IsTestMode, message.EM_IsTestMessage);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
			});
		}
	}
}
