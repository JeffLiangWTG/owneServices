using System;
using System.Data.SqlClient;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class IncomingMessageLoggerTest : BaseComponentTest
	{
		private delegate void InsertToInboxDelegate(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLogIncomingMessage()
		{
			var pipelineContext = new PipelineContext();

			string internalTrackingID = Guid.NewGuid().ToString();
			string messageTrackingID = Guid.NewGuid().ToString();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			Expect.Call(() => { inboxAccessor.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null); }).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					Assert.AreEqual("SENDERID", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(MessageStatus.Processing, status);
					Assert.AreEqual(messageTrackingID, gatewayMessage.MessageTrackingID.ToString());
					Assert.AreEqual("RECIPIENTID", gatewayMessage.ClientID);
					Assert.AreEqual(string.Empty, gatewayMessage.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
					Assert.AreEqual("blah@blah.com", gatewayMessage.EmailSubject);
					Assert.AreEqual("blah.txt", gatewayMessage.FileName);

					message.BodyPart.Data.SeekBegin();
					Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
				}));

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			Expect.Call(component.GetInboxAccessor()).Return(inboxAccessor);

			MockRepository.ReplayAll();

			Assert.AreEqual(message, component.Execute(pipelineContext, message));

			component.Enabled = true;
			Assert.AreEqual(message, component.Execute(pipelineContext, message));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_IncomingMessageLogger()
		{
			var component = new IncomingMessageLogger();
			Assert.AreEqual("Writes incoming message into eHubInbox compressed and Base64-encoded.", component.Description);
			Assert.AreEqual("Message Logger", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("F1137BEA-74C3-4493-92FA-678EAB5DA4B8"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			propertyBag.Expect(x => x.Write("Enabled", (object)false));
			propertyBag.Expect(x => x.Write("ShouldAssignNewInternalTrackingID", (object)false));
			propertyBag.Expect(x => x.Read(Arg.Is("Enabled"), out Arg<Object>.Out(true).Dummy, Arg.Is(0)));
			propertyBag.Expect(x => x.Read(Arg.Is("ShouldAssignNewInternalTrackingID"), out Arg<Object>.Out(true).Dummy, Arg.Is(0)));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.ShouldAssignNewInternalTrackingID = false;
			component.Save(propertyBag, true, true);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.IsTrue(component.ShouldAssignNewInternalTrackingID);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIncomingMessageLoggerDoesNotChangeMessageStreamPosition()
		{
			var pipelineContext = new PipelineContext();

			string internalTrackingID = Guid.NewGuid().ToString();
			string messageTrackingID = Guid.NewGuid().ToString();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			message.BodyPart.GetOriginalDataStream().Position = 11;

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			Expect.Call(() => { inboxAccessor.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null); }).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					message.BodyPart.GetOriginalDataStream().CompressAndEncode();
				}));

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			Expect.Call(component.GetInboxAccessor()).Return(inboxAccessor);

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.Execute(pipelineContext, message);
			Assert.AreEqual(11, message.BodyPart.GetOriginalDataStream().Position);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIncomingMessageLoggerAssignNewInternalTrackingID()
		{
			var pipelineContext = new PipelineContext();

			string internalTrackingID = Guid.NewGuid().ToString();
			string messageTrackingID = Guid.NewGuid().ToString();
			var activityId = Guid.NewGuid();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			message.BodyPart.GetOriginalDataStream().Position = 11;

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			var mostRecentPK = internalTrackingID;
			var previousInternalTrackingID = internalTrackingID;
			Expect.Call(() => { inboxAccessor.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null); }).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					Assert.AreEqual("SENDERID", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(MessageStatus.Processing, status);
					Assert.AreEqual(messageTrackingID, gatewayMessage.MessageTrackingID.ToString());
					Assert.AreEqual("RECIPIENTID", gatewayMessage.ClientID);
					Assert.AreEqual(string.Empty, gatewayMessage.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
					Assert.AreEqual("blah@blah.com", gatewayMessage.EmailSubject);
					Assert.AreEqual("blah.txt", gatewayMessage.FileName);
					Assert.AreEqual(inboxPK.ToString(), message.Context.ReadPropertyString<InternalTrackingID>());

					message.BodyPart.Data.SeekBegin();
					Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
				}))
				.Repeat.Once();
			var logger = MockRepository.StrictMock<ILog>();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] Starting InsertToInbox for PK: {1}")),
				Arg<object[]>.Matches(y => y[0].Equals(activityId)))).Repeat.Once();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] Finished InsertToInbox for PK: {1}")),
				Arg<object[]>.Matches(y => y[0].Equals(activityId)))).Repeat.Once();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("Reassigning InternalTrackingId from '{0}' to '{1}'.")),
				Arg<object[]>.Matches(y => y[0].Equals(previousInternalTrackingID) && !y[1].ToString().Equals(previousInternalTrackingID, StringComparison.InvariantCultureIgnoreCase))));

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			Expect.Call(component.GetInboxAccessor()).Return(inboxAccessor);
			Expect.Call(component.GetActivityId()).Return(activityId);
			Expect.Call(component.GetLogger(message)).Return(logger);

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ShouldAssignNewInternalTrackingID = true;

			component.Execute(pipelineContext, message);
			Assert.AreNotEqual(previousInternalTrackingID, message.Context.ReadPropertyString<InternalTrackingID>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIncomingMessageLoggerKeepInternalTrackingID()
		{
			var pipelineContext = new PipelineContext();

			string internalTrackingID = Guid.NewGuid().ToString();
			string messageTrackingID = Guid.NewGuid().ToString();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			message.BodyPart.GetOriginalDataStream().Position = 11;

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			var mostRecentPK = internalTrackingID;
			var previousInternalTrackingID = internalTrackingID;
			Expect.Call(() => { inboxAccessor.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null); }).IgnoreArguments()
				.Do(new InsertToInboxDelegate((string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage gatewayMessage) =>
				{
					Assert.AreEqual("SENDERID", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(MessageStatus.Processing, status);
					Assert.AreEqual(messageTrackingID, gatewayMessage.MessageTrackingID.ToString());
					Assert.AreEqual("RECIPIENTID", gatewayMessage.ClientID);
					Assert.AreEqual(string.Empty, gatewayMessage.SchemaName);
					Assert.AreEqual(MessageSchemaType.Xml, gatewayMessage.SchemaType);
					Assert.AreEqual("blah@blah.com", gatewayMessage.EmailSubject);
					Assert.AreEqual("blah.txt", gatewayMessage.FileName);
					Assert.AreEqual(inboxPK.ToString(), message.Context.ReadPropertyString<InternalTrackingID>());

					message.BodyPart.Data.SeekBegin();
					Assert.AreEqual(message.BodyPart.Data.ReadToEnd(), gatewayMessage.MessageStream.DecodeAndDecompress().ReadToEnd());
				}))
				.Repeat.Once();

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			Expect.Call(component.GetInboxAccessor()).Return(inboxAccessor);

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ShouldAssignNewInternalTrackingID = false;

			component.Execute(pipelineContext, message);
			Assert.AreEqual(previousInternalTrackingID, message.Context.ReadPropertyString<InternalTrackingID>());

			MockRepository.VerifyAll();
		}

		[TestMethod]
		public void TestIncomingMessageLogger_LogPrimaryKey()
		{
			var pipelineContext = new PipelineContext();
			var internalTrackingID = Guid.NewGuid().ToString();
			var messageTrackingID = Guid.NewGuid().ToString();
			var activityId = Guid.NewGuid();

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");
			message.BodyPart.GetOriginalDataStream().Position = 0;

			var logger = MockRepository.StrictMock<ILog>();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] Starting InsertToInbox for PK: {1}")),
				Arg<object[]>.Matches(y => y[0].Equals(activityId) && y[1].ToString().Equals(internalTrackingID, StringComparison.InvariantCultureIgnoreCase)))).Repeat.Once();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] Finished InsertToInbox for PK: {1}")),
				Arg<object[]>.Matches(y => y[0].Equals(activityId) && y[1].ToString().Equals(internalTrackingID, StringComparison.InvariantCultureIgnoreCase)))).Repeat.Once();

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null))
				.IgnoreArguments().Repeat.Once();

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor);
			component.Expect(x => x.GetLogger(message)).Return(logger);
			component.Expect(x => x.GetActivityId()).Return(activityId);

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ShouldAssignNewInternalTrackingID = false;
			component.Execute(pipelineContext, message);

			MockRepository.VerifyAll();
		}

		[TestMethod]
		public void TestIncomingMessageLogger_LogException()
		{
			var pipelineContext = new PipelineContext();
			var internalTrackingID = Guid.NewGuid().ToString();
			var messageTrackingID = Guid.NewGuid().ToString();
			var activityId = Guid.NewGuid();
			var exception = new Exception("Test exception message");

			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<BTS.SourceParty>("SENDERID");
			message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.Context.WriteProperty<BTS.DestinationParty>("RECIPIENTID");
			message.Context.WriteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/hpo/2010/06#IFCSUM");
			message.Context.WriteProperty<OverrideEmailSubject>("blah@blah.com");
			message.Context.WriteProperty<OverrideFilename>("blah.txt");
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");
			message.BodyPart.GetOriginalDataStream().Position = 0;

			var logger = MockRepository.StrictMock<ILog>();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] Starting InsertToInbox for PK: {1}")),
				Arg<object[]>.Matches(y => y[0].Equals(activityId) && y[1].ToString().Equals(internalTrackingID, StringComparison.InvariantCultureIgnoreCase))))
				.Repeat.Once();
			logger.Expect(x => x.DebugFormat(
				Arg<string>.Matches(y => y.Equals("[{0}] InsertToInbox inboxPK: [{1}], messageTrackingId: [{2}], senderId: [{3}], recipientId: [{4}], fileName: [{5}], emailSubject: [{6}] caught exception: ")),
				Arg<Exception>.Matches(y => y == exception),
				Arg<object[]>.Matches(y => y[0].Equals(activityId) && y[1].ToString().Equals(internalTrackingID, StringComparison.InvariantCultureIgnoreCase) && y[2].Equals(messageTrackingID) && y[3].Equals("SENDERID") && y[4].Equals("RECIPIENTID") && y[5].Equals("blah.txt") && y[6].Equals("blah@blah.com"))));

			var inboxAccessor = MockRepository.StrictMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null))
				.IgnoreArguments()
				.Throw(exception)
				.Repeat.Once();

			var component = MockRepository.PartialMock<IncomingMessageLogger>();
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor);
			component.Expect(x => x.GetLogger(message)).Return(logger);
			component.Expect(x => x.GetActivityId()).Return(activityId);

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ShouldAssignNewInternalTrackingID = false;
			AssertException(() => component.Execute(pipelineContext, message), exception.GetType(), exception.Message);

			MockRepository.VerifyAll();
		}
	}
}
