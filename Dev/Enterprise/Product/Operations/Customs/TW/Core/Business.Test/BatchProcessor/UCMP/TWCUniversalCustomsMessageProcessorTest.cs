using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCUniversalCustomsMessageProcessor))]
	sealed class TWCUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<TWCUniversalCustomsMessageProcessor>
	{
		protected override string ApplicationCode => EDIMessage.ApplicationCodes.TaiwanCustoms;

		public void TestCustomsDeliveryNotificationMessage()
		{
			var message = TestMessageFactory.GetIncomingCustomsDeliveryNotificationEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestTranshipmentProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingTranshipmentProcessorEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestManifestMessageProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingManifestMessageProcessorEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestManifestDeliveryNotificationProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingManifestDeliveryNotificationProcessorEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message, false, true);
		}

		public void TestTransferApplicationFromEHubNotificationProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingProcessTransferApplicationFromEHubNotificationEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestControllingAgencyNotificationProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingControllingAgencyNotificationProcessorEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		public void TestLicensingMessageProcessorMessage()
		{
			var licensingMessageProcessorEDIMessageResult = TestMessageFactory.GetIncomingLicensingMessageProcessorEDIMessage(Factory, ControllingMessageTypeList.Codes.NX301, "2332270800110611000X", "CA  1245600071", MessageTypeList.Codes._302, "NX302", "N");
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(licensingMessageProcessorEDIMessageResult.IncomingMessage, true);

			licensingMessageProcessorEDIMessageResult = TestMessageFactory.GetIncomingLicensingMessageProcessorEDIMessage(Factory, ControllingMessageTypeList.Codes.NX301, "23322708001106110001", "CA  1245600072", MessageTypeList.Codes._302, "NX302", "N");
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(licensingMessageProcessorEDIMessageResult.IncomingMessage);
		}

		public void TestCustomsDeclarationMessageProcessorMessage()
		{
			var message = TestMessageFactory.GetIncomingCustomsDeclarationMessageProcessorEDIMessage(Factory);
			AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(message);
		}

		void AssertTWCUniversalCustomsMessageProcessorCanHandleMessage(EDIMessage message, bool expectDiscarded = false, bool isManifestDeliveryNotificationProcessor = false)
		{
			using (EnableUCMP())
			{
				var logger = TWInboundInterchangeProcessorTest.GetNewLoggerForTesting();
				var processor = new TWCUniversalCustomsMessageProcessor();
				AssertEquals("Pre-condition: message queued", EDIMessage.Status.Queued, message.EM_Status.ToString());

				var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, logger);
				var branch = processor.GetBranch(message, logger, linkedBusinessObjectMetaData.ReturnValue.BranchPk);
				var serializationKeysResult = processor.GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData.ReturnValue);
				if (string.IsNullOrEmpty(serializationKeysResult.DiscardReason))
				{
					message.EM_Status = EDIMessage.Status.PreProcessedOK;
					message.EM_LinkTable = linkedBusinessObjectMetaData.ReturnValue.LinkTableName;
					message.EM_LinkUniqueID = linkedBusinessObjectMetaData.ReturnValue.LinkUniqueID;

					processor.ProcessMessage(message, logger, default);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;
				}

				if (expectDiscarded)
				{
					AssertEquals(EDIMessage.Status.Discarded, message.EM_Status.ToString());
				}
				else
				{
					AssertEquals(isManifestDeliveryNotificationProcessor ? EDIMessage.Status.Sent : EDIMessage.Status.ProcessedOK, message.EM_Status.ToString());
					var log = message.EM_LinkedObject.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageReceived.Code)).FirstOrDefault();
					AssertEquals(message.EM_MessageType, log.SL_Reference);
				}
			}

			static IDisposable EnableUCMP() => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, value: true);
		}
	}
}
