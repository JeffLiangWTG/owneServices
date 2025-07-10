using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;
using Moq;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageTypeProcessors
{
	public class ProtobufMessageTypeProcessorTypesTests : TestCaseWithFactory
	{
		public void TestExpectedEhubMessageProcessorsExist()
		{
			// Arrange
			var internalProcessors = typeProcessor.MessageProcessors.Select(p => p.GetType());

			// Act
			// Assert
			AssertContainsExactElementsInAnyOrder("Expected message processors should exist", expectedMessageProcessors, internalProcessors);
		}

		public void TestAllProcessorsThatExistAreRegistered()
		{
			// Arrange
			var messageProcessorInterface = typeof(IMessageProcessor);
			var excludedTypes = new[]
			{
				typeof(IMessageProcessor),
			};

			var actualMessageProcessorTypes = typeProcessor.MessageProcessors.Select(mp => mp.GetType());

			// Act
			var expectedMessageProcessorTypes = messageProcessorInterface.Assembly
				.GetTypes()
				.Where(type => !excludedTypes.Contains(type))
				.Where(messageProcessorInterface.IsAssignableFrom);

			// Assert
			AssertContainsExactElementsInAnyOrder("All message processors must be registered. Or your message won't be processed...", expectedMessageProcessorTypes, actualMessageProcessorTypes);
		}

		public void TestEhubClientExtensionHook()
		{
			// Arrange
			ProtobufMessageTypeProcessor.SetClientExtensionHook(new[] { new DummyClientExtensionNotificationProcessor() });
			var processorWithClientExtensionHook = new ProtobufMessageTypeProcessor(mockLogger.Object, Factory);
			var internalProcessors = processorWithClientExtensionHook.MessageProcessors.Select(p => p.GetType());

			// Act
			var expectedProcessors = new List<Type>(expectedMessageProcessors)
			{
				typeof(DummyClientExtensionNotificationProcessor)
			};

			// Assert
			AssertContainsExactElementsInAnyOrder("Expected message processors should include dummy processor", expectedProcessors, internalProcessors);
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockLogger = new Mock<ILogger>();
			typeProcessor = new ProtobufMessageTypeProcessor(mockLogger.Object, Factory);
		}

		ProtobufMessageTypeProcessor typeProcessor;
		Mock<ILogger> mockLogger;

		readonly List<Type> expectedMessageProcessors = new List<Type>()
		{
			typeof(M2CDeviceAssignedToSystemNotificationProcessor),
			typeof(W2CDeviceRegistrationResponseMessageProcessor),
			typeof(M2CDeviceRevokedFromSystemNotificationProcessor),
			typeof(M2CDeviceLocationDataNotificationProcessor),
		};

		#region Test Class

		class DummyClientExtensionNotificationProcessor : IMessageProcessor
		{
			public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer message)
			{
			}

			public EHubMessageType MessageType => EHubMessageType.InvalidType;
		}

		#endregion
	}
}
