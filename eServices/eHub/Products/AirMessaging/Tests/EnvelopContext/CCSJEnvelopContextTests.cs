using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class CCSJEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJEnvelopContext_EnvelopContext()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.GenerateStrictMock<PartyResolver>("CCSJ");
			partyResolver.Expect(x => x.ResolveAirline("CI")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("1630540TYO82"), Arg<string>.Is.Anything)).Return("Recipient1");

			using (message.BodyPart.Data = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCSJ", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("6", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/6\r\n297-00000001KIXHKG/T3K4138.9\r\nDEP/CI0001/25JUN/TPEHKG/T1K4000/A1903/A2045\r\nRCF/CI0001/25JUN2313/HKG/T3K4139//A2041\r\n", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("29700000001", envelopContext.ClientAWB);
			}

			partyResolver.VerifyAllExpectations();
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJEnvelopContext_PromotedValue()
		{
			var promotedValue = new CCSJPromotedValue();
			using (var message = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJFSU.xml"))
			{
				Assert.AreEqual("CI", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("1630540TYO82", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("FSU/6\r\n297-00000001KIXHKG/T3K4138.9\r\nDEP/CI0001/25JUN/TPEHKG/T1K4000/A1903/A2045\r\nRCF/CI0001/25JUN2313/HKG/T3K4139//A2041", promotedValue.Find(message, "InternalMessage").TrimEnd());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJEnvelopContext_LargeMessage()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			var partyResolver = MockRepository.GenerateStub<PartyResolver>("CCSJ");

			using (message.BodyPart.Data = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJ_LargeBody.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCSJ", partyResolver);
				Assert.AreEqual("FSU/6\r\n112-27173996KIXPVG/T414\r\nBKD/CK248/07JAN/KIXPVG/T414K13050/S2220\r\nRCS/07JAN2026/KIX/T414K13050/YLK\r\nDEP/CK248/07JAN/KIXPVG/T414K13050/A0123-N\r\nMAN/CK248/07JAN/KIXPVG/P75K1232.1/S2220\r\nMAN/CK248/07JAN/KIXPVG/P33K1673.7/S2LB220\r\nMAN/CK248/07JAN/KIXPVG/P23K1241.9/S2220\r\nMAN/CK248/07JAN/KIXPVG/P41K2059.4/S2220\r\nMAN/CK248/07JAN/KIXPVG/P128K3317.7/S2220\r\nMAN/CK248/07JAN/KIXPVG/P104K1585.2/S2220\r\nMAN/CK248/07JAN/KIXPVG/P10K1940/S2220\r\nDEP/CK248/07JAN/KIXPVG/T414K13050/A0039-N\r\n", envelopContext.InternalMessage);
			}
		}
	}
}
