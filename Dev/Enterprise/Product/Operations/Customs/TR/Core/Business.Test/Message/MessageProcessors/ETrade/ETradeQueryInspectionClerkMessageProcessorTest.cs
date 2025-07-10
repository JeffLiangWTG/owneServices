using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeQueryInspectionClerkMessageProcessorTest : TestCaseWithFactory
	{
		public void TestErrorMessage()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ETGBMuayeneMemuruSorgulaResponse xmlns=""http://tempuri.org/"">
			<ETGBMuayeneMemuruSorgulaResult>&lt;?xml version=""1.0""?&gt;&lt;Sonuc xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Kullanıcı kodu ve şifrenizi kontrol ediniz!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;</ETGBMuayeneMemuruSorgulaResult>
		</ETGBMuayeneMemuruSorgulaResponse>
	</s:Body>
</s:Envelope>";
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var message = CreateETradeMessage(messageText, header.PK);

			Processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Clerk Name should be empty", ZString.Empty, header.InspectionClerk);
				AssertEquals(CustomsStatusList.Codes.QIR, header.RegistrationStatus);
				AssertEquals(TRMessageTypes.Codes.TRI, header.MessageMode);
				AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)header).MessageStatus);
				AssertEquals("EM_MessageInterpretation should contain has been rejected", true, message.EM_MessageInterpretation.Contains("E-Trade Inspection Clerk Message for job  has been rejected."));
				AssertEquals("EM_MessageInterpretation should contain error response detail", true, message.EM_MessageInterpretation.Contains("<tr><td>Kullanıcı kodu ve şifrenizi kontrol ediniz!</td></tr>"));
			});
		}

		public void TestSuccessMessage()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionClerk.QueryForInspectionClerkSuccess.xml");
			var message = CreateETradeMessage(messageText, header.PK);

			Processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Clerk Name", "TEST KULLANICISI", header.InspectionClerk);
				AssertEquals(CustomsStatusList.Codes.QIS, header.RegistrationStatus);
				AssertEquals(TRMessageTypes.Codes.TRL, header.MessageMode);
				AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)header).MessageStatus);
				AssertEquals("EM_MessageInterpretation should contain has been cleared", true, message.EM_MessageInterpretation.Contains("E-Trade Inspection Clerk Message for job  has been cleared."));
				AssertEquals("EM_MessageInterpretation should contain Inspection Clerk", true, message.EM_MessageInterpretation.Contains("<tr><td>Inspection Clerk: </td><td>TEST KULLANICISI</td></tr>"));
			});
		}

		ETradeEDIMessage CreateETradeMessage(ZString messageText, ZGuid headerPk)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = "ULU-2019/00002345";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRI;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkUniqueID = headerPk;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = messageText;
			return message;
		}

		ETradeQueryInspectionClerkMessageProcessor Processor => processor ?? (processor = new ETradeQueryInspectionClerkMessageProcessor(new LoggingInformation()));
		ETradeQueryInspectionClerkMessageProcessor processor;
	}
}
