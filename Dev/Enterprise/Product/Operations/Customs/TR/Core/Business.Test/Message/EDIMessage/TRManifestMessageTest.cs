using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRManifestMessage))]
	class TRManifestMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<TRManifestMessage>();
			AssertEquals(Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
			AssertEquals(ZBool.True, message.NeedToSignMessage);
		}

		public void TestMessageNum()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var message1 = Factory.New<TRManifestMessage>();
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<TRManifestMessage>();
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			AssertEquals("00000000000002", message2.EM_MessageNum);
		}

		public void TestNeedToSignMessage()
		{
			var message = Factory.New<TRManifestMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1O;
			AssertEquals(ZBool.False, message.NeedToSignMessage);

			message.EM_MessageType = TRMessageTypes.Codes.TRO;
			AssertEquals(ZBool.True, message.NeedToSignMessage);
		}

		public void TestFormattedMessageText()
		{
			var message = Factory.New<TRManifestMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1O;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageText = TRMessageTestHelper.GetFileText("T1OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			message.EM_MessageInterpretation = message.EM_MessageText;

			var formattedMessageText = message.EM_FormattedMessageText;
			Assert("Formatted message text should have no XML escape characters.", !formattedMessageText.Contains("&lt;"));
			Assert("Formatted message text should have no XML escape characters.", !formattedMessageText.Contains("&gt;"));
			Assert("Formatted message text should have no XML escape characters.", !formattedMessageText.Contains("&quot;"));
			Assert("Formatted message text should have no XML escape characters.", !formattedMessageText.Contains("&apos;"));
			Assert("Formatted message text should have no XML escape characters.", !formattedMessageText.Contains("&amp;"));

			var message2 = Factory.New<TRManifestMessage>();
			message2.EM_MessageType = TRMessageTypes.Codes.TRO;
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageText = TRMessageTestHelper.GetFileText("TROError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");

			AssertEquals("TRO error message should be formatted", TRMessageTestHelper.GetFileText("TROErrorFormatted.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), message2.EM_FormattedMessageText);
		}

		public void TestGetEM_FormattedMessageTextShouldNotThrowException()
		{
			var message = Factory.New<TRManifestMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1O;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = TRMessageTestHelper.GetFileText("T1ONoGidenXML.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			AssertNoExceptionThrown("Object reference not set to an instance of an object.", () =>
			{
				_ = message.EM_FormattedMessageText;
			});
		}

		public void TestEM_MessageInterpretation()
		{
			var messageTR0 = Factory.New<TRManifestMessage>();
			messageTR0.EM_MessageType = TRMessageTypes.Codes.TRO;
			messageTR0.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTR0.EM_MessageText = TRMessageTestHelper.GetFileText("Manifest.ManifestMessage.xml");
			messageTR0.EM_MessageInterpretation = messageTR0.EM_MessageText;

			var messageT10 = Factory.New<TRManifestMessage>();
			messageT10.EM_MessageType = TRMessageTypes.Codes.T1O;
			messageT10.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageT10.EM_MessageText = TRMessageTestHelper.GetFileText("Manifest.IslemSonucGetir2.xml");
			messageT10.EM_MessageInterpretation = messageT10.EM_MessageText;

			var messageT20 = Factory.New<TRManifestMessage>();
			messageT20.EM_MessageType = TRMessageTypes.Codes.T2O;
			messageT20.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageT20.EM_MessageText = TRMessageTestHelper.GetFileText("Manifest.IslemSorgula3.xml");
			messageT20.EM_MessageInterpretation = messageT20.EM_MessageText;

			var messageT30 = Factory.New<TRManifestMessage>();
			messageT30.EM_MessageType = TRMessageTypes.Codes.T3O;
			messageT30.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageT30.EM_MessageText = TRMessageTestHelper.GetFileText("Manifest.IslemSonucGetir4.xml");
			messageT30.EM_MessageInterpretation = messageT30.EM_MessageText;

			var messageTRM = Factory.New<TRManifestMessage>();
			messageTRM.EM_MessageType = TRMessageTypes.Codes.TRM;
			messageTRM.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTRM.EM_MessageText = TRMessageTestHelper.GetFileText("Manifest.OzbyMuayeneMemuruAdiSorgula.xml");
			messageTRM.EM_MessageInterpretation = messageTRM.EM_MessageText;

			CombineAssertions("Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageTR0.EM_MessageInterpretation.Contains("Global Manifest Message for job MAN0000442 sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageTR0.EM_MessageInterpretation.Contains("<td>Job Number:</td><td>ULU-MAN0000442</td>"));
				Assert("EM_MessageInterpretation should contains 'Manifest Type'", messageTR0.EM_MessageInterpretation.Contains("<td>Manifest Type:</td><td>DENİTH</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageT10.EM_MessageInterpretation.Contains("Global Manifest Message Type T1O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", messageT10.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>b5d51907-d1bf-4852-91c6-5c33e535a167</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageT20.EM_MessageInterpretation.Contains("Global Manifest Message Type T2O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageT20.EM_MessageInterpretation.Contains("<td>Job Number:</td><td>MAN0000001|20201224104</td>"));
				Assert("EM_MessageInterpretation should contains 'Query Date'", messageT20.EM_MessageInterpretation.Contains("Query Date:</td><td>2022-09-08</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageT30.EM_MessageInterpretation.Contains("Global Manifest Message Type T3O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", messageT30.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>344d5a52-938e-4fc4-b38b-c58d2f03319d</td>"));

				Assert("EM_MessageInterpretation should contains 'successfully'", messageTRM.EM_MessageInterpretation.Contains("Global Manifest Message Type TRM sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Customs Office'", messageTRM.EM_MessageInterpretation.Contains("<td>Customs Office:</td><td>066666</td>"));
				Assert("EM_MessageInterpretation should contains 'Registration Number'", messageTRM.EM_MessageInterpretation.Contains("<td>Registration Number:</td><td>22067777IM000002</td>"));
			});
		}
	}
}
