using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents.AssembleDisassemble
{
	[TestClass]
	public class ProcessAcknowledgementMessageComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_ACK()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<BTS.AckType>("ACK");

			var component = MockRepository.PartialMock<ProcessAcknowledgementMessageComponent>();
			component.Enabled = true;

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.dbo_ProcessAcknowledgementMessage_Ack.xml"), result.BodyPart.Data);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_NACK()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<BTS.AckType>("NACK");
			message.Context.PromoteProperty<BTS.AckDescription>("Errrrrrrorrrrrrrr");

			var component = MockRepository.PartialMock<ProcessAcknowledgementMessageComponent>();
			component.Enabled = true;

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.dbo_ProcessAcknowledgementMessage_Nack.xml"), result.BodyPart.Data);

			MockRepository.VerifyAll();
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_NACKUsingOutboxPK()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<OutboxPK>("3333333");
			message.Context.PromoteProperty<BTS.AckType>("NACK");
			message.Context.PromoteProperty<BTS.AckDescription>("Errrrrrrorrrrrrrr");

			var component = MockRepository.PartialMock<ProcessAcknowledgementMessageComponent>();
			component.Enabled = true;
			component.UsingOutboxPK = true;

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.dbo_ProcessAcknowledgementMessage_NackUsingOutboxPK.xml"), result.BodyPart.Data);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_ProcessAcknowledgementMessageComponent()
		{
			var component = new ProcessAcknowledgementMessageComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("Process Acknowledgement Message", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("56874E8F-B8ED-42AE-801C-CC57D9A896B2"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string enabledProp = null;
			bool enabledValue = false;
			object enabledPtr = false;

			string outboxProp = null;
			bool outboxValue = false;
			object outboxPtr = false;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Write("UsingOutboxPK", ref outboxPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { outboxProp = propName; outboxValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			Expect.Call(() => propertyBag.Read("UsingOutboxPK", out outboxPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.Save(propertyBag, true, true);

			Assert.AreEqual("Enabled", enabledProp);
			Assert.IsFalse(enabledValue);
			component.Load(propertyBag, 0);

			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}
	}
}
