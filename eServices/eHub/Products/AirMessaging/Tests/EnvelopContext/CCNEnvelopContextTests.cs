using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class CCNEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNEnvelopContext_EnvelopContextFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("CCN");
			Expect.Call(partyResolver.ResolveAirline("CCNCarrierPIMA")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("CCNForwarderPIMA"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("CCN.TestFiles.CCNFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCN", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("10", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/10\r\n618-44720535MELLAX/T1K142.0\r\nMAN/SQ7297/21SEP/MELSIN/T1K142.0/S2250/S0440", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("61844720535", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CCNEnvelopContext_EnvelopContextFMA()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("CCN");
            Expect.Call(partyResolver.ResolveAirline("CCNCarrierPIMA")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("CCNForwarderPIMA"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("CCN.TestFiles.CCNFMA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCN", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual(null, envelopContext.MessageVersion);
                Assert.AreEqual("FMA\r\nACK/FWB RCVD 09-JUN-2022 23 15 23 GMT -\r\nFWB/16\r\n045-42981632BNELIM/T1K312\r\nFLT/LA0800/13/LA2376/15", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("04542981632", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNEnvelopContext_EnvelopContext_MultipleRecipients_SubscriptionExist()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
			partyAccessor.Expect(x => x.GetClientIDFromAirlineCode("CCNCarrierPIMA", "CCN")).Return("Sender1");
			partyAccessor.Expect(x => x.GetClientIDFromClientAWB("61844720535", "CCN")).Return("Recipient1");
			var partyResolver = MockRepository.PartialMock<PartyResolver>("CCN");
			partyResolver.Expect(x => x.PartyAccessor).Return(partyAccessor).Repeat.Any();

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("CCN.TestFiles.CCNFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCN", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("10", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/10\r\n618-44720535MELLAX/T1K142.0\r\nMAN/SQ7297/21SEP/MELSIN/T1K142.0/S2250/S0440", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("61844720535", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNEnvelopContext_EnvelopContext_MultipleRecipients_SubProvider()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
			partyAccessor.Expect(x => x.GetClientIDFromAirlineCode("CCNCarrierPIMA", "CCN")).Return("Sender1");
			partyAccessor.Expect(x => x.GetClientIDFromClientAWB("61844720535", "CCN")).Return(null);
			partyAccessor.Expect(x => x.GetClientIDFromClientAWB("61844720535", "CCN_CMD")).Return("Recipient1");
			var partyResolver = MockRepository.PartialMock<PartyResolver>("CCN");
			partyResolver.SubServiceProviders = new[] {" CCN_CMD "};
			partyResolver.Expect(x => x.PartyAccessor).Return(partyAccessor).Repeat.Any();

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("CCN.TestFiles.CCNFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCN", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("10", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/10\r\n618-44720535MELLAX/T1K142.0\r\nMAN/SQ7297/21SEP/MELSIN/T1K142.0/S2250/S0440", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("61844720535", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNEnvelopContext_EnvelopContext_MultipleRecipients_NoSubscription()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
			partyAccessor.Expect(x => x.GetClientIDFromAirlineCode("CCNCarrierPIMA", "CCN")).Return("Sender1");
			partyAccessor.Expect(x => x.GetClientIDFromClientAWB("61844720535", "CCN")).Return(null);
			partyAccessor.Expect(x => x.GetClientIDFromAirPIMA("CCNForwarderPIMA", "CCN")).Return("Recipient1");
			var partyResolver = MockRepository.PartialMock<PartyResolver>("CCN");
			partyResolver.Expect(x => x.PartyAccessor).Return(partyAccessor).Repeat.Any();

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("CCN.TestFiles.CCNFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "CCN", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("10", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/10\r\n618-44720535MELLAX/T1K142.0\r\nMAN/SQ7297/21SEP/MELSIN/T1K142.0/S2250/S0440", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("61844720535", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNEnvelopContext_PromotedValue()
		{
			var promotedValue = new CCNPromotedValue();
			using (var message = GetEmbeddedResource("CCN.TestFiles.CCNFSU.xml"))
			{
				Assert.AreEqual("CCNCarrierPIMA", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("CCNForwarderPIMA", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("FSU/10\r\n618-44720535MELLAX/T1K142.0\r\nMAN/SQ7297/21SEP/MELSIN/T1K142.0/S2250/S0440", promotedValue.Find(message, "InternalMessage"));
			}
		}
	}
}
