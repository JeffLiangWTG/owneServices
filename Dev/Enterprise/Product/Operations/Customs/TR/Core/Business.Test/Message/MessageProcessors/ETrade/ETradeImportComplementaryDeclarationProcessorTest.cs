using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeImportComplementaryDeclarationProcessorTest : TestCaseWithFactory
	{
		public void TestAutoPopulatedT2DMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			eTradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			eTradeHeader.AMA_JobReference = "ETG0000001";

			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<RefID>ETGULU230000012|20201224104|9</RefID>
					<GUID>cf35d011-9200-4746-9a8c-29c33d80211e</GUID>
					<Durum>İşleminiz başlamıştır.Teşekkür ederiz.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";

			var guID = "cf35d011-9200-4746-9a8c-29c33d80211e";

			var sessionID = ZGuid.NewZGuid();
			var originalInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, "Request TCD Interchange", sessionID);
			var originalMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, "Request TCD EdiMessage", "TRX1", string.Empty);
			originalMessage.EM_EI = originalInterchange.PK;
			originalMessage.EM_LinkedObject = (CargoWise.EntityFramework.BusinessObject)eTradeHeader;
			originalMessage.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			var incomingInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var incomingMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, messageText, "RCV1", string.Empty);
			incomingMessage.EM_LinkedObject = (CargoWise.EntityFramework.BusinessObject)eTradeHeader;
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			Processor.ProcessMessage(incomingMessage);

			var expectedpopulatedT2DMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:TemETrade=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <TemETrade:ServisCevabiSorgulama>
      <TemETrade:KullaniciAdi>20201224104</TemETrade:KullaniciAdi>
      <TemETrade:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</TemETrade:KullaniciSifre>
      <TemETrade:KayitNo>cf35d011-9200-4746-9a8c-29c33d80211e</TemETrade:KayitNo>
    </TemETrade:ServisCevabiSorgulama>
  </soapenv:Body>
</soapenv:Envelope>";

			CombineAssertions(() =>
			{
				AssertEquals("EM_MessageInterpretation should contain guid", true, incomingMessage.EM_MessageInterpretation.Contains(guID));
				AssertEquals("EM_MessageInterpretation should contain Label", true, incomingMessage.EM_MessageInterpretation.Contains("Query GUID:"));
				AssertEquals("EM_MessageInterpretation should contain successfully", true, incomingMessage.EM_MessageInterpretation.Contains("E-Trade Import Send for Complementary Declaration Message for job ETG0000001 has been cleared."));

				var populatedT2DMessage = (EDIMessage)eTradeHeader.Messages[2];
				AssertEquals("EM_IsActive", true, populatedT2DMessage.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", populatedT2DMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "T2D", populatedT2DMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", populatedT2DMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", populatedT2DMessage.EM_Status);
				AssertEquals("EM_MessageText", expectedpopulatedT2DMessage, populatedT2DMessage.EM_MessageText);
				AssertEquals("EM_MessageInterpretation should contains guid", true, populatedT2DMessage.EM_MessageInterpretation.Contains(guID));
				AssertEquals("EM_MessageInterpretation should contains successfully", true, populatedT2DMessage.EM_MessageInterpretation.Contains("E-Trade Message Type T2D sent successfully."));
				AssertEquals("EM_MessageInterpretation should contains Label", true, populatedT2DMessage.EM_MessageInterpretation.Contains("Label"));
				AssertEquals("EM_MessageInterpretation should contains Value", true, populatedT2DMessage.EM_MessageInterpretation.Contains("Value"));
				AssertEquals("EM_MessageInterpretation should contains Query GUID", true, populatedT2DMessage.EM_MessageInterpretation.Contains("Query GUID:"));
				AssertEquals("EM_MessageInterpretation should contains Action", true, populatedT2DMessage.EM_MessageInterpretation.Contains("Action:"));
			});
		}

		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var errorMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TRE, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, errorMessageText, "RCV1", string.Empty);
			errorMessage.EM_LinkUniqueID = errorETradeHeader.PK;
			errorMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			Processor.ProcessMessage(errorMessage);

			CombineAssertions(() =>
			{
				AssertEquals(CustomsStatusList.Codes.CDR, errorETradeHeader.RegistrationStatus);
				AssertEquals(TRMessageTypes.Codes.TCD, errorETradeHeader.MessageMode);
				AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);
			});

			var successMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar /&gt;&lt;KayitNo&gt;ETGB kapanmış statüsüne getirilmiştir!&lt;/KayitNo&gt;&lt;KayitTarihi&gt;2023-05-02T12:12:56.0389183+03:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";

			var importETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			importETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importSuccessMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TRE, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, successMessageText, "RCV2", string.Empty);
			importSuccessMessage.EM_LinkUniqueID = importETradeHeader.PK;
			importSuccessMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			Processor.ProcessMessage(importSuccessMessage);

			var expectedHtmlContent = "<tr><td>Complementary Declaration Registration Number: </td><td>ETGB kapanmış stat&#252;s&#252;ne getirilmiştir!</td></tr><tr><td>Complementary Declaration Registration Date: </td><td>02-May-23 12:12:56</td></tr>";

			CombineAssertions(() =>
			{
				AssertEquals(CustomsStatusList.Codes.CDS, importETradeHeader.RegistrationStatus);
				AssertEquals(TRMessageTypes.Codes.CPL, importETradeHeader.MessageMode);
				AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)importETradeHeader).MessageStatus);
				AssertEquals("EM_MessageInterpretation should contains Registration Number and Date", true, importSuccessMessage.EM_MessageInterpretation.Contains(expectedHtmlContent));
			});
		}

		public void TestOutputMessageErrorResponse()
		{
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			eTradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			eTradeHeader.AMA_JobReference = "ETG0000001";

			var incomingMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<Durum>Kullanıcı Adı, şifre veya RefID boş gönderilemez! Gönderdiğiniz mesajın düzgün yapılandırılmış xml dosyası olduğundan veya servis şemasına uygun olduğundan emin olun.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";

			var incomingMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, incomingMessageText, "RCV1", string.Empty);
			incomingMessage.EM_LinkedObject = (CargoWise.EntityFramework.BusinessObject)eTradeHeader;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			Processor.ProcessMessage(incomingMessage);
			var expectedResponseMessage = "<td>Kullanıcı Adı, şifre veya RefID boş g&#246;nderilemez! G&#246;nderdiğiniz mesajın d&#252;zg&#252;n yapılandırılmış xml dosyası olduğundan veya servis şemasına uygun olduğundan emin olun.</td>";
			AssertEquals("EM_MessageInterpretation should contain excpected message", true, incomingMessage.EM_MessageInterpretation.Contains(expectedResponseMessage));

			incomingMessage.EM_MessageText = "";
			expectedResponseMessage = "<td>The message missed the Complementary Declaration</td>";
			Processor.ProcessMessage(incomingMessage);
			AssertEquals("EM_MessageInterpretation should contain excpected message", true, incomingMessage.EM_MessageInterpretation.Contains(expectedResponseMessage));
		}

		public void TestOutputMessageShouldNotStatusUpdate()
		{
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			eTradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			eTradeHeader.AMA_JobReference = "ETG0000001";
			((IMessageAttachee)eTradeHeader).MessageStatus = TRMessageStatusCodeList.Codes.Accepted;
			eTradeHeader.RegistrationStatus = CustomsStatusList.Codes.CDS;

			var incomingMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<Durum>İşleminiz başlamıştır.Teşekkür ederiz.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";

			var incomingMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, incomingMessageText, "RCV1", string.Empty);
			incomingMessage.EM_LinkedObject = (CargoWise.EntityFramework.BusinessObject)eTradeHeader;

			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)eTradeHeader).MessageStatus);
				AssertEquals(CustomsStatusList.Codes.CDS, eTradeHeader.RegistrationStatus);
			});
		}

		public void TestOutputMessageShouldStatusUpdate()
		{
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			eTradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			eTradeHeader.AMA_JobReference = "ETG0000001";

			var incomingMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<OutputMessage xmlns=""http://GumrukETApp.OutputMessageSchema"">
			<Record>
				<Sonuc>
					<Durum>Kullanıcı Adı, şifre veya RefID boş gönderilemez! Gönderdiğiniz mesajın düzgün yapılandırılmış xml dosyası olduğundan veya servis şemasına uygun olduğundan emin olun.</Durum>
				</Sonuc>
			</Record>
		</OutputMessage>
	</s:Body>
</s:Envelope>";
			var incomingMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.TCD, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, incomingMessageText, "RCV1", string.Empty);
			incomingMessage.EM_LinkedObject = (CargoWise.EntityFramework.BusinessObject)eTradeHeader;

			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)eTradeHeader).MessageStatus);
				AssertEquals(CustomsStatusList.Codes.CDR, eTradeHeader.RegistrationStatus);
			});
		}

		public void TestErrorResponseT2DMessage()
		{
			var errorMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Sadece onaylanmış satatüdeki ETGB
ler için tamamlayici beyan gönderilebilir!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitNo&gt;Hata&lt;/KayitNo&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";

			var errorETradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var errorMessage = TRMessageTestHelper.CreateMessage<ETradeEDIMessage>(Factory, TRMessageTypes.Codes.T2D, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, errorMessageText, "RCV1", string.Empty);
			errorMessage.EM_LinkUniqueID = errorETradeHeader.PK;
			errorMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			Processor.ProcessMessage(errorMessage);

			var expectedResponseMessage = "<tr><td>Sadece onaylanmış satat&#252;deki ETGB<br>ler i&#231;in tamamlayici beyan g&#246;nderilebilir!</td></tr>";
			AssertEquals("EM_MessageInterpretation should contain excpected message", true, errorMessage.EM_MessageInterpretation.Contains(expectedResponseMessage));

			errorMessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;
Ödenmemiş vergi tahakkuku vardır!!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitNo&gt;Hata&lt;/KayitNo&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";

			errorMessage.EM_MessageText = errorMessageText;
			Processor.ProcessMessage(errorMessage);

			expectedResponseMessage = "<tr><td><br>&#214;denmemiş vergi tahakkuku vardır!!</td></tr>";
			AssertEquals("EM_MessageInterpretation should contain excpected message", true, errorMessage.EM_MessageInterpretation.Contains(expectedResponseMessage));
		}

		ETradeImportComplementaryDeclarationProcessor Processor => processor ?? (processor = new ETradeImportComplementaryDeclarationProcessor(new LoggingInformation()));
		ETradeImportComplementaryDeclarationProcessor processor;
	}
}
