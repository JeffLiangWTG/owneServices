using System.Collections.Generic;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class TraxonEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TraxonEnvelopContext_EnvelopContextFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("Traxon");
			Expect.Call(partyResolver.ResolveParty("REUAIR08AFR")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("REUAGT89CARGOW/SYD01"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("Traxon.TestFiles.TraxonFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Traxon", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("12", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/12\r\n239-12345675FRAMRU/T1K14.1\r\nRCF/MK059/20NOV1300/MRU/T1K14.1//A1117-P\r\n", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("23912345675", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TraxonEnvelopContext_PromotedValue()
		{
			var promotedValue = new TraxonPromotedValue();
			using (var message = GetEmbeddedResource("Traxon.TestFiles.TraxonFSU.xml"))
			{
				Assert.AreEqual("REUAIR08AFR", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("REUAGT89CARGOW/SYD01", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("CIMFSU", promotedValue.Find(message, "MessageType"));
				Assert.AreEqual("12", promotedValue.Find(message, "MessageVersion"));
				Assert.AreEqual("FSU/12\r\n239-12345675FRAMRU/T1K14.1\r\nRCF/MK059/20NOV1300/MRU/T1K14.1//A1117-P\r\n", promotedValue.Find(message, "InternalMessage"));
			}
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TraxonEnvelopContext_EnvelopContextFMA()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Traxon");
            Expect.Call(partyResolver.ResolveParty("REUAIR08AFR")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("REUAGT89CARGOW/SYD01"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();
            using (message.BodyPart.Data = GetEmbeddedResource("Traxon.TestFiles.TraxonFMAFWB.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Traxon", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual("0", envelopContext.MessageVersion);
                Assert.AreEqual("FMA/0\r\nACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER\r\nFWB/16\r\n081-32652362SYDCHI/T10K100\r\nFLT/QF046/20\r\nRTG/CHIQF\r\nSHP\r\n/DIRECT CONTAINER LINE\r\n/LEVEL 9  83 YORK STREET\r\n/SYDNEY/NSW\r\n/AU\r\nCNE\r\n/MID-AMERICA OVERSEAS INC\r\n/1151 NORTH WOOD DALE ROAD WOOD\r\n/DALE/ILLINOIS\r\n/US\r\nAGT/1234/1234567\r\n/TEST\r\n/SYDNEY\r\nCVD/AUD/PP/PP/NVD/NCV/XXX\r\nRTD/1/P10/K100/CQ/W1666.5\r\n/NG/CONSOLIDATION AS PER\r\n/2/NG/TOTAL  10 PACKS\r\n/3/NG/DIMS 100X100X100 CM \r\n/4/NS/10\r\nCER/TEST\r\nISU/20JUL11/SYDNEY/CARGOWISE SUPPORT\r\nREF/SYDFFAU/C00001001\r\n", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("08132652362", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TraxonEnvelopContext_EnvelopContextFMAWithExtraHeader()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Traxon");
            Expect.Call(partyResolver.ResolveParty("REUAIR08AFR")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("REUAGT89CARGOW/SYD01"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();
            using (message.BodyPart.Data = GetEmbeddedResource("Traxon.TestFiles.TraxonFMAFWBWithExternalData.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Traxon", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual("0", envelopContext.MessageVersion);
                Assert.AreEqual("FMA/0\r\nACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER\r\nFWB/16\r\n081-32652362SYDCHI/T10K100\r\nFLT/QF046/20\r\nRTG/CHIQF\r\nSHP\r\n/DIRECT CONTAINER LINE\r\n/LEVEL 9  83 YORK STREET\r\n/SYDNEY/NSW\r\n/AU\r\nCNE\r\n/MID-AMERICA OVERSEAS INC\r\n/1151 NORTH WOOD DALE ROAD WOOD\r\n/DALE/ILLINOIS\r\n/US\r\nAGT/1234/1234567\r\n/TEST\r\n/SYDNEY\r\nCVD/AUD/PP/PP/NVD/NCV/XXX\r\nRTD/1/P10/K100/CQ/W1666.5\r\n/NG/CONSOLIDATION AS PER\r\n/2/NG/TOTAL  10 PACKS\r\n/3/NG/DIMS 100X100X100 CM\r\n/4/NS/10\r\nCER/TEST\r\nISU/20JUL11/SYDNEY/CARGOWISE SUPPORT\r\nREF/SYDFFAU/C00001001\r\n", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("08132652362", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TraxonEnvelopContext_EnvelopContextFMAWithExtraHeaderAndMultiLineAck()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Traxon");
            Expect.Call(partyResolver.ResolveParty("REUAIR08AFR")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("REUAGT89CARGOW/SYD01"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();
            using (message.BodyPart.Data = GetEmbeddedResource("Traxon.TestFiles.TraxonFMAFWBWithExternalDataMultiline.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Traxon", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual("0", envelopContext.MessageVersion);
                Assert.AreEqual("FMA/0\r\nACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER\r\n/Second Line\r\n/Third Line\r\nFWB/16\r\n081-32652362SYDCHI/T10K100\r\nFLT/QF046/20\r\nRTG/CHIQF\r\nSHP\r\n/DIRECT CONTAINER LINE\r\n/LEVEL 9  83 YORK STREET\r\n/SYDNEY/NSW\r\n/AU\r\nCNE\r\n/MID-AMERICA OVERSEAS INC\r\n/1151 NORTH WOOD DALE ROAD WOOD\r\n/DALE/ILLINOIS\r\n/US\r\nAGT/1234/1234567\r\n/TEST\r\n/SYDNEY\r\nCVD/AUD/PP/PP/NVD/NCV/XXX\r\nRTD/1/P10/K100/CQ/W1666.5\r\n/NG/CONSOLIDATION AS PER\r\n/2/NG/TOTAL  10 PACKS\r\n/3/NG/DIMS 100X100X100 CM\r\n/4/NS/10\r\nCER/TEST\r\nISU/20JUL11/SYDNEY/CARGOWISE SUPPORT\r\nREF/SYDFFAU/C00001001\r\n", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("08132652362", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }
    }
}
