using System;
using System.Collections;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents.AssembleDisassemble
{
	[TestClass]
	public class FlatFileDisassembleComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFFDisassemblerExtension()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionTestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var baseDasmMessage = MessageFactory.CreateMessage();
			baseDasmMessage.AddPart("message", MessageFactory.CreateMessagePart(), true);
			baseDasmMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionBaseDasmMessage.xml");
			baseDasmMessage.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var msgHelper = MockRepository.StrictMock<IMessageHelper>();

			Expect.Call(delegate()
				{
					msgHelper.EnqueueMessage(null, null, null, null, false, false);
				})
				.IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileDisassembleComponent>();
			component.ProcessSubscriptions = false;
			component.DiscardUnsubscribedMessages = false;
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(baseDasmMessage);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);

			Assert.AreEqual(baseDasmMessage, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFFDisassemblerExtension_ProcessSubscriptions()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionTestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Shipping Instruction");

			var baseDasmMessage = MessageFactory.CreateMessage();
			baseDasmMessage.AddPart("message", MessageFactory.CreateMessagePart(), true);
			baseDasmMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionBaseDasmMessage.xml");
			baseDasmMessage.Context = MessageFactory.CreateMessageContext();
			baseDasmMessage.Context.WriteProperty<DestinationParty>("RECIPIENT");

			var cloneMsg = MessageFactory.CreateMessage();
			cloneMsg.AddPart("message", MessageFactory.CreateMessagePart(), true);
			cloneMsg.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionBaseDasmMessage.xml");
			cloneMsg.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Any();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();

			Expect.Call(delegate ()
				{
					msgHelper.EnqueueMessage(null, null, null, null, false, false);
				})
				.IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>(
					(p, q, m, u, c, d) =>
					{
						m.Context.WriteProperty<DestinationParty>("SUBSCRIBER1");
						q.Enqueue(m);
						
						if (c)
						{
							cloneMsg.Context.WriteProperty<DestinationParty>("SUBSCRIBER2");
							q.Enqueue(cloneMsg);
						}

						return true;
					}))
				.Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribedMessages = false;

			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(baseDasmMessage);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);

			var result = component.GetNext(pipelineContext);
			Assert.AreEqual(baseDasmMessage, result);
			Assert.AreEqual("SUBSCRIBER1", result.Context.ReadPropertyString<DestinationParty>());

			result = component.GetNext(pipelineContext);
			Assert.AreEqual(cloneMsg, result);
			Assert.AreEqual("SUBSCRIBER2", result.Context.ReadPropertyString<DestinationParty>());

			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFFDisassemblerExtension_DiscardUnsubscribedMessages()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionTestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Shipping Instruction");
			message.Context.WriteProperty<MessageTrackingID>("TrackingID");

			var baseDasmMessage = MessageFactory.CreateMessage();
			baseDasmMessage.AddPart("message", MessageFactory.CreateMessagePart(), true);
			baseDasmMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileDisassembleExtensionBaseDasmMessage.xml");
			baseDasmMessage.Context = MessageFactory.CreateMessageContext();

			var outboxAccessor = MockRepository.StrictMock<IOutboxAccessor>();
			Expect.Call(() => outboxAccessor.UpdateInboxMessageDistributionStatus("TrackingID"));

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var msgHelper = MockRepository.StrictMock<IMessageHelper>();

			Expect.Call(delegate ()
				{
					msgHelper.EnqueueMessage(null, null, null, null, false, false);
				})
				.IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>(
					(p, q, m, u, c, d) => true))
				.Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribedMessages = true;

			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(baseDasmMessage);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();
			Expect.Call(component.GetOutboxAccessor()).Return(outboxAccessor);

			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);

			Assert.IsNull(component.GetNext(pipelineContext));
			MockRepository.VerifyAll();
		}
	}
}
