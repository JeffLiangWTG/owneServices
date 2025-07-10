using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{	
	[TestedType(typeof(ETradeEDIMessage))]
	public class ETradeEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<ETradeEDIMessage>();
			AssertEquals(Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
			message.EM_MessageType = TRMessageTypes.Codes.TRE;
			AssertEquals(ZBool.True, message.NeedToSignMessage);
			message.EM_MessageType = TRMessageTypes.Codes.TRS;
			AssertEquals(ZBool.True, message.NeedToSignMessage);
			message.EM_MessageType = TRMessageTypes.Codes.TRD;
			AssertEquals(ZBool.True, message.NeedToSignMessage);
			message.EM_MessageType = TRMessageTypes.Codes.TCD;
			AssertEquals(ZBool.True, message.NeedToSignMessage);
		}

		public void TestMessageNum()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var message1 = Factory.New<ETradeEDIMessage>();
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<ETradeEDIMessage>();
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000002", message2.EM_MessageNum);
		}

		public void TestFormattedMessageTextForT1E()
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1E;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar /&gt;&lt;KayitNo&gt;22066666GI0000000028&lt;/KayitNo&gt;&lt;KayitTarihi&gt;2022-07-28T14:52:35&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";
			message.EM_MessageInterpretation = message.EM_MessageText;

			message.EM_MessageInterpretation = message.EM_MessageText;
			var expectedMessageContent = "<KayitNo>22066666GI0000000028</KayitNo>";
			AssertionsFormattedMessageText(message.EM_FormattedMessageText, expectedMessageContent);
		}

		public void TestFormattedMessageTextForTRI()
		{
			var messageTRI = Factory.New<ETradeEDIMessage>();
			messageTRI.EM_MessageType = TRMessageTypes.Codes.TRI;
			messageTRI.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageTRI.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><ETGBMuayeneMemuruSorgulaResponse xmlns=""http://tempuri.org/""><ETGBMuayeneMemuruSorgulaResult>TEST KULLANICISI</ETGBMuayeneMemuruSorgulaResult></ETGBMuayeneMemuruSorgulaResponse></s:Body></s:Envelope>";

			messageTRI.EM_MessageInterpretation = messageTRI.EM_MessageText;
			var expectedMessageContent = "<ETGBMuayeneMemuruSorgulaResult>TEST KULLANICISI</ETGBMuayeneMemuruSorgulaResult>";
			AssertionsFormattedMessageText(messageTRI.EM_FormattedMessageText, expectedMessageContent);
		}

		public void TestFormattedMessageTextForTRB()
		{
			var messageTRB = Factory.New<ETradeEDIMessage>();
			messageTRB.EM_MessageType = TRMessageTypes.Codes.TRB;
			messageTRB.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageTRB.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><AyrilanTasimaSenediSorgulaResponse xmlns=""http://tempuri.org/""><AyrilanTasimaSenediSorgulaResult>&lt;?xml version=""1.0""?&gt;&lt;Sonuc xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Sql sorgulama hatası!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;</AyrilanTasimaSenediSorgulaResult></AyrilanTasimaSenediSorgulaResponse></s:Body></s:Envelope>";
			messageTRB.EM_MessageInterpretation = messageTRB.EM_MessageText;

			messageTRB.EM_MessageInterpretation = messageTRB.EM_MessageText;
			var expectedMessageContent = "<HataAciklamasi>Sql sorgulama hatası!</HataAciklamasi>";
			AssertionsFormattedMessageText(messageTRB.EM_FormattedMessageText, expectedMessageContent);
		}

		public void TestFormattedMessageTextForT2D()
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T2D;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
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
			message.EM_MessageInterpretation = message.EM_MessageText;
			var expectedMessageContent = "Ödenmemiş vergi tahakkuku vardır!!";

			AssertionsFormattedMessageText(message.EM_FormattedMessageText, expectedMessageContent);
		}

		public void TestFormattedMessageTextForT1D()
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1D;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ServisCevabiSorgulamaResponse xmlns=""http://tempuri.org/"">
			<ServisCevabiSorgulamaResult xmlns:a=""http://schemas.datacontract.org/2004/07/Gov.GTB.EGEWS.Domain.ETicaret"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
				<a:Hatalar/>
				<a:XmlData>&lt;?xml version=""1.0""?&gt;&lt;ETicaretSoapOut xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Record&gt;&lt;Sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Taşıma satırının markano alanı boş olamaz!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitNo&gt;Hata&lt;/KayitNo&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;&lt;/Record&gt;&lt;/ETicaretSoapOut&gt;</a:XmlData>
			</ServisCevabiSorgulamaResult>
		</ServisCevabiSorgulamaResponse>
	</s:Body>
</s:Envelope>";

			message.EM_MessageInterpretation = message.EM_MessageText;
			var expectedMessageContent = "Taşıma satırının markano alanı boş olamaz!";

			AssertionsFormattedMessageText(message.EM_FormattedMessageText, expectedMessageContent);
		}

		void AssertionsFormattedMessageText(ZString messageText, ZString expectedMessageContent)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&lt;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&gt;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&quot;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&apos;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&amp;"));
				AssertEquals("Formatted message text should contain valid message text.", true, messageText.Contains(expectedMessageContent));
			});
		}

		public void TestMessageInterpretation()
		{
			var messageTransmitTRE = Factory.New<ETradeEDIMessage>();
			messageTransmitTRE.EM_MessageType = TRMessageTypes.Codes.TRE;
			messageTransmitTRE.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRE.EM_MessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.RequestExportMessage.xml");
			messageTransmitTRE.EM_MessageInterpretation = messageTransmitTRE.EM_MessageText;

			var messageTransmitT1E = Factory.New<ETradeEDIMessage>();
			messageTransmitT1E.EM_MessageType = TRMessageTypes.Codes.T1E;
			messageTransmitT1E.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitT1E.EM_MessageText = TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.ServisCevabiSorgulama.xml");
			messageTransmitT1E.EM_MessageInterpretation = messageTransmitT1E.EM_MessageText;

			var messageTransmitTRQ = Factory.New<ETradeEDIMessage>();
			messageTransmitTRQ.EM_MessageType = TRMessageTypes.Codes.TRQ;
			messageTransmitTRQ.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRQ.EM_MessageText = TRMessageTestHelper.GetFileText("ETradeQueryForRegNoMessageTest.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.QueryRegistrationNo.");
			messageTransmitTRQ.EM_MessageInterpretation = messageTransmitTRQ.EM_MessageText;

			var messageTransmitTRI = Factory.New<ETradeEDIMessage>();
			messageTransmitTRI.EM_MessageType = TRMessageTypes.Codes.TRI;
			messageTransmitTRI.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRI.EM_MessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:ETGBMuayeneMemuruSorgula>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:BeyannameNo>22066666GI0000000036</tem:BeyannameNo>
    </tem:ETGBMuayeneMemuruSorgula>
  </soapenv:Body>
</soapenv:Envelope>";
			messageTransmitTRI.EM_MessageInterpretation = messageTransmitTRI.EM_MessageText;

			var messageTransmitTRB = Factory.New<ETradeEDIMessage>();
			messageTransmitTRB.EM_MessageType = TRMessageTypes.Codes.TRB;
			messageTransmitTRB.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRB.EM_MessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:AyrilanTasimaSenediSorgula>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:BeyannameNo>22066666GI0000000036</tem:BeyannameNo>
    </tem:AyrilanTasimaSenediSorgula>
  </soapenv:Body>
</soapenv:Envelope>";
			messageTransmitTRB.EM_MessageInterpretation = messageTransmitTRB.EM_MessageText;

			var messageTransmitTRD = Factory.New<ETradeEDIMessage>();
			messageTransmitTRD.EM_MessageType = TRMessageTypes.Codes.TRD;
			messageTransmitTRD.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRD.EM_MessageText = TRMessageTestHelper.GetFileText("ETrade.ImportDischargeList.RequestImportDischargeList.xml");
			messageTransmitTRD.EM_MessageInterpretation = messageTransmitTRD.EM_MessageText;

			var messageTransmitTCD = Factory.New<ETradeEDIMessage>();
			messageTransmitTCD.EM_MessageType = TRMessageTypes.Codes.TCD;
			messageTransmitTCD.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTCD.EM_MessageText = TRMessageTestHelper.GetFileText("ETrade.ETradeComplementaryDeclerationTest.xml");
			messageTransmitTCD.EM_MessageInterpretation = messageTransmitTCD.EM_MessageText;

			var messageTransmitTRL = Factory.New<ETradeEDIMessage>();
			messageTransmitTRL.EM_MessageType = TRMessageTypes.Codes.TRL;
			messageTransmitTRL.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRL.EM_MessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:BeyannameDurumSorgula>
      <tem:kullaniciAdi>20201224104</tem:kullaniciAdi>
      <tem:kullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:kullaniciSifre>
      <tem:tescilNo>22066666GI0000000036</tem:tescilNo>
    </tem:BeyannameDurumSorgula>
  </soapenv:Body>
</soapenv:Envelope>";
			messageTransmitTRL.EM_MessageInterpretation = messageTransmitTRL.EM_MessageText;

			CombineAssertions("Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTRE.EM_MessageInterpretation.Contains("E-Trade Message Type TRE sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageTransmitTRE.EM_MessageInterpretation.Contains("<td>Job Number:</td><td>ULU-MAN0000442</td>"));
				Assert("EM_MessageInterpretation should contains 'Details'", messageTransmitTRE.EM_MessageInterpretation.Contains("<td>Vehicle Type:</td><td>5</td></tr><tr><td>Discharge Customs Office:</td><td>067777</td></tr><tr><td>Arrival Customs Office:</td><td>067777</td></tr><tr><td>Voyage No:</td><td>TK13245</td></tr><tr><td>Total Packs:</td><td>1</td></tr>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitT1E.EM_MessageInterpretation.Contains("E-Trade Message Type T1E sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Action and Query GUID'", messageTransmitT1E.EM_MessageInterpretation.Contains("<td>Action:</td><td>Query for Temporary Registration No</td></tr><tr><td>Query GUID:</td><td>4142285b-6b4f-4eb8-9bfa-6baa3b884b06</td>"));

				Assert("EM_MessageInterpretation should contains 'has been cleared'", messageTransmitTRI.EM_MessageInterpretation.Contains("E-Trade Message Type TRI sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Inspection Clerk'", messageTransmitTRI.EM_MessageInterpretation.Contains("<td>Action:</td><td>Query for Inspection Clerk</td></tr><tr><td>Registration No:</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTRB.EM_MessageInterpretation.Contains("E-Trade Message Type TRB sent successfully"));
				Assert("EM_MessageInterpretation should contains 'Registration No'", messageTransmitTRB.EM_MessageInterpretation.Contains("<td>Registration No:</td>"));
				Assert("EM_MessageInterpretation should contains 'Action'", messageTransmitTRB.EM_MessageInterpretation.Contains("<td>Action:</td><td>Query Remaining Bills for Import Declaration</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTRD.EM_MessageInterpretation.Contains("E-Trade Message Type TRD sent successfully"));
				Assert("EM_MessageInterpretation should contains 'Registration No'", messageTransmitTRD.EM_MessageInterpretation.Contains("<td>Registration No:</td><td>20341453IM051515</td>"));
				Assert("EM_MessageInterpretation should contains 'Action'", messageTransmitTRD.EM_MessageInterpretation.Contains("<td>Action:</td><td>Import Send for Discharge List</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTRL.EM_MessageInterpretation.Contains("E-Trade Message Type TRL sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Action and Registration No'", messageTransmitTRL.EM_MessageInterpretation.Contains("td>Action:</td><td>Query for Inspection Line</td></tr><tr><td>Registration No:</td><td>22066666GI0000000036</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTRQ.EM_MessageInterpretation.Contains("E-Trade Message Type TRQ sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Temporary Registration No'", messageTransmitTRQ.EM_MessageInterpretation.Contains("<td>Temporary Registration No:</td><td>22066666GI0000000036</td>"));
				Assert("EM_MessageInterpretation should contains 'Action'", messageTransmitTRQ.EM_MessageInterpretation.Contains("<td>Action:</td><td>Query for Registration No</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTransmitTCD.EM_MessageInterpretation.Contains("E-Trade Message Type TCD sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Temporary Registration No'", messageTransmitTCD.EM_MessageInterpretation.Contains("<td>Registration No:</td><td>testRegNoforTest</td>"));
				Assert("EM_MessageInterpretation should contains 'Action'", messageTransmitTCD.EM_MessageInterpretation.Contains("<td>Action:</td><td>Import Send for Complementary Declaration</td>"));
			});
		}
	}
}
