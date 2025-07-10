using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRSPTSAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var sender = GetSender();
			AssertEquals("MessageType", TRMessageTypes.Codes.T1P, sender.MessageType);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<SPTSMessage>();
			originalMessage.EM_SystemCreateUser = "YE";
			var sender = GetSender("b5d51907-d1bf-4852-91c6-5c33e535a167", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml"), sender.MessageTextExposed);
		}

		TRSPTSAutoReceiveResponseMessageSenderForTest GetSender(SPTSMessage originalMessage = null)
		{
			return GetSender(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		TRSPTSAutoReceiveResponseMessageSenderForTest GetSender(ZString guid, SPTSMessage originalMessage = null)
		{
			var sptsHeader = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			var provider = new TRSPTSAutoReceiveResponseMessageProvider(sptsHeader);
			return new TRSPTSAutoReceiveResponseMessageSenderForTest(provider, guid, originalMessage);
		}
	}

	class TRSPTSAutoReceiveResponseMessageSenderForTest : TRSPTSAutoReceiveResponseMessageGenerator
	{
		public TRSPTSAutoReceiveResponseMessageSenderForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;
	}
}
