using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeImportDischargeListMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessT1DResponseMessage_Interpretation()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.ImportDischargeList.ResponseImportDischargeList.xml");
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(TRMessageTypes.Codes.T1D, messageText, header.PK);

			Processor.ProcessMessage(message);

			var guid = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";

			AssertEquals("An E-Trade auto receive response message that contains the guid should be created", true, ((ETradeEDIMessage)header.Messages[0]).EM_MessageText.Contains(guid));
			Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>4142285b-6b4f-4eb8-9bfa-6baa3b884b06</td>"));
		}

		public void TestProcessTRDResponseMessage_CreateAutoReceiveResponseT1D()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "YE";
			staff.GS_EmailAddress = "test@ulutestmail.com";

			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			Factory.Save();

			var messageText = TRMessageTestHelper.GetFileText("ETrade.ImportDischargeList.ResponseImportDischargeList.xml");
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";

			var requestMessage = CreateETradeMessage(TRMessageTypes.Codes.TRD, messageText, header.PK, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
			var requestInterchange = MessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRD, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, requestMessage.PK, "Test Interchange message");

			requestMessage.EM_EI = requestInterchange.PK;

			var responseInterchange = MessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, requestMessage.PK, messageText);
			var responseMessage = CreateETradeMessage(TRMessageTypes.Codes.TRD, messageText, header.PK, EDIMessage.Direction.Receive, EDIMessage.Status.Queued);

			responseMessage.EM_EI = responseInterchange.PK;

			Processor.ProcessMessage(responseMessage);

			var messageLevels = header.Messages.Cast<ETradeEDIMessage>().Single(message => !message.Equals(requestMessage) && !message.Equals(responseMessage));
			AssertEquals("Check EM_MessageType", "T1D", messageLevels.EM_MessageType);
			Assert("EM_MessageText should contains 'KullaniciAdi'", messageLevels.EM_MessageText.Contains("<TemETrade:KullaniciAdi>20201224104</TemETrade:KullaniciAdi>"));
			Assert("EM_MessageText should contains 'KullaniciSifre'", messageLevels.EM_MessageText.Contains("<TemETrade:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</TemETrade:KullaniciSifre>"));
			Assert("EM_MessageText should contains 'KayitNo'", messageLevels.EM_MessageText.Contains("<TemETrade:KayitNo>4142285b-6b4f-4eb8-9bfa-6baa3b884b06</TemETrade:KayitNo>"));
		}

		public void TestErrorResponseInOutputMessage()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<Durum>Elektronik İmza ile kullanıcı kodu üzerindeki bilgiler uyumsuz.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";

			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var message = CreateETradeMessage(TRMessageTypes.Codes.TRD, messageText, header.PK);

			Factory.Save();

			Processor.ProcessMessage(message);

			AssertContains(
				"EM_MessageInterpretation should contain the error message line.",
				"<tr><td>Error Message: </td><td>Elektronik İmza ile kullanıcı kodu &#252;zerindeki bilgiler uyumsuz.</td></tr>",
				message.EM_MessageInterpretation
			);
		}

		public void TestErrorResponseForT1D()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Taşıma satırının markano alanı boş olamaz!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitNo&gt;Hata&lt;/KayitNo&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var message = CreateETradeMessage(TRMessageTypes.Codes.T1D, messageText, header.PK);

			Processor.ProcessMessage(message);

			var expectedHtmlContent = "Error Message</th></tr></thead><tr><td>Taşıma satırının markano alanı boş olamaz!</td></tr>";

			Assert("EM_MessageInterpretation should contains ", message.EM_MessageInterpretation.Contains(expectedHtmlContent));
			Assert("EM_MessageInterpretation should contains ", message.EM_MessageInterpretation.Contains("ETG0000001 has been rejected"));
		}

		public void TestCatchingAlreadyRegisteredDischargeListNo()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;23066666IM000004 beyanname numarası için daha önceden 23066666BL000003 numarası ile boşaltma listesi tescil edilmiştir!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitNo&gt;Hata&lt;/KayitNo&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			header.DischargeRecordNo = ZString.Empty;
			var message = CreateETradeMessage(TRMessageTypes.Codes.T1D, messageText, header.PK);

			Processor.ProcessMessage(message);

			AssertEquals("23066666BL000003", header.DischargeRecordNo);

			var expectedHtmlContent = "<tr><td>23066666IM000004 beyanname numarası i&#231;in daha &#246;nceden 23066666BL000003 numarası ile boşaltma listesi tescil edilmiştir!</td></tr>";
			Assert("EM_MessageInterpretation should contains ", message.EM_MessageInterpretation.Contains(expectedHtmlContent));
			Assert("EM_MessageInterpretation should contains ", message.EM_MessageInterpretation.Contains("ETG0000001 has been cleared"));
		}

		ETradeEDIMessage CreateETradeMessage(ZString messageType, ZString messageText, ZGuid headerPk, string transmit = EDIMessage.Direction.Receive, string status = EDIMessage.Status.Queued)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_ReceiveTransmit = transmit;
			message.EM_Status = status;
			message.EM_MessageType = messageType;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = headerPk;
			message.EM_MessageText = messageText;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_SystemCreateUser = "YE";
			return message;
		}

		ETradeImportDischargeListMessageProcessor Processor => processor ?? (processor = new ETradeImportDischargeListMessageProcessor(new LoggingInformation()));
		ETradeImportDischargeListMessageProcessor processor;
	}
}
