using System.Collections.Generic;
using System.IO;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class BTEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BTEnvelopContext_EnvelopContextFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("BT");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("NDTAKBX"), Arg<string>.Is.Anything)).Return("Recipient1");
			Expect.Call(partyResolver.ResolveParty("REUAIR08DLH")).Return("Sender1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("BT.TestFiles.Text.BTFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "BT", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("6", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/6\r\n001-13697320NRTORD/T11K3109.4\r\nBKD/AA0154/27OCT/NRTORD/T11/S1815/S1555", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("00113697320", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BTEnvelopContext_PromotedValue()
		{
			var promotedValue = new BTPromotedValue();
			using (var message = GetEmbeddedResource("BT.TestFiles.Text.BTFSU.xml"))
			{
				Assert.AreEqual("LONBCCR", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("NDTAKBX", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("REUAIR08DLH", promotedValue.Find(message, "AirlinePIMA"));
				Assert.AreEqual("FSU/6\r\n001-13697320NRTORD/T11K3109.4\r\nBKD/AA0154/27OCT/NRTORD/T11/S1815/S1555", promotedValue.Find(message, "InternalMessage"));

			}
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void BTEnvelopContext_EnvelopContextFNA()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("BT");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("NDTAKBX"), Arg<string>.Is.Anything)).Return("Recipient1");
            Expect.Call(partyResolver.ResolveParty("REUAIR08DLH")).Return("Sender1");

            MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("BT.TestFiles.BTFNA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "BT", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FNA", envelopContext.MessageType);
                Assert.AreEqual(null, envelopContext.MessageVersion);
                Assert.AreEqual("FNA\r\nACK/AWB REJECTED\r\n/COR0020\r\nFWB/14\r\n020-63075456OSLMVD/T2K580\r\nFLT/LH7371/27/LH510/30\r\nRTG/FRALH/EZELH\r\nSHP\r\n/DHL GLOBAL FORWARDING NORWAY\r\n/GNEISVEIEN 3\r\n/SKEDSMOKORSET\r\n/NO/2020\r\nCNE\r\n/AERO GARGAS S.A.\r\n/BUENOS AIRES 282\r\n/MONTEVIDEO\r\n/UY/11000\r\nAGT/574913113/6047017/1333\r\n/DHL GLOBAL FORWARDING AS\r\n/SKEDSMOKORSET\r\nSSR/DOCS ATTACHED TO MAWB\r\nACC/GEN/OSL017164\r\n/GEN/SECURED CUSTOMERS\r\n/GEN/SPX KC NO RA 00009-01 0910\r\n/GEN/2011-10-27 13 46 V.MATHISEN\r\n/GEN/ETA 1700 31 OCT\r\n/GEN/TD.PRO\r\nCVD/NOK/PP/PP/NVD/NCV/XXX\r\nRTD/1/P2/K580/CQ/W580.0/R34.00/T19720.00\r\n/NG/CONSOLIDATED SHIPMEN\r\n/2/NG/PER ATTACHED MANIFES\r\n/3/NG/120X80X145 120X80X58\r\n/4/NG/CMS\r\n/5/NG/SLAC-2\r\n/6/ND//NDA0-0-0/2\r\n/7/NS/2\r\nPPD/WT19720\r\n/CT19720\r\nCER/ON BEHALF OF DHL\r\nISU/27OCT11/OSLO/DHL GLOBAL FWD\r\nREF/LHRAE7X/OSL017164\r\nCOR/X\r\nSPH/HEA", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("02063075456", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }
    }
}
