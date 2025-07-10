using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRManifestAutoReceiveResponseMessageGeneratorT2OTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var provider = new TRManifestAutoReceiveResponseMessageProvider(manifestHeader);
			var sender = new TRManifestAutoReceiveResponseMessageSenderT2OForTest(provider, originalMessage, ZDateTime.Now);
			AssertEquals("MessageType", TRMessageTypes.Codes.T2O, sender.MessageType);
		}

		[TestDate(2022, 09, 05)]
		public void TestMessageText()
		{
			var expectedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:IslemSorgula3>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:RefID>ULU-MAN0000001|20201224104</tem:RefID>
      <tem:BasIslemGunu>2022-09-05</tem:BasIslemGunu>
    </tem:IslemSorgula3>
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

			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			var provider = new TRManifestAutoReceiveResponseMessageProvider(manifestHeader);

			var sender = new TRManifestAutoReceiveResponseMessageSenderT2OForTest(provider, originalMessage, new ZDateTime("2022-09-05"));
			AssertEquals("MessageText", expectedMessageText, sender.MessageTextExposed);
		}
	}

	class TRManifestAutoReceiveResponseMessageSenderT2OForTest : TRManifestAutoReceiveResponseMessageGeneratorT2O
	{
		public TRManifestAutoReceiveResponseMessageSenderT2OForTest(IMessageSender sender, EDIMessage originalMessage, ZDateTime registrationDate) : base(sender, originalMessage, registrationDate)
		{
		}

		public ZString MessageTextExposed => base.MessageText;
	}
}

