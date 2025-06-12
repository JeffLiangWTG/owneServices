using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class ARINCEnvelopContextTests : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCEnvelopContext_EnvelopContext()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("ARINC");
			Expect.Call(partyResolver.ResolveParty("SYDWTXH")).Return("Sender1");
			Expect.Call(partyResolver.ResolveClientAWB(Arg<string>.Is.Anything)).Return(null);
			Expect.Call(partyResolver.ResolveClientPIMA(Arg<string>.Is.Equal("CSGAGT86CTF/SYD01"))).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("ARINC.TestFiles.ARINC_FMA.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "ARINC", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FMA", envelopContext.MessageType);
				Assert.AreEqual(null, envelopContext.MessageVersion);
				Assert.AreEqual("FMA\r\nACK/FWB RECEIVED\r\nFWB/16\r\n157-23583770MELLHR/T8K1624", envelopContext.InternalMessage);
				Assert.AreEqual("", envelopContext.Reference);
				Assert.AreEqual("15723583770", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCEnvelopContext_EnvelopContextFromFFR()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("ARINC");
			Expect.Call(partyResolver.ResolveParty("SYDWTXH")).Return("Sender1");
			Expect.Call(partyResolver.ResolveClientAWB(Arg<string>.Is.Anything)).Return(null);
			Expect.Call(partyResolver.ResolveClientPIMA(Arg<string>.Is.Equal("CSGAGT86CTF/SYD01"))).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("ARINC.TestFiles.ARINC_FMA_From_FFR.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "ARINC", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FFAFMAFNA", envelopContext.MessageType);
				Assert.AreEqual("4", envelopContext.MessageVersion);
				Assert.AreEqual("QD SYDWTXH\r\n.HDQFMQR CX/060129 CSGAGT86CTF/SYD01\r\nFMA/4\r\nACK/FWB RECEIVED\r\nFFR/6\r\n157-23583770MELLHR/T8K1624\r\n", envelopContext.InternalMessage);
				Assert.AreEqual("CX", envelopContext.Reference);
				Assert.AreEqual("15723583770", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCEnvelopContext_EnvelopContextFromFSR()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("ARINC");
			Expect.Call(partyResolver.ResolveParty("SYDWTXH")).Return("Sender1");
			Expect.Call(partyResolver.ResolveClientAWB(Arg<string>.Is.Anything)).Return(null);
			Expect.Call(partyResolver.ResolveClientPIMA(Arg<string>.Is.Equal("CSGAGT86CTF/SYD01"))).Return("Recipient1");

			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("ARINC.TestFiles.ARINC_FNA_From_FSR.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "ARINC", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FSA", envelopContext.MessageType);
				Assert.AreEqual("SYDWTXH\r\n.HDQFMQR CX/060129 CSGAGT86CTF/SYD01\r\nFNA\r\nACK/UNAUTHORIZED REQUEST\r\nFSR\r\n086-57722512\r\n", envelopContext.InternalMessage);
				Assert.AreEqual("CX", envelopContext.Reference);
				Assert.AreEqual(null, envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCEnvelopContext_PromotedValue()
		{
			var promotedValue = new ARINCPromotedValue();
			using (var message = GetEmbeddedResource("ARINC.TestFiles.ARINC_FSA.xml"))
			{
				Assert.AreEqual("QU SYDWTXH", promotedValue.Find(message, "Priority-SenderAddress"));
				Assert.AreEqual("CSGAGT86CTF/SYD01", promotedValue.Find(message, "RecipientPIMA"));
				Assert.AreEqual("HDQFMQR", promotedValue.Find(message, "RecipientAddress"));
				Assert.AreEqual("QR/060129", promotedValue.Find(message, "Reference"));
				Assert.AreEqual("FSA/6\r\n172-33819866CGOATL/P3K64.0T35\r\nDLV/06FEB1417/ATL/P3K64.0/JAS FORWARDING USA INC\r\n172-33819866CGOATL/P32K683.0T35\r\nDEP/CV0992A/03FEB/ORDATL/P32K683.0/A1744-N/E2300-S", promotedValue.Find(message, "InternalMessage"));
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCEnvelopContext_EnvelopContextFNAMultiLines()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var partyResolver = MockRepository.StrictMock<PartyResolver>("ARINC");
			Expect.Call(partyResolver.ResolveParty("SYDWTXH")).Return("Sender1");
			Expect.Call(partyResolver.ResolveClientAWB(Arg<string>.Is.Anything)).Return(null);
			Expect.Call(partyResolver.ResolveClientPIMA(Arg<string>.Is.Equal("CSGAGT86AGA/SYD81"))).Return("Recipient1");


			MockRepository.ReplayAll();

			using (message.BodyPart.Data = GetEmbeddedResource("ARINC.TestFiles.ARINC_FNA_MultiLines.xml"))
			{
				var envelopContext = AirMessageDisassembleComponent.CreateEnvelopContext(message, "ARINC", partyResolver);
				Assert.AreEqual("Sender1", envelopContext.SenderID);
				Assert.AreEqual("Recipient1", envelopContext.RecipientID);
				Assert.AreEqual("FNA", envelopContext.MessageType);
				Assert.AreEqual("1", envelopContext.MessageVersion);
				Assert.AreEqual("FNA/1\r\nACK/SHC MUST BE BUP  WHEN ULD DETAILS ARE PRESENT. ...   AWB EXECUT\r\n/SECOND LINE\r\n/ANOTHER LINE\r\nFWB/16\r\n176-15786094SYDHKG/T1K3869\r\nFLT/EK9820/06\r\nRTG/HKGEK\r\nSHP\r\n/WATT EXPORT PTY LTD\r\n/WAREHOUSE R STORES 8 9\r\n/SYDNEY MARKETS/NSW\r\n/AU/2129/TE/61297644545\r\nCNE\r\n/WANG FAT HONG\r\n/FLAT B 19TH FLR WING TAI BLDG NO 81\r\n/KOWLOON\r\n/HK\r\nAGT//0235820/2125\r\n/TOWERS INTERNATIONAL\r\n/SYDNEY\r\nACC/GEN/CAN AEF399FR9\r\n/GEN/FREIGHT PREPAID\r\n/GEN/REF  1068088\r\nCVD/AUD/PP/PP/NVD/NCV/XXX", envelopContext.InternalMessage);
				Assert.AreEqual("", envelopContext.Reference);
				Assert.AreEqual("17615786094", envelopContext.ClientAWB);
			}

			MockRepository.VerifyAll();
		}
	}
}
