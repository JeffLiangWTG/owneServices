using System.IO;
using CargoWise.eServices.USCustoms.OutboundProcessingService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class USCustomsOutboundMessageTest
	{
		[TestMethod]
		public void TestPopulateDataFromStream_NoRemoveSpace()
		{
			var AMAMessageContent =
@"<?xml version=""1.0"" encoding=""utf-8""?><Message ClientId=""TSTCLIENT"" IsProduction=""False"" MessageTrackingId=""719B3D4F-F9F3-44A2-B3EA-54FDF24EAE2E"" ApplicationCode=""AMA"" MessageType=""""><![CDATA[
\x01WASUCCR
\x2EWTGTTST 
\x02FRX
ALV123
123-12312311-M
ARR/?QF20/28AUG
RFA/02

\x03]]></Message>";
			var expectedContent = @"
\x01WASUCCR
\x2EWTGTTST 
\x02FRX
ALV123
123-12312311-M
ARR/?QF20/28AUG
RFA/02

\x03";
			var exptectContentBinary = "\r\n\\x01WASUCCR\r\n\\x2EWTGTTST \r\n\\x02FRX\r\nALV123\r\n123-12312311-M\r\nARR/?QF20/28AUG\r\nRFA/02\r\n\r\n\\x03";
			var sourceStream = new MemoryStream(MQMessageSender.OutgoingEncoding.GetBytes(AMAMessageContent));
			Stream responseStream = new MemoryStream();
			var USCMessage = new USCustomsOutboundMessage(sourceStream);
			Assert.AreEqual("719B3D4F-F9F3-44A2-B3EA-54FDF24EAE2E", USCMessage.TrackingId);
			Assert.AreEqual("TSTCLIENT", USCMessage.ClientId);
			Assert.AreEqual(false, USCMessage.IsProduction);
			Assert.AreEqual("AMA", USCMessage.ApplicationCode);
			Assert.AreEqual("", USCMessage.MessageType);
			USCMessage.MessageStream.Position = 0;
			var result = new StreamReader(USCMessage.MessageStream).ReadToEnd();
			Assert.AreEqual(expectedContent, result);
			Assert.AreEqual(exptectContentBinary, result);
		}
	}
}
