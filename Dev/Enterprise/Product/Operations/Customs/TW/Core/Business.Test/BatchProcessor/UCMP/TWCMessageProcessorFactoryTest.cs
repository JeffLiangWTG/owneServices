using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCMessageProcessorFactory))]
	sealed class TWCMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestGetMessageProcessor_CustomsDeliveryNotificationProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.ECD;
			AssertType<CustomsDeliveryNotificationProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_TranshipmentProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			AssertType<TranshipmentProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_ManifestMessageProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.FHR;
			AssertType<ManifestMessageProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_ManifestDeliveryNotificationProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.FCF;
			AssertType<ManifestDeliveryNotificationProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_TransferApplicationFromEHubNotificationProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TRA;
			AssertType<TransferApplicationFromEHubNotificationProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_ControllingAgencyNotificationProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetTWNotification("SNT", "NXM", "22099131002309220001", "00000000001305020580", "101");
			testMessage.EM_MessageType = MessageTypeList.Codes._101;
			AssertType<ControllingAgencyNotificationProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_LicensingMessageProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes._102;
			AssertType<LicensingMessageProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_CustomsDeclarationMessageProcessor()
		{
			var testMessage = Factory.New<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TPC;
			AssertType<CustomsDeclarationMessageProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		public void TestGetMessageProcessor_UnsupportedTypeMessageProcessor()
		{
			var testMessage = Factory.New<EDIMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes.TPC;
			AssertType<TWCMessageProcessorFactory.UnsupportedTypeMessageProcessor>(TWCMessageProcessorFactory.GetMessageProcessor(testMessage, logger));
		}

		readonly LoggingInformation logger = new();
	}
}
