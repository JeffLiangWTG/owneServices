using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class DeclarationAutoReceiveResponseMessageGeneratorDT3Test : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.DT3, sender.MessageType);
		}

		public void TestApplicationReference()
		{
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("ApplicationReference", "344d5a52-938e-4fc4-b38b-c58d2f03319d", sender.ApplicationReferenceExposed);
		}

		public void TestWhoSendThisMessage()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var originalMessage = Factory.New<TRImportExportMessage>();
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
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			Factory.Save();

			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageText", expectedMessageText, sender.MessageTextExposed);
		}

		ImportExportAutoReceiveResponseMessageSenderDT3ForTest GetSender(ZString guid, TRBaseMessage originalMessage = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new DeclarationAutoReceiveResponseMessageProvider(cusEntryHeader);
			return new ImportExportAutoReceiveResponseMessageSenderDT3ForTest(provider, guid, originalMessage);
		}
	}

	class ImportExportAutoReceiveResponseMessageSenderDT3ForTest : DeclarationAutoReceiveResponseMessageGeneratorDT3
	{
		public ImportExportAutoReceiveResponseMessageSenderDT3ForTest(IMessageSender sender, ZString guID, EDIMessage originalMessage) : base(sender, guID, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;
		public ZString ApplicationReferenceExposed => base.ApplicationReference;
		public GlbStaff WhoSendThisMessageExposed => base.WhoSendThisMessage;
	}
}
