using CargoWise.eServices.USCustoms.OutboundProcessingService;
using NUnit.Framework;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.IntegrationTests.MQ
{
	[TestFixture]
	public class USCustomsOutboundMessageTest : MQBaseTest
	{
		[Test]
		public void TestUSCustomsOutboundMessage()
		{
			var stream = GetEmbeddedResource("TestFiles.TestFile.xml");
			var message = new USCustomsOutboundMessage(stream);
			Assert.AreEqual("AMS", message.ApplicationCode);
			Assert.AreEqual("BLAH", message.ClientId);
			Assert.IsTrue(message.IsProduction);
			Assert.AreEqual("EI", message.MessageType);
			Assert.AreEqual("42c996e2-c3c8-4e31-b4f0-6ebf36170770", message.TrackingId);
			var content = message.MessageStream.ReadToEnd();
			Assert.AreEqual(@"A3910SV9CAREDI05171101                                               00000057021"
				+ "\n" + "00000057021B018888XJ5EI                                               57253                10A888813-14792700013-147927000                 8         XJ5 7003854801037  IL 20                         408888051711B00155839            112  051711I317     22            26311225524                         00000001PC                    30                                  01              2052711             VE      40001CH00000010000000000100                    0000000050                       50 72027000000000004500000000010000KG 000000000100CKG               CH051711N   51                                                                              60                                        CHHARWIN8PLA                          62          49900000210                                                         8949900000002500                                                                9000000004500           1                       0000000250000000001000          Y  8888XJ5EI00011000000004500                                                   "
				+ "\n" + "Z3910SV9CAREDI05171101                                               00000057021", content, $"Actual content: '{content}'");
		}

		[Test]
		public void TestUSCustomsOutboundMessage_NoIndentInCData()
		{
			var stream = GetEmbeddedResource("TestFiles.TestFileNoIndentInCData.xml");
			var message = new USCustomsOutboundMessage(stream);
			Assert.AreEqual("AMS", message.ApplicationCode);
			Assert.AreEqual("BLAH", message.ClientId);
			Assert.IsTrue(message.IsProduction);
			Assert.AreEqual("EI", message.MessageType);
			Assert.AreEqual("42c996e2-c3c8-4e31-b4f0-6ebf36170770", message.TrackingId);
			var content = message.MessageStream.ReadToEnd();
			Assert.AreEqual(@"A3910SV9CAREDI05171101                                               00000057021"
				+ "\n" + "00000057021B018888XJ5EI                                               57253                10A888813-14792700013-147927000                 8         XJ5 7003854801037  IL 20                         408888051711B00155839            112  051711I317     22            26311225524                         00000001PC                    30                                  01              2052711             VE      40001CH00000010000000000100                    0000000050                       50 72027000000000004500000000010000KG 000000000100CKG               CH051711N   51                                                                              60                                        CHHARWIN8PLA                          62          49900000210                                                         8949900000002500                                                                9000000004500           1                       0000000250000000001000          Y  8888XJ5EI00011000000004500                                                   "
				+ "\n" + "Z3910SV9CAREDI05171101                                               00000057021", content, $"Actual content: '{content}'");
		}

		[Test]
		public void TestUSCustomsOutboundMessageWithWhitespacesEnding_ValidCData()
		{
			var stream = GetEmbeddedResource("TestFiles.TestFileWithWhitespacesEnding.xml");
			var message = new USCustomsOutboundMessage(stream);
			Assert.AreEqual("AMS", message.ApplicationCode);
			Assert.AreEqual("DFOCN0PRO", message.ClientId);
			Assert.IsTrue(message.IsProduction);
			Assert.AreEqual("EI", message.MessageType);
			Assert.AreEqual("b963b008-1f42-4daa-b243-115f26aab8dd", message.TrackingId);
			var content = message.MessageStream.ReadToEnd();
			Assert.AreEqual(@"ACR8CARCAREDIMI                                                                 M01DMAL11DK                       115N             9320257                      M02ZSNA26681_DFOCN0PRO34806                                                     P012709050121                                                                   J01DMAL                                                                         B01ZSNA26681   570780000000416CTN  0000010297KGN                                B020000000060CMYANTIAN                      MEDUDMAL57078     57078             B04OB MEDUCQ843160                                                              N00SH NINESTAR ELECTIONIC CO LTD                                                N02UNIT 503 5F SILVERCORD TOWER NO 2 30 CANTON ROAD                             N03TSIM SHA TSUI                 HK                                             N04SUZY SU                TE+867566258942            EMSUZY.SU@GGIMAGE.COM      N00CN LEXMARK INTERNATION INC                                                   N02BUILDING 32 DOCK 740 WEST NEW CIRCLE RD                                      N03LEXINGTON          KY40550    US                                             N04                       TE+18592321183                                        N00N1 LEXMARK INTERNATION INC                                                   N02BUILDING 32 DOCK 740 WEST NEW CIRCLE RD                                      N03LEXINGTON          KY40550    US                                             N04                       TE+18592321183                                        C01MSMU8579446   FJ11434379                    HV0                    45G0LCY   D00844331     000000000000010297KG                                              D010000000416LASER PRINTER (CONTAIN TONERS, LITHIUM BATTER              CTN     D010000000000Y)                                                                 D02LEXMARK                                                                      ZCR8CAR      MI                   00000                                         ", content, $"Actual content: '{content}'");
		}

		[Test]
		public void TestUSCustomsOutboundMessage_InvalidCData()
		{
			var stream = GetEmbeddedResource("TestFiles.TestFileWithWhitespacesEnding_Invalid.xml");
			var message = new USCustomsOutboundMessage(stream);
			Assert.AreEqual("AMS", message.ApplicationCode);
			Assert.AreEqual("DFOCN0PRO", message.ClientId);
			Assert.IsTrue(message.IsProduction);
			Assert.AreEqual("EI", message.MessageType);
			Assert.AreEqual("b963b008-1f42-4daa-b243-115f26aab8dd", message.TrackingId);
			var content = message.MessageStream.ReadToEnd();
			Assert.AreEqual("\n", content, $"Actual content: '{content}'");
		}
	}
}
