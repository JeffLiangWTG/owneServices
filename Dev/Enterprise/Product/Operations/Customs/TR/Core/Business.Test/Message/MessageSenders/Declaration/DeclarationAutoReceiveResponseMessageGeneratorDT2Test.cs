using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class DeclarationAutoReceiveResponseMessageGeneratorDT2Test : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var sender = GetSender();
			AssertEquals("MessageType", TRMessageTypes.Codes.DT2, sender.MessageType);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender(originalMessage);
			var expectedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:IslemSorgula3>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:RefID>ULU-B00001000</tem:RefID>
      <tem:BasIslemGunu>2022-10-09</tem:BasIslemGunu>
    </tem:IslemSorgula3>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals("MessageText", expectedMessageText, sender.MessageTextExposed);
		}

		ImportExportAutoReceiveResponseMessageSenderDT2ForTest GetSender(TRImportExportMessage originalMessage = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new DeclarationAutoReceiveResponseMessageProvider(cusEntryHeader);
			return new ImportExportAutoReceiveResponseMessageSenderDT2ForTest(provider, originalMessage, new ZDateTime("2022-10-09"));
		}
	}

	class ImportExportAutoReceiveResponseMessageSenderDT2ForTest : DeclarationAutoReceiveResponseMessageGeneratorDT2
	{
		public ImportExportAutoReceiveResponseMessageSenderDT2ForTest(IMessageSender sender, EDIMessage originalMessage, ZDateTime processStartDate) : base(sender, originalMessage, processStartDate)
		{
		}

		public string MessageTextExposed => base.MessageText;
	}
}
