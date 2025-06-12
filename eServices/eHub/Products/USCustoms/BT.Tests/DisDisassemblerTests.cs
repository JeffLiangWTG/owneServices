using System.Collections.Generic;
using System.Reflection;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.USCustoms.BT.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.USCustoms.BT.Tests
{
	[TestClass]
	public class DisDisassemblerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisDisassembler_Subscriptions()
		{
			var message1 = msgFactory.CreateMessage();
			var message2 = msgFactory.CreateMessage();
			message2.AddPart("Body", msgFactory.CreateMessagePart(), true);
			message2.BodyPart.Data = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Products.USCustoms.BT.Tests.TestFiles.DISMessageEnvelope.xml");
			message2.Context = msgFactory.CreateMessageContext();

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscribedClients(message2)).Return(new[] { "CLIENT1" });

			var disassembler = MockRepository.GeneratePartialMock<DisDisassembler>();
			disassembler.Stub(x => x.DisassembleXml(null, message1)).Return(message2);
			disassembler.Stub(x => x.GetSubscriptionAccessor()).Return(subscriptionAccessor);

			disassembler.Disassemble(null, message1);

			List<IBaseMessage> results = new List<IBaseMessage>();
			IBaseMessage result;
			while ((result = disassembler.GetNext(null)) != null)
				results.Add(result);

			Assert.AreEqual(1, results.Count);
			Assert.AreSame(message2, results[0]);
			Assert.AreEqual("CLIENT1", results[0].Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisDisassembler_FilerCode()
		{
			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.GetMessageFactory()).Return(msgFactory);

			var message1 = msgFactory.CreateMessage();
			var message2 = msgFactory.CreateMessage();
			message2.AddPart("Body", msgFactory.CreateMessagePart(), true);
			message2.BodyPart.Data = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Products.USCustoms.BT.Tests.TestFiles.DISMessageEnvelope.xml");
			message2.Context = msgFactory.CreateMessageContext();
			message2.Context.Write("DestinationPartyID", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "AAA");

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscribedClients(message2)).Return(new string[] { });
			subscriptionAccessor.Stub(x => x.SelectUSCustomsClientsForFilerCode("AAA", true)).Return(new[] { "CLIENT1", "CLIENT2" });

			var disassembler = MockRepository.GeneratePartialMock<DisDisassembler>();
			disassembler.Stub(x => x.DisassembleXml(pipelineContext, message1)).Return(message2);
			disassembler.Stub(x => x.GetSubscriptionAccessor()).Return(subscriptionAccessor);

			disassembler.Disassemble(pipelineContext, message1);

			List<IBaseMessage> results = new List<IBaseMessage>();
			IBaseMessage result;
			while ((result = disassembler.GetNext(null)) != null)
				results.Add(result);

			Assert.AreEqual(2, results.Count);
			Assert.AreSame(message2, results[0]);
			Assert.AreNotSame(message2, results[1]);
			Assert.AreEqual("CLIENT1", results[0].Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual("CLIENT2", results[1].Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
		}

		MessageFactory msgFactory = new MessageFactory();
	}
}
