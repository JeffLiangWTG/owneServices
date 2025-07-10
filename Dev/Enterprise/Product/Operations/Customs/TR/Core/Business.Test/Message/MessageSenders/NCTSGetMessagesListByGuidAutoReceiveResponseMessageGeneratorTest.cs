using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class NCTSGetMessagesListByGuidAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var originalMessage = Factory.New<NCTSMessage>();
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRN;
			var sender = GetSender(originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T1N, sender.MessageType);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "TST";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<NCTSMessage>();
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			originalMessage.EM_Status = EDIMessage.Status.Received;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRN;
			originalMessage.EM_SystemCreateUser = "TST";

			var sender = GetSender("5082D1A82A01C3A6E0536803A8C0E158", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("NCTS.GetMessagesListByGuid.xml"), sender.MessageText);
		}

		NCTSGetMessagesListByGuidAutoReceiveResponseMessageSenderForTest GetSender(NCTSMessage originalMessage)
		{
			return GetSender(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		NCTSGetMessagesListByGuidAutoReceiveResponseMessageSenderForTest GetSender(ZString guid, NCTSMessage originalMessage)
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			header.BH_JobReference = "NCT00050102";
			var provider = new NCTSAutoReceiveResponseMessageProvider(header);
			return new NCTSGetMessagesListByGuidAutoReceiveResponseMessageSenderForTest(provider, guid, originalMessage);
		}

		class NCTSGetMessagesListByGuidAutoReceiveResponseMessageSenderForTest : NCTSGetMessagesListByGuidAutoReceiveResponseMessageGenerator
		{
			public NCTSGetMessagesListByGuidAutoReceiveResponseMessageSenderForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
			{
			}

			public new ZString MessageText => base.MessageText;
		}
	}
}
