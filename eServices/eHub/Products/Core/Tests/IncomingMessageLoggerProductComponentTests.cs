using System;
using System.Transactions;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class IncomingMessageLoggerProductComponentTests : BaseComponentTest
	{
		private delegate void InsertToInboxDelegate(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLogIncomingMessage()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<EmailSubject>("EmailSubject");
			message.Context.WriteProperty<FileName>("FileName");
			message.Context.WriteProperty<ClientID>("RECIPIENT1");
			message.Context.WriteProperty<SenderID>("Sender1");
			message.Context.WriteProperty<ApplicationCode>("Code");
			message.Context.WriteProperty<SchemaType>("Xml");
			message.Context.WriteProperty<SchemaName>("http://cargowise.com/products#NZCustoms");
			message.Context.WriteProperty<MessageTrackingID>("FC0E921C-A404-48ED-801C-A26863712C34");

			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					Assert.AreNotEqual(Guid.Empty, inboxPK);
					Assert.AreEqual("Sender1", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(MessageStatus.Processed, status);
					Assert.AreEqual("fc0e921c-a404-48ed-801c-a26863712c34", gatewayMessage.MessageTrackingID.ToString());
					Assert.AreEqual("RECIPIENT1", gatewayMessage.ClientID);
					Assert.AreEqual("http://cargowise.com/products#NZCustoms", gatewayMessage.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
					Assert.AreEqual("EmailSubject", gatewayMessage.EmailSubject);
					Assert.AreEqual("FileName", gatewayMessage.FileName);

					message.BodyPart.Data.SeekBegin();
					Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
				}));

			var component = MockRepository.GenerateStrictMock<IncomingMessageLoggerProductComponent>();
			component.Expect(x => x.GetTransactionScope(Arg<IPipelineContext>.Is.Anything))
				.Do((Func<IPipelineContext, TransactionScope>)delegate(IPipelineContext dummy) { return new TransactionScope(); });
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor);

			Assert.AreEqual(message, component.Execute(pipelineContext, message));

			component.Enabled = true;
			component.MessageStatusName = "Processed";
			Assert.AreEqual(message, component.Execute(pipelineContext, message));

			inboxAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLogIncomingMessage_IdenticalMessages()
		{
			var messageTrackingID = "fc0e921c-a404-48ed-801c-a26863712c34";
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<EmailSubject>("EmailSubject");
			message.Context.WriteProperty<FileName>("FileName");
			message.Context.WriteProperty<ClientID>("RECIPIENT1");
			message.Context.WriteProperty<SenderID>("Sender1");
			message.Context.WriteProperty<ApplicationCode>("Code");
			message.Context.WriteProperty<SchemaType>("Xml");
			message.Context.WriteProperty<SchemaName>("http://cargowise.com/products#NZCustoms");
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);

			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			var inboxAccessor = MockRepository.GenerateStrictMock<IInboxAccessor>();
			var mostRecentPK = "";
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					Assert.AreNotEqual(Guid.Empty, inboxPK);
					Assert.AreEqual("Sender1", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(MessageStatus.Processed, status);
					Assert.AreEqual(messageTrackingID, gatewayMessage.MessageTrackingID.ToString());
					Assert.AreEqual("RECIPIENT1", gatewayMessage.ClientID);
					Assert.AreEqual("http://cargowise.com/products#NZCustoms", gatewayMessage.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
					Assert.AreEqual("EmailSubject", gatewayMessage.EmailSubject);
					Assert.AreEqual("FileName", gatewayMessage.FileName);
					mostRecentPK = inboxPK.ToString();

					message.BodyPart.Data.SeekBegin();
					Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
				}))
				.Repeat.Times(4);

			var component = MockRepository.GenerateStrictMock<IncomingMessageLoggerProductComponent>();
			component.Expect(x => x.GetTransactionScope(Arg<IPipelineContext>.Is.Anything))
				.Do((Func<IPipelineContext, TransactionScope>)delegate(IPipelineContext dummy) { return new TransactionScope(); });
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor).Repeat.Times(4);
			component.Enabled = true;
			component.UseMessageTrackingIDAsInboxPK = false;
			component.MessageStatusName = "Processed";

			// both inbox pks are unique
			Assert.AreEqual(message, component.Execute(pipelineContext, message));
			var message1PK = mostRecentPK;
			Assert.AreEqual(message, component.Execute(pipelineContext, message));
			Assert.AreNotEqual(message1PK, mostRecentPK);

			component.UseMessageTrackingIDAsInboxPK = true;

			// both inbox pks are the MessageTrackingID
			Assert.AreEqual(message, component.Execute(pipelineContext, message));
			Assert.AreEqual(messageTrackingID, mostRecentPK);
			Assert.AreEqual(message, component.Execute(pipelineContext, message));
			Assert.AreEqual(messageTrackingID, mostRecentPK);

			inboxAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_IncomingMessageLogger()
		{
			var component = new IncomingMessageLoggerProductComponent();

			Assert.AreEqual("Product: Writes incoming message into eHubInbox compressed and Base64-encoded.", component.Description);
			Assert.AreEqual("Product: Incoming Message Logger", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));

			Guid guid;
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("2AFDDA97-E998-4435-B08B-C4496FEA4EE8"), guid);

			var propertyBag = MockRepository.GenerateStrictMock<IPropertyBag>();
			propertyBag.Expect(x => x.Write("Enabled", (object)false));
			propertyBag.Expect(x => x.Write("UseMessageTrackingIDAsInboxPK", (object)true));
			propertyBag.Expect(x => x.Write("SenderID", (object)"HYEDAUIKB"));
			propertyBag.Expect(x => x.Write("ApplicationCode", (object)"Bla"));
			propertyBag.Expect(x => x.Write("MessageType", (object)"NZCustoms"));
			propertyBag.Expect(x => x.Write("MessageStatusName", (object)"Processed"));
			propertyBag.Expect(x => x.Read(Arg.Is("Enabled"), out Arg<Object>.Out(true).Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("UseMessageTrackingIDAsInboxPK"), out Arg<Object>.Out(false).Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("SenderID"), out Arg<Object>.Out("HYEDAUIKB1").Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("ApplicationCode"), out Arg<Object>.Out("Bla2").Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("MessageType"), out Arg<Object>.Out("NZCustoms3").Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("MessageStatusName"), out Arg<Object>.Out("Processed4").Dummy, Arg.Is(0)));

			// Test save
			component.Enabled = false;
			component.UseMessageTrackingIDAsInboxPK = true;
			component.SenderID = "HYEDAUIKB";
			component.ApplicationCode = "Bla";
			component.MessageType = "NZCustoms";
			component.MessageStatusName = "Processed";
			component.Save(propertyBag, true, true);

			// Test load
			component.SenderID = "Bla";
			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.IsFalse(component.UseMessageTrackingIDAsInboxPK);
			Assert.AreEqual("HYEDAUIKB1", component.SenderID);
			Assert.AreEqual("Bla2", component.ApplicationCode);
			Assert.AreEqual("NZCustoms3", component.MessageType);
			Assert.AreEqual("Processed4", component.MessageStatusName);

			propertyBag.VerifyAllExpectations();
		}
	}
}
