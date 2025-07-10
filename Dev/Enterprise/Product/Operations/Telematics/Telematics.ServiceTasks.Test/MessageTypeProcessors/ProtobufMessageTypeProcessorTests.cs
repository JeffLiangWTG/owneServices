using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ProtoBuf;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageTypeProcessors
{
	public class MobileServicesEHubMessageProcessorTests : TestCaseWithFactory
	{
		public void TestLoadProtobufDataMessage_InternalIsRaw()
		{
			var factory = Factory;

			var testData = BuildTestData();
			ProtobufEHubHelpers.CreateEHubProtobufMessage(factory, testData);
			factory.Save();

			ehubMessageProcessor.Run(CancellationToken.None);
			AssertExpectedMessages(mockMessageProcessor.ProcessedMessages);
		}

		public void TestLoadProtobufDataMessage_InternalIsCompressed()
		{
			var factory = Factory;

			var testData = BuildCompressedTestData();
			ProtobufEHubHelpers.CreateEHubProtobufMessage(factory, testData);
			factory.Save();

			ehubMessageProcessor.Run(CancellationToken.None);
			AssertExpectedMessages(mockMessageProcessor.ProcessedMessages);
		}

		public void TestLoadProtobufDataMessage_SetsStatusesToRecievedOnSuccess()
		{
			var factory = Factory;

			var testData = BuildTestData();
			var pk = ProtobufEHubHelpers.CreateEHubProtobufMessage(factory, testData);
			factory.Save();

			ehubMessageProcessor.Run(CancellationToken.None);

			var message = factory.Load<EDIMessage>(pk);
			AssertEquals(nameof(EDIMessage), EDIMessageStatusList.Codes.Received, message.EM_Status);
			AssertEquals(nameof(EDIInterchange), EDIInterchangeStatusList.Codes.Received, message.Interchange.EI_Status);
		}

		public void TestLoadProtobufDataMessage_IncrementsRetryCountOnException()
		{
			var factory = Factory;
			var pk = ProtobufEHubHelpers.CreateEHubProtobufMessage(factory, Encoding.UTF8.GetBytes("This isn't protobuf data and should fail the test."));
			factory.Save();

			var message = factory.Load<EDIMessage>(pk);
			AssertEquals("Sanity check: Message should start off as queued", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			CombineAssertions(() =>
			{
				for (var i = 0; i < EHubMessageProcessor.MaximumRetryCount - 1; i++)
				{
					// Run message processor, make sure it doesn't throw
					AssertNoExceptionThrown(() => ehubMessageProcessor.Run(CancellationToken.None));

					message.Reload();
					AssertEquals("Message should remain marked as queued", EDIMessageStatusList.Codes.Queued, message.EM_Status);
					AssertEquals("Interchange should remain marked as queued", EDIInterchangeStatusList.Codes.Queued, message.Interchange.EI_Status);
					AssertEquals("Interchange retry count should be incremented", i + 1, message.Interchange.EI_RetryCount);
				}
			});
			ErrorReporter.Clear();
		}

		public void TestLoadProtobufDataMessage_FailsMessageOnExceptionIfMaximumRetryCountReached()
		{
			var factory = Factory;
			var pk = ProtobufEHubHelpers.CreateEHubProtobufMessage(factory, Encoding.UTF8.GetBytes("This isn't protobuf data and should fail the test."));
			var message = factory.Load<EDIMessage>(pk);
			message.Interchange.EI_RetryCount = EHubMessageProcessor.MaximumRetryCount - 1;

			AssertEquals("Sanity check: Message should start off as queued", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			factory.Save();

			// Run message processor, make sure it doesn't throw
			AssertNoExceptionThrown(() => ehubMessageProcessor.Run(CancellationToken.None));

			message.Reload();
			AssertEquals("Message should be marked as failed", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("Interchange should be marked as failed", EDIInterchangeStatusList.Codes.Failed, message.Interchange.EI_Status);
			ErrorReporter.Clear();
		}

		public void TestProcessorReportsOccuredExceptions()
		{
			// Arrange
			var badPk = ProtobufEHubHelpers.CreateEHubProtobufMessage(Factory, Encoding.UTF8.GetBytes("This isn't protobuf data and should fail the test."));
			var goodPk = ProtobufEHubHelpers.CreateEHubProtobufMessage(Factory, BuildTestData());
			Factory.Save();

			// Act
			ehubMessageProcessor.Run(CancellationToken.None);

			// Assert
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			var message = Factory.Load<EDIMessage>(goodPk);
			AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
			AssertEquals(EDIInterchangeStatusList.Codes.Received, message.Interchange.EI_Status);
			message = Factory.Load<EDIMessage>(badPk);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, message.Interchange.EI_Status);
			ErrorReporter.Clear();
		}

		static void AssertExpectedMessages(IEnumerable<EHubMessageContainer> containers)
		{
			var array = containers.ToArray();
			AssertEquals("Should have processed 4 messages", 4, array.Length);

			var container = array[0];
			AssertEquals(EHubMessageType.M2CDeviceAssignedToSystemNotification, container.message_type);
			var assignmentBody = container.GetInternalMessage<M2CDeviceAssignedToSystemNotificationMessage>();
			Assert("Device identifier should be 0x1", new byte[] { 0x1 }.SequenceEqual(assignmentBody.device_identifier));
			AssertEquals("AI00000001", assignmentBody.device_friendly_identifier);
			AssertEquals("The Fruit Company", assignmentBody.device_manufacturer);
			AssertEquals("RoundedRectangle3,1", assignmentBody.device_model);

			container = array[1];
			AssertEquals(EHubMessageType.M2CDeviceRevokedFromSystemNotification, container.message_type);
			var revocationBody = container.GetInternalMessage<M2CDeviceRevokedFromSystemNotificationMessage>();
			Assert("Device identifier should be 0x1", new byte[] { 0x1 }.SequenceEqual(revocationBody.device_identifier));

			container = array[2];
			AssertEquals(EHubMessageType.M2CDeviceLocationDataNotification, container.message_type);
			var locationBody = container.GetInternalMessage<M2CDeviceLocationDataNotificationMessage>();
			Assert("Device identifier should be 0x1", new byte[] { 0x1 }.SequenceEqual(locationBody.device_identifier));
			AssertEquals("Should have no locations", false, locationBody.device_locations.Any());
		}

		public void TestDifferentMessageSubTypesDoNotBrakeTheService()
		{
			// Arrange
			var messages = Enumerable.Range(0, 10).Select(i => EHubHelpers.CreateEHubMessage(Factory,
					ApplicationCodeList.Codes.Telematics,
					EDIMessageStatusList.Codes.Queued,
					true,
					EDIMessageTypeList.Codes.XDC,
					$"{i:D3}",
					"some content",
					ReceiveTransmitList.Codes.Receive))
				.ToList();
			Factory.Save();

			// Act
			ehubMessageProcessor.Run(CancellationToken.None);

			// Assert
			AssertContainsExactElementsInAnyOrder(messages, Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, SQLComparisonOperator.Equal, messages)).Select(message => message.PK));
		}

		public void TestProperIndexExists()
		{
			AssertEquals(true, IndexLoader.Exists(TestConnection, EDIMessageSchema.Constants.SqlSchemaName, EDIMessageSchema.Constants.TableName
				, EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
		}

		protected override void SetUp()
		{
			var factory = Factory;

			// Messages to ignore
			EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Unknown, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XMS, "", "blah", ReceiveTransmitList.Codes.Receive);
			EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Failed, true, EDIMessageTypeList.Codes.XMS, "", "blah", ReceiveTransmitList.Codes.Receive);
			EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, false, EDIMessageTypeList.Codes.XMS, "", "blah", ReceiveTransmitList.Codes.Receive);
			EHubHelpers.CreateEHubMessage(factory, ApplicationCodeList.Codes.Telematics, EDIMessageStatusList.Codes.Queued, true, EDIMessageTypeList.Codes.XDC, "", "blah", ReceiveTransmitList.Codes.Receive);

			mockLogger = new Mock<ILogger>();
			ehubMessageProcessor = new EHubMessageProcessor(mockLogger.Object, factory);
			mockMessageProcessor = new MockEHubContainerProcessor();
			((ProtobufMessageTypeProcessor)ehubMessageProcessor.messageTypeProcessors[TelematicsMessageList.Codes.ProtobufData]).MessageProcessors = new[] { mockMessageProcessor };
		}

		EHubMessageProcessor ehubMessageProcessor;
		Mock<ILogger> mockLogger;
		MockEHubContainerProcessor mockMessageProcessor;

		#region Mock

		class MockEHubContainerProcessor : IMessageProcessor
		{
			public MockEHubContainerProcessor()
			{
				processedMessages = new List<EHubMessageContainer>();
			}

			public IEnumerable<EHubMessageContainer> ProcessedMessages => processedMessages;

			readonly List<EHubMessageContainer> processedMessages;

			#region IMobileServicesMessageProcessor

			public EHubMessageType MessageType => EHubMessageType.InvalidType;

			public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer container)
			{
				processedMessages.Add(container);
			}

			#endregion
		}

		#endregion

		#region Test Data

		static uint WriteTestData(Stream stream)
		{
			var numMessages = 0u;

			var fruitCompanyDeviceAssigned = new M2CDeviceAssignedToSystemNotificationMessage
			{
				device_identifier = new byte[] { 0x01 },
				device_friendly_identifier = "AI00000001",
				device_manufacturer = "The Fruit Company",
				device_model = "RoundedRectangle3,1"
			};

			var fruitCompanyDeviceRevoked = new M2CDeviceRevokedFromSystemNotificationMessage
			{
				device_identifier = new byte[] { 0x01 }
			};

			var fruitCompanyDeviceLocation = new M2CDeviceLocationDataNotificationMessage
			{
				device_identifier = new byte[] { 0x01 }
			};

			WriteMessage(stream, fruitCompanyDeviceAssigned, EHubMessageType.M2CDeviceAssignedToSystemNotification);
			numMessages++;

			WriteMessage(stream, fruitCompanyDeviceRevoked, EHubMessageType.M2CDeviceRevokedFromSystemNotification);
			numMessages++;

			WriteMessage(stream, fruitCompanyDeviceLocation, EHubMessageType.M2CDeviceLocationDataNotification);
			numMessages++;

			WriteMessage(stream, fruitCompanyDeviceLocation, EHubMessageType.InvalidType);
			numMessages++;

			return numMessages;
		}

		static void WriteMessage<TMessage>(Stream stream, TMessage message, EHubMessageType type)
		{
			var container = new EHubMessageContainer();
			container.SetInternalMessage(message);
			container.message_type = type;

			using (var ms = new MemoryStream())
			{
				Serializer.Serialize(ms, container);

				var writer = new BinaryWriter(stream); // Do not dispose
				writer.Write((uint)ms.Length);
				writer.Write(ms.ToArray());
			}
		}

		static byte[] BuildCompressedTestData()
		{
			var message = new EHubMessageData { compressed = true };

			using (var ms = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(ms, CompressionMode.Compress))
				{
					message.messages_count = WriteTestData(gzipStream);
				}

				message.message_data_stream = ms.ToArray();
			}

			return message.Serialize();
		}

		static byte[] BuildTestData()
		{
			var message = new EHubMessageData { compressed = false };

			using (var ms = new MemoryStream())
			{
				message.messages_count = WriteTestData(ms);
				message.message_data_stream = ms.ToArray();
			}

			return message.Serialize();
		}

		#endregion
	}
}
