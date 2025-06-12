using System.Collections.Generic;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class DeltaEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DeltaEnvelopContext_EnvelopContextFSU()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("Delta");
			Expect.Call(partyResolver.ResolveParty("DeltaQantas")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("DeltaIKB"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("Delta.TestFiles.DeltaFSU.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Delta", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSU", envelopContext.MessageType);
				Assert.AreEqual("7", envelopContext.MessageVersion);
				Assert.AreEqual("FSU/7\r\n006-31442224NRTATL/T12K156.5\r\nRCF/DL280/21OCT1620/ATL/T12K156.5//A1453\r\nULD/PMC73850DD", envelopContext.InternalMessage);
				Assert.AreEqual(null, envelopContext.Reference);
				Assert.AreEqual("00631442224", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeltaEnvelopContext_EnvelopContextFNA()
        {
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

            var partyResolver = MockRepository.StrictMock<PartyResolver>("Delta");
            Expect.Call(partyResolver.ResolveParty("DeltaQantas")).Return("Sender1");
            partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("DeltaIKB"), Arg<string>.Is.Anything)).Return("Recipient1");

            MockRepository.ReplayAll();

            using (message.BodyPart.Data = GetEmbeddedResource("Delta.TestFiles.DeltaFNA.xml"))
            {
                var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "Delta", partyResolver);
                Assert.AreEqual("Sender1", envelopContext.SenderID);
                Assert.AreEqual("Recipient1", envelopContext.RecipientID);
                Assert.AreEqual("FNA", envelopContext.MessageType);
                Assert.AreEqual(null, envelopContext.MessageVersion);
                Assert.AreEqual("FNA\r\nACK/AWBREJECTED\r\n/VOLUME REQUIRED\r\nFWB/16\r\n006-31442224NRTATL/T12K156.5\r\nFLT/DL0280/21\r\nRTG/ATLDL\r\nSHP\r\n/YUSEN LOGISTICS CO..LTD. EAST JAPAN\r\n/1340-49.OYADO.IWAYAMA. SHIBAYAMA-MA\r\n/SANBU-GUN./CHIBA\r\n/JP/289-1608 \r\nCNE\r\n/YUSEN LOGISTICS AMERICAS INC.\r\n/691 AIRPORT SOUTH PARKWAY\r\n/COLLEGEPARK/GA\r\n/US/30349/TE/17709091460\r\nAGT/188033-013/1630540/0996\r\n/YUSEN LOGISTICS CO..LTD. \r\n/CONSOLI CENTER .B\r\nCVD/JPY//PP/NVD/NCV/XXX\r\nRTD/1/P12/K156.5/CQ/W156.5/R1290/T201885\r\n/NG/CONSOLIDATED SHIPMEN\r\n/2/NS/12\r\nPPD/WT201885\r\n/OC18780/CT220665\r\nISU/21OCT11/NARITA. JAPAN\r\nOSI/FIRMS CODE.L209\r\nREF/YASJPXX/1630540", envelopContext.InternalMessage);
                Assert.AreEqual(null, envelopContext.Reference);
                Assert.AreEqual("00631442224", envelopContext.ClientAWB);
            }

            MockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DeltaEnvelopContext_PromotedValue()
		{
			var promotedValue = new DeltaPromotedValue();
			using (var message = GetEmbeddedResource("Delta.TestFiles.DeltaFSU.xml"))
			{
				Assert.AreEqual("DeltaQantas", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("DeltaIKB", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("FSU/7\r\n006-31442224NRTATL/T12K156.5\r\nRCF/DL280/21OCT1620/ATL/T12K156.5//A1453\r\nULD/PMC73850DD", promotedValue.Find(message, "InternalMessage"));
			}
		}
	}
}
