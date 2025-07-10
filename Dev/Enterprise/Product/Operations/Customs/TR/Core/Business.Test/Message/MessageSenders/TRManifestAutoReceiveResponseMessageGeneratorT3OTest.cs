using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRManifestAutoReceiveResponseMessageGeneratorT3OTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T3O, sender.MessageType);
		}

		public void TestApplicationReference()
		{
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("ApplicationReference", "344d5a52-938e-4fc4-b38b-c58d2f03319d", sender.ApplicationReferenceExposed);
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

		[TestDate(2022, 09, 05)]
		public void TestMessageText()
		{
			var expectedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:IslemSonucGetir4>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:GUIDof>344d5a52-938e-4fc4-b38b-c58d2f03319d</tem:GUIDof>
    </tem:IslemSonucGetir4>
  </soapenv:Body>
</soapenv:Envelope>";

			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			Factory.Save();

			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageText", expectedMessageText, sender.MessageTextExposed);
		}

		TRManifestAutoReceiveResponseMessageSenderT3OForTest GetSender(ZString guid, TRBaseMessage originalMessage = null)
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";
			var provider = new TRManifestAutoReceiveResponseMessageProvider(header);
			return new TRManifestAutoReceiveResponseMessageSenderT3OForTest(provider, guid, originalMessage);
		}
	}

	class TRManifestAutoReceiveResponseMessageSenderT3OForTest : TRManifestAutoReceiveResponseMessageGeneratorT3O
	{
		public TRManifestAutoReceiveResponseMessageSenderT3OForTest(IMessageSender sender, ZString guID, EDIMessage originalMessage) : base(sender, guID, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;

		public ZString ApplicationReferenceExposed => base.ApplicationReference;

		public GlbStaff WhoSendThisMessageExposed => base.WhoSendThisMessage;
	}
}

