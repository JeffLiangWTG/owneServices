using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class NCTSDownloadMessageByIndexAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var provider = new NCTSAutoReceiveResponseMessageProvider(Factory.New<Integration.Customs.TR.ICusInBondHeader>());
			var originalMessage = Factory.New<NCTSMessage>();
			var sender = new NCTSDownloadMessageByIndexAutoReceiveResponseMessageGenerator(provider, "39906267", originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T2N, sender.MessageType);
		}

		public void TestMessageText()
		{
			var provider = new NCTSAutoReceiveResponseMessageProvider(Factory.New<Integration.Customs.TR.ICusInBondHeader>());
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "TST";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<NCTSMessage>();
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			originalMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.T1N;
			originalMessage.EM_SystemCreateUser = "TST";

			var sender = GetSender("39906267", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("NCTS.DownloadMessageByIndex.xml"), sender.MessageTextExposed);
		}

		NCTSDownloadMessageByIndexAutoReceiveResponseMessageSenderForTest GetSender(ZString guid, NCTSMessage originalMessage)
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			header.BH_JobReference = "NCT00050102";
			var provider = new NCTSAutoReceiveResponseMessageProvider(header);
			return new NCTSDownloadMessageByIndexAutoReceiveResponseMessageSenderForTest(provider, guid, originalMessage);
		}

		class NCTSDownloadMessageByIndexAutoReceiveResponseMessageSenderForTest : NCTSDownloadMessageByIndexAutoReceiveResponseMessageGenerator
		{
			public NCTSDownloadMessageByIndexAutoReceiveResponseMessageSenderForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
			{
			}

			public ZString MessageTextExposed => base.MessageText;
		}
	}
}
