using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.OceanInsights.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using MIME;
using MSMQT;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace CargoWise.eHub.Products.OceanInsights.Tests
{
	[TestClass]
	public class OceanInsightsIncomingMessageLoggerTests : BaseComponentTest
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

			var inboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments()
				.Do(new InsertToInboxDelegate((senderID, envelopeTrackingID, inboxPK, status, gatewayMessage) =>
				{
					Assert.AreEqual("SENDERID", senderID);
					Assert.AreEqual(Guid.Empty, envelopeTrackingID);
					Assert.AreEqual(internalTrackingID, inboxPK.ToString());
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

			var component = MockRepository.GeneratePartialMock<OceanInsightsIncomingMessageLoggerTest>();
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor);

			Assert.AreEqual(message, component.Execute(pipelineContext, message));
			
			inboxAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}
	}

	public class OceanInsightsIncomingMessageLoggerTest : OceanInsightsIncomingMessageLogger
	{
		protected override TransactionScope GetTransactionScope(IPipelineContext context)
		{
			return new TransactionScope();
		}
	}
}

