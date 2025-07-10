using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRManifestAutoReceiveResponseMessageGeneratorTRMTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var provider = new TRManifestAutoReceiveResponseMessageProvider(manifestHeader);
			var sender = new TRManifestAutoReceiveResponseMessageSenderTRMForTest(provider, originalMessage, "066666", "22067777IM000002");
			AssertEquals("MessageType", TRMessageTypes.Codes.TRM, sender.MessageType);
		}

		[TestDate(2022, 09, 05)]
		public void TestMessageText()
		{
			var expectedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:ozbyMuayeneMemuruAdiSorgula>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:Gumruk>066666</tem:Gumruk>
      <tem:TescilNo>22067777IM000002</tem:TescilNo>
    </tem:ozbyMuayeneMemuruAdiSorgula>
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
			manifestHeader.AMA_CustomsOffice = "066666";
			manifestHeader.RegistrationNumber = "22067777IM000002";
			var provider = new TRManifestAutoReceiveResponseMessageProvider(manifestHeader);

			var sender = new TRManifestAutoReceiveResponseMessageSenderTRMForTest(provider, originalMessage, "TR066666", "22067777IM000002");
			AssertEquals("MessageText", expectedMessageText, sender.MessageTextExposed);
		}
	}

	class TRManifestAutoReceiveResponseMessageSenderTRMForTest : TRManifestAutoReceiveResponseMessageGeneratorTRM
	{
		public TRManifestAutoReceiveResponseMessageSenderTRMForTest(IMessageSender sender, EDIMessage originalMessage, ZString cutomsOffice, ZString registrationNumber) : base(sender, originalMessage, cutomsOffice, registrationNumber)
		{
		}

		public ZString MessageTextExposed => base.MessageText;
	}
}

