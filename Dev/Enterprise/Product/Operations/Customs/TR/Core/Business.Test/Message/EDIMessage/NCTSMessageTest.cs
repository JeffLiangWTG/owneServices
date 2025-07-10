using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(NCTSMessage))]
	public class NCTSMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<NCTSMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
		}

		public void TestMessageNum()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			var message1 = Factory.New<NCTSMessage>();
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<NCTSMessage>();
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000002", message2.EM_MessageNum);
		}

		public void TestMessageTextIndentedXml()
		{
			var messageTransmit = Factory.New<NCTSMessage>();
			messageTransmit.EM_MessageType = TRMessageTypes.Codes.TRN;
			messageTransmit.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmit.EM_MessageText = "~Test";
			messageTransmit.EM_MessageInterpretation = messageTransmit.EM_MessageText;

			var receiveTRN = Factory.New<NCTSMessage>();
			receiveTRN.EM_MessageType = TRMessageTypes.Codes.TRN;
			receiveTRN.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			receiveTRN.EM_MessageText = @"<ns0:submitdeclarationResponse xmlns:ns0=""http://ws/"" xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><return><corrGuid>5082D1A82A01C3A6E0536803A8C0E158</corrGuid><error>000</error></return></ns0:submitdeclarationResponse>";
			var messageContentTRN = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");

			var receiveT1N = Factory.New<NCTSMessage>();
			receiveT1N.EM_MessageType = TRMessageTypes.Codes.T1N;
			receiveT1N.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			receiveT1N.EM_MessageText = @"<S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><S:Body><ns0:getMessagesListByGuidResponse xmlns:ns0=""http://ws/""><return><error>000</error><list>39906267</list><list>39906263</list><list>39916956</list><list>39903172</list><list>39906268</list><list>39916957</list></return></ns0:getMessagesListByGuidResponse></S:Body></S:Envelope>";
			var messageContentT1N = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");

			var receiveT2N = Factory.New<NCTSMessage>();
			receiveT2N.EM_MessageType = TRMessageTypes.Codes.T2N;
			receiveT2N.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			receiveT2N.EM_MessageText = @"<soapenv:Envelope xmlns:ws=""http://ws/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""><soapenv:Header /><soapenv:Body><ws:downloadmessagebyindex><FIRM_ID>ULUKOM</FIRM_ID><USER_ID>NCT00050102,20201224104,25d55ad283aa400af464c76d713c07ad</USER_ID><INDEX>39906267</INDEX></ws:downloadmessagebyindex></soapenv:Body></soapenv:Envelope>";
			var messageContentT2N = TRMessageTestHelper.GetFileText("DownloadMessageByIndex.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");

			CombineAssertions("EM_MessageTextIndentedXml", () =>
			{
				AssertEquals("TRO Transmit", messageTransmit.EM_MessageText, messageTransmit.EM_MessageTextIndentedXml);
				AssertEquals("TRO Receive", messageContentTRN, receiveTRN.EM_MessageTextIndentedXml);
				AssertEquals("T1N Receive", messageContentT1N, receiveT1N.EM_MessageTextIndentedXml);
				AssertEquals("T2N Receive", messageContentT2N, receiveT2N.EM_MessageTextIndentedXml);
			});

			var receiveT2NError = Factory.New<NCTSMessage>();
			receiveT2NError.EM_MessageType = TRMessageTypes.Codes.T2N;
			receiveT2NError.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			receiveT2NError.EM_MessageText = @"<S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/""><S:Body><ns0:downloadmessagebyindexResponse xmlns:ns0=""http://ws/""><return><corrGuid>16E8B80A8281CE59E0636903A8C02F98</corrGuid><error>000</error><msgContent>&lt;CC015B_RES&gt;&lt;GUID&gt;16E8B80A8281CE59E0636903A8C02F98&lt;/GUID&gt;&lt;ERR&gt;&lt;TEC&gt;NOT OKORA-12899: value too large for column ""NCTS_TRA_DEP"".""CC015B"".""STRANDNUMPC122"" (actual: 47, maximum: 35)&lt;/TEC&gt;&lt;VAL&gt;&lt;/VAL&gt;&lt;/ERR&gt;&lt;/CC015B_RES&gt;</msgContent></return></ns0:downloadmessagebyindexResponse></S:Body></S:Envelope>";
			var messageText = receiveT2NError.EM_MessageTextIndentedXml;

			CombineAssertions("T2N Receive XML escape characters", () =>
			{
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&lt;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&gt;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&quot;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&apos;"));
				AssertEquals("Formatted message text should have no XML escape characters.", false, messageText.Contains("&amp;"));
			});
		}

		public void TestMessageInterpretation()
		{
			var messageTransmitTRN = Factory.New<NCTSMessage>();
			messageTransmitTRN.EM_MessageType = TRMessageTypes.Codes.TRN;
			messageTransmitTRN.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitTRN.EM_MessageText = TRMessageTestHelper.GetFileText("CC015B.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			messageTransmitTRN.EM_MessageInterpretation = messageTransmitTRN.EM_MessageText;
			var messageInterpretationTRN = messageTransmitTRN.EM_MessageInterpretation;

			CombineAssertions("TRN Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretationTRN.Contains("NCTS Message for job 202200000231 sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Label'", messageInterpretationTRN.Contains(">Label<"));
				Assert("EM_MessageInterpretation should contains 'Value'", messageInterpretationTRN.Contains(">Value<"));
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageInterpretationTRN.Contains("<td>Job Number:</td><td>202200000231</td>"));
				Assert("EM_MessageInterpretation should contains 'Declaration Type'", messageInterpretationTRN.Contains("<td>Declaration Type:</td><td>TR</td>"));
				Assert("EM_MessageInterpretation should contains 'DEP Customs Office'", messageInterpretationTRN.Contains("<td>DEP Customs Office:</td><td>TR066666</td>"));
				Assert("EM_MessageInterpretation should contains 'DES Customs Office'", messageInterpretationTRN.Contains("<td>DES Customs Office:</td><td>TR066666</td>"));
				Assert("EM_MessageInterpretation should contains 'Exit Customs Office'", messageInterpretationTRN.Contains("<td>Exit Customs Office:</td><td>340300</td>"));
				Assert("EM_MessageInterpretation should contains 'Principal'", messageInterpretationTRN.Contains("<td>Principal:</td><td>ULUKOM LOGISTICS</td>"));
				Assert("EM_MessageInterpretation should contains 'Consignor'", messageInterpretationTRN.Contains("<td>Consignor:</td><td>BOLLORE LOGISTICS UK LTD</td>"));
				Assert("EM_MessageInterpretation should contains 'Consignee'", messageInterpretationTRN.Contains("<td>Consignee:</td><td>ULUKOM LOJİSTİK</td>"));
				Assert("EM_MessageInterpretation should contains 'Transport ID (DEP)'", messageInterpretationTRN.Contains("<td>Transport ID (DEP):</td><td>MSCPEGYY</td>"));
				Assert("EM_MessageInterpretation should contains 'Containers'", messageInterpretationTRN.Contains("<td>Containers:</td><td>111111<br>222222<br>33333<br>44444<br>5555555</td>"));
			});

			var messageTransmitT1N = Factory.New<NCTSMessage>();
			messageTransmitT1N.EM_MessageType = TRMessageTypes.Codes.T1N;
			messageTransmitT1N.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitT1N.EM_MessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuid.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			messageTransmitT1N.EM_MessageInterpretation = messageTransmitT1N.EM_MessageText;
			var messageInterpretationT1N = messageTransmitT1N.EM_MessageInterpretation;

			CombineAssertions("T1N Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretationT1N.Contains("NCTS Message Type T1N sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Label'", messageInterpretationT1N.Contains(">Label<"));
				Assert("EM_MessageInterpretation should contains 'Value'", messageInterpretationT1N.Contains(">Value<"));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", messageInterpretationT1N.Contains("<td>Query GUID:</td><td>5082D1A82A01C3A6E0536803A8C0E158</td>"));
			});

			var messageTransmitT2N = Factory.New<NCTSMessage>();
			messageTransmitT2N.EM_MessageType = TRMessageTypes.Codes.T2N;
			messageTransmitT2N.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmitT2N.EM_MessageText = TRMessageTestHelper.GetFileText("DownloadMessageByIndex.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			messageTransmitT2N.EM_MessageInterpretation = messageTransmitT2N.EM_MessageText;
			var messageInterpretationT2N = messageTransmitT2N.EM_MessageInterpretation;

			CombineAssertions("T2N Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretationT2N.Contains("NCTS Message Type T2N sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Label'", messageInterpretationT2N.Contains(">Label<"));
				Assert("EM_MessageInterpretation should contains 'Value'", messageInterpretationT2N.Contains(">Value<"));
				Assert("EM_MessageInterpretation should contains 'Index'", messageInterpretationT2N.Contains("<td>Index:</td><td>39906267</td>"));
			});
		}
	}
}
