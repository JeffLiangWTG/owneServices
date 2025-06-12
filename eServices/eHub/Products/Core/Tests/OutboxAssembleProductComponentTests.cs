using System;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class OutboxAssembleComponentTests : BaseComponentTest
	{
		public IBaseMessage CreateTestMessage()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<CargoWise.eHub.Products.Core.PropertySchemas.MessageTrackingID>("02FA29DB-6657-4190-8F81-C55FAA60E3F5");
			message.Context.PromoteProperty<InternalTrackingID>("19198CD8-0387-481E-A12A-ADE98FDF0FAE");
			message.Context.PromoteProperty<BTS.MessageType>("NZCustoms");
			return message;
		}

		public OutboxAssembleProductComponent CreatePipelineComponent()
		{
			var component = MockRepository.GenerateStrictMock<OutboxAssembleProductComponent>();
			component.Expect(x => x.GetCurrentUTCString()).Return(new DateTime(2012, 10, 23, 10, 12, 33).ToString("yyyy-MM-dd HH:mm:ss.fff"));
			component.Expect(x => x.GetNewGuid()).Return(new Guid("99C2726E-B67C-43E2-B96E-CCE2745F7195"));
			component.Enabled = true;
			return component;
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestOutboxAssembleComponentUseMessageTrackingIDAsInboxPK()
		{
			var pipelineContext = new PipelineContext();
			var message = CreateTestMessage();
			var component = CreatePipelineComponent();
			component.UseMessageTrackingIDAsInboxPK = true;

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.InsertOutboxMessage1.xml"), result.BodyPart.Data);

			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestOutboxAssembleComponentDontUseMessageTrackingIDAsInboxPK()
		{
			var pipelineContext = new PipelineContext();
			var message = CreateTestMessage();
			var component = CreatePipelineComponent();
			component.UseMessageTrackingIDAsInboxPK = false;

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.InsertOutboxMessage2.xml"), result.BodyPart.Data);

			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_OutboxAssembleComponent()
		{
			var component = new OutboxAssembleProductComponent();
			Assert.AreEqual("Product: Outbox message assemble component.", component.Description);
			Assert.AreEqual("Product: Outbox Message Assembler", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));

			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("35c2a01d-6cef-4d60-87c5-bdcfdbd7da3a"), guid);

			var propertyBag = MockRepository.GenerateStrictMock<IPropertyBag>();
			propertyBag.Expect(x => x.Write("Enabled", (object)false));
			propertyBag.Expect(x => x.Write("UseMessageTrackingIDAsInboxPK", (object)true));
			propertyBag.Expect(x => x.Read(Arg.Is("Enabled"), out Arg<Object>.Out(true).Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("UseMessageTrackingIDAsInboxPK"), out Arg<Object>.Out(false).Dummy, Arg.Is(0)));

			// Test save
			component.Enabled = false;
			component.UseMessageTrackingIDAsInboxPK = true;
			component.Save(propertyBag, true, true);

			// Test load
			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.IsFalse(component.UseMessageTrackingIDAsInboxPK);

			propertyBag.VerifyAllExpectations();
		}
	}
}
