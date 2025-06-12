using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests.EnvelopContext
{
    [TestClass]
    public class QatarEnvelopContextTests : BaseComponentTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void QatarEnvelopContext_EnvelopContext()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Qatar");
            Expect.Call(partyResolver.ResolveParty("SYDWTXH")).Return("Sender1");
			Expect.Call(partyResolver.ResolveClientAWB(Arg<string>.Is.Anything)).Return(null);
			Expect.Call(partyResolver.ResolveClientPIMA(Arg<string>.Is.Equal("CSGAGT86CTF/SYD01"))).Return("99988888888");

			MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("Qatar.TestFiles.Qatar_FMA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Qatar", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("99988888888", envelopContext.RecipientID);
                Assert.AreEqual("15723583770", envelopContext.ClientAWB);
                Assert.AreEqual("CSGAGT86CTF/SYD01", envelopContext.RecipientPIMA);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual(null, envelopContext.MessageVersion);
                Assert.AreEqual("FMA\r\nACK/FWB RECEIVED\r\nFWB/16\r\n157-23583770MELLHR/T8K1624", envelopContext.InternalMessage);
                Assert.AreEqual("", envelopContext.Reference);
            }

            MockRepository.VerifyAll();
        }
    }
}
