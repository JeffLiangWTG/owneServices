using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
    [TestClass]
	public class CargonautEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CargonautEnvelopContext_EnvelopContextFromFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Cargonaut");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("XXX"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("Cargonaut.TestFiles.Cargonaut_FSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Cargonaut", partyResolver);
				Assert.AreEqual("XXX", envelopContext.RecipientPIMA);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("6", envelopContext.MessageVersion);
                Assert.AreEqual("FSU/6\r\n235-79889154AMSBOM/T34K2522.5\r\nDEP/TK6466/12FEB/ISLBOM/T34K2522.5/A1641/S2000\r\n"
                   , envelopContext.InternalMessage);
            }

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CargonautEnvelopContext_EnvelopContextFromFMA()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Cargonaut");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("XXX"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("Cargonaut.TestFiles.Cargonaut_FMA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Cargonaut", partyResolver);
                Assert.AreEqual("XXX", envelopContext.RecipientPIMA);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual("1", envelopContext.MessageVersion);
				Assert.AreEqual("17620649506", envelopContext.ClientAWB);
                Assert.AreEqual("FMA\r\nACK/FWB RCVD AND FORWARDED TO HANDLING PTY.AGS..210212.1447\r\nFWB/17\r\n176-20649506\r\n"
                   , envelopContext.InternalMessage);
            }

            MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CargonautEnvelopContext_EnvelopContextFromFNA()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Cargonaut");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("XXX"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("Cargonaut.TestFiles.Cargonaut_FNA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Cargonaut", partyResolver);
                Assert.AreEqual("XXX", envelopContext.RecipientPIMA);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FNA", envelopContext.MessageType);
                Assert.AreEqual("1", envelopContext.MessageVersion);
				Assert.AreEqual("11284999933", envelopContext.ClientAWB);
                Assert.IsTrue(envelopContext.InternalMessage.StartsWith("FNA/1\r\nACK/AMSVPCRCESMSG REJECTED-ERROR-ILLEGAL SEGMENT-\r\n/SEG NR-51 TAG- ELM--\r\nFWB/16\r\n"));
                Assert.AreEqual(663, envelopContext.InternalMessage.Length);
            }

            MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CargonautEnvelopContext_PromotedValue()
		{
			var promotedValue = new CargonautPromotedValue();
			using (var message = GetEmbeddedResource("Cargonaut.TestFiles.Cargonaut_FSU.xml"))
            {
                Assert.AreEqual("XXX", promotedValue.Find(message, "ClientPIMA"));
                Assert.AreEqual("FSU", promotedValue.Find(message, "MessageType"));
                Assert.AreEqual("6", promotedValue.Find(message, "MessageVersion"));
                Assert.AreEqual("FSU/6\r\n235-79889154AMSBOM/T34K2522.5\r\nDEP/TK6466/12FEB/ISLBOM/T34K2522.5/A1641/S2000\r\n"
                    , promotedValue.Find(message, "InternalMessage"));
            }
        }
	}
}
