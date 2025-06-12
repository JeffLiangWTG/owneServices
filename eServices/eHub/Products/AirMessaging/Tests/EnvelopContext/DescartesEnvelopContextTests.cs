using System.Collections.Generic;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSMQT;
using Rhino.Mocks;
using Tests;
using CargoWise.eHub.Products.AirMessaging.Schemas.Properies;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class DescartesEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DescartesEnvelopContext_EnvelopContextFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<SenderPIMA>("SenderPIMA1");
			message.Context.WriteProperty<RecipientPIMA>("RecipientPIMA1");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("Descartes");
			Expect.Call(partyResolver.ResolveParty("SenderPIMA1")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("RecipientPIMA1"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("Descartes.TestFiles.DescartesFWB.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Descartes", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FWB", envelopContext.MessageType);
				Assert.AreEqual("9", envelopContext.MessageVersion);
				Assert.AreEqual("FWB/9\r\n125-91595066DXBHAJ/T2K34.5\r\nFLT/BA108/04/BA902/05\r\nRTG/LHRBA/FRABA/HAJBA\r\nSHP\r\n/DANZAS AEI EMIRATES LLC\r\n/POST BOX 47814\r\n/DUBAI\r\n/AE\r\nCNE\r\n/DHL GLOBAL FORWARDING GMBH\r\n/BUILDING E\r\n/LANGENHAGEN\r\n/DE/30855\r\nAGT//8647090/0014\r\n/DANZAS AEI EMIRATES LLC\r\n/DUBAI\r\nSSR/02 CTNS MRKD   LBLD PLS NTFY CNEE IMM UPON ARVL\r\n/DOCS ATTACHED\r\nACC/GEN/DXB073598\r\n/GEN/FREIGHT PREPAID\r\nCVD/AED/PP/PP/NVD/NCV/XXX\r\nRTD/1/P2/K34.5/CN/W34.5/R27.85/T960.83\r\n/NG/ANGLES SHAPES AND\r\n/2/NG/SECTIONS\r\n/3/NG/DIMS 100X40X14CMS 1\r\n/4/NG/30X26X14CMS 1\r\n/5/NG/           SLAC-2\r\n/6/NS/2\r\nOTH/P/AWA25.00MOC35.00MYC142.00\r\n/P/SCC14.00\r\nPPD/WT960.83\r\n/OA25.00/OC191.00/CT1176.83\r\nCER/FXB ON BEHALF OF DHL\r\nISU/03JUL12/DUBAI            /DHL GLOBAL FWD\r\nREF/LHRAE7X/DXB073598", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("12591595066", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DescartesEnvelopContext_EnvelopContextFMA()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.WriteProperty<SenderPIMA>("SenderPIMA1");
            message.Context.WriteProperty<RecipientPIMA>("RecipientPIMA1");
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Descartes");
            Expect.Call(partyResolver.ResolveParty("SenderPIMA1")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("RecipientPIMA1"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("Descartes.TestFiles.DescartesFMA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Descartes", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FMA", envelopContext.MessageType);
                Assert.AreEqual(null, envelopContext.MessageVersion);
                Assert.AreEqual("FMA\r\nACK/MESSAGE FOR EAWB SHIPMENT RECEIVED AND PROCESSED BY TURKISH CARGO\r\n/FWB RCVD 01-APR-2021 00 00 04 GMT -\r\nFWB/17\r\n235-17576031LHRCAN/T1K3900", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("23517576031", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DescartesEnvelopContext_PromotedValue()
		{
			var promotedValue = new DescartesPromotedValue();
			using (var message = GetEmbeddedResource("Descartes.TestFiles.DescartesFWB.xml"))
			{
				Assert.AreEqual("FWB/9\r\n125-91595066DXBHAJ/T2K34.5\r\nFLT/BA108/04/BA902/05\r\nRTG/LHRBA/FRABA/HAJBA\r\nSHP\r\n/DANZAS AEI EMIRATES LLC\r\n/POST BOX 47814\r\n/DUBAI\r\n/AE\r\nCNE\r\n/DHL GLOBAL FORWARDING GMBH\r\n/BUILDING E\r\n/LANGENHAGEN\r\n/DE/30855\r\nAGT//8647090/0014\r\n/DANZAS AEI EMIRATES LLC\r\n/DUBAI\r\nSSR/02 CTNS MRKD   LBLD PLS NTFY CNEE IMM UPON ARVL\r\n/DOCS ATTACHED\r\nACC/GEN/DXB073598\r\n/GEN/FREIGHT PREPAID\r\nCVD/AED/PP/PP/NVD/NCV/XXX\r\nRTD/1/P2/K34.5/CN/W34.5/R27.85/T960.83\r\n/NG/ANGLES SHAPES AND\r\n/2/NG/SECTIONS\r\n/3/NG/DIMS 100X40X14CMS 1\r\n/4/NG/30X26X14CMS 1\r\n/5/NG/           SLAC-2\r\n/6/NS/2\r\nOTH/P/AWA25.00MOC35.00MYC142.00\r\n/P/SCC14.00\r\nPPD/WT960.83\r\n/OA25.00/OC191.00/CT1176.83\r\nCER/FXB ON BEHALF OF DHL\r\nISU/03JUL12/DUBAI            /DHL GLOBAL FWD\r\nREF/LHRAE7X/DXB073598", promotedValue.Find(message, "InternalMessage"));
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DescartesEnvelopContext_MissingRecipientPIMA()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<SenderPIMA>("SenderPIMA1");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("Descartes");
			Expect.Call(partyResolver.ResolveParty("SenderPIMA1")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveClientAWB(Arg<string>.Is.Equal("23517576031"))).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("Descartes.TestFiles.DescartesFMA.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Descartes", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FMA", envelopContext.MessageType);
				Assert.AreEqual(null, envelopContext.MessageVersion);
				Assert.AreEqual("FMA\r\nACK/MESSAGE FOR EAWB SHIPMENT RECEIVED AND PROCESSED BY TURKISH CARGO\r\n/FWB RCVD 01-APR-2021 00 00 04 GMT -\r\nFWB/17\r\n235-17576031LHRCAN/T1K3900", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("23517576031", envelopContext.ClientAWB);

			}
			MockRepository.VerifyAll();
		}
	}
}
