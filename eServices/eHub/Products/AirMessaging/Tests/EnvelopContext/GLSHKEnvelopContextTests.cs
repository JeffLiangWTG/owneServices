using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class GLSHKEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKEnvelopContext_EnvelopContext()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("GLSHK");
			Expect.Call(partyResolver.ResolveParty("RHKAIR08CPA")).Return("Sender1");
			partyResolver.Expect(x => x.ResolveRecipient(Arg<string>.Is.Equal("RHKAGT02TEST/HKG81"), Arg<string>.Is.Anything)).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFNAFHL.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "GLSHK", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FNA", envelopContext.MessageType);
				Assert.AreEqual("0", envelopContext.MessageVersion);
				Assert.AreEqual("FNA\r\nACK/AWB9060 AWB NUMBER ALREADY USED\r\nFHL/4\r\nMBI/081-32652312SYDCHI/T10K100\r\nHBS/242435352423/SYDCHI/10/K100/10/TEST\r\nTXT/TEST\r\nOCI/AU/DNR/D/0028C\r\n/AU/DNR/D/1455\r\nSHP/AUSTRALIA JOHNTON\r\n/UNIT 2  63 WARREN STREET ST\r\n/LUCIA/QLD\r\n/AU/2230\r\nCNE/I.M.C. HOLDINGS\r\n/95 SOUTH ROUTE 83\r\n/GRAYSLAKE  USA/ILLINOIS\r\n/US\r\nCVD/AUD/CP/NVD/1000/1000\r\n", envelopContext.InternalMessage);
				Assert.AreEqual("0813265231224243535242323232323", envelopContext.Reference);
				Assert.AreEqual("08132652312", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKEnvelopContext_PromotedValue()
		{
			var promotedValue = new GLSHKPromotedValue();
			using (var message = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFNAFHL.xml"))
			{
				Assert.AreEqual("RHKAIR08CPA", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("RHKAGT02TEST/HKG81", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("FNA\r\nACK/AWB9060 AWB NUMBER ALREADY USED\r\nFHL/4\r\nMBI/081-32652312SYDCHI/T10K100\r\nHBS/242435352423/SYDCHI/10/K100/10/TEST\r\nTXT/TEST\r\nOCI/AU/DNR/D/0028C\r\n/AU/DNR/D/1455\r\nSHP/AUSTRALIA JOHNTON\r\n/UNIT 2  63 WARREN STREET ST\r\n/LUCIA/QLD\r\n/AU/2230\r\nCNE/I.M.C. HOLDINGS\r\n/95 SOUTH ROUTE 83\r\n/GRAYSLAKE  USA/ILLINOIS\r\n/US\r\nCVD/AUD/CP/NVD/1000/1000\r\n", promotedValue.Find(message, "InternalMessage"));
				Assert.AreEqual("CIMFNA", promotedValue.Find(message, "MessageType"));
				Assert.AreEqual("0", promotedValue.Find(message, "MessageVersion"));
				Assert.AreEqual("0813265231224243535242323232323", promotedValue.Find(message, "Reference"));
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKEnvelopContext_PromotedValue_FSA()
		{
			var promotedValue = new GLSHKPromotedValue();
			using (var message = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFSA.xml"))
			{
				Assert.AreEqual("RHKAIR01MSPFMPO", promotedValue.Find(message, "SenderPIMA"));
				Assert.AreEqual("RHKAGT021337382/HKG85", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("FSA/9\r\n403-13388185HKGSFO/T2K1050\r\nDEP/PO4235D/16JUN/LAXSFO/T2K1050/A1700/A0900-N\r\nRCF/PO916/14JUN2025/LAX/T2K1050/A1927/A2326\r\nULD/PMC542345Y\r\n",
					promotedValue.Find(message, "InternalMessage"));
				Assert.AreEqual("CIMFSA", promotedValue.Find(message, "MessageType"));
				Assert.AreEqual("9", promotedValue.Find(message, "MessageVersion"));
				Assert.AreEqual("TBC21071515342", promotedValue.Find(message, "Reference"));
			}
		}
	}
}
