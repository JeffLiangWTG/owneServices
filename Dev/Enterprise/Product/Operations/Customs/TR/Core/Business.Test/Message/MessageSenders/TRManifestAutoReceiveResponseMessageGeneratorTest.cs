using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRManifestAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var sender = GetSender();
			AssertEquals("MessageType", TRMessageTypes.Codes.T1O, sender.MessageType);
		}

		public void TestWhoSendThisMessage()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageOwner", "KNZ", sender.WhoSendThisMessageExposed.GS_Code);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "YE";
			var sender = GetSender("b5d51907-d1bf-4852-91c6-5c33e535a167", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("Manifest.IslemSonucGetir2.xml"), sender.MessageTextExposed);
		}

		TRManifestAutoReceiveResponseMessageSenderForTest GetSender(TRManifestMessage originalMessage = null)
		{
			return GetSender(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		TRManifestAutoReceiveResponseMessageSenderForTest GetSender(ZString guid, TRManifestMessage originalMessage = null)
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var provider = new TRManifestAutoReceiveResponseMessageProvider(manifestHeader);
			return new TRManifestAutoReceiveResponseMessageSenderForTest(provider, guid, originalMessage);
		}
	}

	class TRManifestAutoReceiveResponseMessageSenderForTest : TRManifestAutoReceiveResponseMessageGenerator
	{
		public TRManifestAutoReceiveResponseMessageSenderForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;

		public GlbStaff WhoSendThisMessageExposed => base.WhoSendThisMessage;
	}
}
