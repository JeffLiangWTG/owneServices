using System;
using System.Collections.Generic;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class NewTrackingIDAssignmentComponentTest: BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignNewTrackingID()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("EDI", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.X12.txt");

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			var subscribers = new List<string>();
			Expect.Call(msgHelper.GetSubscribers(message)).Return(subscribers).Repeat.Any();

			var component = MockRepository.PartialMock<NewTrackingIDAssignmentComponent>();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			var accessor = MockRepository.StrictMock<IOutboxAccessor>();

			Expect.Call(() => { accessor.UpdateMessageStatus(null, null, null, null, null); }).IgnoreArguments()
				.Do(new Action<string, string, string, int?, int?>((m, i, o, istatus, ostatus) =>
				{
					Assert.IsNotNull(m);
					Assert.AreEqual(3, istatus);
				})).Repeat.Times(2);

			Expect.Call(component.GetOutboxAccessor()).Return(accessor).Repeat.Times(2);

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.ProcessSubscriptions = false;
			var messageTrackingID = Guid.NewGuid().ToString().ToUpper();
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(messageTrackingID,result.Context.ReadPropertyString<MessageTrackingID>());

			component.Enabled = true;
			component.ProcessSubscriptions = true;
			subscribers.Add("TESTRECIPIENT1");
		
			result = component.Execute(pipelineContext, message);
			Assert.AreNotEqual(messageTrackingID, result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(message, result);

			component.Enabled = false;
			component.ProcessSubscriptions = true;
			result = component.Execute(pipelineContext, message);
			Assert.AreNotEqual(messageTrackingID, result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(subscribers[0], result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual(message, result);

			component.Enabled = false;
			component.ProcessSubscriptions = true;
			subscribers.Add("TESTRECIPIENT2");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Multiple subcribers found for message which is not supported for this pipeline component. Selected subcribers are: TESTRECIPIENT1, TESTRECIPIENT2");

			component.Enabled = true;
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribeMessage = true;
			subscribers.Clear();
			message.Context.WriteProperty<BTS.DestinationParty>(null);
			result = component.Execute(pipelineContext, message);
			Assert.IsNull(result, "Process Subscription is enabled and no Subscription is found therefore message should be discarded");

			component.Enabled = true;
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribeMessage = false;		
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result, "DiscardUnscribedMessage is not activated, the message won't be discarded");
			Assert.IsNull(result.Context.ReadPropertyString<BTS.DestinationParty>());

			component.Enabled = false;
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribeMessage = true;		
			result = component.Execute(pipelineContext, message);
			Assert.IsNull(result, "Process Subscription is enabled and no Subscription is found therefore message should be discarded");
			
			component.Enabled = true;
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribeMessage = true;
			subscribers.Clear();
			message.Context.WriteProperty<BTS.DestinationParty>("TESTRECIPIENT2");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);
			Assert.AreEqual("TESTRECIPIENT2", result.Context.ReadPropertyString<BTS.DestinationParty>());

			component.Enabled = false;
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribeMessage = true;		
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);
			Assert.AreEqual("TESTRECIPIENT2", result.Context.ReadPropertyString<BTS.DestinationParty>());
			MockRepository.VerifyAll();

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignNewTrackingID_DiscardX12_997_With315Message()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.MessageType>("http://schemas.microsoft.com/BizTalk/EDI/X12/2006#X12_00401_315");
			message.AddPart("EDI", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.X12.txt");


			var component = MockRepository.PartialMock<NewTrackingIDAssignmentComponent>();

			MockRepository.ReplayAll();

			var messageTrackingID = Guid.NewGuid().ToString().ToUpper();
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);

			component.Enabled = true;
			component.DiscardX12_997 = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreNotEqual(messageTrackingID, result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(message, result);

			component.DiscardX12_997 = false;
			result = component.Execute(pipelineContext, message);
			Assert.AreNotEqual(messageTrackingID, result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(message, result);
		}		

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignNewTrackingID_DiscardX12_997_With997Message()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.MessageType>("http://schemas.microsoft.com/Edi/X12#X12_997_Root");
			message.AddPart("EDI", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.X12_997.txt");


			var component = MockRepository.PartialMock<NewTrackingIDAssignmentComponent>();

			var accessor = MockRepository.StrictMock<IOutboxAccessor>();

			Expect.Call(() => { accessor.UpdateMessageStatus(null, null, null, null, null); }).IgnoreArguments()
				.Do(new Action<string, string, string, int?, int?>((m, i, o, istatus, ostatus) =>
				{
					Assert.IsNotNull(m);
					Assert.AreEqual(3, istatus);
				})).Repeat.Times(2);

			Expect.Call(component.GetOutboxAccessor()).Return(accessor).Repeat.Times(2);

			MockRepository.ReplayAll();

			var messageTrackingID = Guid.NewGuid().ToString().ToUpper();
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);

			component.Enabled = true;
			component.DiscardX12_997 = true;
			var result = component.Execute(pipelineContext, message);
			Assert.IsNull(result);

			component.DiscardX12_997 = false;
			result = component.Execute(pipelineContext, message);
			Assert.AreNotEqual(messageTrackingID, result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignNewTrackingID_IgnoreAS2MDN()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.RouteDirectToTP>("True");
			message.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			var messageHelper = MockRepository.GenerateStub<IMessageHelper>();
			messageHelper.Stub(x => x.GetSubscribers(Arg<IBaseMessage>.Is.Anything)).Return(new List<string>());

			var component = MockRepository.GeneratePartialMock<NewTrackingIDAssignmentComponent>();
			component.ProcessSubscriptions = true;
			component.Stub(x => x.GetMessageHelper()).Return(messageHelper);

			var result = component.Execute(pipelineContext, message);

			Assert.AreSame(message, result);
			messageHelper.AssertWasNotCalled(x => x.GetSubscribers(Arg<IBaseMessage>.Is.Anything));
		}
	}
}
